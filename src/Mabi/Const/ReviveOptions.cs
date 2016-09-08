// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	[Flags]
	public enum ReviveOptions : uint
	{
		None = 0x00,

		/// <summary>
		/// Revive in town.
		/// </summary>
		Town = 0x01,

		/// <summary>
		/// Revive here and now.
		/// </summary>
		Here = 0x02,

		/// <summary>
		/// Revive in the dungeon's lobby.
		/// </summary>
		DungeonEntrance = 0x04,

		/// <summary>
		/// Revive at the last save statue.
		/// </summary>
		StatueOfGoddess = 0x08,

		/// <summary>
		/// Revive on the side of the arena.
		/// </summary>
		ArenaSide = 0x10,

		/// <summary>
		/// Reive in the arena's lobby.
		/// </summary>
		ArenaLobby = 0x20,

		/// <summary>
		/// Sent to actually revive, after NaoStone.
		/// </summary>
		NaoStoneRevive = 0x80,

		/// <summary>
		/// Put up feather, so other's can revive the player.
		/// </summary>
		WaitForRescue = 0x100,

		/// <summary>
		/// Actual revive, after WaitForRescue.
		/// </summary>
		PhoenixFeather = 0x200,

		/// <summary>
		/// Revive in Tir Na Nog square.
		/// </summary>
		TirNaNog = 0x400,

		/// <summary>
		/// Revive in Barri lobby (for Another World?).
		/// </summary>
		BarriLobby = 0x800,

		/// <summary>
		/// Revive in Tir Chonaill.
		/// </summary>
		TirChonaill = 0x8000,

		/// <summary>
		/// Revive here and now, without penalty (GMs?).
		/// </summary>
		HereNoPenalty = 0x20000,

		/// <summary>
		/// Revive here and now, during PvP?
		/// </summary>
		HerePvP = 0x40000,

		/// <summary>
		/// Revive at the player's camp.
		/// </summary>
		InCamp = 0x80000,

		/// <summary>
		/// Revive in the arena's waiting room.
		/// </summary>
		ArenaWaitingRoom = 0x100000,

		/// <summary>
		/// Revive using a Nao Soul Stone.
		/// </summary>
		NaoStone = 0x4000000,
	}
}
