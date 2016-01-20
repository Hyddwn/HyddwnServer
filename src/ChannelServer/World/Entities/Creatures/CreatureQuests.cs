﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World.Quests;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureQuests
	{
		private Creature _creature;
		private List<Quest> _quests;
		private Dictionary<PtjType, PtjTrackRecord> _ptjRecords;

		/// <summary>
		/// Raised whenever a PTJ's track record changes, i.e. when the
		/// creature finishes or fails it.
		/// </summary>
		public event Action<Creature, PtjTrackRecord> PtjTrackRecordChanged;

		/// <summary>
		/// Creates new quest manager for creature.
		/// </summary>
		/// <param name="creature"></param>
		public CreatureQuests(Creature creature)
		{
			_creature = creature;
			_quests = new List<Quest>();
			_ptjRecords = new Dictionary<PtjType, PtjTrackRecord>();
		}

		/// <summary>
		/// Adds quest to manager, does not update client, send owl,
		/// or anything else.
		/// </summary>
		/// <remarks>
		/// This method is for initialization, use Give during run-time.
		/// </remarks>
		/// <param name="quest"></param>
		public void AddSilent(Quest quest)
		{
			lock (_quests)
				_quests.Add(quest);
		}

		/// <summary>
		/// Adds quest to manager and informs the client about it.
		/// </summary>
		/// <param name="quest"></param>
		public void Add(Quest quest)
		{
			// Check quest item
			if (quest.QuestItem == null)
				throw new InvalidOperationException("Quest item can't be null.");

			if (!_creature.Inventory.Has(quest.QuestItem))
				throw new InvalidOperationException("The quest item needs to be in the creature's inventory first.");

			this.AddSilent(quest);

			// Quest info
			Send.NewQuest(_creature, quest);

			// Start PTJ clock
			if (quest.Data.Type == QuestType.Deliver)
				Send.QuestStartPtj(_creature, quest.UniqueId);

			// Initial objective check, for things like collect and reach rank,
			// that may be done already.
			quest.Data.CheckCurrentObjective(_creature);

			// Give item to deliver for first deliver objective
			var deliverObjective = quest.Data.Objectives[quest.CurrentObjectiveOrLast.Ident] as QuestObjectiveDeliver;
			if (deliverObjective != null)
			{
				var item = new Item(deliverObjective.ItemId);
				item.Amount = Math.Min(1, deliverObjective.Amount);

				_creature.Inventory.Add(item, true);
			}
		}

		/// <summary>
		/// Returns true if creature has quest with the given quest id,
		/// completed or not.
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public bool Has(int questId)
		{
			lock (_quests)
				return _quests.Exists(a => a.Id == questId);
		}

		/// <summary>
		/// Returns true if creature has the given quest.
		/// </summary>
		/// <param name="quest"></param>
		/// <returns></returns>
		public bool Has(Quest quest)
		{
			lock (_quests)
				return _quests.Contains(quest);
		}

		/// <summary>
		/// Returns first uncompleted quest with the given quest id,
		/// or null if none were found.
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public Quest GetFirstIncomplete(int questId)
		{
			return this.Get(a => a.Id == questId && a.State == QuestState.InProgress);
		}

		/// <summary>
		/// Returns quest by unique quest id, or null if it wasn't found.
		/// </summary>
		/// <param name="uniqueId"></param>
		/// <returns></returns>
		public Quest Get(long uniqueId)
		{
			return this.Get(a => a.UniqueId == uniqueId);
		}

		/// <summary>
		/// Returns first quest that maches the predicate, or null if there
		/// were none.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public Quest Get(Func<Quest, bool> predicate)
		{
			lock (_quests)
				return _quests.FirstOrDefault(predicate);
		}

		/// <summary>
		/// Returns all incomplete quests with the given id.
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public Quest[] GetAllIncomplete(int questId)
		{
			lock (_quests)
				return _quests.Where(a => a.Id == questId && a.State == QuestState.InProgress).ToArray();
		}

		/// <summary>
		/// Calls <see cref="Get(long)"/>. If the result is null, throws <see cref="SevereViolation"/>.
		/// </summary>
		/// <param name="uniqueId"></param>
		/// <returns></returns>
		public Quest GetSafe(long uniqueId)
		{
			var quest = this.Get(uniqueId);
			if (quest == null)
				throw new SevereViolation("Creature does not have quest 0x{0:X}", uniqueId);

			return quest;
		}

		/// <summary>
		/// Returns true if quest with the given id has been completed.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool IsComplete(int id)
		{
			lock (_quests)
				return _quests.Exists(a => a.Id == id && a.State == QuestState.Complete);
		}

		/// <summary>
		/// Returns new list of all quests in manager.
		/// </summary>
		/// <returns></returns>
		public ICollection<Quest> GetList()
		{
			lock (_quests)
				return _quests.ToArray();
		}

		/// <summary>
		/// Returns new list of incomplete quests in manager.
		/// </summary>
		/// <returns></returns>
		public ICollection<Quest> GetIncompleteList()
		{
			lock (_quests)
				return _quests.Where(a => a.State != QuestState.Complete).ToArray();
		}

		/// <summary>
		/// Sends an owl to deliver a quest scroll fort he given quest id
		/// to the player.
		/// </summary>
		/// <param name="questId"></param>
		public void SendOwl(int questId)
		{
			var item = Item.CreateQuestScroll(questId);

			Send.QuestOwlNew(_creature, item.QuestId);

			// Do quests that are received via owl *always* go into the
			// quest pocket?
			_creature.Inventory.Add(item, Pocket.Quests);
		}

		/// <summary>
		/// Gives quest scroll for the given quest id to the player.
		/// </summary>
		/// <param name="questId"></param>
		public void Start(int questId)
		{
			var item = Item.CreateQuestScroll(questId);

			// Do quests that are received via owl *always* go into the
			// quest pocket?
			_creature.Inventory.Add(item, Pocket.Quests);
		}

		/// <summary>
		/// Finishes objective for quest, returns false if quest doesn't exist
		/// or doesn't have the objective.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public bool Finish(int questId, string objective)
		{
			var quest = this.GetFirstIncomplete(questId);
			if (quest == null) return false;

			var progress = quest.GetProgress(objective);
			if (progress == null)
				throw new Exception("Quest.Finish: No progress found for objective '" + objective + "'.");

			quest.SetDone(objective);

			Send.QuestUpdate(_creature, quest);

			return true;
		}

		/// <summary>
		/// Completes and removes first incomplete instance of the given quest,
		/// provided that an incomplete one exists.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="owl">Show owl delivering the quest?</param>
		/// <returns></returns>
		public bool Complete(int questId, bool owl)
		{
			var quest = this.Get(a => a.Id == questId && a.State == QuestState.InProgress);
			if (quest == null)
				return false;

			return this.Complete(quest, owl);
		}

		/// <summary>
		/// Completes all objectives and the quest, if it exists,
		/// and removes quest from log. Rewards are taken from
		/// group 0, result "Perfect", because the objectives are set done.
		/// </summary>
		/// <param name="quest"></param>
		/// <param name="owl">Show owl delivering the quest?</param>
		/// <returns></returns>
		public bool Complete(Quest quest, bool owl)
		{
			if (!this.Has(quest))
				throw new ArgumentException("Quest not found in this manager.");

			quest.CompleteAllObjectives();

			return this.Complete(quest, 0, owl);
		}

		/// <summary>
		/// Completes quest and takes rewards from group and the result
		/// based on the progress on the current or last objective.
		/// Primarily used by PTJs.
		/// </summary>
		/// <param name="quest"></param>
		/// <param name="rewardGroup">Reward group to use.</param>
		/// <param name="owl">Show owl delivering the quest?</param>
		/// <returns></returns>
		public bool Complete(Quest quest, int rewardGroup, bool owl)
		{
			if (!this.Has(quest))
				throw new ArgumentException("Quest not found in this manager.");

			var success = this.EndQuest(quest, rewardGroup, owl);
			if (success)
			{
				quest.State = QuestState.Complete;

				// Remove quest item after the state was set, so the item
				// is removed, but not the quest.
				_creature.Inventory.Remove(quest.QuestItem);

				// Remove collected items if last objective was Collect.
				// Should mainly apply to PTJ and scroll quests, that only
				// have that one objective.
				var progress = quest.CurrentObjectiveOrLast;
				var objective = quest.Data.Objectives[progress.Ident];
				var collectObjective = objective as QuestObjectiveCollect;
				if (collectObjective != null)
					_creature.Inventory.Remove(collectObjective.ItemId, Math.Min(collectObjective.Amount, progress.Count));

				ChannelServer.Instance.Events.OnPlayerCompletesQuest(_creature, quest.Id);
			}
			return success;
		}

		/// <summary>
		/// Completes and removes quest without rewards, if it exists.
		/// </summary>
		/// <param name="quest"></param>
		/// <returns></returns>
		public bool GiveUp(Quest quest)
		{
			if (!this.Has(quest))
				throw new ArgumentException("Quest not found in this manager.");

			var success = this.EndQuest(quest, -1, false);

			// Remove quest item on success, which will also remove the
			// quest from the manager.
			if (success)
				_creature.Inventory.Remove(quest.QuestItem);

			return success;
		}

		/// <summary>
		/// Completes and removes quest, if it exists, giving the rewards
		/// in the process, if warranted.
		/// </summary>
		/// <param name="quest"></param>
		/// <param name="rewardGroup">Reward group to use, set to -1 for no rewards.</param>
		/// <param name="owl">Show owl delivering the rewards?</param>
		/// <returns></returns>
		private bool EndQuest(Quest quest, int rewardGroup, bool owl)
		{
			var result = quest.GetResult();

			// Increase PTJ done/success
			if (quest.Data.Type == QuestType.Deliver)
				this.ModifyPtjTrackRecord(quest.Data.PtjType, +1, (result == QuestResult.Perfect ? +1 : 0));

			// Rewards
			if (rewardGroup != -1)
			{
				var rewards = quest.Data.GetRewards(rewardGroup, result);
				if (rewards.Count == 0)
					Log.Warning("CreatureQuests.EndQuest: No rewards for group '{0}' at result '{1}' in quest '{2}'.", rewardGroup, result, quest.Id);
				else
					this.GiveRewards(quest, rewards, owl);
			}

			// Remove from quest log.
			Send.QuestClear(_creature, quest.UniqueId);

			// Update PTJ stuff and stop clock
			if (quest.Data.Type == QuestType.Deliver)
			{
				var record = this.GetPtjTrackRecord(quest.Data.PtjType);

				Send.QuestUpdatePtj(_creature, quest.Data.PtjType, record.Done, record.Success);
				Send.QuestEndPtj(_creature);
			}

			return true;
		}

		/// <summary>
		/// Gives quest rewards to creature.
		/// </summary>
		/// <param name="quest">Quest the rewards come from.</param>
		/// <param name="rewards">Rewards to give to the creature.</param>
		/// <param name="owl">Show owl delivering the rewards?</param>
		private void GiveRewards(Quest quest, ICollection<QuestReward> rewards, bool owl)
		{
			if (rewards.Count == 0)
				return;

			if (owl)
				Send.QuestOwlComplete(_creature, quest.UniqueId);

			foreach (var reward in rewards)
			{
				try
				{
					reward.Reward(_creature, quest);
				}
				catch (NotImplementedException)
				{
					Log.Unimplemented("Quest.Complete: Reward '{0}'.", reward.Type);
				}
			}
		}

		/// <summary>
		/// Returns true if the quest is in progress, optionally also checking
		/// if it's on the given objective .
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public bool IsActive(int questId, string objective = null)
		{
			var quest = this.GetFirstIncomplete(questId);
			if (quest == null) return false;

			var current = quest.CurrentObjective;
			if (current == null) return false;

			if (objective != null && current.Ident != objective)
				return false;

			return (quest.State == QuestState.InProgress);
		}

		/// <summary>
		/// Modifies track record, changing success, done, and last change.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="done"></param>
		/// <param name="success"></param>
		public void ModifyPtjTrackRecord(PtjType type, int done, int success)
		{
			var record = this.GetPtjTrackRecord(type);

			record.Done += done;
			record.Success += success;
			record.LastChange = DateTime.Now;

			this.PtjTrackRecordChanged.Raise(_creature, record);
		}

		/// <summary>
		/// Returns new list of all track records.
		/// </summary>
		/// <returns></returns>
		public PtjTrackRecord[] GetPtjTrackRecords()
		{
			lock (_ptjRecords)
				return _ptjRecords.Values.ToArray();
		}

		/// <summary>
		/// Returns track record for type.
		/// </summary>
		/// <returns></returns>
		public PtjTrackRecord GetPtjTrackRecord(PtjType type)
		{
			PtjTrackRecord record;
			lock (_ptjRecords)
				if (!_ptjRecords.TryGetValue(type, out record))
					_ptjRecords[type] = (record = new PtjTrackRecord(type, 0, 0, DateTime.MinValue));

			return record;
		}

		/// <summary>
		/// Returns current PTJ quest or null.
		/// </summary>
		/// <returns></returns>
		public Quest GetPtjQuest()
		{
			return this.Get(a => a.Data.Type == QuestType.Deliver && a.State != QuestState.Complete);
		}
	}
}
