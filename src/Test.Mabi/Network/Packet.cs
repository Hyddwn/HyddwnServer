// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using Aura.Mabi.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Aura.Tests.Mabi.Network
{
	public class PacketTests
	{
		private Packet GetTestPacket()
		{
			var packet = new Packet(0x01234567, 0x0123456789101112);
			packet.PutByte(byte.MaxValue / 2);
			packet.PutShort(short.MaxValue / 2);
			packet.PutUShort(ushort.MaxValue / 2);
			packet.PutInt(int.MaxValue / 2);
			packet.PutUInt(uint.MaxValue / 2);
			packet.PutLong(long.MaxValue / 2);
			packet.PutULong(ulong.MaxValue / 2);
			packet.PutFloat(float.MaxValue / 2);
			packet.PutString("foobar^2");
			packet.PutBin(new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });

			return packet;
		}

		[Fact]
		public void PacketCreation()
		{
			// Build packet
			var packet = GetTestPacket();

			Assert.Equal(
				"012345670123456789101112420A00017F023FFF027FFF033FFFFFFF037FFFFFFF043FFFFFFFFFFFFFFF047FFFFFFFFFFFFFFF05FFFFFF7E060009666F6F6261725E320007000A09080706050403020100",
				BitConverter.ToString(packet.Build()).Replace("-", "")
			);

			// Build packet in a given buffer
			var buffer = new byte[3 + packet.GetSize()];
			buffer[0] = 2;
			buffer[1] = 3;
			buffer[2] = 1;
			packet.Build(ref buffer, 3);

			Assert.Equal(
				"020301" + "012345670123456789101112420A00017F023FFF027FFF033FFFFFFF037FFFFFFF043FFFFFFFFFFFFFFF047FFFFFFFFFFFFFFF05FFFFFF7E060009666F6F6261725E320007000A09080706050403020100",
				BitConverter.ToString(buffer).Replace("-", "")
			);
		}

		[Fact]
		public void PacketReading()
		{
			var testPacket = GetTestPacket();

			// Read from packet
			var buffer = testPacket.Build();
			var packet = new Packet(buffer, 0);

			Assert.Equal(0x01234567, packet.Op);
			Assert.Equal(0x0123456789101112, packet.Id);
			Assert.Equal(byte.MaxValue / 2, packet.GetByte());
			Assert.Equal(short.MaxValue / 2, packet.GetShort());
			Assert.Equal(ushort.MaxValue / 2, packet.GetUShort());
			Assert.Equal(int.MaxValue / 2, packet.GetInt());
			Assert.Equal(uint.MaxValue / 2, packet.GetUInt());
			Assert.Equal(long.MaxValue / 2, packet.GetLong());
			Assert.Equal(ulong.MaxValue / 2, packet.GetULong());
			Assert.Equal(float.MaxValue / 2, packet.GetFloat());
			Assert.Equal("foobar^2", packet.GetString());
			Assert.Equal(new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }, packet.GetBin());
			Assert.Equal(PacketElementType.None, packet.Peek());

			// Read from offset packet
			var buffer2 = new byte[3 + testPacket.GetSize()];
			buffer2[0] = 2;
			buffer2[1] = 3;
			buffer2[2] = 1;
			testPacket.Build(ref buffer2, 3);
			var packet2 = new Packet(buffer2, 3);

			Assert.Equal(0x01234567, packet2.Op);
			Assert.Equal(0x0123456789101112, packet2.Id);
			Assert.Equal(byte.MaxValue / 2, packet2.GetByte());
			Assert.Equal(short.MaxValue / 2, packet2.GetShort());
			Assert.Equal(ushort.MaxValue / 2, packet2.GetUShort());
			Assert.Equal(int.MaxValue / 2, packet2.GetInt());
			Assert.Equal(uint.MaxValue / 2, packet2.GetUInt());
			Assert.Equal(long.MaxValue / 2, packet2.GetLong());
			Assert.Equal(ulong.MaxValue / 2, packet2.GetULong());
			Assert.Equal(float.MaxValue / 2, packet2.GetFloat());
			Assert.Equal("foobar^2", packet2.GetString());
			Assert.Equal(new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }, packet2.GetBin());
			Assert.Equal(PacketElementType.None, packet.Peek());
		}
	}
}
