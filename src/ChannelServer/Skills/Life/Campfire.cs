// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Campfire skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Regeneration Bonus
	/// Var2: ?
	/// Var3: Players Accommodated
	/// Var4: ?
	/// 
	/// Duration is apparently not in the db.
	/// 
	/// Without Firewood you get the msg "not an appropriate place".
	/// </remarks>
	[Skill(SkillId.Campfire, SkillId.CampfireKit)]
	public class Campfire : ISkillHandler, IPreparable, IReadyable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Prop to spawn
		/// </summary>
		private const int PropId = 203;
		private const int HalloweenPropId = 44455;
		private const int ChristmasPropId = 44867;
		private const int SeventhAnnvPropId = 44809;
		private const int EighthAnnvPropId = 44960;

		/// <summary>
		/// How much Firewood is required/being removed.
		/// </summary>
		private const int FirewoodCost = 5;

		/// <summary>
		/// Prepares skill (effectively does nothing)
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (skill.Info.Id == SkillId.Campfire)
			{
				var itemId = packet.GetInt();

				Send.SkillPrepare(creature, skill.Info.Id, itemId);
			}
			else
			{
				var dict = packet.GetString();

				Send.SkillPrepare(creature, skill.Info.Id, dict);
			}

			return true;
		}

		/// <summary>
		/// Readies skill, saving the item id to use for later.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			if (skill.Info.Id == SkillId.Campfire)
			{
				creature.Temp.FirewoodItemId = packet.GetInt();

				Send.SkillReady(creature, skill.Info.Id, creature.Temp.FirewoodItemId);
			}
			else
			{
				var dict = packet.GetString();

				creature.Temp.CampfireKitItemEntityId = MabiDictionary.Fetch<long>("ITEMID", dict);

				Send.SkillReady(creature, skill.Info.Id, dict);
			}

			return true;
		}

		/// <summary>
		/// Uses skill, checking if the campfire can be built at the given
		/// position.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var positionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Check position
			if (!IsValidPosition(creature, new Position(positionId)))
			{
				Send.Notice(creature, Localization.Get("It's a little cramped here to make a Campfire."));

				creature.Skills.CancelActiveSkill();
				Send.SkillUseSilentCancel(creature);
				return;
			}

			Send.SkillUse(creature, skill.Info.Id, positionId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Completes skill, placing the campfire.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var positionId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();
			var propId = PropId;

			// Check position
			var pos = new Position(positionId);
			var validPosition = IsValidPosition(creature, pos);

			if (validPosition)
			{
				// Handle items
				if (skill.Info.Id == SkillId.Campfire)
				{
					// Check Firewood, the client should stop the player long before Complete.
					if (creature.Inventory.Count(creature.Temp.FirewoodItemId) < FirewoodCost)
						throw new ModerateViolation("Used Campfire without Firewood.");

					// Remove Firewood
					creature.Inventory.Remove(creature.Temp.FirewoodItemId, FirewoodCost);
				}
				else
				{
					// Check kit
					var item = creature.Inventory.GetItem(creature.Temp.CampfireKitItemEntityId);
					if (item == null)
						throw new ModerateViolation("Used CampfireKit with invalid kit.");

					propId = this.GetPropId(item); // Change the prop ID based on what campfire kit was used

					// Reduce kit
					creature.Inventory.Decrement(item);
				}

				// Set up Campfire
				var effect = (skill.Info.Rank < SkillRank.RB ? "campfire_01" : "campfire_02");
				var prop = new Prop(propId, creature.RegionId, pos.X, pos.Y, MabiMath.ByteToRadian(creature.Direction), 1); // Logs
				prop.State = "single";
				if (prop.Data.Id != HalloweenPropId)
					prop.Xml.SetAttributeValue("EFFECT", effect); // Fire effect
				prop.DisappearTime = DateTime.Now.AddMinutes(this.GetDuration(skill.Info.Rank, creature.RegionId)); // Disappear after x minutes

				// Temp data for Rest
				prop.Temp.CampfireSkillRank = skill.RankData;
				if (skill.Info.Id == SkillId.Campfire)
					prop.Temp.CampfireFirewood = AuraData.ItemDb.Find(creature.Temp.FirewoodItemId);

				creature.Region.AddProp(prop);

				// Training
				if (skill.Info.Id == SkillId.Campfire && skill.Info.Rank == SkillRank.Novice)
					skill.Train(1); // Use Campfire.
			}

			// Complete
			Send.SkillComplete(creature, skill.Info.Id, positionId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Returns true if a campfire can be built at the given position.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public static bool IsValidPosition(Creature creature, Position pos)
		{
			var validPosition = true;

			// Collisions betwen player and position
			if (creature.Region.Collisions.Any(creature.GetPosition(), pos))
				validPosition = false;
			// Too close to a creature
			else if (creature.Region.GetCreaturesInRange(pos, 90).Count != 0)
				validPosition = false;
			// Too close to a collision
			else if (creature.Region.Collisions.AnyInRange(creature.GetPosition(), 250))
				validPosition = false;

			return validPosition;
		}

		/// <summary>
		/// Canceles skill (no special actions required)
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Returns duration for rank in minutes.
		/// </summary>
		/// <param name="rank"></param>
		/// <returns></returns>
		private int GetDuration(SkillRank rank, int regionId)
		{
			var duration = 4;
			if (rank >= SkillRank.RC && rank <= SkillRank.R6)
				duration = 5;
			else if (rank >= SkillRank.R5)
				duration = 6;

			// Lower duration during rain
			var weatherType = ChannelServer.Instance.Weather.GetWeatherType(regionId);
			if (weatherType == WeatherType.Rain)
				duration /= 2; // Unofficial

			return duration;
		}

		/// <summary>
		/// Gets the prop ID
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private int GetPropId(Item item)
		{
			if (item != null)
			{
				if (item.HasTag("/halloween_campfire_kit/"))
				{
					return HalloweenPropId;
				}
				else if (item.HasTag("/burner/"))
				{
					return ChristmasPropId;
				}
				else if (item.Info.Id == 63291)
				{
					return SeventhAnnvPropId;
				}
				else if (item.Info.Id == 63343)
				{
					return EighthAnnvPropId;
				}
			}

			return PropId;
		}

		/// <summary>
		/// Returns a campfire in range of creature, or null if there is none.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static Prop GetNearbyCampfire(Creature creature, int range)
		{
			var campfires = creature.Region.GetProps(a => a.Info.Id == 203 && a.GetPosition().InRange(creature.GetPosition(), 500));
			if (campfires.Count == 0)
				return null;

			return campfires[0];
		}
	}
}