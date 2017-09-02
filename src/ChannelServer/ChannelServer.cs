// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Aura.Channel.Database;
using Aura.Channel.Network;
using Aura.Channel.Network.Handlers;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using Aura.Channel.Skills;
using Aura.Channel.Util;
using Aura.Channel.Util.Configuration;
using Aura.Channel.World;
using Aura.Channel.World.GameEvents;
using Aura.Channel.World.Guilds;
using Aura.Channel.World.Weather;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared;
using Aura.Shared.Database;
using Aura.Shared.Network;
using Aura.Shared.Network.Crypto;
using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;

namespace Aura.Channel
{
    public class ChannelServer : ServerMain
    {
        /// <summary>
        ///     Milliseconds between connection tries.
        /// </summary>
        private const int LoginTryTime = 10 * 1000;

        public static readonly ChannelServer Instance = new ChannelServer();

        private bool _running;

        private ChannelServer()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Server = new DefaultServer<ChannelClient>();
            Server.Handlers = new ChannelServerHandlers();
            Server.Handlers.AutoLoad();
            Server.ClientDisconnected += OnClientDisconnected;

            ServerList = new ServerInfoManager();

            CommandProcessor = new GmCommandManager();
            ConsoleCommands = new ChannelConsoleCommands();

            ScriptManager = new ScriptManager();
            SkillManager = new SkillManager();
            Events = new EventManager();
            Weather = new WeatherManager();
            PartyManager = new PartyManager();
            GuildManager = new GuildManager();
            GameEventManager = new GameEventManager();

            Timer = new Timer(ShutdownTimerDone);
        }

        /// <summary>
        ///     Instance of the actual server component.
        /// </summary>
        public DefaultServer<ChannelClient> Server { get; protected set; }

        /// <summary>
        ///     List of servers and channels.
        /// </summary>
        public ServerInfoManager ServerList { get; }

        /// <summary>
        ///     Configuration
        /// </summary>
        public ChannelConf Conf { get; private set; }

        /// <summary>
        ///     Database
        /// </summary>
        public ChannelDb Database { get; private set; }

        /// <summary>
        ///     Client connecting to the login server.
        /// </summary>
        public InternalClient LoginServer { get; private set; }

        public GmCommandManager CommandProcessor { get; }
        public ChannelConsoleCommands ConsoleCommands { get; }

        public ScriptManager ScriptManager { get; }
        public SkillManager SkillManager { get; }
        public EventManager Events { get; }
        public WeatherManager Weather { get; }
        public PartyManager PartyManager { get; }
        public GuildManager GuildManager { get; }
        public GameEventManager GameEventManager { get; }

        public WorldManager World { get; private set; }
        public bool ShuttingDown { get; private set; }
        private Timer Timer { get; }

        /// <summary>
        ///     Loads all necessary components and starts the server.
        /// </summary>
        public void Run()
        {
            if (_running)
                throw new Exception("Server is already running.");

            CliUtil.WriteHeader("Channel Server", ConsoleColor.DarkGreen);
            CliUtil.LoadingTitle();

            NavigateToRoot();

            // Conf
            LoadConf(Conf = new ChannelConf());

            // Database
            InitDatabase(Database = new ChannelDb(), Conf);

            // Data
            LoadData(DataLoad.ChannelServer, false);

            // Localization
            LoadLocalization(Conf);

            // World
            InitializeWorld();

            // Skills
            LoadSkills();

            // Scripts
            LoadScripts();

            // Weather
            Weather.Initialize();

            // Autoban
            if (Conf.Autoban.Enabled)
                Events.SecurityViolation += e => Autoban.Incident(e.Client, e.Level, e.Report, e.StackReport);

            // Start
            Server.Start(Conf.Channel.ChannelPort);

            // Inter
            ConnectToLogin(true);
            StartStatusUpdateTimer();

            CliUtil.RunningTitle();
            _running = true;

            // Commands
            ConsoleCommands.Wait();
        }

