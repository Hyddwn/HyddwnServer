// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System;

namespace Aura.Channel.World.Entities
{
	public class PlayingInstrumentProp : Prop
	{
		public bool HasMML { get { return !string.IsNullOrWhiteSpace(this.CompressedMML); } }
		public string CompressedMML { get; set; }
		public int ScoreId { get; set; }
		public int Quality { get; set; }
		public InstrumentType Instrument { get; set; }
		public DateTime StartTime { get; set; }
		public long CreatureEntityId { get; set; }

		public PlayingInstrumentProp(int regionId, int x, int y)
			: base(45528, regionId, x, y, 0)
		{
		}
	}
}
