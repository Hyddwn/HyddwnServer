// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
using Aura.Mabi.Const;
using Aura.Channel.World.Entities;
using System;
using System.Linq;
using Aura.Channel.Network.Sending;
using System.Collections.Generic;
using Aura.Shared.Util;

namespace Aura.Channel.World.Quests
{
	public class QuestRewardGroup
	{
		/// <summary>
		/// Group's id
		/// </summary>
		public int Id { get; protected set; }

		/// <summary>
		/// Group's type (affects the reward icon)
		/// </summary>
		public RewardGroupType Type { get; protected set; }

		/// <summary>
		/// List of rewards in this group.
		/// </summary>
		public List<QuestReward> Rewards { get; protected set; }

		/// <summary>
		/// Returns true if there are no rewards for non-perfect results.
		/// </summary>
		public bool PerfectOnly { get { return this.Rewards.All(a => a.Result == QuestResult.Perfect); } }

		/// <summary>
		/// Creates new QuestRewardGroup
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="type"></param>
		public QuestRewardGroup(int groupId, RewardGroupType type)
		{
			this.Rewards = new List<QuestReward>();

			this.Id = groupId;
			this.Type = type;
		}

		/// <summary>
		/// Adds reward to group.
		/// </summary>
		/// <param name="reward"></param>
		public void Add(QuestReward reward)
		{
			this.Rewards.Add(reward);
		}

		/// <summary>
		/// Returns true if group contains rewards for result.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public bool HasRewardsFor(QuestResult result)
		{
			return this.Rewards.Any(a => a.Result == result);
		}
	}

	/// <summary>
	/// Common quest reward base class
	/// </summary>
	public abstract class QuestReward
	{
		/// <summary>
		/// The reward type
		/// </summary>
		public abstract RewardType Type { get; }

		/// <summary>
		/// The required result to get this reward.
		/// </summary>
		public QuestResult Result { get; set; }

		/// <summary>
		/// Specifies whether the reward is visible on the client.
		/// </summary>
		/// <remarks>
		/// Hidden rewards are still sent to the client with the quest info,
		/// but if this switch is set, they aren't displayed, leaving a blank
		/// space.
		/// </remarks>
		public bool Visible { get; set; }

		/// <summary>
		/// Gives reward to creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="quest"></param>
		public abstract void Reward(Creature creature, Quest quest);

		/// <summary>
		/// Returns string for this reward, displayed on the client.
		/// </summary>
		/// <returns></returns>
		public override abstract string ToString();
	}

	/// <summary>
	/// Rewards item
	/// </summary>
	public class QuestRewardItem : QuestReward
	{
		public override RewardType Type { get { return RewardType.Item; } }

		public int ItemId { get; protected set; }
		public int Amount { get; protected set; }

		public QuestRewardItem(int itemId, int amount)
		{
			this.ItemId = itemId;
			this.Amount = amount;
		}

		public override string ToString()
		{
			var data = AuraData.ItemDb.Find(this.ItemId);
			if (data == null)
				return "Unknown item";
			return string.Format("{0} {1}", this.Amount, data.Name);
		}

		public override void Reward(Creature creature, Quest quest)
		{
			creature.AcquireItem(Item.Create(this.ItemId, this.Amount));
		}
	}

	/// <summary>
	/// Rewards Enchant Scroll.
	/// </summary>
	/// <remarks>
	/// Uses Type and ToString from Item reward, but generates an enchant
	/// scroll on Reward, based on the option set id, ignoring the Item
	/// information.
	/// </remarks>
	public class QuestRewardEnchant : QuestRewardItem
	{
		public int OptionSetId { get; protected set; }

		public QuestRewardEnchant(int optionSetId)
			: base(62005, 1) // Enchant Scroll
		{
			this.OptionSetId = optionSetId;
		}

		public override void Reward(Creature creature, Quest quest)
		{
			creature.AcquireItem(Item.CreateEnchant(OptionSetId));
		}
	}

	/// <summary>
	/// Rewards quest (scroll).
	/// </summary>
	public class QuestRewardQuestScroll : QuestReward
	{
		public override RewardType Type { get { return RewardType.Item; } }

		public int QuestId { get; protected set; }

		public QuestRewardQuestScroll(int questId)
		{
			this.QuestId = questId;
		}

