// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Mabi.Const
{
	public enum PartyFinishRule
	{
		BiggestContributer = 0,
		Turn = 1,
		Anyone = 2,
	}

	public enum PartyExpSharing
	{
		Equal = 0,
		MoreToFinish = 1,
		AllToFinish = 2,
	}

	public enum PartyType
	{
		Normal = 0,
		Dungeon = 1,
		Jam = 3,
	}

	public enum PartyJoinResult
	{
		Full = 0, // Full or failure?
		Success = 1,
		WrongPass = 4
	}
}
