// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Network;
using Aura.Msgr.Database;
using System.Collections.Generic;

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
				packet.PutByte((byte)client.Contact.State);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends note list to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="notes">Set to null for negative response.</param>
		public static void NoteListRequestR(MsgrClient client, List<Note> notes)
		{
			var packet = new Packet(Op.Msgr.NoteListRequestR, 0);

			packet.PutByte(notes != null);
			if (notes != null)
			{
				packet.PutInt(notes.Count);
				foreach (var note in notes)
				{
					packet.PutLong(note.Id);
					packet.PutString(note.Sender);
					packet.PutString(note.Message);
					packet.PutLong(note.GetLongTime());
					packet.PutByte(note.Read);
					packet.PutByte(0); // Hidden if 1?
				}
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends note to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="note">Set to null for negative response.</param>
		public static void ReadNoteR(MsgrClient client, Note note)
		{
			var packet = new Packet(Op.Msgr.ReadNoteR, 0);

			packet.PutByte(note != null);
			if (note != null)
			{
				packet.PutLong(note.Id);
				packet.PutString(note.FromCharacterName);
				packet.PutString(note.FromServer);
				packet.PutLong(note.GetLongTime());
				packet.PutByte(0); // Notification note? (reply disabled)
				packet.PutString(note.Message);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends note to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void SendNoteR(MsgrClient client)
		{
			var packet = new Packet(Op.Msgr.SendNoteR, 0);

			packet.PutByte(0);

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
