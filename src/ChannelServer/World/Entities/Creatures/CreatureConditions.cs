// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Mabi;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
    /// <summary>
    ///     Holds all conditions of a creature.
    /// </summary>
    /// <remarks>
    ///     "Extra" values are information about conditions that are stored in
    ///     MabiDictionaries, they appear after the actual conditions in the
    ///     ConditionUpdate packet. An example of such a condition is ConditionsC.Hurry,
    ///     which is used to modify your movement speed.
    ///     To set those values, prepare a MabiDictionary before calling Activate
    ///     and pass it as the optional "extra" value. They are removed
    ///     automatically on deactivating.
    /// </remarks>
    /// <example>
    ///     creature.Conditions.Activate(ConditionsA.Petrified);
    ///     var extra = new MabiDictionary();
    ///     extra.SetShort("VAL", speedBonus);
    ///     creature.Conditions.Activate(ConditionsC.Hurry, extra);
    /// </example>
    public class CreatureConditions
    {
        private readonly Creature _creature;
        private readonly Dictionary<int, DateTime> _durations;

        private readonly Dictionary<int, MabiDictionary> _extra;
        private ICollection<KeyValuePair<int, MabiDictionary>> _extraCache;

        public CreatureConditions(Creature creature)
        {
            _creature = creature;
            _extra = new Dictionary<int, MabiDictionary>();
            _durations = new Dictionary<int, DateTime>();
        }

        public ConditionsA A { get; private set; }
        public ConditionsB B { get; private set; }
        public ConditionsC C { get; private set; }
        public ConditionsD D { get; private set; }
        public ConditionsE E { get; private set; }
        public ConditionsF F { get; private set; }
        public ConditionsG G { get; private set; }

        /// <summary>
        ///     Raised when any conditions change.
        /// </summary>
        public event Action<Creature> Changed;

        public bool Has(ConditionsA condition)
        {
            return (A & condition) != 0;
        }

        public bool Has(ConditionsB condition)
        {
            return (B & condition) != 0;
        }

        public bool Has(ConditionsC condition)
        {
            return (C & condition) != 0;
        }

        public bool Has(ConditionsD condition)
        {
            return (D & condition) != 0;
        }

        public bool Has(ConditionsE condition)
        {
            return (E & condition) != 0;
        }

        public bool Has(ConditionsF condition)
        {
            return (F & condition) != 0;
        }

        public bool Has(ConditionsG condition)
        {
            return (G & condition) != 0;
        }

        public bool Has(int condition)
        {
            var c = 1L << (condition % 64);

            if (condition < 64 * 1)
                return Has((ConditionsA) c);
            if (condition < 64 * 2)
                return Has((ConditionsB) c);
            if (condition < 64 * 3)
                return Has((ConditionsC) c);
            if (condition < 64 * 4)
                return Has((ConditionsD) c);
            if (condition < 64 * 5)
                return Has((ConditionsE) c);
            if (condition < 64 * 6)
                return Has((ConditionsF) c);
            if (condition < 64 * 7)
                return Has((ConditionsG) c);

            throw new ArgumentException("Condition " + condition + " is outside of the known ones.");
        }

        public void Activate(ConditionsA condition, MabiDictionary extra = null, int duration = -1)
        {
            A |= condition;
            if (extra != null) SetExtra((double) condition, 0, extra);
            if (duration > 0) SetDuration((double) condition, 0, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Activate(ConditionsB condition, MabiDictionary extra = null, int duration = -1)
        {
            B |= condition;
            if (extra != null) SetExtra((double) condition, 1, extra);
            if (duration > 0) SetDuration((double) condition, 1, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Activate(ConditionsC condition, MabiDictionary extra = null, int duration = -1)
        {
            C |= condition;
            if (extra != null) SetExtra((double) condition, 2, extra);
            if (duration > 0) SetDuration((double) condition, 2, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Activate(ConditionsD condition, MabiDictionary extra = null, int duration = -1)
        {
            D |= condition;
            if (extra != null) SetExtra((double) condition, 3, extra);
            if (duration > 0) SetDuration((double) condition, 3, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Activate(ConditionsE condition, MabiDictionary extra = null, int duration = -1)
        {
            E |= condition;
            if (extra != null) SetExtra((double) condition, 4, extra);
            if (duration > 0) SetDuration((double) condition, 4, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Activate(ConditionsF condition, MabiDictionary extra = null, int duration = -1)
        {
            F |= condition;
            if (extra != null) SetExtra((double) condition, 5, extra);
            if (duration > 0) SetDuration((double) condition, 5, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Activate(ConditionsG condition, MabiDictionary extra = null, int duration = -1)
        {
            G |= condition;
            if (extra != null) SetExtra((double) condition, 6, extra);
            if (duration > 0) SetDuration((double) condition, 6, duration);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsA condition)
        {
            A &= ~condition;
            RemoveExtra((double) condition, 0);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsB condition)
        {
            B &= ~condition;
            RemoveExtra((double) condition, 1);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsC condition)
        {
            C &= ~condition;
            RemoveExtra((double) condition, 2);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsD condition)
        {
            D &= ~condition;
            RemoveExtra((double) condition, 3);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsE condition)
        {
            E &= ~condition;
            RemoveExtra((double) condition, 4);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsF condition)
        {
            F &= ~condition;
            RemoveExtra((double) condition, 5);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(ConditionsG condition)
        {
            G &= ~condition;
            RemoveExtra((double) condition, 6);
            Send.ConditionUpdate(_creature);
            Changed.Raise(_creature);
        }

        public void Deactivate(int condition)
        {
            var c = 1L << (condition % 64);

            if (condition < 64 * 1)
                Deactivate((ConditionsA) c);
            else if (condition < 64 * 2)
                Deactivate((ConditionsB) c);
            else if (condition < 64 * 3)
                Deactivate((ConditionsC) c);
            else if (condition < 64 * 4)
                Deactivate((ConditionsD) c);
            else if (condition < 64 * 5)
                Deactivate((ConditionsE) c);
            else if (condition < 64 * 6)
                Deactivate((ConditionsF) c);
            else if (condition < 64 * 7)
                Deactivate((ConditionsG) c);
        }

        private void SetDuration(double condition, int offset, int milliseconds)
        {
            var id = (int) Math.Log(condition, 2) + 64 * offset;
            lock (_durations)
            {
                _durations[id] = DateTime.Now.AddMilliseconds(milliseconds);
            }
        }

        /// <summary>
        ///     Removes overdue conditions.
        /// </summary>
        /// <param name="time"></param>
        public void OnSecondsTimeTick(ErinnTime time)
        {
            lock (_durations)
            {
                var deactivate = _durations.Where(a => time.DateTime > a.Value).Select(a => a.Key).ToArray();

                foreach (var conditionId in deactivate)
                {
                    Deactivate(conditionId);
                    _durations.Remove(conditionId);
                }
            }
        }

        private void SetExtra(double condition, int offset, MabiDictionary extra)
        {
            var id = (int) Math.Log(condition, 2) + 64 * offset;
            lock (_extra)
            {
                _extra[id] = extra;
            }

            _extraCache = null;
        }

        private void RemoveExtra(double condition, int offset)
        {
            var id = (int) Math.Log(condition, 2) + 64 * offset;
            lock (_extra)
            {
                if (_extra.ContainsKey(id))
                    _extra.Remove(id);
            }

            _extraCache = null;
        }

        /// <summary>
        ///     Resets all conditions and sends update.
        /// </summary>
        public void Clear()
        {
            A = 0;
            B = 0;
            C = 0;
            D = 0;
            E = 0;
            F = 0;
            G = 0;
            lock (_extra)
            {
                _extra.Clear();
            }
            _extraCache = null;

            Send.ConditionUpdate(_creature);
        }

        /// <summary>
        ///     Returns new list of all extra values.
        /// </summary>
        /// <returns></returns>
        public ICollection<KeyValuePair<int, MabiDictionary>> GetExtraList()
        {
            if (_extraCache != null)
                return _extraCache;

            lock (_extra)
            {
                return _extraCache = _extra.ToArray();
            }
        }

        /// <summary>
        ///     Returns extra val for id, or 0.
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

        /// <summary>
        ///     Returns extra data for id, or null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetExtraField(int id, string name)
        {
            lock (_extra)
            {
                if (!_extra.ContainsKey(id))
                    return null;

                var value = _extra[id].Get(name);
                return value;
            }
        }

        public override string ToString()
        {
            return "(" + A + " ; " + B + " ; " + C + " ; " + D + " ; " + E + " ; " + F + " ; " + G + ")";
        }
    }
}