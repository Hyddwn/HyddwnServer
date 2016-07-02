// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.World.Quests
{
	/// <summary>
	/// Represents a queued quest owl.
	/// </summary>
	public class QuestOwl
	{
		public int QuestId { get; private set; }
		public DateTime Arrival { get; private set; }

		public QuestOwl(int questId, DateTime arrival)
		{
			this.QuestId = questId;
			this.Arrival = arrival;
		}
	}
}
