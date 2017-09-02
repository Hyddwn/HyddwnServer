// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Mabi.Const;

namespace Aura.Shared.Database
{
    public class GuildMember
    {
        public long GuildId { get; set; }
        public long CharacterId { get; set; }
        public GuildMemberRank Rank { get; set; }
        public DateTime JoinedDate { get; set; }
        public string Application { get; set; }
        public GuildMessages Messages { get; set; }
        public string Name { get; set; }

        public bool IsLeader => Rank == GuildMemberRank.Leader;
        public bool IsOfficer => Rank == GuildMemberRank.Officer;
        public bool IsLeaderOrOfficer => IsLeader || IsOfficer;
        public bool IsApplicant => Rank == GuildMemberRank.Applied;
        public bool IsDeclined => Rank == GuildMemberRank.Declined;
        public bool IsMember => Rank < GuildMemberRank.Applied;
    }
}