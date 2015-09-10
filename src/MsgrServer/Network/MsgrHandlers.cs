// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Msgr.Network
{
	public partial class MsgrServerHandlers : PacketHandlerManager<MsgrClient>
	{
		/// <summary>
		/// Sent upon logging into to channel, to log into msgr as well.
		/// </summary>
		/// <example>
		/// 001 [................] String : -1
		/// 002 [0010000000000002] Long   : 4503599627370498
		/// 003 [................] String : Zerono
		/// 004 [................] String : Aura
		/// 005 [................] String : Ch1
		/// 006 [................] String : admin
		/// 007 [0AACDC6249D7B0EC] Long   : 769231951077290220
		/// 008 [0000000000000000] Long   : 0
		/// 009 [........00000000] Int    : 0
		/// </example>
		[PacketHandler(Op.Msgr.Login)]
		public void MsgrLogin(MsgrClient client, Packet packet)
		{
			var unkString = packet.GetString();
			var entityId = packet.GetLong();
			var entityName = packet.GetString();
			var server = packet.GetString();
			var channelName = packet.GetString();
			var accountId = packet.GetString();
			var sessionKey = packet.GetLong();
			var unkLong = packet.GetLong();
			var unkInt = packet.GetInt();

			// Check session
			if (!MsgrServer.Instance.Database.CheckSession(accountId, sessionKey))
			{
				Log.Warning("Someone tried to login with invalid session (Account: {0}).", accountId);
				client.Kill();
				return;
			}

			// Check if pet
			if (entityId >= MabiId.Pets)
			{
				Send.LoginR(client, LoginResult.Pet);
				return;
			}

			// Check character
			if (!MsgrServer.Instance.Database.AccountHasCharacter(accountId, entityId, server))
			{
				Log.Warning("User '{0}' tried to login with invalid character.", accountId);
				client.Kill();
				return;
			}

			// Get contact
			// If no contact with this data exists in the db yet it's created,
			// this way we don't have to worry about creating contacts for existing
			// characters and the msgr server can operate independently.
			client.Contact = MsgrServer.Instance.Database.GetOrCreateContact(accountId, entityId, entityName, server, channelName);
			if (client.Contact == null)
			{
				Log.Warning("Failed to get or create contact for user '{0}'.", accountId);
				Send.LoginR(client, LoginResult.Fail);
				return;
			}

			Log.Info("User '{0}' logged in as '{1}'.", client.Contact.AccountId, client.Contact.FullName);

			Send.LoginR(client, LoginResult.Okay);
		}
	}
}
