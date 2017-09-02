// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Handler for hidden guild stone placement skill, casted when using
	/// Guild Stone Installation Permit item.
	/// </summary>
	[Skill(SkillId.HiddenGuildStoneSetting)]
	public class HiddenGuildStoneSetting : IPreparable, ICancelable, IUseable, ICompletable
	{
		/// <summary>
		/// Minimum distance to other stones.
		/// </summary>
		private const int MinStoneDistance = 1000;

		/// <summary>
		/// Maximum distance between character and stone.
		/// </summary>
		private const int MaxStoneDistance = 500;

		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <remarks>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var parameters = packet.GetString();
			var itemEntityId = MabiDictionary.Fetch<long>("ITEMID", parameters);
			var guild = creature.Guild;

			if (itemEntityId == 0 || creature.Inventory.GetItem(itemEntityId) == null)
			{
				Log.Warning("HiddenGuildStoneSetting.Prepare: User '{0}' tried to use skill with invalid item.", creature.Client.Account.Id);
				return false;
			}
			else if (guild == null)
			{
				Send.MsgBox(creature, Localization.Get("You're not in a guild."));
				return false;
			}
			else if (creature.GuildMember.Rank != GuildMemberRank.Leader)
			{
				Send.MsgBox(creature, Localization.Get("Only the guild leader can place the guild stone."));
				return false;
			}
			else if (guild.HasStone)
			{
				Send.MsgBox(creature, Localization.Get("Your guild already has a guild stone."));
				return false;
			}

			skill.Stacks = 1;
			skill.State = SkillState.Ready;

			Send.Echo(creature, Op.SkillReady, packet);

			return true;
		}

		/// <summary>
		/// Cancels skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Uses skill, attempting to place the stone.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var locationId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			var guild = creature.Guild;
			var region = creature.Region;
			var pos = new Position(locationId);
			var creaturePos = creature.GetPosition();

			// Check range
			if (!creaturePos.InRange(pos, MaxStoneDistance))
			{
				creature.Unlock(Locks.Walk | Locks.Run);

				Send.Notice(creature, Localization.Get("You're too far away."));
				Send.SkillUseSilentCancel(creature);
				return;
			}

			// Check distance to other stones
			var otherStones = region.GetProps(a => a.HasTag("/guildstone/") && a.GetPosition().InRange(pos, MinStoneDistance));
			if (otherStones.Count != 0)
			{
				creature.Unlock(Locks.Walk | Locks.Run);

				Send.Notice(creature, Localization.Get("You're too close to another Guild Stone to put yours up."));
				Send.SkillUseSilentCancel(creature);
				return;
			}

			// Check street
			if (creature.Region.IsOnStreet(pos))
			{
				Send.Notice(creature, Localization.Get("You can't place a Guild Stone on the street."));
				Send.SkillUseSilentCancel(creature);
				return;
			}

			// Place stone (from complete)
			creature.Skills.Callback(skill.Info.Id, () =>
			{
				guild.Stone.PropId = GuildStonePropId.Normal;
				guild.Stone.RegionId = region.Id;
				guild.Stone.X = pos.X;
				guild.Stone.Y = pos.Y;
				guild.Stone.Direction = MabiMath.ByteToRadian(creature.Direction);

				ChannelServer.Instance.GuildManager.SetStone(guild);

				Send.Notice(NoticeType.Top, 20000, Localization.Get("{0} Guild has been formed. Guild Leader : {1}"), guild.Name, guild.LeaderName);

				creature.Inventory.Remove(63041); // Guild Stone Installation Permit
			});

			// TODO: Skills that don't necessarily end in Use need a way to get
			//   back to Ready, we currently don't properly support that.
			//   Use will probably require a return value, like Prepare.
			//   Temporary solution: working with stacks.
			skill.Stacks = 0;

			Send.Echo(creature, Op.SkillUse, packet);
		}

		/// <summary>
		/// Completes skill, placing the stone.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var locationId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			// Ignore parameters, we got everything we need in Use.
			creature.Skills.Callback(skill.Info.Id);

			Send.Echo(creature, Op.SkillComplete, packet);
		}
	}
}
