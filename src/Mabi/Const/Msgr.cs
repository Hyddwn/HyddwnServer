// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	public enum ContactStatus : byte
	{
		None = 0x00,
		Online = 0x10,
		Secret = 0x20,
		OutToLunch = 0x30,
		Away = 0x40,
		InCombat = 0x50,
		DoingBusiness = 0x60,
		Offline = 0xE0,
		// Others are displayed as "Unknown".
	}

	// Looks like a bitmask... but are there other values?
	[Flags]
	public enum ChatOptions : uint
	{
		NotifyOnFriendLogIn = 0x80000000,
	}
}
