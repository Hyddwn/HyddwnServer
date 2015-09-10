// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Network;
namespace Aura.Msgr.Network
{
	public static partial class Send
	{
		public static void LoginR(MsgrClient client, LoginResult result)
		{
			var packet = new Packet(Op.Msgr.LoginR, 0);
			packet.PutInt((int)result);
			if (result == LoginResult.Okay)
			{
				packet.PutInt(client.Contact.Id);
				packet.PutString(client.Contact.FullName);
				packet.PutString("");
				packet.PutUInt(0x80000000);
				packet.PutByte(0x10);
			}

			client.Send(packet);
		}
	}

	public enum LoginResult
	{
		Okay = 0,
		Fail = 1,
		Pet = 11,
	}
}
