// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	/// <summary>
	/// Vague entity data type, used in EntityAppears.
	/// </summary>
	public enum DataType : short
	{
		Creature = 16,
		Item = 80,
		Prop = 160
	}

	public enum CreaturePacketType : byte
	{
		/// <summary>
		/// (1) Used by the login server.
		/// </summary>
		Minimal = 1,

		/// <summary>
		/// (2) Used for private information (5209).
		/// </summary>
		Private = 2,

		/// <summary>
		/// (5) Used for public entity appears.
		/// </summary>
		Public = 5,
	}

	public enum SubordinateType : byte
	{
		RpCharacter = 1,
		Pet = 2,
		Transport = 3,
		PartnerVehicle = 4,
	}
}
