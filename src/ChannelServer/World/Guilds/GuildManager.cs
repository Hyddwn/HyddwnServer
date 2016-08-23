// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Database;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aura.Channel.World.Guilds
{
	public class GuildManager
	{
		private object _syncLock = new object();

		private Dictionary<long, Guild> _guilds;
		private Dictionary<long, Prop> _stones;

		/// <summary>
		/// Returns number of guilds on this server.
		/// </summary>
		public int Count { get { lock (_syncLock) return _guilds.Count; } }

		/// <summary>
		/// Creates new manager instance.
		/// </summary>
		public GuildManager()
		{
			_guilds = new Dictionary<long, Guild>();
			_stones = new Dictionary<long, Prop>();
		}

		/// <summary>
		/// Initializes manager, loading all guilds from database.
		/// </summary>
		public void Initialize()
		{
			this.LoadGuilds();
			ChannelServer.Instance.Events.MabiTick += this.OnMabiTick;
			ChannelServer.Instance.Events.CreatureConnected += this.OnCreatureConnected;
		}

		/// <summary>
		/// Called when a creature connected to the channel.
		/// </summary>
		/// <param name="obj"></param>
		private void OnCreatureConnected(Creature creature)
		{
			var guild = creature.Guild;

			if (guild == null || !guild.HasStone)
				return;

			if (AuraData.FeaturesDb.IsEnabled("MarkerMyGuildStone"))
				Send.GuildStoneLocation(creature, guild.Stone);
		}

		/// <summary>
		/// Called every 5 minutes to synchronize guild information.
		/// </summary>
		/// <param name="now"></param>
		private void OnMabiTick(ErinnTime now)
		{
			// Synchronizing the guilds can take a few milliseconds if there
			// are many in the db. Run it in a thread so we don't block
			// the world thread.
			// Should this be the default for all time events...?
			Task.Factory.StartNew(SynchronizeGuilds);
		}

		/// <summary>
		/// Synchronizes loaded guilds with current information
		/// from the database.
		/// </summary>
		private void SynchronizeGuilds()
		{
			lock (_syncLock)
			{
				var dbGuilds = ChannelServer.Instance.Database.GetGuilds();
				var removedGuilds = new List<Guild>();

				foreach (var guild in _guilds.Values)
				{
					// Check for removed or disbanded guilds
					Guild dbGuild;
					if (!dbGuilds.TryGetValue(guild.Id, out dbGuild) || dbGuild.Disbanded == true)
					{
						removedGuilds.Add(guild);
					}

					// Update guild
					guild.IntroMessage = dbGuild.IntroMessage;
					guild.WelcomeMessage = dbGuild.WelcomeMessage;
					guild.LeavingMessage = dbGuild.LeavingMessage;
					guild.RejectionMessage = dbGuild.RejectionMessage;
					guild.Type = dbGuild.Type;
					guild.Level = dbGuild.Level;
					guild.Options = dbGuild.Options;

					// Update members
					var members = guild.GetMembers();
					foreach (var member in members)
					{
						var creature = ChannelServer.Instance.World.GetCreature(member.CharacterId);
						var update = false;
						var remove = false;

						// Check for whether the member was removed, set to
						// declined, or the guild was disbanded.
						var dbMember = dbGuild.GetMember(member.CharacterId);
						if (dbMember == null || (member.Rank < GuildMemberRank.Applied && dbMember.Rank == GuildMemberRank.Declined) || dbGuild.Disbanded)
						{
							if (creature != null)
								Send.GuildMessage(creature, guild, guild.LeavingMessage, guild.Name);

							update = true;
							remove = true;
						}
						// Check for accepted members
						else if (member.Rank == GuildMemberRank.Applied && dbMember.Rank < GuildMemberRank.Applied)
						{
							if (creature != null)
								Send.GuildMessage(creature, guild, guild.WelcomeMessage, guild.Name);

							update = true;
						}
						// Check for declined members
						else if (member.Rank == GuildMemberRank.Applied && dbMember.Rank == GuildMemberRank.Declined)
						{
							if (creature != null)
								Send.GuildMessage(creature, guild, guild.RejectionMessage, guild.Name);

							update = true;
							remove = true;
						}

						// Check for rank update
						if (!update)
							update = (member.Rank != dbMember.Rank);

						// Update creature
						if (update)
						{
							if (remove)
							{
								guild.RemoveMember(member);
								ChannelServer.Instance.Database.RemoveGuildMember(member);

								if (creature != null)
								{
									creature.Guild = null;
									creature.GuildMember = null;
									Send.GuildUpdateMember(creature, null, null);
									if (AuraData.FeaturesDb.IsEnabled("MarkerMyGuildStone"))
										Send.GuildStoneLocation(creature, null);
								}
							}
							else
							{
								member.Rank = dbMember.Rank;

								if (creature != null)
									Send.GuildUpdateMember(creature, guild, member);
							}
						}
					}
				}

				// Remove guilds
				foreach (var guild in removedGuilds)
				{
					_guilds.Remove(guild.Id);
					this.DestroyStone(guild);
					ChannelServer.Instance.Database.RemoveGuild(guild);
				}
			}
		}

		/// <summary>
		/// Loads all guilds from database.
		/// </summary>
		private void LoadGuilds()
		{
			var guilds = ChannelServer.Instance.Database.GetGuilds();
			foreach (var guild in guilds.Values)
				this.LoadGuild(guild);
		}

		/// <summary>
		/// Loads given guild.
		/// </summary>
		/// <param name="guild"></param>
		private void LoadGuild(Guild guild)
		{
			lock (_syncLock)
				_guilds[guild.Id] = guild;
			this.PlaceStone(guild);
		}

		/// <summary>
		/// Places stone for guild in world.
		/// </summary>
		/// <param name="guild"></param>
		public void PlaceStone(Guild guild)
		{
			lock (_syncLock)
			{
				if (_stones.ContainsKey(guild.Id))
					return;
			}

			var stone = guild.Stone;
			if (stone.RegionId == 0)
				return;

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

			this.UpdateStoneLocation(guild);
		}

		/// <summary>
		/// Executes the given action for all members of guild that are online.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="action"></param>
		private static void ForOnlineMembers(Guild guild, Action<Creature> action)
		{
			var members = guild.GetMembers();
			foreach (var member in members)
			{
				var creature = ChannelServer.Instance.World.GetCreature(member.CharacterId);
				if (creature == null)
					continue;

				action(creature);
			}
		}

		/// <summary>
		/// Updates the guild stone's location for all members.
		/// </summary>
		/// <param name="guild"></param>
		private void UpdateStoneLocation(Guild guild)
		{
			if (!AuraData.FeaturesDb.IsEnabled("MarkerMyGuildStone"))
				return;

			var stone = guild.Stone;

			if (stone.RegionId != 0)
				ForOnlineMembers(guild, creature => Send.GuildStoneLocation(creature, stone));
			else
				ForOnlineMembers(guild, creature => Send.GuildStoneLocation(creature, null));
		}

		/// <summary>
		/// Prop behavior for guild stones.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		private static void OnStoneTouch(Creature creature, Prop prop)
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
					Send.GuildInfoApplied(creature, guild);
				}
			}
			else if (creature.GuildId != 0)
			{
				Send.GuildInfoApplied(creature, guild);
			}
			else
			{
				Send.GuildInfoNoGuild(creature, guild);
			}
		}

		/// <summary>
		/// Returns the guild with the given id.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public Guild GetGuild(long guildId)
		{
			Guild result;
			lock (_syncLock)
				_guilds.TryGetValue(guildId, out result);
			return result;
		}

		/// <summary>
		/// Sets the character's Guild and GuildMember properties
		/// if they're in a guild.
		/// </summary>
		/// <param name="character"></param>
		public void SetGuildForCharacter(Creature character)
		{
			var guild = this.FindGuildWithMember(character.EntityId);
			if (guild == null)
				return;

			character.Guild = guild;
			character.GuildMember = guild.GetMember(character.EntityId);
		}

		/// <summary>
		/// Returns the guild that has a character with the given id as
		/// member if any.
		/// </summary>
		/// <param name="characterId"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Gives all of creature's play points to guild as guild points.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		public void ConvertPlayPoints(Creature creature, Guild guild)
		{
			var points = creature.PlayPoints;

			creature.PlayPoints = 0;
			guild.Points += points;

			ChannelServer.Instance.Database.UpdateGuildResources(guild);

			Send.GuildMessage(creature, guild, Localization.GetPlural("Added {0:n0} Point.", "Added {0:n0} Points.", points), points);
		}

		/// <summary>
		/// Donates the given amount of gold from the creature to guild.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>d
		/// <param name="amount"></param>d
		public void DonateGold(Creature creature, Guild guild, int amount)
		{
			this.AddGold(guild, amount);
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

			this.AddGold(guild, amount);
			creature.Inventory.Remove(item);

			Send.GuildMessage(creature, guild, Localization.GetPlural("You have donated {0:n0} Gold.", "You have donated {0:n0} Gold.", amount), amount);
		}

		/// <summary>
		/// Adds gold and saves guild's resources to db.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="amount"></param>
		private void AddGold(Guild guild, int amount)
		{
			guild.Gold += amount;
			ChannelServer.Instance.Database.UpdateGuildResources(guild);
		}

		/// <summary>
		/// Destroy's guild's stone.
		/// </summary>
		/// <param name="guild"></param>
		public void DestroyStone(Guild guild)
		{
			Prop stone;
			lock (_syncLock)
			{
				if (!_stones.TryGetValue(guild.Id, out stone))
					return;

				_stones.Remove(guild.Id);
			}

			guild.Stone.RegionId = 0;
			guild.Stone.X = 0;
			guild.Stone.Y = 0;
			guild.Stone.Direction = 0;

			stone.Region.RemoveProp(stone);

			this.UpdateStoneLocation(guild);
		}

		/// <summary>
		/// Returns stone prop of the given guild.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		private Prop GetStone(long guildId)
		{
			Prop stone;
			lock (_syncLock)
				_stones.TryGetValue(guildId, out stone);
			return stone;
		}

		/// <summary>
		/// Places stone and saves its location to database.
		/// </summary>
		/// <param name="guild"></param>
		public void SetStone(Guild guild)
		{
			this.PlaceStone(guild);
			ChannelServer.Instance.Database.UpdateGuildStone(guild);
		}

		/// <summary>
		/// Adds creature to the guild as applicant.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		public void Apply(Creature creature, Guild guild, string application)
		{
			if (guild.HasMember(creature.EntityId))
				throw new ArgumentException("Character is already a member of this guild. (" + creature.Name + ", " + guild.Name + ")");

			var member = new GuildMember();
			member.GuildId = guild.Id;
			member.CharacterId = creature.EntityId;
			member.Rank = GuildMemberRank.Applied;
			member.JoinedDate = DateTime.Now;
			member.Application = application;

			creature.Guild = guild;
			creature.GuildMember = member;

			guild.AddMember(member);

			ChannelServer.Instance.Database.AddGuildMember(member);
		}

		/// <summary>
		/// Changes the look of the guild's stone.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="stoneType"></param>
		public void ChangeStone(Guild guild, GuildStoneType stoneType)
		{
			var stone = this.GetStone(guild.Id);

			var regionId = guild.Stone.RegionId;
			var x = guild.Stone.X;
			var y = guild.Stone.Y;
			var direction = guild.Stone.Direction;

			this.DestroyStone(guild);

			switch (stoneType)
			{
				default:
				case GuildStoneType.Normal: guild.Stone.PropId = GuildStonePropId.Normal; break;
				case GuildStoneType.Hope: guild.Stone.PropId = GuildStonePropId.Hope; break;
				case GuildStoneType.Courage: guild.Stone.PropId = GuildStonePropId.Courage; break;
			}

			guild.Stone.RegionId = regionId;
			guild.Stone.X = x;
			guild.Stone.Y = y;
			guild.Stone.Direction = direction;

			this.PlaceStone(guild);
		}

		/// <summary>
		/// Creates new guild with the members from the party.
		/// </summary>
		/// <param name="party"></param>
		/// <exception cref="ArgumentException">
		/// Thrown if one of the party members already is in a guild.
		/// </exception>
		public void CreateGuild(Party party, string name, GuildType type, GuildVisibility visibility)
		{
			var partyMembers = party.GetMembers();
			if (partyMembers.Any(a => a.GuildId != 0))
				throw new ArgumentException("One of the party members is in a guild already.");

			var leader = party.Leader;

			lock (_syncLock)
			{
				// Add guild
				var guild = new Guild();
				guild.Name = name;
				guild.LeaderName = leader.Name;
				guild.Title = "";
				guild.EstablishedDate = DateTime.Now;
				guild.Server = ChannelServer.Instance.Conf.Channel.ChannelServer;
				guild.Type = type;
				guild.Visibility = visibility;
				guild.IntroMessage = string.Format(Localization.Get("Guild stone for the {0} guild."), guild.Name);
				guild.WelcomeMessage = string.Format(Localization.Get("Welcome to the {0} guild!"), guild.Name);
				guild.LeavingMessage = string.Format(Localization.Get("You have left the {0} guild."), guild.Name);
				guild.RejectionMessage = string.Format(Localization.Get("You have been denied admission to the {0} guild."), guild.Name);

				ChannelServer.Instance.Database.AddGuild(guild);
				_guilds.Add(guild.Id, guild);

				// Add members
				foreach (var creature in partyMembers)
				{
					var guildMember = new GuildMember();
					guildMember.GuildId = guild.Id;
					guildMember.CharacterId = creature.EntityId;
					guildMember.JoinedDate = DateTime.Now;
					guildMember.Application = "";
					if (creature != leader)
						guildMember.Rank = GuildMemberRank.Member;

					ChannelServer.Instance.Database.AddGuildMember(guildMember);
					guild.AddMember(guildMember);

					creature.Guild = guild;
					creature.GuildMember = guildMember;
					Send.GuildUpdateMember(creature, guild, guildMember);
				}
			}
		}
	}
}
