// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.GameEvents
{
	public class GlobalBonusManager
	{
		private List<GlobalBonus> _bonuses = new List<GlobalBonus>();
		private List<GlobalDrop> _drops = new List<GlobalDrop>();
		private List<GlobalFishingGround> _fishingGrounds = new List<GlobalFishingGround>();

		/// <summary>
		/// Adds global bonus.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="name"></param>
		/// <param name="stat"></param>
		/// <param name="multiplier"></param>
		public void AddBonus(string identifier, string name, GlobalBonusStat stat, float multiplier)
		{
			var bonus = new GlobalBonus(identifier, name, stat, multiplier);
			lock (_bonuses)
				_bonuses.Add(bonus);
		}

		/// <summary>
		/// Removes all global bonuses with given event id.
		/// </summary>
		/// <param name="identifier"></param>
		public void RemoveBonuses(string identifier)
		{
			lock (_bonuses)
				_bonuses.RemoveAll(a => a.Identifier == identifier);
		}

		/// <summary>
		/// Returns whether there are any bonuses for the given stat,
		/// and if so returns the total multiplier and the names of the
		/// events that affected it via out parameter.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="multiplier"></param>
		/// <param name="eventNames"></param>
		public bool GetBonusMultiplier(GlobalBonusStat stat, out float multiplier, out string eventNames)
		{
			multiplier = 0;
			eventNames = "";

			lock (_bonuses)
			{
				if (!_bonuses.Any(a => a.Stat == stat))
					return false;
			}

			var names = new HashSet<string>();

			lock (_bonuses)
			{
				foreach (var bonus in _bonuses.Where(a => a.Stat == stat))
				{
					multiplier += bonus.Multiplier;
					if (!string.IsNullOrWhiteSpace(bonus.Name))
						names.Add(bonus.Name);
				}
			}

			eventNames = string.Join(", ", names);

			return true;
		}

		/// <summary>
		/// Adds global drop.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="drop"></param>
		public void AddDrop(string identifier, GlobalDrop drop)
		{
			lock (_drops)
				_drops.Add(drop);
		}

		/// <summary>
		/// Removes all global drops associated with the given identifier.
		/// </summary>
		/// <param name="identifier"></param>
		public void RemoveAllDrops(string identifier)
		{
			lock (_drops)
				_drops.RemoveAll(a => a.Identifier == identifier);
		}

		/// <summary>
		/// Returns list of all global drops the given creature might drop.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public List<DropData> GetDrops(Creature creature)
		{
			var result = new List<DropData>();

			lock (_drops)
				result.AddRange(_drops.Where(a => a.Matches(creature)).Select(a => a.Data));

			return result;
		}

		/// <summary>
		/// Adds event fishing ground.
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="drop"></param>
		public void AddFishingGround(string identifier, FishingGroundData fishingGroundData)
		{
			lock (_fishingGrounds)
				_fishingGrounds.Add(new GlobalFishingGround(identifier, fishingGroundData));
		}

		/// <summary>
		/// Removes all event fishing grounds associated with the given
		/// identifier.
		/// </summary>
		/// <param name="identifier"></param>
		public void RemoveAllFishingGrounds(string identifier)
		{
			lock (_fishingGrounds)
				_fishingGrounds.RemoveAll(a => a.Identifier == identifier);
		}

		/// <summary>
		/// Returns a list of all event fishing grounds.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="fishingGroundData"></param>
		/// <returns></returns>
		public List<FishingGroundData> GetFishingGrounds()
		{
			var result = new List<FishingGroundData>();

			lock (_fishingGrounds)
				result.AddRange(_fishingGrounds.Select(a => a.Data));

			return result;
		}
	}

	public class GlobalBonus
	{
		public string Identifier { get; private set; }
		public string Name { get; private set; }
		public GlobalBonusStat Stat { get; private set; }
		public float Multiplier { get; private set; }

		public GlobalBonus(string identifier, string name, GlobalBonusStat stat, float multiplier)
		{
			this.Identifier = identifier;
			this.Name = name;
			this.Stat = stat;
			this.Multiplier = multiplier;
		}
	}

	public abstract class GlobalDrop
	{
		public string Identifier { get; private set; }
		public DropData Data { get; private set; }

		public GlobalDrop(string identifier, DropData data)
		{
			this.Identifier = identifier;
			this.Data = data;
		}

		public abstract bool Matches(Creature creature);
	}

	public class GlobalDropById : GlobalDrop
	{
		public int RaceId { get; private set; }

		public GlobalDropById(string identifier, int raceId, DropData data)
			: base(identifier, data)
		{
			this.RaceId = raceId;
		}

		public override bool Matches(Creature creature)
		{
			var isRace = (creature.RaceId == this.RaceId);
			return isRace;
		}
	}

	public class GlobalDropByTag : GlobalDrop
	{
		public string Tag { get; private set; }

		public GlobalDropByTag(string identifier, string tag, DropData data)
			: base(identifier, data)
		{
			this.Tag = tag;
		}

		public override bool Matches(Creature creature)
		{
			var isTag = (creature.HasTag(this.Tag));
			return isTag;
		}
	}

	public class GlobalDropByType : GlobalDrop
	{
		public GlobalDropType Type { get; private set; }

		public GlobalDropByType(string identifier, GlobalDropType type, DropData data)
			: base(identifier, data)
		{
			this.Type = type;
		}

		public override bool Matches(Creature creature)
		{
			switch (this.Type)
			{
				case GlobalDropType.Npcs:
					var isMonster = creature.Has(CreatureStates.Npc);
					return isMonster;

				case GlobalDropType.Players:
					var isPlayer = (creature.IsPlayer);
					return isPlayer;
			}

			return false;
		}
	}

	public class GlobalFishingGround
	{
		public string Identifier { get; private set; }
		public FishingGroundData Data { get; private set; }

		public GlobalFishingGround(string identifier, FishingGroundData data)
		{
			this.Identifier = identifier;
			this.Data = data;
		}
	}

	public enum GlobalBonusStat
	{
		LevelUpAp,
		CombatExp,
		QuestExp,
		SkillTraining,
		ItemDropRate,
		GoldDropRate,
		GoldDropAmount,
		LuckyFinishRate,
	}

	public enum GlobalDropType
	{
		Npcs,
		Players,
	}
}