        /// <summary>
        ///     Tries to connect to login server, keeps trying every 10 seconds
        ///     till there is a success. Blocking.
        /// </summary>
        public void ConnectToLogin(bool firstTime)
        {
            if (LoginServer != null && LoginServer.State == ClientState.LoggedIn)
                throw new Exception("Channel already connected to login server.");

            Log.WriteLine();

            if (firstTime)
            {
                Log.Info("Trying to connect to login server at {0}:{1}...", Instance.Conf.Channel.LoginHost,
                    Instance.Conf.Channel.LoginPort);
            }
            else
            {
                Log.Info("Trying to re-connect to login server in {0} seconds.", LoginTryTime / 1000);
                Thread.Sleep(LoginTryTime);
            }

            var success = false;
            while (!success)
                try
                {
                    if (LoginServer != null && LoginServer.State != ClientState.Dead)
                        LoginServer.Kill();

                    LoginServer = new InternalClient();
                    LoginServer.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    LoginServer.Socket.Connect(Instance.Conf.Channel.LoginHost, Instance.Conf.Channel.LoginPort);

                    var buffer = new byte[255];

                    // Recv Seed, send back empty packet to get done with the challenge.
                    LoginServer.Socket.Receive(buffer);
                    LoginServer.Crypto = new MabiCrypto(BitConverter.ToUInt32(buffer, 0), false);
                    LoginServer.Send(Packet.Empty());

                    // Challenge end
                    LoginServer.Socket.Receive(buffer);

                    // Inject login server into normal data receiving.
                    Server.AddReceivingClient(LoginServer);

                    // Identify
                    LoginServer.State = ClientState.LoggingIn;

                    success = true;

                    Send.Internal_ServerIdentify();
                }
                catch (Exception ex)
                {
                    Log.Error("Unable to connect to login server. ({0})", ex.Message);
                    Log.Info("Trying again in {0} seconds.", LoginTryTime / 1000);
                    Thread.Sleep(LoginTryTime);
                }

            Log.Info("Connection to login server at '{0}' established.", LoginServer.Address);
            Log.WriteLine();
        }

        /// <summary>
        ///     Calculates the state of the channel.
        /// </summary>
        /// <remarks>
        ///     When calculating the <see cref="ChannelState" /> we take into account
        ///     whether the server is running as well as if it is in Maintenance.
        /// </remarks>
        /// <returns></returns>
        public ChannelState CalculateChannelState()
        {
            // Just in case this gets called
            if (ShuttingDown)
                return ChannelState.Maintenance;

            var current = World.CountPlayers();
            var max = Conf.Channel.MaxUsers;

            if (max == 0)
            {
                Log.Warning("Max user count was zero, falling back to Normal.");

                // Fallback value
                return ChannelState.Normal;
            }

            double stress = current / max * 100;

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
            if (client == LoginServer)
                ConnectToLogin(false);
        }

        /// <summary>
        ///     Handler for unhandled exceptions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Error("Oh no! Ferghus escaped his memory block and infected the rest of the server!");
                Log.Error(
                    "Aura has encountered an unexpected and unrecoverable error. We're going to try to save as much as we can.");
            }
            catch
            {
            }
            try
            {
                Server.Stop();
            }
            catch
            {
            }
            try
            {
                // save the world
            }
            catch
            {
            }
            try
            {
                Log.Exception((Exception) e.ExceptionObject);
                Log.Status("Closing server.");
            }
            catch
            {
            }

            CliUtil.Exit(1, false);
        }

        private void StartStatusUpdateTimer()
        {
            Events.MinutesTimeTick += _ =>
            {
                if (LoginServer == null || LoginServer.State != ClientState.LoggedIn || ShuttingDown)
                    return;

                Send.Internal_ChannelStatus();
            };
        }

        private void InitializeWorld()
        {
            Log.Info("Initializing world...");

            World = new WorldManager();
            World.Initialize();
            PartyManager.Initialize();
            GuildManager.Initialize();
            GameEventManager.Initialize();

            Log.Info("  done loading {0} regions.", World.Count);
            Log.Info("  done loading {0} guilds.", GuildManager.Count);
        }

        private void LoadScripts()
        {
            ScriptManager.Init();
            ScriptManager.Load();
        }

        private void LoadSkills()
        {
            SkillManager.AutoLoad();
        }

        public override void InitDatabase(AuraDb db, BaseConf conf)
        {
            base.InitDatabase(db, conf);

            // If items end up with temp ids in the db we'd get entity ids
            // that exist twice, when creating new temps later on.
            if (Instance.Database.TmpItemsExist())
                Log.Warning("InitDatabase: Found items with temp entity ids.");
        }

        /// <summary>
        ///     Shutdown procedure for the current channel.
        /// </summary>
        /// <param name="time">The amount of time in seconds until shutdown.</param>
        public void Shutdown(int time)
        {
            ShuttingDown = true;

            var channel = ServerList.GetChannel(Conf.Channel.ChannelServer, Conf.Channel.ChannelName);

            if (channel == null)
            {
                Log.Warning("Unregistered channel.");
            }
            else
            {
                channel.State = ChannelState.Maintenance;
                Send.Internal_ChannelStatus();
                Log.Info("{0} switched to maintenance.", Conf.Channel.ChannelName);
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

            Timer.Change(time * 1000, Timeout.Infinite);
        }

        private void ShutdownTimerDone(object timer)
        {
            ((Timer) timer).Dispose();

            // Kill clients
            KillConnectedClients();

            // Save global variables
            Database.SaveVars("Aura System", 0, ScriptManager.GlobalVars.Perm);

            CliUtil.Exit(0, false);
        }

        /// <summary>
        ///     Kills all clients currently connected to the channel.
        /// </summary>
        private void KillConnectedClients()
        {
            // Grab a copy of the list of users still currently logged in
            var shutdownClientList = Server.Clients.ToList();

            // Kill all clients still logged in
            foreach (var user in shutdownClientList)
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