// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Scripting.Scripts;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Quests
{
    public class PtjTrackRecord
    {
        /// <summary>
        ///     Creates new track record.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="done"></param>
        /// <param name="success"></param>
        /// <param name="last"></param>
        public PtjTrackRecord(PtjType type, int done, int success, DateTime last)
        {
            Type = type;
            Done = done;
            Success = success;
            LastChange = last;
        }

        /// <summary>
        ///     The type of PTJ
        /// </summary>
        public PtjType Type { get; set; }

        /// <summary>
        ///     How many times the player reported, no matter the outcome
        /// </summary>
        public int Done { get; set; }

        /// <summary>
        ///     How many times the PTJ was successful
        /// </summary>
        public int Success { get; set; }

        /// <summary>
        ///     When the player reported last.
        /// </summary>
        public DateTime LastChange { get; set; }

        /// <summary>
        ///     PTJ success rate
        /// </summary>
        public float SuccessRate => Done == 0 ? 0 : 100f / Done * Success;

        /// <summary>
        ///     Calculates quest level, based on success rate.
        /// </summary>
        /// <returns></returns>
        public QuestLevel GetQuestLevel()
        {
            var successRate = SuccessRate;

            if (Done > 100 && successRate >= 92)
                return QuestLevel.Adv;

            if (Done > 50 && successRate >= 70)
                return QuestLevel.Int;

            return QuestLevel.Basic;
        }
    }
}