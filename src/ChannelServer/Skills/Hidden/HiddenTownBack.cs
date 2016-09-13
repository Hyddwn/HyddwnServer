// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Text.RegularExpressions;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Handler for the hidden skill used by some "Warp Scrolls", like wings.
	/// </summary>
	[Skill(SkillId.HiddenTownBack)]
	public class HiddenTownBack : IPreparable, ICompletable
	{
		/// <summary>
		/// Prepares skill, reading the item to use from the packet.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var dictStr = packet.GetString();
			bool warpPartyMembers = false;
			if (packet.Peek() == PacketElementType.Byte)
				warpPartyMembers = packet.GetBool();

			// Get item entity id
			var itemEntityId = MabiDictionary.Fetch<long>("ITEMID", dictStr);
			if (itemEntityId == 0)
			{
				Log.Warning("HiddenTownBack: Item entity id missing.");
				return false;
			}

			// Get item
			var item = creature.Inventory.GetItem(itemEntityId);
			if (item == null)
			{
				Log.Warning("HiddenTownBack: Creature '{0:X16}' tried to use non-existing item.", creature.EntityId);
				return false;
			}

			// Set callback for Complete
			creature.Skills.Callback(skill.Info.Id, () =>
			{
				// Try to warp and remove item if successful
				if (Warp(creature, item, warpPartyMembers))
					creature.Inventory.Decrement(item);
			});

			Send.SkillUse(creature, skill.Info.Id, itemEntityId, warpPartyMembers, "");
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Complates skill, warping creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			creature.Skills.Callback(skill.Info.Id);

			Send.Echo(creature, packet);
		}

		/// <summary>
		/// Warps creature, based on the item's properties.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="warpPartyMembers"></param>
		/// <returns>Whether a warp happened or not.</returns>
		public static bool Warp(Creature creature, Item item, bool warpPartyMembers)
		{
			if (creature == null)
				throw new ArgumentNullException("creature");

			if (item == null)
				throw new ArgumentNullException("item");

			// Check meta data
			if (!item.MetaData1.Has("TARGET"))
			{
				Send.ServerMessage(creature, Localization.Get("No target found."));
				return false;
			}

			// Get target
			var target = item.MetaData1.GetString("TARGET");

			// Get location based on target
			Location loc;
			if (target.StartsWith("pos")) // pos@regionId,x,y
			{
				var match = Regex.Match(target, @"pos@(?<regionId>[0-9]+),(?<x>[0-9]+),(?<y>[0-9]+)");
				if (!match.Success)
				{
					Log.Warning("HiddenTownBack: Invalid position target: {0}", target);
					Send.ServerMessage(creature, Localization.Get("Invalid target."));
					return false;
				}

				loc.RegionId = Convert.ToInt32(match.Groups["regionId"].Value);
				loc.X = Convert.ToInt32(match.Groups["x"].Value);
				loc.Y = Convert.ToInt32(match.Groups["y"].Value);
			}
			else if (target.StartsWith("portal")) // portal@name
			{
				// Remove "portal@" prefix
				target = target.Substring(7).Trim();

				// Get portal data
				var portalData = AuraData.PortalDb.Find(target);
				if (portalData == null)
				{
					Log.Warning("HiddenTownBack: Unknown target: {0}", target);
					Send.ServerMessage(creature, Localization.Get("Unknown target."));
					return false;
				}

				// Get location
				try
				{
					loc = new Location(portalData.Location);
				}
				catch
				{
					Log.Warning("HiddenTownBack: Invalid portal location: {0}", target);
					Send.ServerMessage(creature, Localization.Get("Invalid portal location."));
					return false;
				}
			}
			else if (target == "last_town")
			{
				loc = new Location(creature.LastTown);
			}
			else
			{
				Log.Warning("HiddenTownBack: Unknown target type: {0}", target);
				Send.ServerMessage(creature, Localization.Get("Unknown target type."));
				return false;
			}

			// Warp
			if (warpPartyMembers && item.HasTag("/party_enable/") && creature.Party.Leader == creature)
			{
				var partyMembers = creature.Party.GetMembersInRange(creature);
				foreach (var member in partyMembers)
					member.Warp(loc);
			}
			else
			{
				creature.Warp(loc);
			}

			return true;
		}
	}
}
