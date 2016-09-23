// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Database;
using Aura.Channel.Network;
using Aura.Channel.Network.Handlers;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using Aura.Channel.Skills;
using Aura.Channel.Util;
using Aura.Channel.Util.Configuration;
using Aura.Channel.World;
using Aura.Channel.World.Weather;
using Aura.Mabi.Network;
using Aura.Shared;
using Aura.Shared.Database;
using Aura.Shared.Network;
using Aura.Shared.Network.Crypto;
using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Channel.World.Guilds;

namespace Aura.Channel
{
	public class ChannelServer : ServerMain
	{
		public static readonly ChannelServer Instance = new ChannelServer();

		/// <summary>
		/// Milliseconds between connection tries.
		/// </summary>
		private const int LoginTryTime = 10 * 1000;

		private bool _running;

		/// <summary>
		/// Instance of the actual server component.
		/// </summary>
		public DefaultServer<ChannelClient> Server { get; protected set; }

		/// <summary>
		/// List of servers and channels.
		/// </summary>
		public ServerInfoManager ServerList { get; private set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public ChannelConf Conf { get; private set; }

		/// <summary>
		/// Database
		/// </summary>
		public ChannelDb Database { get; private set; }

		/// <summary>
		/// Client connecting to the login server.
		/// </summary>
		public InternalClient LoginServer { get; private set; }

		public GmCommandManager CommandProcessor { get; private set; }
		public ChannelConsoleCommands ConsoleCommands { get; private set; }

		public ScriptManager ScriptManager { get; private set; }
		public SkillManager SkillManager { get; private set; }
		public EventManager Events { get; private set; }
		public WeatherManager Weather { get; private set; }
		public PartyManager PartyManager { get; private set; }
		public GuildManager GuildManager { get; private set; }

		public WorldManager World { get; private set; }
		public bool ShuttingDown { get; private set; }
		private Timer Timer { get; set; }

		private ChannelServer()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			this.Server = new DefaultServer<ChannelClient>();
			this.Server.Handlers = new ChannelServerHandlers();
			this.Server.Handlers.AutoLoad();
			this.Server.ClientDisconnected += this.OnClientDisconnected;

			this.ServerList = new ServerInfoManager();

			this.CommandProcessor = new GmCommandManager();
			this.ConsoleCommands = new ChannelConsoleCommands();

			this.ScriptManager = new ScriptManager();
			this.SkillManager = new SkillManager();
			this.Events = new EventManager();
			this.Weather = new WeatherManager();
			this.PartyManager = new PartyManager();
			this.GuildManager = new GuildManager();

			this.Timer = new Timer(new TimerCallback(ShutdownTimerDone));
		}

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");

			CliUtil.WriteHeader("Channel Server", ConsoleColor.DarkGreen);
			CliUtil.LoadingTitle();

			this.NavigateToRoot();

			// Conf
			this.LoadConf(this.Conf = new ChannelConf());

			// Database
			this.InitDatabase(this.Database = new ChannelDb(), this.Conf);

			// Data
			this.LoadData(DataLoad.ChannelServer, false);

			// Localization
			this.LoadLocalization(this.Conf);

			// World
			this.InitializeWorld();

			// Skills
			this.LoadSkills();

			// Scripts
			this.LoadScripts();

			// Weather
			this.Weather.Initialize();

			// Autoban
			if (this.Conf.Autoban.Enabled)
				this.Events.SecurityViolation += (e) => Autoban.Incident(e.Client, e.Level, e.Report, e.StackReport);

			// Start
			this.Server.Start(this.Conf.Channel.ChannelPort);

			// Inter
			this.ConnectToLogin(true);
			this.StartStatusUpdateTimer();

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			this.ConsoleCommands.Wait();
		}

		/// <summary>
		/// Tries to connect to login server, keeps trying every 10 seconds
		/// till there is a success. Blocking.
		/// </summary>
		public void ConnectToLogin(bool firstTime)
		{
			if (this.LoginServer != null && this.LoginServer.State == ClientState.LoggedIn)
				throw new Exception("Channel already connected to login server.");

			Log.WriteLine();

			if (firstTime)
				Log.Info("Trying to connect to login server at {0}:{1}...", ChannelServer.Instance.Conf.Channel.LoginHost, ChannelServer.Instance.Conf.Channel.LoginPort);
			else
			{
				Log.Info("Trying to re-connect to login server in {0} seconds.", LoginTryTime / 1000);
				Thread.Sleep(LoginTryTime);
			}

			var success = false;
			while (!success)
			{
				try
				{
					if (this.LoginServer != null && this.LoginServer.State != ClientState.Dead)
						this.LoginServer.Kill();

					this.LoginServer = new InternalClient();
					this.LoginServer.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					this.LoginServer.Socket.Connect(ChannelServer.Instance.Conf.Channel.LoginHost, ChannelServer.Instance.Conf.Channel.LoginPort);

					var buffer = new byte[255];

					// Recv Seed, send back empty packet to get done with the challenge.
					this.LoginServer.Socket.Receive(buffer);
					this.LoginServer.Crypto = new MabiCrypto(BitConverter.ToUInt32(buffer, 0), false);
					this.LoginServer.Send(Packet.Empty());

					// Challenge end
					this.LoginServer.Socket.Receive(buffer);

					// Inject login server into normal data receiving.
					this.Server.AddReceivingClient(this.LoginServer);

					// Identify
					this.LoginServer.State = ClientState.LoggingIn;

					success = true;

					Send.Internal_ServerIdentify();
				}
				catch (Exception ex)
				{
					Log.Error("Unable to connect to login server. ({0})", ex.Message);
					Log.Info("Trying again in {0} seconds.", LoginTryTime / 1000);
					Thread.Sleep(LoginTryTime);
				}
			}

			Log.Info("Connection to login server at '{0}' established.", this.LoginServer.Address);
			Log.WriteLine();
		}

