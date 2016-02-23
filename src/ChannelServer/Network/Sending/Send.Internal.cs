// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends Internal.ServerIdentify to login server.
		/// </summary>
		public static void Internal_ServerIdentify()
		{
			var packet = new Packet(Op.Internal.ServerIdentify, 0);
			packet.PutString(Password.Hash(ChannelServer.Instance.Conf.Internal.Password));

			ChannelServer.Instance.LoginServer.Send(packet);
		}

		/// <summary>
		/// Sends Internal.ChannelStatus to login server.
		/// </summary>
		public static void Internal_ChannelStatus()
		{
			var cur = ChannelServer.Instance.World.CountPlayers();
			var max = ChannelServer.Instance.Conf.Channel.MaxUsers;

			var packet = new Packet(Op.Internal.ChannelStatus, 0);
			packet.PutString(ChannelServer.Instance.Conf.Channel.ChannelServer);
			packet.PutString(ChannelServer.Instance.Conf.Channel.ChannelName);
			packet.PutString(ChannelServer.Instance.Conf.Channel.ChannelHost);
			packet.PutInt(ChannelServer.Instance.Conf.Channel.ChannelPort);
			packet.PutInt(cur);
			packet.PutInt(max);
			packet.PutInt((int) GetServerState(cur, max, ChannelServer.Instance.IsRunning, 
				ChannelServer.Instance.IsInMaintenance));

			ChannelServer.Instance.LoginServer.Send(packet);
		}

		/// <summary>
		/// Calculates the state of the channel.
		/// </summary>
		/// <remarks>
		/// When calculating the <see cref="ChannelState"/> we take into account
		/// whether the server is running as well as if it is in Maintenance.
		/// </remarks>
		/// <param name="current"></param>
		/// <param name="max"></param>
		/// <param name="isRunning"></param>
		/// <param name="isInMaintenance"></param>
		/// <returns></returns>
		private static ChannelState GetServerState(int current, int max, bool isRunning, bool isInMaintenance)
		{
			if (isInMaintenance)
				// In case we do support the booting channel state
				return isRunning ? ChannelState.Maintenance : ChannelState.Booting;

			double stress;

			try
			{
				stress = (current / max) * 100;
			}
			catch (DivideByZeroException ex)
			{
				Log.Exception(ex, "Max user count was zero, falling back to Normal.");
				
				// Fallback value
				return ChannelState.Normal;
			}

			if (stress >= 40 && stress <= 70)
				return ChannelState.Busy;
			if (stress > 70 && stress <= 95)
				return ChannelState.Full;
			if (stress > 95)
				return ChannelState.Bursting;

			return ChannelState.Normal;
		}

		/// <summary>
		/// Sends Internal.Broadcast to login server.
		/// </summary>
		public static void Internal_Broadcast(string message)
		{
			var packet = new Packet(Op.Internal.BroadcastNotice, 0);
			packet.PutString(message);

			ChannelServer.Instance.LoginServer.Send(packet);
		}
	}
}
