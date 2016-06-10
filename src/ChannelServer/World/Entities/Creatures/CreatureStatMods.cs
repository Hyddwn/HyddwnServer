// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using Aura.Mabi;
using System;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureStatMods
	{
		private readonly Creature _creature;
		private readonly Dictionary<Stat, List<StatMod>> _mods;
		private readonly Dictionary<Stat, float> _cache;

		public CreatureStatMods(Creature creature)
		{
			_creature = creature;
			_mods = new Dictionary<Stat, List<StatMod>>();
			_cache = new Dictionary<Stat, float>();
		}

		/// <summary>
		/// Returns true if any mod for the given source and ident exists.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		/// <returns></returns>
		public bool Has(StatModSource source, long ident)
		{
			lock (_mods)
			{
				foreach (var mods in _mods)
				{
					if (mods.Value.Any(a => a.Source == source && a.Ident == ident))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds stat mod.
		/// </summary>
		/// <param name="stat">Stat to change</param>
		/// <param name="value">Amount</param>
		/// <param name="source">What is changing the stat?</param>
		/// <param name="ident">Identificator for the source, eg skill or title id.</param>
		/// <param name="timeout">Time in seconds after which the mod is removed.</param>
		public void Add(Stat stat, float value, StatModSource source, long ident, int timeout = 0)
		{
			lock (_mods)
			{
				if (!_mods.ContainsKey(stat))
					_mods.Add(stat, new List<StatMod>(1));

				if (_mods[stat].Any(a => a.Source == source && a.Ident == ident))
					Log.Warning("StatMods.Add: Double stat mod for '{0}:{1}'.", source, ident);

				_mods[stat].Add(new StatMod(stat, value, source, ident, timeout));
			}

			this.UpdateCache(stat);
		}

		/// <summary>
		/// Removes stat mod.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		public void Remove(Stat stat, StatModSource source, SkillId ident)
		{
			this.Remove(stat, source, (long)ident);
		}

		/// <summary>
		/// Removes stat mod.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		public void Remove(Stat stat, StatModSource source, long ident)
		{
			lock (_mods)
			{
				if (!_mods.ContainsKey(stat))
					return;

				_mods[stat].RemoveAll(a => a.Source == source && a.Ident == ident);
			}

			this.UpdateCache(stat);
		}

		/// <summary>
		/// Removes all stat mods for source and ident.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="source"></param>
		/// <param name="ident"></param>
		public void Remove(StatModSource source, long ident)
		{
			lock (_mods)
			{
				foreach (var mod in _mods)
				{
					mod.Value.RemoveAll(a => a.Source == source && a.Ident == ident);
					this.UpdateCache(mod.Key);
				}
			}
		}

		/// <summary>
		/// Returns total stat mod for stat.
		/// </summary>
		/// <param name="stat"></param>
		/// <returns></returns>
		public float Get(Stat stat)
		{
			lock (_mods)
			{
				if (!_cache.ContainsKey(stat))
					return 0;

				return _cache[stat];
			}
		}

		/// <summary>
		/// Recalculates cached value for stat.
		/// </summary>
		/// <param name="stat"></param>
		private void UpdateCache(Stat stat)
		{
			lock (_cache)
				_cache[stat] = _mods[stat].Sum(a => a.Value);
		}

		/// <summary>
		/// Called once a second, updates stat mods with timeout.
		/// </summary>
		/// <param name="time"></param>
		public void OnSecondsTimeTick(ErinnTime time)
		{
			lock (_mods)
			{
				foreach (var mod in _mods)
				{
					var stat = mod.Key;
					var list = mod.Value;
					var creature = _creature;

					var count = mod.Value.RemoveAll(a => a.TimeoutReached);
					if (count != 0)
					{
						this.UpdateCache(stat);

						Send.StatUpdate(creature, StatUpdateType.Private, stat);
						if (stat >= Stat.Life && stat <= Stat.LifeMaxMod)
							Send.StatUpdate(creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax);
					}
				}
			}
		}
	}

	public class StatMod
	{
		public Stat Stat { get; protected set; }
		public float Value { get; protected set; }
		public StatModSource Source { get; protected set; }
		public long Ident { get; protected set; }

		public int Timeout { get; protected set; }
		public DateTime Start { get; protected set; }
		public DateTime End { get; protected set; }

		/// <summary>
		/// Returns true if stat mod reached timeout and should be removed.
		/// </summary>
		public bool TimeoutReached { get { return (this.Timeout > 0 && this.End < DateTime.Now); } }

		public StatMod(Stat stat, float value, StatModSource source, long ident, int timeout)
		{
			this.Stat = stat;
			this.Value = value;
			this.Source = source;
			this.Ident = ident;

			this.Timeout = timeout;
			this.Start = DateTime.Now;
			this.End = (this.Timeout > 0 ? this.Start.AddSeconds(this.Timeout) : DateTime.MaxValue);
		}
	}

	public enum StatModSource
	{
		Skill,
		SkillRank,
		Title,
		Equipment,
		TmpFood,
	}
}
