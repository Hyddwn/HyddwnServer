// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.GameEvents
{
	public class GlobalBonusManager
	{
		private List<GlobalBonus> _bonuses = new List<GlobalBonus>();

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
		/// Returns all bonuses for the given stat and the related event's
		/// names via out parameter.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="eventNames"></param>
		public float GetBonusMultiplier(GlobalBonusStat stat, out string eventNames)
		{
			var result = 0f;
			var names = new HashSet<string>();
			var found = 0;

			lock (_bonuses)
			{
				foreach (var bonus in _bonuses.Where(a => a.Stat == stat))
				{
					found++;
					result += bonus.Multiplier;
					if (!string.IsNullOrWhiteSpace(bonus.Name))
						names.Add(bonus.Name);
				}
			}

			if (found == 0)
			{
				eventNames = "";
				return 1;
			}

			eventNames = string.Join(", ", names);

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

	public enum GlobalBonusStat
	{
		Ap,
		CombatExp,
		QuestExp,
		SkillTraining,
		ItemDropRate,
		GoldDropRate,
		GoldDropAmount,
		LuckyFinishRate,
	}
}
