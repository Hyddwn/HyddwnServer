// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.Entities.Creatures
{
    public class CreatureCoolDowns
    {
        private readonly Dictionary<string, DateTime> _coolDowns = new Dictionary<string, DateTime>();

        /// <summary>
        ///     Creates a new instance for creature.
        /// </summary>
        /// <param name="creature"></param>
        public CreatureCoolDowns(Creature creature)
        {
            Creature = creature;
        }

        /// <summary>
        ///     Owner of this instance.
        /// </summary>
        public Creature Creature { get; }

        /// <summary>
        ///     Adds cool down.
        /// </summary>
        /// <remarks>
        ///     A string is used as the identifier, for maximum flexibility.
        ///     Because of how easy it is to run into duplicates this way
        ///     however, one has to be careful and use appropriate prefixes.
        /// </remarks>
        /// <param name="identifier">Identifier for this specific cool down.</param>
        /// <param name="coolDownEnd">Time at which the cool down ends.</param>
        public void Add(string identifier, DateTime coolDownEnd)
        {
            var now = DateTime.Now;

            // Check if end lies in the past
            if (coolDownEnd < now)
                return;

            lock (_coolDowns)
            {
                _coolDowns[identifier] = coolDownEnd;
            }
        }

        /// <summary>
        ///     Removes the cool down with the given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        public void Reset(string identifier)
        {
            lock (_coolDowns)
            {
                _coolDowns.Remove(identifier);
            }
        }

        /// <summary>
        ///     Returns true if a cool down for the given identifier is
        ///     currently active.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool IsCoolingDown(string identifier)
        {
            lock (_coolDowns)
            {
                // Check existence
                if (!_coolDowns.ContainsKey(identifier))
                    return false;

                // Check time
                var end = _coolDowns[identifier];
                var now = DateTime.Now;

                if (now < end)
                    return true;

                // If cool down is past its end, remove it
                _coolDowns.Remove(identifier);
            }

            return false;
        }

        /// <summary>
        ///     Returns list of all active cool downs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, DateTime>> GetList()
        {
            var now = DateTime.Now;

            lock (_coolDowns)
            {
                return _coolDowns.Where(a => a.Value > now).ToList();
            }
        }
    }
}