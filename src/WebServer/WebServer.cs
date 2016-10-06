// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using Aura.Shared;
using Aura.Shared.Database;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using Aura.Web.Scripting;
using Aura.Web.Util;
using Swebs;
using System;
using System.Collections.Generic;
using System.Net;

namespace Aura.Web
{
	public class WebServer : ServerMain
	{
		public static readonly WebServer Instance = new WebServer();

		private bool _running = false;
		private List<object> _swebsReferences = new List<object>();

		/// <summary>
		/// Actual web server
		/// </summary>
		public HttpServer HttpServer { get; private set; }

		/// <summary>
		/// Database
		/// </summary>
		public AuraDb Database { get; private set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public WebConf Conf { get; private set; }

		/// <summary>
		/// Script manager
		/// </summary>
		public ScriptManager ScriptManager { get; private set; }

		/// <summary>
		/// Initializes fields and properties
		/// </summary>
		private WebServer()
		{
			this.ScriptManager = new ScriptManager();
		}

		/// <summary>
		/// Loads all necessary components and starts the server.
		/// </summary>
		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");

			CliUtil.WriteHeader("Web Server", ConsoleColor.DarkRed);
			CliUtil.LoadingTitle();

			this.NavigateToRoot();

			// Conf
			this.LoadConf(this.Conf = new WebConf());

			// Database
			this.InitDatabase(this.Database = new AuraDb(), this.Conf);

			// Data
			this.LoadData(DataLoad.Features, false);

			// Localization
			this.LoadLocalization(this.Conf);

			// Server
			this.StartWebServer();

			// Scripts (after web server)
			this.LoadScripts();

			CliUtil.RunningTitle();
			_running = true;

			// Commands
			var commands = new ConsoleCommands();
			commands.Wait();
		}

		/// <summary>
		/// Sets up default controllers and starts web server
		/// </summary>
		public void StartWebServer()
		{
			Log.Info("Starting web server...");

			// Trick compiler into referencing Mabi.dll and Data.dll,
			// so Swebs references it in the C# scripts as well.
			_swebsReferences.Add(Mabi.Const.GuildMemberRank.Applied);
			_swebsReferences.Add(AuraData.FeaturesDb);

			var conf = new Configuration();
			conf.Port = this.Conf.Web.Port;
			conf.SourcePaths.Add("user/web/");
			conf.SourcePaths.Add("system/web/");

			this.HttpServer = new HttpServer(conf);
			this.HttpServer.RequestReceived += (s, e) =>
			{
				Log.Debug("[{0}] - {1}", e.Request.HttpMethod, e.Request.Path);
				//Log.Debug(e.Request.UserAgent); // Client: TEST_ARI
			};
			this.HttpServer.UnhandledException += (s, e) => Log.Exception(e.Exception);

			try
			{
				this.HttpServer.Start();

				Log.Status("Server ready, listening on 0.0.0.0:{0}.", this.Conf.Web.Port);
			}
			catch (NHttpException)
			{
				Log.Error("Failed to start web server.");
				Log.Info("Port {0} might already be in use, make sure no other application, like other web servers or Skype, are using it or set a different port in web.conf.", this.Conf.Web.Port);
				CliUtil.Exit(1);
			}
		}

		/// <summary>
		/// Loads web scripts
		/// </summary>
		private void LoadScripts()
		{
			this.ScriptManager.LoadScripts("system/scripts/scripts_web.txt");
		}
	}
}
