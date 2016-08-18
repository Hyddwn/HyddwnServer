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
		private Dictionary<long, Prop> _stones;

		public int Count { get { lock (_syncLock)return _guilds.Count; } }

		public GuildManager()
		{
			_guilds = new Dictionary<long, Guild>();
			_stones = new Dictionary<long, Prop>();
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
			this.PlaceStone(guild);
		}

		private void PlaceStone(Guild guild)
		{
			lock (_syncLock)
			{
				if (_stones.ContainsKey(guild.Id))
					return;
			}

			var stone = guild.Stone;

			var region = ChannelServer.Instance.World.GetRegion(stone.Location.RegionId);
			if (region == null)
				throw new ArgumentException("Region doesn't exist.");

			var prop = new Prop(stone.PropId, stone.Location.RegionId, stone.Location.X, stone.Location.Y, stone.Direction);
			prop.Title = guild.Name;
			prop.Xml.SetAttributeValue("guildid", guild.Id);
			if (guild.Has(GuildOptions.Warp))
				prop.Xml.SetAttributeValue("gh_warp", true);

			prop.Behavior = GuildStone.OnTouch;

			region.AddProp(prop);

			lock (_syncLock)
				_stones[guild.Id] = prop;
		}
	}
}
