// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;

namespace Aura.Login.Database
{
    public class Skill
    {
        public Skill(SkillId id, SkillRank rank = SkillRank.Novice)
        {
            Id = id;
            Rank = rank;
        }

        public SkillId Id { get; set; }
        public SkillRank Rank { get; set; }
    }
}