		/// <summary>
		/// Calculates the state of the channel.
		/// </summary>
		/// <remarks>
		/// When calculating the <see cref="ChannelState"/> we take into account
		/// whether the server is running as well as if it is in Maintenance.
		/// </remarks>
		/// <returns></returns>
		public ChannelState CalculateChannelState()
		{
			// Just in case this gets called
			if (this.ShuttingDown)
				return ChannelState.Maintenance;

			var current = this.World.CountPlayers();
			var max = this.Conf.Channel.MaxUsers;

			if (max == 0)
			{
				Log.Warning("Max user count was zero, falling back to Normal.");

				// Fallback value
				return ChannelState.Normal;
			}

			double stress = (current / max) * 100;

			if (stress >= 40 && stress <= 70)
				return ChannelState.Busy;
			if (stress > 70 && stress <= 95)
				return ChannelState.Full;
			if (stress > 95)
				return ChannelState.Bursting;

			return ChannelState.Normal;
		}

		private void OnClientDisconnected(ChannelClient client)
		{
			if (client == this.LoginServer)
				this.ConnectToLogin(false);
		}

		/// <summary>
		/// Handler for unhandled exceptions.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Log.Error("Oh no! Ferghus escaped his memory block and infected the rest of the server!");
				Log.Error("Aura has encountered an unexpected and unrecoverable error. We're going to try to save as much as we can.");
			}
			catch { }
			try
			{
				this.Server.Stop();
			}
			catch { }
			try
			{
				// save the world
			}
			catch { }
			try
			{
				Log.Exception((Exception)e.ExceptionObject);
				Log.Status("Closing server.");
			}
			catch { }

			CliUtil.Exit(1, false);
		}

		private void StartStatusUpdateTimer()
		{
			this.Events.MinutesTimeTick += (_) =>
			{
				if (this.LoginServer == null || this.LoginServer.State != ClientState.LoggedIn || this.ShuttingDown)
					return;

				Send.Internal_ChannelStatus();
			};
		}

		private void InitializeWorld()
		{
			Log.Info("Initializing world...");

			this.World = new WorldManager();
			this.World.Initialize();
			this.PartyManager.Initialize();
			this.GuildManager.Initialize();

			Log.Info("  done loading {0} regions.", this.World.Count);
			Log.Info("  done loading {0} guilds.", this.GuildManager.Count);
		}

		private void LoadScripts()
		{
			this.ScriptManager.Init();
			this.ScriptManager.Load();
		}

		private void LoadSkills()
		{
			this.SkillManager.AutoLoad();
		}

		public override void InitDatabase(AuraDb db, BaseConf conf)
		{
			base.InitDatabase(db, conf);

			// If items end up with temp ids in the db we'd get entity ids
			// that exist twice, when creating new temps later on.
			if (ChannelServer.Instance.Database.TmpItemsExist())
			{
				Log.Warning("InitDatabase: Found items with temp entity ids.");
				// TODO: clean up dbs
			}
		}

		/// <summary>
		/// Shutdown procedure for the current channel.
		/// </summary>
		/// <param name="time">The amount of time in seconds until shutdown.</param>
		public void Shutdown(int time)
		{
			this.ShuttingDown = true;

			var channel = this.ServerList.GetChannel(this.Conf.Channel.ChannelServer, this.Conf.Channel.ChannelName);

			if (channel == null)
			{
				Log.Warning("Unregistered channel.");
			}
			else
			{
				channel.State = ChannelState.Maintenance;
				Send.Internal_ChannelStatus();
				Log.Info("{0} switched to maintenance.", this.Conf.Channel.ChannelName);
			}

			var notice = Localization.GetPlural(
				"The server will be brought down for maintenance in {0} second. Please log out safely before then.",
				"The server will be brought down for maintenance in {0} seconds. Please log out safely before then.",
				time
			);

			Send.Internal_Broadcast(string.Format(notice, time));
			Send.RequestClientDisconnect(time);

			// Add a few seconds, as `time` is the moment when all clients
			// send their DC request, because of RequestClientDisconnect.
			// The channel should shutdown *after* that's done. 10 seconds
			// should be plenty.
			time += 10;

			Log.Info("Shutting down in {0} seconds...", time);

			this.Timer.Change(time * 1000, Timeout.Infinite);
		}

		private void ShutdownTimerDone(object timer)
		{
			((Timer)timer).Dispose();

			// Kill clients
			this.KillConnectedClients();

			// Save global variables
			this.Database.SaveVars("Aura System", 0, this.ScriptManager.GlobalVars.Perm);

			CliUtil.Exit(0, false);
		}

		/// <summary>
		/// Kills all clients currently connected to the channel.
		/// </summary>
		private void KillConnectedClients()
		{
			// Grab a copy of the list of users still currently logged in
			var shutdownClientList = this.Server.Clients.ToList<ChannelClient>();

			// Kill all clients still logged in
			foreach (var user in shutdownClientList)
			{
				try
				{
					if (user.State == ClientState.LoggedIn)
						user.Kill();
				}
				catch (Exception e)
				{
					Log.Exception(e, "Error killing client.");
				}
			}
		}
	}
}
