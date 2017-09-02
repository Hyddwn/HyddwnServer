// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;

namespace Aura.Channel.Skills.Magic
{
    /// <summary>
    ///     Super Icebolt skill handler (GM skill)
    /// </summary>
    /// <remarks>
    ///     Var1: Min damage
    ///     Var2: Max damage
    ///     Seems to be just a copy of Icebolt, with only one rank, no casting time,
    ///     and very high damage.
    /// </remarks>
    [Skill(SkillId.SuperIcebolt)]
    public class SuperIcebolt : Icebolt
    {
        public override void Init()
        {
        }
    }
}