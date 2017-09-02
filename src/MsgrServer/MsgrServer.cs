// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Msgr.Chat;
using Aura.Msgr.Database;
using Aura.Msgr.Network;
using Aura.Msgr.Util;
using Aura.Shared;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;

namespace Aura.Msgr
{
    public class MsgrServer : ServerMain
    {
        public static readonly MsgrServer Instance = new MsgrServer();

        private bool _running;

        /// <summary>
        ///     Initializes msgr server.
        /// </summary>
        private MsgrServer()
        {
            Database = new MsgrDb();
            Conf = new MsgrConf();
            UserManager = new UserManager();
            GuildManager = new GuildManager();
            ChatSessionManager = new ChatSessionManager();

            Server = new MsgrServerServer();
            Server.Handlers = new MsgrServerHandlers();
            Server.Handlers.AutoLoad();
        }

        /// <summary>
        ///     Instance of the actual server component.
        /// </summary>
        // TODO: Our naming sucks, rename "servers" to connection managers or
        //   something, rename "clients" to connections.
        private MsgrServerServer Server { get; }

        /// <summary>
        ///     Database
        /// </summary>
        public MsgrDb Database { get; private set; }

        /// <summary>
        ///     Configuration
        /// </summary>
        public MsgrConf Conf { get; private set; }

        /// <summary>
        ///     Manager for all online users.
        /// </summary>
        public UserManager UserManager { get; }

        /// <summary>
        ///     Manager for all guilds.
        /// </summary>
        public GuildManager GuildManager { get; }

        /// <summary>
        ///     Manager for all chat sessions.
        /// </summary>
        public ChatSessionManager ChatSessionManager { get; }

        public void Run()
        {
            if (_running)
                throw new Exception("Server is already running.");
            _running = true;

            CliUtil.WriteHeader("Msgr Server", ConsoleColor.DarkCyan);
            CliUtil.LoadingTitle();

            NavigateToRoot();

            // Conf
            LoadConf(Conf = new MsgrConf());

            // Database
            InitDatabase(Database = new MsgrDb(), Conf);

            // Localization
            LoadLocalization(Conf);

            // Initialization
            GuildManager.Initialize();

            // Start
            Server.Start(Conf.Msgr.Port);

            CliUtil.RunningTitle();

            var cmd = new ConsoleCommands();
            cmd.Wait();
        }
    }
}