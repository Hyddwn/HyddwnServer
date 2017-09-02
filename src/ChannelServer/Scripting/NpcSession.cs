// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting
{
    /// <summary>
    ///     NPC converstation session
    /// </summary>
    public class NpcSession
    {
        /// <summary>
        ///     Creatures new session.
        /// </summary>
        public NpcSession()
        {
            // We'll only set this once for every char, for the entire session.
            // In some cases the client doesn't seem to take the new id,
            // which results in a mismatch.
            Id = RandomProvider.Get().Next(1, 5000);
        }

        /// <summary>
        ///     NPC the player is talking to
        /// </summary>
        public NPC Target { get; private set; }

        /// <summary>
        ///     Unique session id
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Current used NPC script
        /// </summary>
        public NpcScript Script { get; set; }

        /// <summary>
        ///     Starts a new session and calls Talk.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="creature"></param>
        public void StartTalk(NPC target, Creature creature)
        {
            if (!Start(target, creature))
                return;

            Script.TalkAsync();
        }


        /// <summary>
        ///     Starts a new session and calls Gift.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="creature"></param>
        /// <param name="gift"></param>
        public void StartGift(NPC target, Creature creature, Item gift)
        {
            if (!Start(target, creature))
                return;

            Script.GiftAsync(gift);
        }

        /// <summary>
        ///     Starts session
        /// </summary>
        /// <param name="target"></param>
        /// <param name="creature"></param>
        private bool Start(NPC target, Creature creature)
        {
            Target = target;

            if (target.ScriptType == null)
                return false;

            var script = Activator.CreateInstance(target.ScriptType) as NpcScript;
            script.NPC = target;
            script.Player = creature;
            Script = script;
            return true;
        }

        /// <summary>
        ///     Cancels script and resets session.
        /// </summary>
        public void Clear()
        {
            if (Script != null)
                Script.Cancel();
            Script = null;
            Target = null;
        }

        /// <summary>
        ///     Returns true if there is a state and target's id is npcId.
        /// </summary>
        public bool IsValid(long npcId)
        {
            return IsValid() && Target.EntityId == npcId;
        }

        /// <summary>
        ///     Returns true if there is a state and a target.
        /// </summary>
        public bool IsValid()
        {
            return Target != null && Script != null;
        }

        /// <summary>
        ///     Checks <see cref="IsValid(long)" />. If false, throws <see cref="ModerateViolation" />.
        /// </summary>
        public void EnsureValid(long npcId)
        {
            if (!IsValid(npcId))
                throw new ModerateViolation("Invalid NPC session");
        }

        /// <summary>
        ///     Checks <see cref="IsValid()" />. If false, throws <see cref="ModerateViolation" />.
        /// </summary>
        public void EnsureValid()
        {
            if (!IsValid())
                throw new ModerateViolation("Invalid NPC session");
        }
    }
}