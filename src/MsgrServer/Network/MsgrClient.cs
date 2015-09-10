// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Network;
using Aura.Msgr.Database;
using Aura.Shared.Network;

namespace Aura.Msgr.Network
{
	public class MsgrClient : BaseClient
	{
		public Contact Contact { get; set; }

		protected override void EncodeBuffer(byte[] buffer)
		{
		}

		public override void DecodeBuffer(byte[] buffer)
		{
		}

		protected override byte[] BuildPacket(Packet packet)
		{
			var packetSize = packet.GetSize();

			// Calculate header size
			var headerSize = 3;
			int n = packetSize;
			do { headerSize++; n >>= 7; } while (n != 0);

			// Write header
			var result = new byte[headerSize + packetSize];
			result[0] = 0x55;
			result[1] = 0x12;
			result[2] = 0x00;

			// Length
			var ptr = 3;
			n = packetSize;
			do
			{
				result[ptr++] = (byte)(n > 0x7F ? (0x80 | (n & 0xFF)) : n & 0xFF);
				n >>= 7;
			}
			while (n != 0);

			// Write packet
			packet.Build(ref result, ptr);

			return result;
		}
	}
}
