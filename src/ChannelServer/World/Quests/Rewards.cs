// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.GameEvents;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Quests
{
    public class QuestRewardGroup
    {
        /// <summary>
        ///     Creates new QuestRewardGroup
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="type"></param>
        public QuestRewardGroup(int groupId, RewardGroupType type)
        {
            Rewards = new List<QuestReward>();

            Id = groupId;
            Type = type;
        }

        /// <summary>
        ///     Group's id
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        ///     Group's type (affects the reward icon)
        /// </summary>
        public RewardGroupType Type { get; protected set; }

        /// <summary>
        ///     List of rewards in this group.
        /// </summary>
        public List<QuestReward> Rewards { get; protected set; }

        /// <summary>
        ///     Returns true if there are no rewards for non-perfect results.
        /// </summary>
        public bool PerfectOnly
        {
            get { return Rewards.All(a => a.Result == QuestResult.Perfect); }
        }

        /// <summary>
        ///     Adds reward to group.
        /// </summary>
        /// <param name="reward"></param>
        public void Add(QuestReward reward)
        {
            Rewards.Add(reward);
        }

        /// <summary>
        ///     Returns true if group contains rewards for result.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool HasRewardsFor(QuestResult result)
        {
            return Rewards.Any(a => a.Result == result);
        }
    }

    /// <summary>
    ///     Common quest reward base class
    /// </summary>
    public abstract class QuestReward
    {
        /// <summary>
        ///     The reward type
        /// </summary>
        public abstract RewardType Type { get; }

        /// <summary>
        ///     The required result to get this reward.
        /// </summary>
        public QuestResult Result { get; set; }

        /// <summary>
        ///     Specifies whether the reward is visible on the client.
        /// </summary>
        /// <remarks>
        ///     Hidden rewards are still sent to the client with the quest info,
        ///     but if this switch is set, they aren't displayed, leaving a blank
        ///     space.
        /// </remarks>
        public bool Visible { get; set; }

        /// <summary>
        ///     Gives reward to creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="quest"></param>
        public abstract void Reward(Creature creature, Quest quest);

        /// <summary>
        ///     Returns string for this reward, displayed on the client.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    ///     Rewards item
    /// </summary>
    public class QuestRewardItem : QuestReward
    {
        public QuestRewardItem(int itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public override RewardType Type => RewardType.Item;

        public int ItemId { get; protected set; }
        public int Amount { get; protected set; }

        public override string ToString()
        {
            var data = AuraData.ItemDb.Find(ItemId);
            if (data == null)
                return "Unknown item";
            return string.Format("{0} {1}", Amount, data.Name);
        }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.AcquireItem(Item.Create(ItemId, Amount));
        }
    }

    /// <summary>
    ///     Rewards keyword.
    /// </summary>
    public class QuestRewardKeyword : QuestReward
    {
        public QuestRewardKeyword(string keyword)
        {
            if (!AuraData.KeywordDb.Exists(keyword))
                throw new ArgumentException("Keyword '" + keyword + "' not found in data.");

            Keyword = keyword;
        }

        public override RewardType Type => RewardType.Keyword;

        public string Keyword { get; protected set; }

        public override string ToString()
        {
            return Localization.Get("New Keyword");
        }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.Keywords.Give(Keyword);
        }
    }

    /// <summary>
    ///     Rewards Enchant Scroll.
    /// </summary>
    /// <remarks>
    ///     Uses Type and ToString from Item reward, but generates an enchant
    ///     scroll on Reward, based on the option set id, ignoring the Item
    ///     information.
    /// </remarks>
    public class QuestRewardEnchant : QuestRewardItem
    {
        public QuestRewardEnchant(int optionSetId)
            : base(62005, 1) // Enchant Scroll
        {
            OptionSetId = optionSetId;
        }

        public int OptionSetId { get; protected set; }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.AcquireItem(Item.CreateEnchant(OptionSetId));
        }
    }

    /// <summary>
    ///     Rewards Blacksmithing/Tailoring Pattern.
    /// </summary>
    /// <remarks>
    ///     Uses Type from Item reward, but generates a pattern on Reward.
    /// </remarks>
    public class QuestRewardPattern : QuestRewardItem
    {
        public QuestRewardPattern(int itemId, int formId, int useCount)
            : base(itemId, 1)
        {
            FormId = formId;
            UseCount = useCount;
        }

        public int FormId { get; protected set; }
        public int UseCount { get; protected set; }

        public override string ToString()
        {
            var pattern = AuraData.ManualDb.Find(ManualCategory.Tailoring, FormId);
            if (pattern == null)
            {
                pattern = AuraData.ManualDb.Find(ManualCategory.Blacksmithing, FormId);
                if (pattern == null)
                    return "Unknown pattern";
            }
            // else pattern data found

            return string.Format("{0} - {1}",
                pattern.Category == ManualCategory.Blacksmithing ? "Blacksmith Manual" : "Sewing Pattern",
                pattern.ItemData.Name
            );
        }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.AcquireItem(Item.CreatePattern(ItemId, FormId, UseCount));
        }
    }

    /// <summary>
    ///     Rewards Warp Scroll.
    /// </summary>
    /// <remarks>
    ///     Uses Type and ToString from Item reward, but generates a warp scroll
    ///     on Reward.
    /// </remarks>
    public class QuestRewardWarpScroll : QuestRewardItem
    {
        public QuestRewardWarpScroll(int itemId, string portal)
            : base(itemId, 1)
        {
            Portal = portal;
        }

        public string Portal { get; protected set; }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.AcquireItem(Item.CreateWarpScroll(ItemId, Portal));
        }
    }

    /// <summary>
    ///     Rewards quest (scroll).
    /// </summary>
    public class QuestRewardQuestScroll : QuestReward
    {
        public QuestRewardQuestScroll(int questId)
        {
            QuestId = questId;
        }

        public override RewardType Type => RewardType.Item;

        public int QuestId { get; protected set; }

        public override string ToString()
        {
            var data = ChannelServer.Instance.ScriptManager.QuestScripts.Get(QuestId);
            if (data == null)
                return "Unknown quest";
            return data.Name;
        }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.Inventory.Add(Item.CreateQuestScroll(QuestId), true);
        }
    }

    /// <summary>
    ///     Rewards skill x of rank y. Optionally trains a condition.
    /// </summary>
    public class QuestRewardSkill : QuestReward
    {
        public QuestRewardSkill(SkillId id, SkillRank rank, int training = 0)
        {
            SkillId = id;
            Rank = rank;
            Training = training;
        }

        public override RewardType Type => RewardType.Skill;

        public SkillId SkillId { get; protected set; }
        public SkillRank Rank { get; protected set; }
        public int Training { get; protected set; }

        public override string ToString()
        {
            var data = AuraData.SkillDb.Find((ushort) SkillId);
            if (data == null)
                return "Unknown skill";
            return string.Format("[Skill] {0}", data.Name);
        }

        public override void Reward(Creature creature, Quest quest)
        {
            // Only give skill if char doesn't have it or rank is lower.
            if (creature.Skills.Has(SkillId, Rank))
                return;

            creature.Skills.Give(SkillId, Rank);

            if (Training != 0)
                creature.Skills.Train(SkillId, Rank, Training);
        }
    }

    /// <summary>
    ///     Rewards gold
    /// </summary>
    public class QuestRewardGold : QuestReward
    {
        public QuestRewardGold(int amount)
        {
            Amount = amount;
        }

        public override RewardType Type => RewardType.Gold;

        public int Amount { get; protected set; }

        public override string ToString()
        {
            return string.Format("{0}G", Amount);
        }

        public override void Reward(Creature creature, Quest quest)
        {
            var amount = Amount;

            // Friday: Increase in rewards for completing part-time jobs.
            // (20% increase in EXP and Gold rewards)
            if (quest.Data.Type == QuestType.Deliver && ErinnTime.Now.Month == ErinnMonth.AlbanElved)
                amount = (int) (amount * 1.2f);

            creature.Inventory.AddGold(amount);
            Send.AcquireInfo(creature, "gold", amount);
        }
    }

    /// <summary>
    ///     Rewards exp
    /// </summary>
    public class QuestRewardExp : QuestReward
    {
        public QuestRewardExp(int amount)
        {
            Amount = amount;
        }

        public override RewardType Type => RewardType.Exp;

        public int Amount { get; protected set; }

        public override string ToString()
        {
            return string.Format("{0} Experience Point", Amount);
        }

        public override void Reward(Creature creature, Quest quest)
        {
            var amount = Amount;

            // Add global bonus
            float bonusMultiplier;
            string bonuses;
            if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(GlobalBonusStat.QuestExp,
                out bonusMultiplier, out bonuses))
                amount = (int) (amount * bonusMultiplier);

            // Friday: Increase in rewards for completing part-time jobs.
            // (20% increase in EXP and Gold rewards)
            if (quest.Data.Type == QuestType.Deliver && ErinnTime.Now.Month == ErinnMonth.AlbanElved)
                amount = (int) (amount * 1.2f);

            creature.GiveExp(amount);
            Send.AcquireInfo(creature, "exp", amount);
        }
    }

    /// <summary>
    ///     Rewards exploration exp
    /// </summary>
    public class QuestRewardExplExp : QuestReward
    {
        public QuestRewardExplExp(int amount)
        {
            Amount = amount;
        }

        public override RewardType Type => RewardType.ExplExp;

        public int Amount { get; protected set; }

        public override string ToString()
        {
            return string.Format("Exploration EXP {0}", Amount);
        }

        public override void Reward(Creature creature, Quest quest)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Rewards ability points
    /// </summary>
    /// <remarks>
    ///     The quest AP rate option is only applied when the scripts are loaded,
    ///     so the players see the actual AP they'll get if they complete the
    ///     quest. Should the rate be changed at runtime, players will still see
    ///     the previous AP amount, and will get that amount if they complete a
    ///     quest.
    /// </remarks>
    public class QuestRewardAp : QuestReward
    {
        public QuestRewardAp(short amount)
        {
            Amount = amount;
        }

        public override RewardType Type => RewardType.AP;

        public short Amount { get; protected set; }

        public override string ToString()
        {
            return string.Format("{0} Ability Point", Amount);
        }

        public override void Reward(Creature creature, Quest quest)
        {
            creature.GiveAp(Amount);
            Send.AcquireInfo(creature, "ap", Amount);
        }
    }

    /// <summary>
    ///     Rewards stat points.
    /// </summary>
    public class QuestRewardStatBonus : QuestReward
    {
        public QuestRewardStatBonus(Stat stat, int amount)
        {
            if (stat != Stat.Str && stat != Stat.Dex && stat != Stat.Int && stat != Stat.Will && stat != Stat.Luck)
                throw new ArgumentException("Unsupported stat '" + stat + "'");

            if (amount <= 0)
                throw new ArgumentException("Amount must be a positive value.");

            Stat = stat;
            Amount = amount;
        }

        public override RewardType Type => RewardType.Skill;

        public Stat Stat { get; protected set; }
        public int Amount { get; protected set; }

        public override string ToString()
        {
            switch (Stat)
            {
                case Stat.Str: return string.Format(Localization.Get("Strength +{0}"), Amount);
                case Stat.Dex: return string.Format(Localization.Get("Dexterity +{0}"), Amount);
                case Stat.Int: return string.Format(Localization.Get("Intelligence +{0}"), Amount);
                case Stat.Will: return string.Format(Localization.Get("Will +{0}"), Amount);
                case Stat.Luck: return string.Format(Localization.Get("Luck +{0}"), Amount);
                default: return string.Format(Localization.Get("Unknown +{0}"), Amount);
            }
        }

        public override void Reward(Creature creature, Quest quest)
        {
            switch (Stat)
            {
                case Stat.Str:
                    creature.StrBonus += Amount;
                    break;
                case Stat.Dex:
                    creature.DexBonus += Amount;
                    break;
                case Stat.Int:
                    creature.IntBonus += Amount;
                    break;
                case Stat.Will:
                    creature.WillBonus += Amount;
                    break;
                case Stat.Luck:
                    creature.LuckBonus += Amount;
                    break;
            }

            Send.StatUpdate(creature, StatUpdateType.Private, Stat.Str, Stat.Dex, Stat.Int, Stat.Will, Stat.Luck);
        }
    }

    [Flags]
    public enum RewardOptions
    {
        None = 0,

        /// <summary>
        ///     Hides reward on the client side.
        /// </summary>
        /// <remarks>
        ///     Hidden rewards are still sent to the client with the quest info,
        ///     but if this switch is set, they aren't displayed, leaving a blank
        ///     space.
        /// </remarks>
        Hidden = 1
    }
}