		public override string ToString()
		{
			var data = ChannelServer.Instance.ScriptManager.QuestScripts.Get(this.QuestId);
			if (data == null)
				return "Unknown quest";
			return data.Name;
		}

		public override void Reward(Creature creature, Quest quest)
		{
			creature.Inventory.Add(Item.CreateQuestScroll(this.QuestId), true);
		}
	}

	/// <summary>
	/// Rewards skill x of rank y. Optionally trains a condition.
	/// </summary>
	public class QuestRewardSkill : QuestReward
	{
		public override RewardType Type { get { return RewardType.Skill; } }

		public SkillId SkillId { get; protected set; }
		public SkillRank Rank { get; protected set; }
		public int Training { get; protected set; }

		public QuestRewardSkill(SkillId id, SkillRank rank, int training = 0)
		{
			this.SkillId = id;
			this.Rank = rank;
			this.Training = training;
		}

		public override string ToString()
		{
			var data = AuraData.SkillDb.Find((ushort)this.SkillId);
			if (data == null)
				return "Unknown skill";
			return string.Format("[Skill] {0}", data.Name);
		}

		public override void Reward(Creature creature, Quest quest)
		{
			// Only give skill if char doesn't have it or rank is lower.
			if (creature.Skills.Has(this.SkillId, this.Rank))
				return;

			creature.Skills.Give(this.SkillId, this.Rank);

			if (this.Training != 0)
				creature.Skills.Train(this.SkillId, this.Rank, this.Training);
		}
	}

	/// <summary>
	/// Rewards gold
	/// </summary>
	public class QuestRewardGold : QuestReward
	{
		public override RewardType Type { get { return RewardType.Gold; } }

		public int Amount { get; protected set; }

		public QuestRewardGold(int amount)
		{
			this.Amount = amount;
		}

		public override string ToString()
		{
			return string.Format("{0}G", this.Amount);
		}

		public override void Reward(Creature creature, Quest quest)
		{
			creature.Inventory.AddGold(this.Amount);
			Send.AcquireInfo(creature, "gold", this.Amount);
		}
	}

	/// <summary>
	/// Rewards exp
	/// </summary>
	public class QuestRewardExp : QuestReward
	{
		public override RewardType Type { get { return RewardType.Exp; } }

		public int Amount { get; protected set; }

		public QuestRewardExp(int amount)
		{
			this.Amount = amount;
		}

		public override string ToString()
		{
			return string.Format("{0} Experience Point", this.Amount);
		}

		public override void Reward(Creature creature, Quest quest)
		{
			creature.GiveExp(this.Amount);
			Send.AcquireInfo(creature, "exp", this.Amount);
		}
	}

	/// <summary>
	/// Rewards exploration exp
	/// </summary>
	public class QuestRewardExplExp : QuestReward
	{
		public override RewardType Type { get { return RewardType.ExplExp; } }

		public int Amount { get; protected set; }

		public QuestRewardExplExp(int amount)
		{
			this.Amount = amount;
		}

		public override string ToString()
		{
			return string.Format("Exploration EXP {0}", this.Amount);
		}

		public override void Reward(Creature creature, Quest quest)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Rewards ability points
	/// </summary>
	/// <remarks>
	/// The quest AP rate option is only applied when the scripts are loaded,
	/// so the players see the actual AP they'll get if they complete the
	/// quest. Should the rate be changed at runtime, players will still see
	/// the previous AP amount, and will get that amount if they complete a
	/// quest.
	/// </remarks>
	public class QuestRewardAp : QuestReward
	{
		public override RewardType Type { get { return RewardType.AP; } }

		public short Amount { get; protected set; }

		public QuestRewardAp(short amount)
		{
			this.Amount = (short)Math2.Clamp(0, short.MaxValue, amount * ChannelServer.Instance.Conf.World.QuestApRate);
		}

		public override string ToString()
		{
			return string.Format("{0} Ability Point", this.Amount);
		}

		public override void Reward(Creature creature, Quest quest)
		{
			creature.GiveAp(this.Amount);
			Send.AcquireInfo(creature, "ap", this.Amount);
		}
	}

	[Flags]
	public enum RewardOptions
	{
		None = 0,

		/// <summary>
		/// Hides reward on the client side.
		/// </summary>
		/// <remarks>
		/// Hidden rewards are still sent to the client with the quest info,
		/// but if this switch is set, they aren't displayed, leaving a blank
		/// space.
		/// </remarks>
		Hidden = 1,
	}
}
