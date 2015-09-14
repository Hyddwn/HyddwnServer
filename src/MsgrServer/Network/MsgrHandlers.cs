// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Text.RegularExpressions;

namespace Aura.Msgr.Network
{
	public partial class MsgrServerHandlers : PacketHandlerManager<MsgrClient>
	{
		private Regex _receiverRegex = new Regex(@"^[a-z0-9]+@[a-z0-9_]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Checks if user is logged in before calling non-login handlers.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		public override void Handle(MsgrClient client, Packet packet)
		{
			// Check if logged in for non-login packets
			if (packet.Op != Op.Msgr.Login && client.Contact == null)
			{
				Log.Warning("Attempted sending of non-login packet from '{0}' before login.", client.Address);
				return;
			}

			base.Handle(client, packet);
		}

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
		public void Login(MsgrClient client, Packet packet)
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

			// TODO: Load and save.
			client.Contact.State = ContactState.Online;

			Log.Info("User '{0}' logged in as '{1}'.", client.Contact.AccountId, client.Contact.FullName);

			Send.LoginR(client, LoginResult.Okay);
		}

		/// <summary>
		/// Sent when opening notes, requests lists of existing notes.
		/// </summary>
		/// <example>
		/// 001 [0000000000000000] Long   : 0
		/// </example>
		[PacketHandler(Op.Msgr.NoteListRequest)]
		public void NoteListRequest(MsgrClient client, Packet packet)
		{
			var unkLong = packet.GetLong();

			var notes = MsgrServer.Instance.Database.GetNotes(client.Contact);

			Send.NoteListRequestR(client, notes);
		}

		/// <summary>
		/// Sent when double-clicking note, to open and read it.
		/// </summary>
		/// <example>
		/// 001 [0000000000000001] Long   : 1
		/// </example>
		[PacketHandler(Op.Msgr.ReadNote)]
		public void ReadNote(MsgrClient client, Packet packet)
		{
			var noteId = packet.GetLong();

			// TODO: Cache everything in memory?
			var note = MsgrServer.Instance.Database.GetNote(noteId);

			// Check note
			if (note == null)
			{
				Log.Warning("User '{0}' requested a non-existent note.", client.Contact.AccountId);
				Send.ReadNoteR(client, null);
				return;
			}

			// Check receiver
			if (note.Receiver != client.Contact.FullName)
			{
				Log.Warning("User '{0}' tried to read a note that's not his.", client.Contact.AccountId);
				Send.ReadNoteR(client, null);
				return;
			}

			MsgrServer.Instance.Database.SetNoteRead(noteId);

			Send.ReadNoteR(client, note);
		}

		/// <summary>
		/// Sent when opening notes, requests lists of existing notes.
		/// </summary>
		/// <example>
		/// 001 [................] String : admin
		/// 002 [................] String : Foobar@Aura
		/// 003 [................] String : hi there
		/// </example>
		[PacketHandler(Op.Msgr.SendNote)]
		public void SendNote(MsgrClient client, Packet packet)
		{
			var fromAccountId = packet.GetString().Trim();
			var receiver = packet.GetString().Trim();
			var message = packet.GetString().Trim();

			// Check message length
			if (message.Length > 200)
			{
				Log.Warning("User '{0}' tried to send a message that's longer than 200 characters.", client.Contact.AccountId);
				return;
			}

			// Check validity of receiver
			if (!_receiverRegex.IsMatch(receiver))
			{
				Log.Warning("User '{0}' tried to send a message to invalid receiver '{1}'.", client.Contact.AccountId, receiver);
				return;
			}

			// TODO: The messenger is kinda made for direct database access,
			//   but doing that with MySQL isn't exactly efficient...
			//   Maybe we should use a different solution for the msgr?

			// TODO: You should be able to send a message to a character that
			//   has never logged in, so we can't check for contact existence,
			//   but this way someone could flood the database. Spam check?

			MsgrServer.Instance.Database.AddNote(client.Contact.FullName, receiver, message);

			Send.SendNoteR(client);
		}
	}
}
