// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

namespace Aura.Login
{
    public class LoginServer : ServerMain
    {
        public static readonly LoginServer Instance = new LoginServer();

        private bool _running;

        /// <summary>
        ///     Initializes fields and properties
        /// </summary>
        private LoginServer()
        {
            Server = new DefaultServer<LoginClient>();
            Server.Handlers = new LoginServerHandlers();
            Server.Handlers.AutoLoad();
            Server.ClientDisconnected += OnClientDisconnected;

            ServerList = new ServerInfoManager();

            ChannelClients = new List<LoginClient>();

            ScriptManager = new ScriptManager();
        }

        /// <summary>
        ///     Instance of the actual server component.
        /// </summary>
        public DefaultServer<LoginClient> Server { get; set; }

        /// <summary>
        ///     List of servers and channels.
        /// </summary>
        public ServerInfoManager ServerList { get; }

        /// <summary>
        ///     Database
        /// </summary>
        public LoginDb Database { get; private set; }

        /// <summary>
        ///     Configuration
        /// </summary>
        public LoginConf Conf { get; private set; }

        /// <summary>
        ///     List of connected channel clients.
        /// </summary>
        public List<LoginClient> ChannelClients { get; }

        /// <summary>
        ///     Web API server
        /// </summary>
        public HttpServer HttpServer { get; private set; }

        /// <summary>
        ///     Login's script manager
        /// </summary>
        public ScriptManager ScriptManager { get; }

        /// <summary>
        ///     Loads all necessary components and starts the server.
        /// </summary>
        public void Run()
        {
            if (_running)
                throw new Exception("Server is already running.");

            CliUtil.WriteHeader("Login Server", ConsoleColor.Magenta);
            CliUtil.LoadingTitle();

            NavigateToRoot();

            // Conf
            LoadConf(Conf = new LoginConf());

            // Database
            InitDatabase(Database = new LoginDb(), Conf);

            // Check if there are any updates
            CheckDatabaseUpdates();

            // Data
            LoadData(DataLoad.LoginServer, false);

            // Localization
            LoadLocalization(Conf);

            // Web API
            LoadWebApi();

            // Scripts
            LoadScripts();

            // Start
            Server.Start(Conf.Login.Port);

            CliUtil.RunningTitle();
            _running = true;

            // Commands
            var commands = new LoginConsoleCommands();
            commands.Wait();
        }

        private void OnClientDisconnected(LoginClient client)
        {
            var update = false;

            lock (ChannelClients)
            {
                if (ChannelClients.Contains(client))
                {
                    ChannelClients.Remove(client);
                    update = true;
                }
            }

            if (update)
            {
                var channel = client.Account != null ? ServerList.GetChannel(client.Account.Name) : null;
                if (channel == null)
                {
                    Log.Warning("Unregistered channel disconnected.");
                    return;
                }
                Log.Status("Channel '{0}' disconnected, switched to Maintenance.", client.Account.Name);
                channel.State = ChannelState.Maintenance;

                Send.ChannelStatus(ServerList.List);
                Send.Internal_ChannelStatus(ServerList.List);
            }
        }

        private void CheckDatabaseUpdates()
        {
            Log.Info("Checking for updates...");

            var files = Directory.GetFiles("sql");
            foreach (var filePath in files.Where(file => Path.GetExtension(file).ToLower() == ".sql"))
                RunUpdate(Path.GetFileName(filePath));
        }

        private void RunUpdate(string updateFile)
        {
            if (Instance.Database.CheckUpdate(updateFile))
                return;

            Log.Info("Update '{0}' found, executing...", updateFile);

            Instance.Database.RunUpdate(updateFile);
        }

        public void Broadcast(Packet packet)
        {
            lock (Server.Clients)
            {
                foreach (var client in Server.Clients.Where(a => a.State == ClientState.LoggedIn))
                    client.Send(packet);
            }
        }

        public void BroadcastChannels(Packet packet)
        {
            lock (Server.Clients)
            {
                foreach (var client in Server.Clients.Where(a =>
                    a.State == ClientState.LoggedIn && ChannelClients.Contains(a)))
                    client.Send(packet);
            }
        }

        public void BroadcastPlayers(Packet packet)
        {
            lock (Server.Clients)
            {
                foreach (var client in Server.Clients.Where(a =>
                    a.State == ClientState.LoggedIn && !ChannelClients.Contains(a)))
                    client.Send(packet);
            }
        }

        /// <summary>
        ///     Starts web server for API
        /// </summary>
        private void LoadWebApi()
        {
            Log.Info("Loading Web API...");

            // Trick compiler into referencing Mabi.dll, so Swebs references
            // it in the C# scripts as well.
            var x = GuildMemberRank.Applied;

            var conf = new Configuration();
            conf.Port = Conf.Login.WebPort;
            conf.SourcePaths.Add("user/api/");
            conf.SourcePaths.Add("system/api/");

            HttpServer = new HttpServer(conf);
            HttpServer.UnhandledException += (s, e) => Log.Exception(e.Exception);

            try
            {
                HttpServer.Start();

                Log.Info("Web API listening on 0.0.0.0:{0}.", Conf.Login.WebPort);
            }
            catch (NHttpException)
            {
                Log.Error("Failed to start web server.");
                Log.Info(
                    "Port {0} might already be in use, make sure no other application, like other web servers or Skype, are using it or set a different port in web.conf.",
                    Conf.Login.WebPort);
                CliUtil.Exit(1);
            }
        }

        /// <summary>
        ///     Loads all login scripts.
        /// </summary>
        private void LoadScripts()
        {
            ScriptManager.LoadScripts("system/scripts/scripts_login.txt");
        }

        /// <summary>
        ///     Sends request to kill account's connection to all channels.
        /// </summary>
        /// <param name="accountName"></param>
        public void RequestDisconnect(string accountName)
        {
            // Check if client is connected to this login server.
            var client = Server.Clients.FirstOrDefault(a =>
                a.State != ClientState.Dead && a.Account != null && a.Account.Name == accountName);
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
            Database.SetAccountLoggedIn(accountName, false);
        }
    }
}