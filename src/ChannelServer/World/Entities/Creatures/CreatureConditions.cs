// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Mabi;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.Entities.Creatures
{
	/// <summary>
	/// Holds all conditions of a creature.
	/// </summary>
	/// <remarks>
	/// "Extra" values are information about conditions that are stored in
	/// MabiDictionaries, they appear after the actual conditions in the
	/// ConditionUpdate packet. An example of such a condition is ConditionsC.Hurry,
	/// which is used to modify your movement speed.
	/// To set those values, prepare a MabiDictionary before calling Activate
	/// and pass it as the optional "extra" value. They are removed
	/// automatically on deactivating.
	/// </remarks>
	/// <example>
	/// creature.Conditions.Activate(ConditionsA.Petrified);
	/// 
	/// var extra = new MabiDictionary();
	/// extra.SetShort("VAL", speedBonus);
	/// creature.Conditions.Activate(ConditionsC.Hurry, extra);
	/// </example>
	public class CreatureConditions
	{
		private Creature _creature;

		private Dictionary<int, MabiDictionary> _extra;
		private ICollection<KeyValuePair<int, MabiDictionary>> _extraCache;

		public ConditionsA A { get; private set; }
		public ConditionsB B { get; private set; }
		public ConditionsC C { get; private set; }
		public ConditionsD D { get; private set; }
		public ConditionsE E { get; private set; }
		public ConditionsF F { get; private set; }

		public CreatureConditions(Creature creature)
		{
			_creature = creature;
			_extra = new Dictionary<int, MabiDictionary>();
		}

		public bool Has(ConditionsA condition) { return ((this.A & condition) != 0); }
		public bool Has(ConditionsB condition) { return ((this.B & condition) != 0); }
		public bool Has(ConditionsC condition) { return ((this.C & condition) != 0); }
		public bool Has(ConditionsD condition) { return ((this.D & condition) != 0); }
		public bool Has(ConditionsE condition) { return ((this.E & condition) != 0); }
		public bool Has(ConditionsF condition) { return ((this.F & condition) != 0); }

		public void Activate(ConditionsA condition, MabiDictionary extra = null) { this.A |= condition; if (extra != null) this.SetExtra((double)condition, 0, extra); Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsB condition, MabiDictionary extra = null) { this.B |= condition; if (extra != null) this.SetExtra((double)condition, 1, extra); Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsC condition, MabiDictionary extra = null) { this.C |= condition; if (extra != null) this.SetExtra((double)condition, 2, extra); Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsD condition, MabiDictionary extra = null) { this.D |= condition; if (extra != null) this.SetExtra((double)condition, 3, extra); Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsE condition, MabiDictionary extra = null) { this.E |= condition; if (extra != null) this.SetExtra((double)condition, 4, extra); Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsF condition, MabiDictionary extra = null) { this.F |= condition; if (extra != null) this.SetExtra((double)condition, 5, extra); Send.ConditionUpdate(_creature); }

		public void Deactivate(ConditionsA condition) { this.A &= ~condition; this.RemoveExtra((double)condition, 0); Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsB condition) { this.B &= ~condition; this.RemoveExtra((double)condition, 1); Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsC condition) { this.C &= ~condition; this.RemoveExtra((double)condition, 2); Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsD condition) { this.D &= ~condition; this.RemoveExtra((double)condition, 3); Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsE condition) { this.E &= ~condition; this.RemoveExtra((double)condition, 4); Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsF condition) { this.F &= ~condition; this.RemoveExtra((double)condition, 5); Send.ConditionUpdate(_creature); }

		private void SetExtra(double condition, int offset, MabiDictionary extra)
		{
			var id = (int)Math.Log(condition, 2) + (64 * offset);
			lock (_extra)
				_extra[id] = extra;

			_extraCache = null;
		}

		private void RemoveExtra(double condition, int offset)
		{
			var id = (int)Math.Log(condition, 2) + (64 * offset);
			lock (_extra)
			{
				if (_extra.ContainsKey(id))
					_extra.Remove(id);
			}

			_extraCache = null;
		}

		/// <summary>
		/// Resets all conditions and sends update.
		/// </summary>
		public void Clear()
		{
			this.A = 0;
			this.B = 0;
			this.C = 0;
			this.D = 0;
			this.E = 0;
			this.F = 0;
			lock (_extra)
				_extra.Clear();
			_extraCache = null;

			Send.ConditionUpdate(_creature);
		}

		/// <summary>
		/// Returns new list of all extra values.
		/// </summary>
		/// <returns></returns>
		public ICollection<KeyValuePair<int, MabiDictionary>> GetExtraList()
		{
			if (_extraCache != null)
				return _extraCache;

			lock (_extra)
				return (_extraCache = _extra.ToArray());
		}

		/// <summary>
		/// Returns extra val for id, or 0.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public short GetExtraVal(int id)
		{
			lock (_extra)
			{
				if (!_extra.ContainsKey(id))
					return 0;
				return _extra[id].GetShort("VAL");
			}
		}

		public override string ToString()
		{
			return ("(" + this.A + " ; " + this.B + " ; " + this.C + " ; " + this.D + " ; " + this.E + " ; " + this.F + ")");
		}
	}
}
