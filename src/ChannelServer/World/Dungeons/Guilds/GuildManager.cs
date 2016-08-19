// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Database;
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
				this.LoadGuild(guild);
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

			var region = ChannelServer.Instance.World.GetRegion(stone.RegionId);
			if (region == null)
				throw new ArgumentException("Region doesn't exist.");

			var prop = new Prop(stone.PropId, stone.RegionId, stone.X, stone.Y, stone.Direction);
			prop.Title = guild.Name;
			prop.Xml.SetAttributeValue("guildid", guild.Id);
			if (guild.Has(GuildOptions.Warp))
				prop.Xml.SetAttributeValue("gh_warp", true);

			prop.Behavior = OnStoneTouch;

			region.AddProp(prop);

			lock (_syncLock)
				_stones[guild.Id] = prop;
		}

		public static void OnStoneTouch(Creature creature, Prop prop)
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

		public Guild GetGuild(long guildId)
		{
			Guild result;
			lock (_syncLock)
				_guilds.TryGetValue(guildId, out result);
			return result;
		}

		public void SetGuildForCharacter(Creature character)
		{
			var guild = this.FindGuildWithMember(character.EntityId);
			if (guild == null)
				return;

			character.Guild = guild;
			character.GuildMember = guild.GetMember(character.EntityId);
		}

		public Guild FindGuildWithMember(long characterId)
		{
			lock (_syncLock)
			{
				foreach (var guild in _guilds.Values)
				{
					var member = guild.GetMember(characterId);
					if (member != null)
						return guild;
				}
			}

			return null;
		}

		public void ConvertPlayPoints(Creature creature, Guild guild)
		{
			var points = creature.PlayPoints;

			creature.PlayPoints = 0;
			guild.Points += points;

			Send.GuildMessage(creature, guild, Localization.GetPlural("Added {0:n0} Point.", "Added {0:0n} Points.", points), points);
		}

		/// <summary>
		/// Donates the given amount of gold from the creature to guild.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>d
		/// <param name="amount"></param>d
		public void DonateGold(Creature creature, Guild guild, int amount)
		{
			guild.Gold += amount;
			creature.Inventory.RemoveGold(amount);

			Send.GuildMessage(creature, guild, Localization.GetPlural("You have donated {0:n0} Gold.", "You have donated {0:n0} Gold.", amount), amount);
		}

		/// <summary>
		/// Donates the given item from the creature to guild.
		/// The item is converted to its worth, e.g. Gold and checks
		/// add their actual amounts, while other items use their
		/// sell price.
		/// </summary>
		/// <remarks>
		/// While it's seemingly pointless to make a dedicated DonateItem
		/// method, maybe we can use this for something at some point,
		/// e.g. donating items that other members can retrieve?
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public void DonateItem(Creature creature, Guild guild, Item item)
		{
			var amount = 0;

			// Gold (Pouch)
			if (item.Info.Id == 2000 || item.Data.StackItemId == 2000)
				amount = item.MetaData1.GetInt("EVALUE");
			// Checks/Licenses/Others?
			else if (item.MetaData1.Has("EVALUE"))
				amount = item.MetaData1.GetInt("EVALUE");
			// Others
			else
				amount = item.OptionInfo.SellingPrice;

			guild.Gold += amount;
			creature.Inventory.Remove(item);

			Send.GuildMessage(creature, guild, Localization.GetPlural("You have donated {0:n0} Gold.", "You have donated {0:n0} Gold.", amount), amount);
		}
	}
}
