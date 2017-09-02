// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Const;

namespace Aura.Channel.Skills.Combat
{
    /// <summary>
    ///     Super Windmill skill handler (GM skill)
    /// </summary>
    /// <remarks>
    ///     Var 1: Damage multiplicator?
    ///     Var 2: Range? (1500)
    /// </remarks>
    [Skill(SkillId.SuperWindmill)]
    public class SuperWindmill : Windmill
    {
        /// <summary>
        ///     No training.
        /// </summary>
        public override void Init()
        {
        }

        /// <summary>
        ///     Returns range.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        protected override int GetRange(Creature attacker, Skill skill)
        {
            return (int) skill.RankData.Var2;
        }
    }
}