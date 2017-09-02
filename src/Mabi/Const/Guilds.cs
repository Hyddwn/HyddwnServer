// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	public enum GuildLevel : byte
	{
		Beginner = 0,
		Basic = 1,
		Advanced = 2,
		Great = 3,
		Grand = 4,
	}

	public enum GuildType : byte
	{
		Combat = 0,
		Adventure = 1,
		Production = 2,
		Commerce = 3,
		Social = 4,
		Misc = 5,
		All = 9,
	}

	public enum GuildVisibility
	{
		Public = 0,
		Private = 1,
	}

	public enum GuildMemberRank
	{
		Leader = 0,
		Officer = 1,
		BronzeMedal = 2,
		SeniorMember = 3,
		UnkMember = 4,
		Member = 5,
		Applied = 254,
		Declined = 255,
	}

	[Flags]
	public enum GuildMessages : byte
	{
		None = 0x00,
		Accepted = 0x01,
		Rejected = 0x02,
		NewApplication = 0x04,
		MemberLeft = 0x08,
		RankChanged = 0x10,
	}

	[Flags]
	public enum GuildOptions : byte
	{
		None = 0x00,
		GuildHall = 0x01,
		Warp = 0x02,
	}

	public static class GuildStonePropId
	{
		public const int Normal = 211;
		public const int Hope = 40209;
		public const int Courage = 40210;
		public const int GuildHall = 42091;
	}

	public enum GuildStoneType
	{
		Normal = 1,
		Hope = 2,
		Courage = 3,
	}

	public enum GuildPermitCheckResult
	{
		/// <summary>
		/// Some korean message.
		/// </summary>
		Korean = 0,

		/// <summary>
		/// Success, continue to client-side checks.
		/// </summary>
		Success = 1,

		/// <summary>
		/// Nothing happens.
		/// </summary>
		Nothing = 2,
	}

	/// <summary>
	/// Guild level to search for.
	/// </summary>
	/// <remarks>
	/// These are actually the max number of members, but that's what the
	/// client sends and if we want to support other max member numbers
	/// we can't use the numbers for the search.
	/// </remarks>
	public enum GuildSearchLevel
	{
		All = 0,
		Beginner = 5,
		Basic = 10,
		Advanced = 20,
		Great = 50,
		Grand = 250,
	}

	/// <summary>
	/// Member number ranges in guild search.
	/// </summary>
	public enum GuildSearchMembers
	{
		All = 0,
		Lv1_5 = 1,
		Lv6_10 = 2,
		Lv11_20 = 3,
		Lv21_50 = 4,
		Lv51_X = 5,
	}

	/// <summary>
	/// Member number ranges in guild search.
	/// </summary>
	public enum GuildSortBy
	{
		None = 0,
		Level = 1,
		Members = 2,
		Type = 3,
		Name = 4,
	}

	/// <summary>
	/// Member number ranges in guild search.
	/// </summary>
	public enum GuildSortType
	{
		Asc = 0,
		Desc = 1,
	}
}
