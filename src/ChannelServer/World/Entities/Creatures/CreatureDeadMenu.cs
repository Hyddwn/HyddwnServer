// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Text;
using Aura.Mabi.Const;
using Aura.Channel.World.Dungeons;
using Aura.Channel.Network.Sending;
using Aura.Data;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureDeadMenu
	{
		public Creature Creature { get; private set; }

		public ReviveOptions Options { get; set; }

		public CreatureDeadMenu(Creature creature)
		{
			this.Creature = creature;
		}

		public void Add(ReviveOptions option)
		{
			this.Options |= option;
		}

		public bool Has(ReviveOptions option)
		{
			return ((this.Options & option) != 0);
		}

		public void Clear()
		{
			this.Options = ReviveOptions.None;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (this.Has(ReviveOptions.ArenaLobby))
				sb.Append("arena_lobby;");
			if (this.Has(ReviveOptions.ArenaSide))
				sb.Append("arena_side;");
			if (this.Has(ReviveOptions.ArenaWaitingRoom))
				sb.Append("arena_waiting;");
			if (this.Has(ReviveOptions.TirNaNog))
				sb.Append("tirnanog;");
			if (this.Has(ReviveOptions.BarriLobby))
				sb.Append("barri_lobby;");
			if (this.Has(ReviveOptions.NaoStone))
				sb.Append("naocoupon;");
			if (this.Has(ReviveOptions.DungeonEntrance))
				sb.Append("dungeon_lobby;");
			if (this.Has(ReviveOptions.Here))
				sb.Append("here;");
			if (this.Has(ReviveOptions.HereNoPenalty))
				sb.Append("trnsfrm_pvp_here;");
			if (this.Has(ReviveOptions.HerePvP))
				sb.Append("showdown_pvp_here;");
			if (this.Has(ReviveOptions.InCamp))
				sb.Append("camp;");
			if (this.Has(ReviveOptions.StatueOfGoddess))
				sb.Append("dungeon_statue;");
			if (this.Has(ReviveOptions.TirChonaill))
				sb.Append("tirchonaill;");
			if (this.Has(ReviveOptions.Town))
				sb.Append("town;");
			if (this.Has(ReviveOptions.WaitForRescue))
				sb.Append("stay;");

			return sb.ToString().Trim(';');
		}

		/// <summary>
		/// Updates menu, based on where its creature is and updates
		/// nearby clients.
		/// </summary>
		public void Update()
		{
			var before = this.Options;

			this.Clear();

			if (this.Creature.IsDead)
			{
				var isInTirNaNog = this.Creature.IsInTirNaNog;

				// Defaults
				this.Add(ReviveOptions.WaitForRescue);
				if (this.Creature.IsPet)
					this.Add(ReviveOptions.PhoenixFeather);

				// Town
				if (!isInTirNaNog)
				{
					this.Add(ReviveOptions.Town);
				}
				// Tir Na Nog
				else
				{
					// Use TNN or Barri, depending on whether the bind quest
					// was done.
					if (this.Creature.Keywords.Has("g1_bind"))
						this.Add(ReviveOptions.TirNaNog);
					else
						this.Add(ReviveOptions.BarriLobby);
				}

				// Nao Stone option if it's enabled, and creature is not
				// in Tir Na Nog.
				if (AuraData.FeaturesDb.IsEnabled("NaoCoupon") && !isInTirNaNog)
					this.Add(ReviveOptions.NaoStone);

				// Dungeons
				if (this.Creature.Region is DungeonRegion)
				{
					this.Add(ReviveOptions.DungeonEntrance);

					// Show statue option only if there is a statue on this floor
					var floorRegion = (this.Creature.Region as DungeonFloorRegion);
					if (floorRegion == null || floorRegion.Floor.Statue)
						this.Add(ReviveOptions.StatueOfGoddess);
				}
				// Fields
				else
				{
					if (!isInTirNaNog)
						this.Add(ReviveOptions.Here);
				}

				// Special
				if (this.Creature.Titles.SelectedTitle == TitleId.devCAT)
					this.Add(ReviveOptions.HereNoPenalty);
			}

			if (this.Options != before || this.Creature.IsDead)
				Send.DeadFeather(this.Creature);
		}

		/// <summary>
		/// Returns exp penalty for given level and option.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="option"></param>
		/// <returns></returns>
		public int GetExpPenalty(int level, ReviveOptions option)
		{
			float optionMultiplicator;
			switch (option)
			{
				// Check both Nao Stone options for exp, as the exp aren't
				// reduced until the second option gets in.
				case ReviveOptions.NaoStone: optionMultiplicator = 0.001f; break;
				case ReviveOptions.NaoStoneRevive: optionMultiplicator = 0.001f; break;

				case ReviveOptions.PhoenixFeather: optionMultiplicator = 0.0025f; break;
				case ReviveOptions.DungeonEntrance: optionMultiplicator = 0.005f; break;
				case ReviveOptions.StatueOfGoddess: optionMultiplicator = 0.0075f; break;
				case ReviveOptions.Here: optionMultiplicator = 0.01f; break;

				case ReviveOptions.Town:
					var premium = true;
					if (premium)
						goto default;
					optionMultiplicator = 0.005f; break;

				default: return 0;
			}

			float levelMultiplicator;
			if (level < 10) levelMultiplicator = 1;
			else if (level < 30) levelMultiplicator = 3;
			else if (level < 50) levelMultiplicator = 4;
			else if (level < 100) levelMultiplicator = 5;
			else levelMultiplicator = 6;

			var exp = AuraData.ExpDb.GetForLevel(level);

			return (int)(exp * optionMultiplicator * levelMultiplicator);
		}
	}
}
