// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;

namespace Aura.Channel.World
{
	/// <summary>
	/// Tracks damage and amount of hits done by one creature to another.
	/// </summary>
	public class HitTracker
	{
		/// <summary>
		/// Id of this tracker, unique to one creature.
		/// </summary>
		public long Id { get; private set; }

		/// <summary>
		/// The creature using this tracker.
		/// </summary>
		public Creature Target { get; private set; }

		/// <summary>
		/// The creature who's hits are tracked.
		/// </summary>
		public Creature Attacker { get; private set; }

		/// <summary>
		/// The total damage done by the attacker.
		/// </summary>
		public float Damage { get; private set; }

		/// <summary>
		/// The total hits done by the attacker.
		/// </summary>
		public int Hits { get; private set; }

		/// <summary>
		/// Creates new hit tracker for target, for hits by attacker.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="attacker"></param>
		public HitTracker(long id, Creature target, Creature attacker)
		{
			this.Id = id;
			this.Target = target;
			this.Attacker = attacker;
		}

		/// <summary>
		/// Increases hit count and damage.
		/// </summary>
		/// <param name="damage"></param>
		public void RegisterHit(float damage)
		{
			this.Hits++;
			this.Damage += damage;
		}
	}
}
