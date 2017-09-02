// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Shared;
using Aura.Shared.Database;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using Aura.Web.Scripting;
using Aura.Web.Util;
using MySql.Data.MySqlClient;
using Swebs;

namespace Aura.Web
{
    public class WebServer : ServerMain
    {
        public static readonly WebServer Instance = new WebServer();

        private bool _running;
        private readonly List<object> _swebsReferences = new List<object>();

        /// <summary>
        ///     Initializes fields and properties
        /// </summary>
        private WebServer()
        {
            ScriptManager = new ScriptManager();
        }

        /// <summary>
        ///     Actual web server
        /// </summary>
        public HttpServer HttpServer { get; private set; }

        /// <summary>
        ///     Database
        /// </summary>
        public AuraDb Database { get; private set; }

        /// <summary>
        ///     Configuration
        /// </summary>
        public WebConf Conf { get; private set; }

        /// <summary>
        ///     Script manager
        /// </summary>
        public ScriptManager ScriptManager { get; }

        /// <summary>
        ///     Loads all necessary components and starts the server.
        /// </summary>
        public void Run()
        {
            if (_running)
                throw new Exception("Server is already running.");

            CliUtil.WriteHeader("Web Server", ConsoleColor.DarkRed);
            CliUtil.LoadingTitle();

            NavigateToRoot();

            // Conf
            LoadConf(Conf = new WebConf());

            // Database
            InitDatabase(Database = new AuraDb(), Conf);

            // Data
            LoadData(DataLoad.Features, false);

            // Localization
            LoadLocalization(Conf);

            // Server
            StartWebServer();

            // Scripts (after web server)
            LoadScripts();

            CliUtil.RunningTitle();
            _running = true;

            // Commands
            var commands = new ConsoleCommands();
            commands.Add("reloaddata", "Reloads all data.", ReloadData);
            commands.Wait();
        }

        /// <summary>
        ///     Handles reloaddata command, reloading all data.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private CommandResult ReloadData(string command, IList<string> args)
        {
            LoadData(DataLoad.Features, true);
            return CommandResult.Okay;
        }

        /// <summary>
        ///     Sets up default controllers and starts web server
        /// </summary>
        public void StartWebServer()
        {
            Log.Info("Starting web server...");

            // Trick compiler into referencing Mabi.dll and Data.dll,
            // so Swebs references it in the C# scripts as well.
            _swebsReferences.Add(GuildMemberRank.Applied);
            _swebsReferences.Add(AuraData.FeaturesDb);
            _swebsReferences.Add(typeof(MySqlCommand));

            var conf = new Configuration();
            conf.Port = Conf.Web.Port;
            conf.SourcePaths.Add("user/web/");
            conf.SourcePaths.Add("system/web/");

            HttpServer = new HttpServer(conf);
            HttpServer.RequestReceived += (s, e) =>
            {
                Log.Debug("[{0}] - {1}", e.Request.HttpMethod, e.Request.Path);
                //Log.Debug(e.Request.UserAgent); // Client: TEST_ARI
            };
            HttpServer.UnhandledException += (s, e) => Log.Exception(e.Exception);

            try
            {
                HttpServer.Start();

                Log.Status("Server ready, listening on 0.0.0.0:{0}.", Conf.Web.Port);
            }
            catch (NHttpException)
            {
                Log.Error("Failed to start web server.");
                Log.Info(
                    "Port {0} might already be in use, make sure no other application, like other web servers or Skype, are using it or set a different port in web.conf.",
                    Conf.Web.Port);
                CliUtil.Exit(1);
            }
        }

        /// <summary>
        ///     Loads web scripts
        /// </summary>
        private void LoadScripts()
        {
            ScriptManager.LoadScripts("system/scripts/scripts_web.txt");
        }
    }
}