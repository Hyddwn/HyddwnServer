// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	public enum ContactState : byte
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
}
