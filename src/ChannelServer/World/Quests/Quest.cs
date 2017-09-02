// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Quests
{
    public class Quest
    {
        private static long _questId = MabiId.QuestsTmp;

        private readonly OrderedDictionary<string, QuestObjectiveProgress> _progresses;

        /// <summary>
        ///     Initializer constructor
        /// </summary>
        private Quest()
        {
            MetaData = new MabiDictionary();
        }

        /// <summary>
        ///     Creates Quest based on existing data.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="state"></param>
        /// <param name="metaData"></param>
        public Quest(int questId, long uniqueId, QuestState state, string metaData)
            : this()
        {
            Data = ChannelServer.Instance.ScriptManager.QuestScripts.Get(questId);
            if (Data == null)
                throw new Exception("Quest '" + questId + "' does not exist.");

            Id = questId;
            UniqueId = uniqueId;
            State = state;
            MetaData.Parse(metaData);

            _progresses = new OrderedDictionary<string, QuestObjectiveProgress>();
            foreach (var objective in Data.Objectives)
                _progresses[objective.Key] = new QuestObjectiveProgress(objective.Key);
            _progresses[0].Unlocked = true;
        }

        /// <summary>
        ///     Creates new Quest based on script data.
        /// </summary>
        /// <param name="questId"></param>
        public Quest(int questId)
            : this(questId, Interlocked.Increment(ref _questId), QuestState.InProgress, "")
        {
            // Default meta data entries
            MetaData.SetFloat("QMBEXP", 1);
            MetaData.SetFloat("QMBGLD", 1);
            MetaData.SetFloat("QMSMEXP", 1);
            MetaData.SetFloat("QMSMGLD", 1);
            MetaData.SetFloat("QMAMEXP", 1);
            MetaData.SetFloat("QMAMGLD", 1);
            MetaData.SetInt("QMBHDCTADD", 0);
            MetaData.SetFloat("QMGNRB", 1);

            // Makes client show the NPC's face as icon?
            //this.MetaData.SetString("QRQSTR", "_caitin");
        }

        /// <summary>
        ///     Unique id of this quests.
        /// </summary>
        public long UniqueId { get; set; }

        /// <summary>
        ///     General id to identify this quests.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     State the quest is in.
        /// </summary>
        public QuestState State { get; set; }

        /// <summary>
        ///     Returns quest script
        /// </summary>
        public QuestScript Data { get; protected set; }

        /// <summary>
        ///     Additional information
        /// </summary>
        public MabiDictionary MetaData { get; set; }

        /// <summary>
        ///     Deadline used for PTJs.
        /// </summary>
        public DateTime Deadline { get; set; }

        /// <summary>
        ///     Returns true if all objectives are done.
        /// </summary>
        public bool IsDone
        {
            get { return _progresses.Values.All(progress => progress.Done); }
        }

        /// <summary>
        ///     Returns true if any progress has been made towards completing
        ///     the quest.
        /// </summary>
        public bool HasProgress
        {
            get { return _progresses.Values.Any(a => a.Done || a.Count != 0); }
        }

        /// <summary>
        ///     Returns progress for current objective or null,
        ///     if all objectives are done.
        /// </summary>
        public QuestObjectiveProgress CurrentObjective
        {
            get { return _progresses.Values.FirstOrDefault(progress => !progress.Done); }
        }

        /// <summary>
        ///     Returns progress for current objective or last one,
        ///     if all objectives are done.
        /// </summary>
        public QuestObjectiveProgress CurrentObjectiveOrLast
        {
            get
            {
                foreach (var progress in _progresses.Values)
                    if (!progress.Done)
                        return progress;
                return _progresses[_progresses.Count - 1];
            }
        }

        /// <summary>
        ///     The item this quest belongs to.
        /// </summary>
        /// <remarks>
        ///     QuestItem is null after the quest was completed, as the item will
        ///     be removed from the inventory at that point. The quest is kept
        ///     around, so we know it was completed.
        /// </remarks>
        public Item QuestItem { get; set; }

        /// <summary>
        ///     Returns progress for objective.
        ///     Returns null if objective doesn't exist.
        /// </summary>
        /// <param name="objective"></param>
        /// <returns></returns>
        public QuestObjectiveProgress GetProgress(string objective)
        {
            QuestObjectiveProgress result;
            _progresses.TryGetValue(objective, out result);
            return result;
        }

        /// <summary>
        ///     Returns progress for objective by index.
        ///     Returns null if out of bounds.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public QuestObjectiveProgress GetProgress(int index)
        {
            if (index < 0 || index > _progresses.Count - 1)
                return null;

            return _progresses[index];
        }

        /// <summary>
        ///     Returns list of all objective progresses.
        /// </summary>
        /// <remarks>
        ///     Values in our generic OrderedDictioanry aleady creates a copy.
        /// </remarks>
        /// <returns></returns>
        public ICollection<QuestObjectiveProgress> GetList()
        {
            return _progresses.Values;
        }

        /// <summary>
        ///     Sets objective done and unlocks the next one.
        /// </summary>
        /// <param name="objective"></param>
        public void SetDone(string objective)
        {
            if (!_progresses.ContainsKey(objective))
                throw new Exception("SetDone: No progress found for objective '" + objective + "'.");

            for (var i = 0; i < _progresses.Count; ++i)
            {
                _progresses[i].Done = true;
                _progresses[i].Unlocked = false;
                if (_progresses[i].Count < Data.Objectives[i].Amount)
                    _progresses[i].Count = Data.Objectives[i].Amount;

                if (_progresses[i].Ident != objective)
                    continue;

                if (i + 1 < _progresses.Count)
                    _progresses[i + 1].Unlocked = true;
                break;
            }
        }

        /// <summary>
        ///     Sets objective undone.
        /// </summary>
        /// <param name="objective"></param>
        public void SetUndone(string objective)
        {
            if (!_progresses.ContainsKey(objective))
                throw new Exception("SetUndone: No progress found for objective '" + objective + "'.");

            for (var i = _progresses.Count - 1; i >= 0; --i)
            {
                _progresses[i].Done = false;
                _progresses[i].Unlocked = _progresses[i].Ident == objective;
                _progresses[i].Count = 0;

                if (_progresses[i].Unlocked)
                    break;
            }
        }

        /// <summary>
        ///     Sets all objectives done.
        /// </summary>
        public void CompleteAllObjectives()
        {
            var last = _progresses.Values.Last();
            SetDone(last.Ident);
        }

        /// <summary>
        ///     Returns how well the quest/objective has been done (so far).
        /// </summary>
        /// <remarks>
        ///     Only ever needed for PTJs? Ignore multi objective?
        /// </remarks>
        /// <returns></returns>
        public QuestResult GetResult()
        {
            var objective = CurrentObjectiveOrLast;
            var doneRate = 100f / Data.Objectives[objective.Ident].Amount * objective.Count;

            if (doneRate >= 100)
                return QuestResult.Perfect;
            if (doneRate >= 50)
                return QuestResult.Mid;
            if (doneRate > 0)
                return QuestResult.Low;

            return QuestResult.None;
        }
    }

    public class QuestObjectiveProgress
    {
        public QuestObjectiveProgress(string objective)
        {
            Ident = objective;
        }

        public string Ident { get; set; }
        public int Count { get; set; }
        public bool Done { get; set; }
        public bool Unlocked { get; set; }
    }

    /// <summary>
    ///     State of a quest
    /// </summary>
    public enum QuestState
    {
        InProgress,
        Complete
    }
}