// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;

namespace Aura.Channel.World.Dungeons.Guilds
{
	public class GuildManager
	{
		private object _syncLock = new object();

		private Dictionary<long, Guild> _guilds;

		public int Count { get { lock (_syncLock)return _guilds.Count; } }

		public GuildManager()
		{
			_guilds = new Dictionary<long, Guild>();
		}

		public void Initialize()
		{
			this.LoadGuilds();
		}

		private void LoadGuilds()
		{
			var guilds = ChannelServer.Instance.Database.GetGuilds();
			foreach (var guild in guilds)
			{
				Log.Debug("0x{0:X16} - {1}", guild.Id, guild.Name);
				this.LoadGuild(guild);
			}
		}

		private void LoadGuild(Guild guild)
		{
			lock (_syncLock)
				_guilds[guild.Id] = guild;
		}
	}
}
