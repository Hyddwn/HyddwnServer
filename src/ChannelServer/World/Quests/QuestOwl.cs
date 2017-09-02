// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.World.Quests
{
    /// <summary>
    ///     Represents a queued quest owl.
    /// </summary>
    public class QuestOwl
    {
        public QuestOwl(int questId, DateTime arrival)
        {
            QuestId = questId;
            Arrival = arrival;
        }

        public int QuestId { get; }
        public DateTime Arrival { get; }
    }
}