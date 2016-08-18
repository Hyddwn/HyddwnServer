// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

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
				return;

			var guildId = Convert.ToInt64(prop.Xml.Attribute("guildid").Value);
			Log.Debug("0x{0:X16}", guildId);

			if (creature.GuildId == guildId)
			{
				// If member
				if (creature.GuildMember.Rank < GuildMemberRank.Applied)
				{
					// Open guild panel
				}
				else
				{
					// Show guild info
				}
			}
			else
			{
				// Show guild info incl join
			}
		}
	}
}
