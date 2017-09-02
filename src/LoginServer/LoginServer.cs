// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Login.Network;
using Aura.Login.Network.Handlers;
using Aura.Login.Scripting;
using Aura.Login.Util;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Swebs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Aura.Login
{
	public class LoginServer : ServerMain
	{
		public static readonly LoginServer Instance = new LoginServer();

		private bool _running = false;

		/// <summary>
		/// Instance of the actual server component.
		/// </summary>
		public DefaultServer<LoginClient> Server { get; set; }

		/// <summary>
		/// List of servers and channels.
		/// </summary>
		public ServerInfoManager ServerList { get; private set; }

		/// <summary>
		/// Database
		/// </summary>
		public LoginDb Database { get; private set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public LoginConf Conf { get; private set; }

		/// <summary>
		/// List of connected channel clients.
		/// </summary>
		public List<LoginClient> ChannelClients { get; private set; }

		/// <summary>
		/// Web API server
		/// </summary>
		public HttpServer HttpServer { get; private set; }

		/// <summary>
		/// Login's script manager
		/// </summary>
		public ScriptManager ScriptManager { get; private set; }

		/// <summary>
		/// Initializes fields and properties
		/// </summary>
		private LoginServer()
		{
			this.Server = new DefaultServer<LoginClient>();
			this.Server.Handlers = new LoginServerHandlers();
			this.Server.Handlers.AutoLoad();
			this.Server.ClientDisconnected += this.OnClientDisconnected;

			this.ServerList = new ServerInfoManager();

			this.ChannelClients = new List<LoginClient>();

			this.ScriptManager = new ScriptManager();
		}

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");

			CliUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
			CliUtil.LoadingTitle();

			this.NavigateToRoot();

			// Conf
			this.LoadConf(this.Conf = new LoginConf());

			// Database
			this.InitDatabase(this.Database = new LoginDb(), this.Conf);

			// Check if there are any updates
			this.CheckDatabaseUpdates();

			// Data
			this.LoadData(DataLoad.LoginServer, false);

			// Localization
			this.LoadLocalization(this.Conf);

			// Web API
			this.LoadWebApi();

			// Scripts
			this.LoadScripts();

			// Start
			this.Server.Start(this.Conf.Login.Port);

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			var commands = new LoginConsoleCommands();
			commands.Wait();
		}

		private void OnClientDisconnected(LoginClient client)
		{
			var update = false;

			lock (this.ChannelClients)
			{
				if (this.ChannelClients.Contains(client))
				{
					this.ChannelClients.Remove(client);
					update = true;
				}
			}

			if (update)
			{
				var channel = (client.Account != null ? this.ServerList.GetChannel(client.Account.Name) : null);
				if (channel == null)
				{
					Log.Warning("Unregistered channel disconnected.");
					return;
				}
				Log.Status("Channel '{0}' disconnected, switched to Maintenance.", client.Account.Name);
				channel.State = ChannelState.Maintenance;

				Send.ChannelStatus(this.ServerList.List);
				Send.Internal_ChannelStatus(this.ServerList.List);
			}
		}

		private void CheckDatabaseUpdates()
		{
			Log.Info("Checking for updates...");

			var files = Directory.GetFiles("sql");
			foreach (var filePath in files.Where(file => Path.GetExtension(file).ToLower() == ".sql"))
				this.RunUpdate(Path.GetFileName(filePath));
		}

		private void RunUpdate(string updateFile)
		{
			if (LoginServer.Instance.Database.CheckUpdate(updateFile))
				return;

			Log.Info("Update '{0}' found, executing...", updateFile);

			LoginServer.Instance.Database.RunUpdate(updateFile);
		}

		public void Broadcast(Packet packet)
		{
			lock (this.Server.Clients)
			{
				foreach (var client in this.Server.Clients.Where(a => a.State == ClientState.LoggedIn))
				{
					client.Send(packet);
				}
			}
		}

		public void BroadcastChannels(Packet packet)
		{
			lock (this.Server.Clients)
			{
				foreach (var client in this.Server.Clients.Where(a => a.State == ClientState.LoggedIn && this.ChannelClients.Contains(a)))
				{
					client.Send(packet);
				}
			}
		}

		public void BroadcastPlayers(Packet packet)
		{
			lock (this.Server.Clients)
			{
				foreach (var client in this.Server.Clients.Where(a => a.State == ClientState.LoggedIn && !this.ChannelClients.Contains(a)))
				{
					client.Send(packet);
				}
			}
		}

		/// <summary>
		/// Starts web server for API
		/// </summary>
		private void LoadWebApi()
		{
			Log.Info("Loading Web API...");

			// Trick compiler into referencing Mabi.dll, so Swebs references
			// it in the C# scripts as well.
			var x = Mabi.Const.GuildMemberRank.Applied;

			var conf = new Configuration();
			conf.Port = this.Conf.Login.WebPort;
			conf.SourcePaths.Add("user/api/");
			conf.SourcePaths.Add("system/api/");

			this.HttpServer = new HttpServer(conf);
			this.HttpServer.UnhandledException += (s, e) => Log.Exception(e.Exception);

			try
			{
				this.HttpServer.Start();

				Log.Info("Web API listening on 0.0.0.0:{0}.", this.Conf.Login.WebPort);
			}
			catch (NHttpException)
			{
				Log.Error("Failed to start web server.");
				Log.Info("Port {0} might already be in use, make sure no other application, like other web servers or Skype, are using it or set a different port in web.conf.", this.Conf.Login.WebPort);
				CliUtil.Exit(1);
			}
		}

		/// <summary>
		/// Loads all login scripts.
		/// </summary>
		private void LoadScripts()
		{
			this.ScriptManager.LoadScripts("system/scripts/scripts_login.txt");
		}

		/// <summary>
		/// Sends request to kill account's connection to all channels.
		/// </summary>
		/// <param name="accountName"></param>
		public void RequestDisconnect(string accountName)
		{
			// Check if client is connected to this login server.
			var client = this.Server.Clients.FirstOrDefault(a => a.State != ClientState.Dead && a.Account != null && a.Account.Name == accountName);
			if (client != null)
				client.Kill();

			// Send DC request regardless, just in case.
			Send.Internal_RequestDisconnect(accountName);

			// Give everyone a moment to react
			Thread.Sleep(2000);

			// We've DCed from login and told all channels to DC the account.
			// However, if the account is wrongfully marked as being logged
			// in, we still have to set it to false, because the channels
			// and login couldn't find the account.
			this.Database.SetAccountLoggedIn(accountName, false);
		}
	}
}
