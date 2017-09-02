// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills;
using Aura.Channel.World;
using Aura.Channel.World.Dungeons;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Quests;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
    /// <summary>
    ///     Provides information about a quests.
    /// </summary>
    public class QuestScript : GeneralScript
    {
        /// <summary>
        ///     Creates a new quest script instance.
        /// </summary>
        public QuestScript()
        {
            Prerequisites = new List<QuestPrerequisite>();
            Objectives = new OrderedDictionary<string, QuestObjective>();
            RewardGroups = new Dictionary<int, QuestRewardGroup>();

            MetaData = new MabiDictionary();

            Type = QuestType.Normal;
            Category = QuestCategory.Basic;
            Class = QuestClass.None;
            Icon = QuestIcon.Default;

            ScrollId = 70024; // Hunting Quest
        }

        /// <summary>
        ///     Quest id.
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        ///     Name display in quest log, etc.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Description displayed in quest information.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        ///     Additional information displayed in quest information.
        /// </summary>
        public string AdditionalInfo { get; protected set; }

        /// <summary>
        ///     The type of quest.
        /// </summary>
        /// <remarks>
        ///     It's important that this type is set correctly, as it affects
        ///     packet structures.
        /// </remarks>
        public QuestType Type { get; protected set; }

        /// <summary>
        ///     The PTJ type, used for the Deliver quest type.
        /// </summary>
        public PtjType PtjType { get; protected set; }

        /// <summary>
        ///     The quest level, mainly used by PTJ quests, for different reward
        ///     tiers.
        /// </summary>
        public QuestLevel Level { get; protected set; }

        /// <summary>
        ///     The category/tab the quest belongs to.
        /// </summary>
        public QuestCategory Category { get; protected set; }

        /// <summary>
        ///     The quest's category, which is displayed in square brackets
        ///     in front of the quest name.
        /// </summary>
        public QuestClass Class { get; protected set; }

        /// <summary>
        ///     The icon used for this quest.
        /// </summary>
        public QuestIcon Icon { get; protected set; }

        /// <summary>
        ///     The Erinn hour at which the quest can be started.
        /// </summary>
        public int StartHour { get; protected set; }

        /// <summary>
        ///     The Erinn hour at which the quest results can be reported.
        /// </summary>
        public int ReportHour { get; protected set; }

        /// <summary>
        ///     The Erinn hour at which the quest has to reported, otherwise the
        ///     NPC won't give you a positive result.
        /// </summary>
        public int DeadlineHour { get; protected set; }

        /// <summary>
        ///     The method by which the quest is received.
        /// </summary>
        /// <remarks>
        ///     While Manually requires the quest to be given to a player by an
        ///     NPC or via item, Automatically will give it automatically, as soon
        ///     as all prerequisites are met.
        /// </remarks>
        public Receive ReceiveMethod { get; protected set; }

        /// <summary>
        ///     Specifies whether the quest can be canceled.
        /// </summary>
        /// <remarks>
        ///     The client will always show the [Give up] button if you're using
        ///     the devCAT title.
        /// </remarks>
        public bool Cancelable { get; protected set; }

        /// <summary>
        ///     Prerequisites that have to be met for the quest to be given
        ///     automatically.
        /// </summary>
        public List<QuestPrerequisite> Prerequisites { get; protected set; }

        /// <summary>
        ///     Objectives that have to carried out, to complete the quest.
        /// </summary>
        public OrderedDictionary<string, QuestObjective> Objectives { get; protected set; }

        /// <summary>
        ///     Groups of rewards that the player can get for completing the quest.
        /// </summary>
        /// <remarks>
        ///     Usually quests have only one reward group (group 0),
        ///     multiple groups are mainly used by PTJs.
        /// </remarks>
        public Dictionary<int, QuestRewardGroup> RewardGroups { get; protected set; }

        /// <summary>
        ///     Used in quest items, although seemingly not required.
        /// </summary>
        public MabiDictionary MetaData { get; protected set; }

        /// <summary>
        ///     The quest scroll item to use.
        /// </summary>
        public int ScrollId { get; protected set; }

        /// <summary>
        ///     Who this quest is available to.
        /// </summary>
        protected QuestAvailability Availability { get; set; }

        /// <summary>
        ///     Returns true if this quest is a party quest.
        /// </summary>
        /// <remarks>
        ///     Party quests don't have a unique type, but they do have a
        ///     unique id range, which we can use to identify them.
        /// </remarks>
        public bool IsPartyQuest => Math2.Between(Id, 100001, 109999);

        /// <summary>
        ///     Returns true if this quest is a guild quest.
        /// </summary>
        /// <remarks>
        ///     Guild quests don't have a unique type, but they do have a
        ///     unique id range, which we can use to identify them.
        /// </remarks>
        public bool IsGuildQuest => Math2.Between(Id, 110000, 119999);

        /// <summary>
        ///     Delay with which the quest arrives if it's sent via owl.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        ///     Initializes the script, loading the information and adding it to
        ///     the script manager.
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            Load();

            if (Id == 0 || ChannelServer.Instance.ScriptManager.QuestScripts.ContainsKey(Id))
            {
                Log.Error("{1}.Init: Invalid id or already in use ({0}).", Id, GetType().Name);
                return false;
            }

            if (Objectives.Count == 0)
            {
                Log.Error("{1}.Init: Quest '{0}' doesn't have any objectives.", Id, GetType().Name);
                return false;
            }

            if (ReceiveMethod == Receive.Automatically)
                ChannelServer.Instance.Events.PlayerLoggedIn += OnPlayerLoggedIn;

            MetaData.SetString("QSTTIP", "N_{0}|D_{1}|A_|R_{2}|T_0", Name, Description,
                string.Join(", ", GetDefaultRewardGroup().Rewards));

            ChannelServer.Instance.ScriptManager.QuestScripts.Add(Id, this);

            return true;
        }

        /// <summary>
        ///     Disposes quest script, removing all subscriptions.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            ChannelServer.Instance.Events.PlayerLoggedIn -= OnPlayerLoggedIn;
            ChannelServer.Instance.Events.CreatureFinishedByPlayer -= OnCreatureKilledByPlayer;
            ChannelServer.Instance.Events.PlayerReceivesItem -= OnPlayerReceivesOrRemovesItem;
            ChannelServer.Instance.Events.PlayerRemovesItem -= OnPlayerReceivesOrRemovesItem;
            ChannelServer.Instance.Events.PlayerCompletesQuest -= OnPlayerCompletesQuest;
            ChannelServer.Instance.Events.SkillRankChanged -= OnSkillRankChanged;
            ChannelServer.Instance.Events.CreatureLevelUp -= OnCreatureLevelUp;
            ChannelServer.Instance.Events.CreatureGotKeyword -= CreatureGotKeyword;
            ChannelServer.Instance.Events.PlayerEquipsItem -= OnPlayerEquipsItem;
            ChannelServer.Instance.Events.CreatureGathered -= OnCreatureGathered;
            ChannelServer.Instance.Events.PlayerUsedSkill -= OnPlayerUsedSkill;
            ChannelServer.Instance.Events.PlayerClearedDungeon -= OnPlayerClearedDungeon;
        }

        // Setup
        // ------------------------------------------------------------------

        /// <summary>
        ///     Sets id of quest.
        /// </summary>
        /// <param name="id"></param>
        protected void SetId(int id)
        {
            Id = id;
        }

        /// <summary>
        ///     Sets name of quest.
        /// </summary>
        /// <param name="name"></param>
        protected void SetName(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Sets description of quest.
        /// </summary>
        /// <param name="description"></param>
        protected void SetDescription(string description)
        {
            Description = description;
        }

        /// <summary>
        ///     Sets additional info of quest.
        /// </summary>
        /// <param name="info"></param>
        protected void SetAdditionalInfo(string info)
        {
            AdditionalInfo = info;
        }

        /// <summary>
        ///     Sets type of quest.
        /// </summary>
        /// <param name="type"></param>
        protected void SetType(QuestType type)
        {
            Type = type;
        }

        /// <summary>
        ///     Sets PTJ hours.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="report"></param>
        /// <param name="report"></param>
        protected void SetHours(int start, int report, int deadline)
        {
            StartHour = start;
            ReportHour = report;
            DeadlineHour = deadline;
        }

        /// <summary>
        ///     Sets type for PTJs.
        /// </summary>
        /// <param name="type"></param>
        protected void SetPtjType(PtjType type)
        {
            PtjType = type;
        }

        /// <summary>
        ///     Sets quest's level (required for PTJ).
        /// </summary>
        /// <param name="level"></param>
        protected void SetLevel(QuestLevel level)
        {
            Level = level;
        }

        /// <summary>
        ///     Sets the way you receive the quest.
        /// </summary>
        /// <param name="method"></param>
        protected void SetReceive(Receive method)
        {
            ReceiveMethod = method;
        }

        /// <summary>
        ///     Sets id of the quest's item scroll.
        /// </summary>
        /// <param name="id"></param>
        protected void SetScrollId(int id)
        {
            ScrollId = id;
        }

        /// <summary>
        ///     Sets delay with which the quest arrives if sent via owl.
        /// </summary>
        /// <param name="delay"></param>
        protected void SetDelay(int delay)
        {
            Delay = delay;
        }

        /// <summary>
        ///     Sets quest's category, which is used to determin the tab it
        ///     appears in.
        /// </summary>
        /// <param name="category"></param>
        protected void SetCategory(QuestCategory category)
        {
            Category = category;
        }

        /// <summary>
        ///     Sets quest's class, which is displayed in squred brackets in
        ///     front of its name.
        /// </summary>
        /// <param name="class_"></param>
        protected void SetClass(QuestClass class_)
        {
            Class = class_;
        }

        /// <summary>
        ///     Sets quest's icon.
        /// </summary>
        /// <param name="class_"></param>
        protected void SetIcon(QuestIcon icon)
        {
            Icon = icon;
        }

        /// <summary>
        ///     Sets whether the quest can be canceled.
        /// </summary>
        /// <param name="cancelable"></param>
        protected void SetCancelable(bool cancelable)
        {
            Cancelable = cancelable;
        }

        /// <summary>
        ///     Adds prerequisite that has to be met before auto receiving the quest.
        /// </summary>
        /// <param name="prerequisite"></param>
        protected void AddPrerequisite(QuestPrerequisite prerequisite)
        {
            Prerequisites.Add(prerequisite);

            if (prerequisite.Is(typeof(QuestPrerequisiteQuestCompleted)))
            {
                ChannelServer.Instance.Events.PlayerCompletesQuest -= OnPlayerCompletesQuest;
                ChannelServer.Instance.Events.PlayerCompletesQuest += OnPlayerCompletesQuest;
            }

            if (prerequisite.Is(typeof(QuestPrerequisiteReachedLevel)) ||
                prerequisite.Is(typeof(QuestPrerequisiteReachedTotalLevel)))
            {
                ChannelServer.Instance.Events.CreatureLevelUp -= OnCreatureLevelUp;
                ChannelServer.Instance.Events.CreatureLevelUp += OnCreatureLevelUp;
            }

            if (prerequisite.Is(typeof(QuestPrerequisiteReachedRank)))
            {
                ChannelServer.Instance.Events.SkillRankChanged -= OnSkillRankChanged;
                ChannelServer.Instance.Events.SkillRankChanged += OnSkillRankChanged;
            }
        }

        /// <summary>
        ///     Adds objective that has to be cleared to complete the quest.
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="description"></param>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="objective"></param>
        protected void AddObjective(string ident, string description, int regionId, int x, int y,
            QuestObjective objective)
        {
            if (Objectives.ContainsKey(ident))
            {
                Log.Error("{0}: Objectives must have an unique identifier.", GetType().Name);
                return;
            }

            objective.Ident = ident;
            objective.Description = description;
            objective.RegionId = regionId;
            objective.X = x;
            objective.Y = y;

            if (objective.Type == ObjectiveType.Kill)
            {
                ChannelServer.Instance.Events.CreatureFinishedByPlayer -= OnCreatureKilledByPlayer;
                ChannelServer.Instance.Events.CreatureFinishedByPlayer += OnCreatureKilledByPlayer;
            }

            if (objective.Type == ObjectiveType.Collect)
            {
                ChannelServer.Instance.Events.PlayerReceivesItem -= OnPlayerReceivesOrRemovesItem;
                ChannelServer.Instance.Events.PlayerReceivesItem += OnPlayerReceivesOrRemovesItem;
                ChannelServer.Instance.Events.PlayerRemovesItem -= OnPlayerReceivesOrRemovesItem;
                ChannelServer.Instance.Events.PlayerRemovesItem += OnPlayerReceivesOrRemovesItem;
            }

            if (objective.Type == ObjectiveType.Create)
            {
                ChannelServer.Instance.Events.CreatureCreatedItem -= OnCreatureCreatedOrProducedItem;
                ChannelServer.Instance.Events.CreatureCreatedItem += OnCreatureCreatedOrProducedItem;
                ChannelServer.Instance.Events.CreatureProducedItem -= OnCreatureCreatedOrProducedItem;
                ChannelServer.Instance.Events.CreatureProducedItem += OnCreatureCreatedOrProducedItem;
            }

            if (objective.Type == ObjectiveType.ReachRank)
            {
                ChannelServer.Instance.Events.SkillRankChanged -= OnSkillRankChanged;
                ChannelServer.Instance.Events.SkillRankChanged += OnSkillRankChanged;
            }

            if (objective.Type == ObjectiveType.ReachLevel)
            {
                ChannelServer.Instance.Events.CreatureLevelUp -= OnCreatureLevelUp;
                ChannelServer.Instance.Events.CreatureLevelUp += OnCreatureLevelUp;
            }

            if (objective.Type == ObjectiveType.GetKeyword)
            {
                ChannelServer.Instance.Events.CreatureGotKeyword -= CreatureGotKeyword;
                ChannelServer.Instance.Events.CreatureGotKeyword += CreatureGotKeyword;
            }

            if (objective.Type == ObjectiveType.Equip)
            {
                ChannelServer.Instance.Events.PlayerEquipsItem -= OnPlayerEquipsItem;
                ChannelServer.Instance.Events.PlayerEquipsItem += OnPlayerEquipsItem;
            }

            if (objective.Type == ObjectiveType.Gather)
            {
                ChannelServer.Instance.Events.CreatureGathered -= OnCreatureGathered;
                ChannelServer.Instance.Events.CreatureGathered += OnCreatureGathered;
            }

            if (objective.Type == ObjectiveType.UseSkill)
            {
                ChannelServer.Instance.Events.PlayerUsedSkill -= OnPlayerUsedSkill;
                ChannelServer.Instance.Events.PlayerUsedSkill += OnPlayerUsedSkill;
            }

            if (objective.Type == ObjectiveType.ClearDungeon)
            {
                ChannelServer.Instance.Events.PlayerClearedDungeon -= OnPlayerClearedDungeon;
                ChannelServer.Instance.Events.PlayerClearedDungeon += OnPlayerClearedDungeon;
            }

            Objectives.Add(ident, objective);
        }

        /// <summary>
        ///     Adds reward the player can get for completing the quest.
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="options"></param>
        protected void AddReward(QuestReward reward, RewardOptions options = RewardOptions.None)
        {
            AddReward(0, RewardGroupType.Item, QuestResult.Perfect, reward, options);
        }

        /// <summary>
        ///     Adds reward to a specific reward group, that the player can select
        ///     after completing the quest.
        /// </summary>
        /// <remarks>
        ///     Mainly used for PTJs.
        /// </remarks>
        /// <param name="groupId"></param>
        /// <param name="type"></param>
        /// <param name="result"></param>
        /// <param name="reward"></param>
        /// <param name="options"></param>
        protected void AddReward(int groupId, RewardGroupType type, QuestResult result, QuestReward reward,
            RewardOptions options = RewardOptions.None)
        {
            if (!RewardGroups.ContainsKey(groupId))
                RewardGroups[groupId] = new QuestRewardGroup(groupId, type);

            reward.Result = result;
            reward.Visible = (options & RewardOptions.Hidden) == 0;

            RewardGroups[groupId].Add(reward);
        }

        /// <summary>
        ///     Returns the default reward group (0 or 1).
        /// </summary>
        /// <returns></returns>
        public QuestRewardGroup GetDefaultRewardGroup()
        {
            QuestRewardGroup result;
            if (!RewardGroups.TryGetValue(0, out result))
                if (!RewardGroups.TryGetValue(1, out result))
                    throw new Exception("QuestScript.GetDefaultRewardGroup: No default group found.");

            return result;
        }

        /// <summary>
        ///     Returns rewards for the given group and result.
        /// </summary>
        /// <remarks>
        ///     Mainly used for PTJs.
        /// </remarks>
        /// <param name="rewardGroup"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public ICollection<QuestReward> GetRewards(int rewardGroup, QuestResult result)
        {
            var rewards = new List<QuestReward>();

            QuestRewardGroup group;
            RewardGroups.TryGetValue(rewardGroup, out group);
            if (group != null && result != QuestResult.None)
                rewards.AddRange(group.Rewards.Where(a => a.Result == result));

            return rewards;
        }

        // Prerequisite Factory
        // ------------------------------------------------------------------

        protected QuestPrerequisite Completed(int questId)
        {
            return new QuestPrerequisiteQuestCompleted(questId);
        }

        protected QuestPrerequisite ReachedLevel(int level)
        {
            return new QuestPrerequisiteReachedLevel(level);
        }

        protected QuestPrerequisite ReachedTotalLevel(int level)
        {
            return new QuestPrerequisiteReachedTotalLevel(level);
        }

        protected QuestPrerequisite ReachedRank(SkillId skillId, SkillRank rank)
        {
            return new QuestPrerequisiteReachedRank(skillId, rank);
        }

        protected QuestPrerequisite ReachedAge(int age)
        {
            return new QuestPrerequisiteReachedAge(age);
        }

        protected QuestPrerequisite NotSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
        {
            return new QuestPrerequisiteNotSkill(skillId, rank);
        }

        protected QuestPrerequisite EventActive(string gameEventId)
        {
            return new QuestPrerequisiteEventActive(gameEventId);
        }

        protected QuestPrerequisite And(params QuestPrerequisite[] prerequisites)
        {
            return new QuestPrerequisiteAnd(prerequisites);
        }

        protected QuestPrerequisite Or(params QuestPrerequisite[] prerequisites)
        {
            return new QuestPrerequisiteOr(prerequisites);
        }

        // Objective Factory
        // ------------------------------------------------------------------

        protected QuestObjective Kill(int amount, string raceType)
        {
            return new QuestObjectiveKill(amount, raceType);
        }

        protected QuestObjective Collect(int itemId, int amount)
        {
            return new QuestObjectiveCollect(itemId, amount);
        }

        protected QuestObjective Talk(string npcName)
        {
            return new QuestObjectiveTalk(npcName);
        }

        protected QuestObjective Deliver(int itemId, string npcName)
        {
            return new QuestObjectiveDeliver(itemId, 1, npcName);
        }

        protected QuestObjective Create(int itemId, int amount, SkillId skillId, int quality = -1000)
        {
            return new QuestObjectiveCreate(itemId, amount, skillId, quality);
        }

        protected QuestObjective ReachRank(SkillId skillId, SkillRank rank)
        {
            return new QuestObjectiveReachRank(skillId, rank);
        }

        protected QuestObjective ReachLevel(int level)
        {
            return new QuestObjectiveReachLevel(level);
        }

        protected QuestObjective GetKeyword(string keyword)
        {
            return new QuestObjectiveGetKeyword(keyword);
        }

        protected QuestObjective Equip(string tag)
        {
            return new QuestObjectiveEquip(tag);
        }

        protected QuestObjective Gather(int itemId, int amount)
        {
            return new QuestObjectiveGather(itemId, amount);
        }

        protected QuestObjective UseSkill(SkillId skillId)
        {
            return new QuestObjectiveUseSkill(skillId);
        }

        protected QuestObjective ClearDungeon(string dungeonName)
        {
            return new QuestObjectiveClearDungeon(dungeonName);
        }

        // Reward Factory
        // ------------------------------------------------------------------

        protected QuestReward Item(int itemId, int amount = 1)
        {
            return new QuestRewardItem(itemId, amount);
        }

        protected QuestReward Keyword(string keyword)
        {
            return new QuestRewardKeyword(keyword);
        }

        protected QuestReward Enchant(int optionSetId)
        {
            return new QuestRewardEnchant(optionSetId);
        }

        protected QuestReward WarpScroll(int itemId, string portal)
        {
            return new QuestRewardWarpScroll(itemId, portal);
        }

        protected QuestReward QuestScroll(int questId)
        {
            return new QuestRewardQuestScroll(questId);
        }

        protected QuestReward Skill(SkillId skillId, SkillRank rank)
        {
            return new QuestRewardSkill(skillId, rank, 0);
        }

        protected QuestReward Skill(SkillId skillId, SkillRank rank, int training)
        {
            return new QuestRewardSkill(skillId, rank, training);
        }

        protected QuestReward Pattern(int itemId, int formId, int useCount)
        {
            return new QuestRewardPattern(itemId, formId, useCount);
        }

        protected QuestReward Gold(int amount)
        {
            return new QuestRewardGold(Math2.MultiplyChecked(amount,
                ChannelServer.Instance.Conf.World.GoldQuestRewardRate));
        }

        protected QuestReward Exp(int amount)
        {
            return new QuestRewardExp(Math2.MultiplyChecked(amount, ChannelServer.Instance.Conf.World.QuestExpRate));
        }

        protected QuestReward ExplExp(int amount)
        {
            return new QuestRewardExplExp(Math2.MultiplyChecked(amount,
                ChannelServer.Instance.Conf.World.QuestExpRate));
        }

        protected QuestReward AP(short amount)
        {
            return new QuestRewardAp(Math2.MultiplyChecked(amount, ChannelServer.Instance.Conf.World.QuestApRate));
        }

        protected QuestReward StatBonus(Stat stat, int amount)
        {
            return new QuestRewardStatBonus(stat, amount);
        }

        // Events
        // ------------------------------------------------------------------

        public virtual void OnReceive(Creature creature)
        {
        }

        public virtual void OnComplete(Creature creature)
        {
        }

        // Where the magic happens~
        // ------------------------------------------------------------------

        /// <summary>
        ///     Checks and starts auto quests.
        /// </summary>
        /// <param name="character"></param>
        private void OnPlayerLoggedIn(Creature character)
        {
            if (CheckPrerequisites(character))
                character.Quests.SendOwl(Id, Delay);
        }

        /// <summary>
        ///     Returns true if all prerequisites are met, receive method is auto,
        ///     and the creature doesn't have the quest yet.
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private bool CheckPrerequisites(Creature character)
        {
            // Check if creature can receive the quest,
            // based on the quest's settings.
            if (Availability <= QuestAvailability.Characters && !character.IsCharacter)
                return false;

            // Check if receive method is auto and creature doesn't have it yet.
            if (ReceiveMethod != Receive.Automatically || character.Quests.Has(Id))
                return false;

            // Actually check prerequisites
            return Prerequisites.All(prerequisite => prerequisite.Met(character));
        }

        /// <summary>
        ///     Updates quest on client(s), depending on its type.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="quest"></param>
        private void UpdateQuest(Creature creature, Quest quest)
        {
            if (!IsPartyQuest)
                Send.QuestUpdate(creature, quest);
            else
                Send.QuestUpdate(creature.Party, quest);
        }

        /// <summary>
        ///     Returns true if creature can make progress on this quest.
        /// </summary>
        /// <remarks>
        ///     Used from objective event handlers, to see if the quest should
        ///     receive the progress.
        /// </remarks>
        /// <param name="creature"></param>
        /// <param name="quest"></param>
        /// <returns></returns>
        private bool CanMakeProgress(Creature creature, Quest quest)
        {
            // Party quests can only make progress if they're active
            if (IsPartyQuest)
                return creature.IsInParty && creature.Party.Quest == quest;

            // TODO: Guild quests, outside, delay

            return true;
        }

        /// <summary>
        ///     Checks and updates current obective's count.
        /// </summary>
        /// <param name="creature"></param>
        public void CheckCurrentObjective(Creature creature)
        {
            if (creature == null || !creature.IsPlayer)
                return;

            var quests = creature.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(creature, quest))
                    continue;

                var progress = quest.CurrentObjectiveOrLast;
                if (progress == null) return;

                var objective = Objectives[progress.Ident];
                if (objective == null) return;

                var prevCount = progress.Count;
                switch (objective.Type)
                {
                    case ObjectiveType.ReachRank:
                        var reachRankObjective = objective as QuestObjectiveReachRank;
                        var skillId = reachRankObjective.Id;
                        var rank = reachRankObjective.Rank;
                        var skill = creature.Skills.Get(skillId);

                        if (skill != null && skill.Info.Rank >= rank)
                            quest.SetDone(progress.Ident);
                        else
                            quest.SetUndone(progress.Ident);

                        break;

                    case ObjectiveType.ReachLevel:
                        var reachLevelObjective = objective as QuestObjectiveReachLevel;

                        if (creature.Level >= reachLevelObjective.Amount)
                            quest.SetDone(progress.Ident);

                        break;

                    case ObjectiveType.Collect:
                        var itemId = (objective as QuestObjectiveCollect).ItemId;

                        // Do not count incomplete items (e.g. tailoring, blacksmithing).
                        var count = creature.Inventory.Count(item =>
                            (item.Info.Id == itemId || item.Data.StackItemId == itemId) && !item.IsIncomplete);

                        if (!progress.Done && count >= objective.Amount)
                            quest.SetDone(progress.Ident);
                        else if (progress.Done && count < objective.Amount)
                            quest.SetUndone(progress.Ident);

                        // Set(Un)Done modifies the count, has to be set afterwards
                        progress.Count = count;
                        break;

                    case ObjectiveType.GetKeyword:
                        var getKeywordObjective = objective as QuestObjectiveGetKeyword;

                        if (creature.Keywords.Has((ushort) getKeywordObjective.KeywordId))
                            quest.SetDone(progress.Ident);

                        break;

                    default:
                        // Objective that can't be checked here.
                        break;
                }

                if (progress.Count != prevCount)
                    UpdateQuest(creature, quest);
            }
        }

        /// <summary>
        ///     Updates kill objectives.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="killer"></param>
        private void OnCreatureKilledByPlayer(Creature creature, Creature killer)
        {
            if (creature == null || killer == null) return;

            var quests = killer.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(killer, quest))
                    continue;

                var progress = quest.CurrentObjective;
                if (progress == null) return;

                var objective = Objectives[progress.Ident] as QuestObjectiveKill;
                if (objective == null || objective.Type != ObjectiveType.Kill || !objective.Check(creature)) return;

                if (progress.Count >= objective.Amount) return;

                progress.Count++;

                if (progress.Count >= objective.Amount)
                    quest.SetDone(progress.Ident);

                UpdateQuest(killer, quest);
            }
        }

        /// <summary>
        ///     Updates collect objectives.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        private void OnPlayerReceivesOrRemovesItem(Creature creature, int itemId, int amount)
        {
            CheckCurrentObjective(creature);
        }

        /// <summary>
        ///     Updates reach rank objectives.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="skill"></param>
        private void OnSkillRankChanged(Creature creature, Skill skill)
        {
            if (CheckPrerequisites(creature))
                creature.Quests.SendOwl(Id, Delay);

            CheckCurrentObjective(creature);
        }

        /// <summary>
        ///     Checks prerequisites.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="questId"></param>
        private void OnPlayerCompletesQuest(Creature creature, int questId)
        {
            if (CheckPrerequisites(creature))
                creature.Quests.SendOwl(Id, Delay);
        }

        /// <summary>
        ///     Checks prerequisites.
        /// </summary>
        /// <param name="creature"></param>
        private void OnCreatureLevelUp(Creature creature)
        {
            if (CheckPrerequisites(creature))
                creature.Quests.SendOwl(Id, Delay);

            CheckCurrentObjective(creature);
        }

        /// <summary>
        ///     Checks and updates current objective.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="keywordId"></param>
        private void CreatureGotKeyword(Creature creature, int keywordId)
        {
            CheckCurrentObjective(creature);
        }

        /// <summary>
        ///     Updates equip objectives.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="item"></param>
        private void OnPlayerEquipsItem(Creature creature, Item item)
        {
            if (creature == null || !creature.IsPlayer || item == null || !item.Info.Pocket.IsEquip())
                return;

            var quests = creature.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(creature, quest))
                    continue;

                var progress = quest.CurrentObjectiveOrLast;
                if (progress == null) return;

                var objective = Objectives[progress.Ident];
                if (objective == null || objective.Type != ObjectiveType.Equip) return;

                var equipObjective = objective as QuestObjectiveEquip;
                if (!progress.Done && item.HasTag(equipObjective.Tag))
                {
                    quest.SetDone(progress.Ident);
                    UpdateQuest(creature, quest);
                }
            }
        }

        /// <summary>
        ///     Updates gathering objectives.
        /// </summary>
        /// <param name="args"></param>
        private void OnCreatureGathered(CollectEventArgs args)
        {
            var creature = args.Creature;

            var quests = creature.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(creature, quest))
                    continue;

                var progress = quest.CurrentObjectiveOrLast;
                if (progress == null) return;

                var objective = Objectives[progress.Ident];
                if (objective == null || objective.Type != ObjectiveType.Gather) return;

                var gatherObjective = objective as QuestObjectiveGather;
                if (!progress.Done && args.Success && args.ItemId == gatherObjective.ItemId)
                {
                    progress.Count++;
                    if (progress.Count == gatherObjective.Amount)
                        quest.SetDone(progress.Ident);

                    UpdateQuest(creature, quest);
                }
            }
        }

        /// <summary>
        ///     Updates UseSkill objectives.
        /// </summary>
        /// <param name="args"></param>
        private void OnPlayerUsedSkill(Creature creature, Skill skill)
        {
            if (creature == null || skill == null)
                return;

            var quests = creature.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(creature, quest))
                    continue;

                var progress = quest.CurrentObjectiveOrLast;
                if (progress == null) return;

                var objective = Objectives[progress.Ident];
                if (objective == null || objective.Type != ObjectiveType.UseSkill) return;

                var useSkillObjective = objective as QuestObjectiveUseSkill;
                if (!progress.Done && skill.Info.Id == useSkillObjective.Id)
                {
                    quest.SetDone(progress.Ident);
                    UpdateQuest(creature, quest);
                }
            }
        }

        /// <summary>
        ///     Updates ClearDungeon objectives.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="dungeon"></param>
        private void OnPlayerClearedDungeon(Creature creature, Dungeon dungeon)
        {
            if (creature == null || dungeon == null)
                return;

            var quests = creature.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(creature, quest))
                    continue;

                var progress = quest.CurrentObjectiveOrLast;
                if (progress == null) return;

                var objective = Objectives[progress.Ident];
                if (objective == null || objective.Type != ObjectiveType.ClearDungeon) return;

                var clearDungeonObjective = objective as QuestObjectiveClearDungeon;
                if (!progress.Done && dungeon.Name.ToLower() == clearDungeonObjective.DungeonName.ToLower())
                {
                    quest.SetDone(progress.Ident);
                    UpdateQuest(creature, quest);
                }
            }
        }

        /// <summary>
        ///     Updates Create objectives.
        /// </summary>
        /// <remarks>
        ///     Creation and Production events share same event
        ///     due to having the same ObjectiveType code.
        /// </remarks>
        /// <param name="args">
        ///     An object of type CreationEventArgs or ProductionEventArgs, otherwise an exception will be issued to
        ///     the logger.
        /// </param>
        private void OnCreatureCreatedOrProducedItem(EventArgs args)
        {
            Creature creature;
            int itemId;
            string skill; // Is there a better way to check for skill match?

            var crargs = args as CreationEventArgs;
            var prargs = args as ProductionEventArgs;
            if (crargs != null) // Try cast as CreationEventArgs
            {
                creature = crargs.Creature;
                itemId = crargs.Item.Info.Id;
                skill = crargs.Method.ToString();
            }
            else if (prargs != null) // Try cast as ProductionEventArgs
            {
                // Cancel if it wasn't a success
                if (!prargs.Success)
                    return;

                creature = prargs.Creature;
                itemId = prargs.ProductionData.ItemId; // Use production data in case Item is null
                skill = prargs.ProductionData.Category.ToString();
                if (skill == "Spinning")
                    skill = "Weaving"; // Shared SkillId.
            }
            else // Error: Cannot cast as either one.
            {
                Log.Exception(new InvalidCastException(string.Format(
                    "Unable to cast EventArgs as CreationEventArgs nor ProductionEventArgs (Quest Name: {0}, ID: {1})",
                    Name, Id)));
                return;
            }

            var quests = creature.Quests.GetAllIncomplete(Id);
            foreach (var quest in quests)
            {
                if (!CanMakeProgress(creature, quest))
                    continue;

                var progress = quest.CurrentObjectiveOrLast;
                if (progress == null) return;

                var objective = Objectives[progress.Ident];
                if (objective == null || objective.Type != ObjectiveType.Create) return;

                var createObjective = objective as QuestObjectiveCreate;
                if (!progress.Done && itemId == createObjective.ItemId && skill == createObjective.SkillId.ToString())
                {
                    var done = ++progress.Count == createObjective.Amount;
                    if (done)
                        quest.SetDone(progress.Ident);

                    UpdateQuest(creature, quest);

                    // Hot-fix for #390, after a creation objective might
                    // come a collect objective for the finished items,
                    // the new active objective has to be checked.
                    // This should happen generally, but some refactoring is
                    // in order, to not make such a mess out of it.
                    if (done)
                        CheckCurrentObjective(creature);
                }
            }
        }
    }

    /// <summary>
    ///     The method of how a player can get a quest.
    /// </summary>
    public enum Receive
    {
        Manually,
        Automatically
    }

    /// <summary>
    ///     The PTJ quest level.
    /// </summary>
    public enum QuestLevel
    {
        None,
        Basic,
        Int,
        Adv
    }

    /// <summary>
    ///     Who this quest is available to.
    /// </summary>
    public enum QuestAvailability
    {
        Characters,
        CharactersAndPets
    }
}