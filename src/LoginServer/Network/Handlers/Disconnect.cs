// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Sent before going back to login screen or continuing to a channel,
		/// apparently related to the favorites list.
		/// </summary>
		/// <example>
		/// 0001 [................] String : admin
		/// 0002 [4D80902C6DF00000] Long   : ?
		/// 0003 [........800000EE] Int    : 2147483886
		/// 0004 [........00000000] Int    : 0
		/// 0005 [........00000000] Int    : 0
		/// </example>
		[PacketHandler(Op.DisconnectInform)]
		public void Disconnect(LoginClient client, Packet packet)
		{
			var accountName = packet.GetString();
			var unkLong = packet.GetLong();
			var unkInt1 = packet.GetInt();

			// The following two ints appear to be counts for lists.
			// They seem to contain updates about the favorite characters/pets.

			var characterCount = packet.GetInt();
			for (int i = 0; i < characterCount; ++i)
			{
				var entityId = packet.GetLong();
				var serverName = packet.GetString();
				var unkByte = packet.GetByte();
			}

			var petCount = packet.GetInt();
			for (int i = 0; i < petCount; ++i)
			{
				var entityId = packet.GetLong();
				var serverName = packet.GetString();
				var unkByte = packet.GetByte();
			}

			if (accountName != client.Account.Name)
				return;

			LoginServer.Instance.Database.UpdateAccount(client.Account);

			Log.Info("'{0}' is closing the connection.", accountName);
		}
	}
}
