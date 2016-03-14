// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi.Const
{
	public enum ChannelState
	{
		/// <summary>
		/// Server is offline for maintenance.
		/// </summary>
		Maintenance = 0,

		/// <summary>
		/// Server is online and stress is 0~39%.
		/// </summary>
		Normal = 1,

		/// <summary>
		/// Server is online and stress is 40~69%.
		/// </summary>
		Busy = 2,

		/// <summary>
		/// Server is online and stress is 70~94%.
		/// </summary>
		Full = 3,

		/// <summary>
		/// Server is online and stress is 95~100%.
		/// </summary>
		/// <remarks>
		/// In this state, the client won't allow the player to move to the channel.
		/// </remarks>
		Bursting = 4,

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// This state has never been directly observed. Maybe used internally,
		/// or possibly if a channel crashes.
		/// Shows up as Maintenance on the client.
		/// </remarks>
		Booting = 5,

		/// <summary>
		/// Any other value is interpreted as Error, unknown if this affects
		/// the client's behavior.
		/// </summary>
		Error = 6
	}

	[Flags]
	public enum ChannelEvent
	{
		None = 0,
		Event = 1,
		PvP = 2
	}
}
