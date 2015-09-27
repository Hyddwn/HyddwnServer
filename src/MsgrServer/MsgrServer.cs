// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Msgr.Chat;
using Aura.Msgr.Database;
using Aura.Msgr.Network;
using Aura.Msgr.Util;
using Aura.Shared;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using System;

namespace Aura.Msgr
{
	public class MsgrServer : ServerMain
	{
		public readonly static MsgrServer Instance = new MsgrServer();

		private bool _running = false;

		/// <summary>
		/// Instance of the actual server component.
		/// </summary>
		// TODO: Our naming sucks, rename "servers" to connection managers or
		//   something, rename "clients" to connections.
		private MsgrServerServer Server { get; set; }

		/// <summary>
		/// Database
		/// </summary>
		public MsgrDb Database { get; private set; }

		/// <summary>
		/// Configuration
		/// </summary>
		public MsgrConf Conf { get; private set; }

		/// <summary>
		/// Manager for all online users.
		/// </summary>
		public UserManager UserManager { get; private set; }

		/// <summary>
		/// Manager for all chat sessions.
		/// </summary>
		public ChatSessionManager ChatSessionManager { get; private set; }

		/// <summary>
		/// Initializes msgr server.
		/// </summary>
		private MsgrServer()
		{
			this.Database = new MsgrDb();
			this.Conf = new MsgrConf();
			this.UserManager = new UserManager();
			this.ChatSessionManager = new ChatSessionManager();

			this.Server = new MsgrServerServer();
			this.Server.Handlers = new MsgrServerHandlers();
			this.Server.Handlers.AutoLoad();
		}

		public void Run()
		{
			if (_running)
				throw new Exception("Server is already running.");
			_running = true;

			CliUtil.WriteHeader("Msgr Server", ConsoleColor.DarkCyan);
			CliUtil.LoadingTitle();

			this.NavigateToRoot();

			// Conf
			this.LoadConf(this.Conf = new MsgrConf());

			// Database
			this.InitDatabase(this.Database = new MsgrDb(), this.Conf);

			// Start
			this.Server.Start(this.Conf.Msgr.Port);

			CliUtil.RunningTitle();

			var cmd = new ConsoleCommands();
			cmd.Wait();
		}
	}
}
