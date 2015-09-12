// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	/// <summary>
	/// Various ids used by Mabi.
	/// </summary>
	public static class MabiId
	{
		// Packet ids
		// ------------------------------------------------------------------

		/// <summary>
		/// Used as packet id by the login server, since there's no actual
		/// target creature yet.
		/// </summary>
		public const long Login = 0x1000000000000010;

		/// <summary>
		/// Used by the channel server when there's no actual target creature,
		/// like during login/out.
		/// </summary>
		public const long Channel = 0x1000000000000001;

		/// <summary>
		/// Used by the channel for packets without clear sender or target.
		/// </summary>
		public const long Broadcast = 0x3000000000000000;

		// Id ranges
		// ------------------------------------------------------------------

		/// <summary>
		/// Start of card id range.
		/// </summary>
		public const long Cards = 0x0001000000000001;

		/// <summary>
		/// Start of character entitiy id range.
		/// </summary>
		/// <remarks>
		/// If a character's id is not in a certain range, you can't login
		/// with it.
		/// </remarks>
		public const long Characters = 0x0010000000000001;

		/// <summary>
		/// Start of pet entitiy id range.
		/// </summary>
		public const long Pets = 0x0010010000000001;

		/// <summary>
		/// Start of partner entity id range.
		/// </summary>
		/// <remarks>
		/// The client uses this range to keep you from logging in. If you
		/// put a partner into the pet id range you login with it.
		/// </remarks>
		public const long Partners = 0x0010030000000001;

		/// <summary>
		/// Start if NPC entity id range.
		/// </summary>
		public const long Npcs = 0x0010F00000000001;

		/// <summary>
		/// Start of guild id range.
		/// </summary>
		public const long Guilds = 0x0300000000500000;

		/// <summary>
		/// Start of item entity id range.
		/// </summary>
		public const long Items = 0x0050000000000001;

		/// <summary>
		/// Start of quest item entity id range.
		/// </summary>
		public const long QuestItems = 0x005000F000000001;

		/// <summary>
		/// Start of temp item entity id range.
		/// </summary>
		/// <remarks>
		/// This range is used by Aura for newly created items, they get a proper
		/// id once they're saved to the db.
		/// </remarks>
		public const long TmpItems = 0x0050F00000000001;

		/// <summary>
		/// Start of prop entity id range of client props.
		/// </summary>
		/// <remarks>
		/// To differentiate between props already in the client and ones spawned
		/// by the server, they are put into different ranges.
		/// All prop and event entity ids have the following format: 00AABBBBCCCCDDDD
		/// A: Client or Server
		/// B: Region id
		/// C: Area id
		/// D: Id of prop in the area of that region in that category
		/// </remarks>
		public const long ClientProps = 0x00A0000000000000;

		/// <summary>
		/// Start of prop entity id range of server props.
		/// </summary>
		public const long ServerProps = 0x00A1000000000000;

		/// <summary>
		/// Start of event entity ids.
		/// </summary>
		/// <remarks>
		/// Everything we see in the packets suggests that there could be
		/// server-spawned events, but it's unknown whether that's true,
		/// or which entity type they would use.
		/// </remarks>
		public const long AreaEvents = 0x00B0000000000000;

		/// <summary>
		/// Start of party id range.
		/// </summary>
		public const long Parties = 0x0040000000000001;

		/// <summary>
		/// Start of quest id range.
		/// </summary>
		/// <remarks>
		/// Quests are probably 0x0060000000000001, but we'll leave some space
		/// between quests and (quest) items, just in case.
		/// </remarks>
		public const long Quests = 0x006000F000000001;

		/// <summary>
		/// Start of temp quest id range.
		/// </summary>
		/// <remarks>
		/// Just like items, quests get a proper id once they're saved to the db.
		/// It's mimmicking the item system because quest items usually have
		/// the exact same id, plus the following offset.
		/// </remarks>
		public const long QuestsTmp = 0x0060F00000000001;

		/// <summary>
		/// Offset between quest and quest item ids.
		/// </summary>
		public const long QuestItemOffset = 0x0010000000000000;

		/// <summary>
		/// Start of dungeon instance id range.
		/// </summary>
		public const long Instances = 0x0100000000000001;

		// Random ids
		// ------------------------------------------------------------------

		/// <summary>
		/// Default type for pet/partner cards.
		/// </summary>
		public const int PetCardType = 102;

		/// <summary>
		/// Start of the dungeon regions id range.
		/// </summary>
		public const int DungeonRegions = 10010;

		/// <summary>
		/// Start of the dynamic regions id range
		/// </summary>
		public const int DynamicRegions = 35001;

		// NPCs
		// ------------------------------------------------------------------

		/// <summary>
		/// Nao's unique NPC entity id.
		/// </summary>
		/// <remarks>
		/// There are very NPCs (Nao and Tin are the only currently known ones)
		/// that require a fixed entity id, since the client uses those
		/// exact numbers when spawning them client-sided in the soul-stream.
		/// </remarks>
		public const long Nao = 0x0010FFFFFFFFFFFF;

		/// <summary>
		/// Tin's unique NPC entity id.
		/// </summary>
		public const long Tin = 0x0010FFFFFFFFFFFE;
	}
}
