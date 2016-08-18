// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;

namespace Aura.Channel.World.Dungeons.Guilds
{
	public class GuildStone
	{
		public int PropId { get; set; }
		public Location Location { get; set; }
		public float Direction { get; set; }

		public static void OnTouch(Creature creature, Prop prop)
		{
			if (prop.Xml.Attribute("guildid") == null)
			{
				Log.Warning("GuildStone.OnTouch: Stone is missing its guildid attribute.");
				return;
			}

			var guildId = Convert.ToInt64(prop.Xml.Attribute("guildid").Value);
			var guild = ChannelServer.Instance.GuildManager.GetGuild(guildId);
			if (guild == null)
			{
				Log.Warning("GuildStone.OnTouch: Guild '0x{0:X16}' not found.", guildId);
				return;
			}

			Log.Debug("0x{0:X16}", guildId);

			if (creature.GuildId == guildId)
			{
				// If member
				if (creature.GuildMember.Rank < GuildMemberRank.Applied)
				{
					Send.GuildPanel(creature, guild);
				}
				else
				{
					Send.GuildInfo(creature, guild);
				}
			}
			else
			{
				Send.GuildInfoNoGuild(creature, guild);
			}
		}
	}
}
