// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Network.Sending;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities.Creatures
{
    /// <summary>
    ///     Aim meter for ranged combat.
    /// </summary>
    public class AimMeter
    {
        /// <summary>
        ///     Maximum aim for walking target
        /// </summary>
        private const int MaxChanceWalking = 95;

        /// <summary>
        ///     Maximum aim for running target
        /// </summary>
        private const int MaxChanceRunning = 90;

        /// <summary>
        ///     Aim offset for an elf.
        /// </summary>
        private float _aimOffset;

        /// <summary>
        ///     Creatues new aim meter for creature.
        /// </summary>
        /// <param name="creature"></param>
        public AimMeter(Creature creature)
        {
            StartTime = DateTime.MinValue;

            Creature = creature;
        }

        /// <summary>
        ///     Creature this aim meter belongs to.
        /// </summary>
        public Creature Creature { get; }

        /// <summary>
        ///     Time at which aiming was started, equal to MinValue if not aiming.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        ///     Returns true if meter's creature is aiming.
        /// </summary>
        public bool IsAiming => StartTime != DateTime.MinValue;

        /// <summary>
        ///     Starts aiming timer and sends CombatSetAimR.
        /// </summary>
        /// <param name="targetEntityId"></param>
        /// <param name="flag"></param>
        public void Start(long targetEntityId, byte flag = 0)
        {
            // Use 0 as fallback for now, until we're sure there's no
            // "no skill" ranged.
            var activeSkillId = Creature.Skills.ActiveSkill == null ? 0 : Creature.Skills.ActiveSkill.Info.Id;

            if (flag > 0 && Creature.IsElf)
            {
                var chance = GetAimChance(Creature.Region.GetCreature(targetEntityId));
                if (chance > 50f)
                {
                    _aimOffset = 0.5f;
                    StartTime = DateTime.Now;
                }
            }
            else
            {
                Creature.StopMove();
                _aimOffset = 0f;
                StartTime = DateTime.Now;
            }
            Send.CombatSetAimR(Creature, targetEntityId, activeSkillId, flag);
        }

        /// <summary>
        ///     Stops aiming timer and sends CombatSetAimR.
        /// </summary>
        /// <param name="flag"></param>
        public void Stop(byte flag = 0)
        {
            StartTime = DateTime.MinValue;
            Send.CombatSetAimR(Creature, 0, SkillId.None, flag);
        }

        /// <summary>
        ///     Returns the time since Start was called.
        /// </summary>
        /// <returns></returns>
        public double GetAimTime()
        {
            if (StartTime == DateTime.MinValue)
                return 0;

            return (DateTime.Now - StartTime).TotalMilliseconds;
        }

        /// <summary>
        ///     Returns the chance to hit target at the current aim time.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double GetAimChance(Creature target)
        {
            // Check collision, 0 chance if the client didn't prevent this shot.
            if (Creature.Region.Collisions.Any(Creature.GetPosition(), target.GetPosition()))
            {
                // Only warn, could be caused by lag.
                Log.Warning("GetAimChance: Creature '{0:X16}' tried to shoot through a wall.", Creature.EntityId);
                return 0;
            }

            var activeSkill = Creature.Skills.ActiveSkill;
            var activeSkillId = activeSkill == null ? SkillId.None : activeSkill.Info.Id;

            var d1 = 5000.0;
            var d2 = 500.0;

            if (activeSkill != null && activeSkill.Info.Id == SkillId.MagnumShot)
            {
                d1 = 8000.0;
                d2 = 1000.0;
            }

            var distance = Creature.GetPosition().GetDistance(target.GetPosition());
            var bowRange = Creature.RightHand == null ? 0 : Creature.RightHand.OptionInfo.EffectiveRange;

            if (distance > bowRange || distance <= 0)
                return 0;

            var aimTime = GetAimTime();
            var aimMod = aimTime;

            // Bonus for ranged attack
            if (activeSkillId == SkillId.RangedAttack || activeSkillId == SkillId.SupportShot ||
                activeSkillId == SkillId.ArrowRevolver || activeSkillId == SkillId.ArrowRevolver2)
            {
                var rangedSkill = Creature.Skills.Get(SkillId.RangedAttack);
                if (rangedSkill != null)
                    aimMod *= rangedSkill.RankData.Var3 / 100f;
            }

            var hitRatio = 1.0;
            hitRatio = (d1 - d2) / bowRange * distance * hitRatio + d2;

            var chance = Math.Sqrt(_aimOffset * _aimOffset + aimMod / hitRatio) * 100f;

            // Arrow Revolver bonus
            if (activeSkillId == SkillId.ArrowRevolver2 && activeSkill.Stacks == activeSkill.RankData.StackMax)
                chance += activeSkill.RankData.Var3;

            // Aim chance for moving elf caps at 50%
            if (Creature.IsMoving && Creature.IsElf)
                if (chance > 50f)
                    chance = 50f;

            // 100% after x time (unofficial)
            if (chance >= 120)
                chance = 100;
            else
                chance = Math.Min(99, chance);

            if (target.IsMoving)
                if (target.IsWalking)
                    chance = Math.Min(MaxChanceWalking, chance);
                else
                    chance = Math.Min(MaxChanceRunning, chance);

            // Debug for devCATs
            if (Creature.Titles.SelectedTitle == TitleId.devCAT)
                Send.ServerMessage(Creature, "Debug: Aim {0}, Distance {1}, Time {2}", chance, distance, aimTime);

            return chance;
        }
    }
}