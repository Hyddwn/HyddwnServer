// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System;

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

		public bool IsLeader { get { return (this.Rank == GuildMemberRank.Leader); } }
		public bool IsOfficer { get { return (this.Rank == GuildMemberRank.Officer); } }
		public bool IsLeaderOrOfficer { get { return (this.IsLeader || this.IsOfficer); } }
		public bool IsApplicant { get { return (this.Rank == GuildMemberRank.Applied); } }
		public bool IsDeclined { get { return (this.Rank == GuildMemberRank.Declined); } }
		public bool IsMember { get { return (this.Rank < GuildMemberRank.Applied); } }
	}
}
