// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Msgr.Network
{
	public class MsgrServerServer : BaseServer<MsgrClient>
	{
		/// <summary>
		/// Reads var int length from buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="ptr"></param>
		/// <returns></returns>
		protected override int GetPacketLength(byte[] buffer, int ptr)
		{
			// <0x55><0x??><0x02><length...>

			var result = 0;
			ptr += 3;

			for (int i = 0; ; ++i)
			{
				result |= (buffer[ptr] & 0x7f) << (i * 7);

				if ((buffer[ptr++] & 0x80) == 0)
					break;
			}

			return result + ptr;
		}

		protected override void HandleBuffer(MsgrClient client, byte[] buffer)
		{
			var length = buffer.Length;
			if (length < 5) return;

			// Challenge
			if (client.State == ClientState.BeingChecked)
			{
				if (buffer[4] == 0x00)
				{
					client.Socket.Send(new byte[] { 0x55, 0xfb, 0x02, 0x05, 0x00, 0x00, 0x00, 0x00, 0x40 });
				}
				else if (buffer[4] == 0x01)
				{
					client.Socket.Send(new byte[] { 0x55, 0xff, 0x02, 0x09, 0x01, 0x1e, 0xf7, 0x5d, 0x68, 0x00, 0x00, 0x00, 0x40 });
				}
				else if (buffer[4] == 0x02)
				{
					client.Socket.Send(new byte[] { 0x55, 0x12, 0x02, 0x01, 0x02 });
					client.State = ClientState.LoggingIn;
				}
			}
			// Actual packets
			else
			{
				// Get to the end of the protocol header
				var start = 3;
				while (start < length)
				{ if (buffer[++start] == 0) break; }

				var packet = new Packet(buffer, start);
				this.Handlers.Handle(client, packet);
			}
		}
	}
}
