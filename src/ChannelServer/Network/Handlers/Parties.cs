// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when opening the party creation.
		/// </summary>
		/// <remarks>
		/// Request for squad information?
		/// </remarks>
		/// <example>
		/// 001 [........00015E22] Int    : 89634
		/// </example>
		[PacketHandler(Op.SquadUnk)]
		public void SquadUnk(ChannelClient client, Packet packet)
		{
			var num = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			Send.SquadUnkR(creature, num);
		}

		/// <summary>
		/// Sent to create a new party.
		/// </summary>
		/// <example>
		/// 001 [........00000000] Int    : 0
		/// 002 [................] String : test
		/// 003 [................] String : 1234
		/// 004 [........00000007] Int    : 7
		/// 005 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.PartyCreate)]
		public void PartyCreate(ChannelClient client, Packet packet)
		{
			var dungeonLevel = "";
			var info = "";

			var type = (PartyType)packet.GetInt();
			var name = packet.GetString();
			if (type == PartyType.Dungeon)
			{
				dungeonLevel = packet.GetString();
				info = packet.GetString();
			}
			var password = packet.GetString();
			var maxSize = packet.GetInt();
			var partyBoard = packet.GetBool();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check if party can be created
			// TODO: Is party creation disabled in all dynamic regions?
			//   What about homesteads?
			if (creature.IsInParty || creature.Region.IsDynamic)
			{
				Log.Warning("PartyCreate: User '{0}' tried to create a party while already being in one.", client.Account.Id);
				Send.CreatePartyR(creature, null);
				return;
			}

			// Check name length
			if (name.Length < 4 || name.Length > 32)
			{
				Log.Warning("PartyCreate: User '{0}' tried to create a party with invalid name.", client.Account.Id);
				Send.CreatePartyR(creature, null);
				return;
			}

			// Check party max size
			if (maxSize > ChannelServer.Instance.Conf.World.PartyMaxSize)
				Send.MsgBox(creature, Localization.Get("The maximum party size allowed on this server is {0}."), ChannelServer.Instance.Conf.World.PartyMaxSize);

			// Create new party
			creature.Party = Party.Create(creature, type, name, dungeonLevel, info, password, maxSize);

			// Response
			Send.CreatePartyR(creature, creature.Party);

			// Start members wanted ad
			creature.Party.Open();
		}

		/// <summary>
		/// Sent when changing the party settings.
		/// </summary>
		/// <example>
		/// 001 [........00000000] Int    : 0
		/// 002 [................] String : test
		/// 003 [................] String : 1234
		/// 004 [........00000007] Int    : 7
		/// 005 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.PartyChangeSetting)]
		public void PartyChangeSettings(ChannelClient client, Packet packet)
		{
			var dungeonLevel = "";
			var info = "";

			var type = (PartyType)packet.GetInt();
			var name = packet.GetString();
			if (type == PartyType.Dungeon)
			{
				dungeonLevel = packet.GetString();
				info = packet.GetString();
			}
			var password = packet.GetString();
			var maxSize = packet.GetInt();
			var partyBoard = packet.GetBool();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check if creature can change settings
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party settings illicitly.", client.Account.Id);
				return;
			}

			// Check party max size
			if (maxSize > ChannelServer.Instance.Conf.World.PartyMaxSize)
				Send.MsgBox(creature, Localization.Get("The maximum party size allowed on this server is {0}."), ChannelServer.Instance.Conf.World.PartyMaxSize);

			// Update settings
			party.ChangeSettings(type, name, dungeonLevel, info, password, maxSize);

			// TODO: Party board
			if (partyBoard)
			{
			}

			// Response
			Send.PartyChangeSettingR(creature);
		}

		/// <summary>
		/// Request to join a party.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.PartyJoin)]
		public void PartyJoin(ChannelClient client, Packet packet)
		{
			var leaderEntityId = packet.GetLong();
			var password = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check party leader
			var partyLeader = ChannelServer.Instance.World.GetCreature(leaderEntityId);
			if (partyLeader == null)
			{
				Log.Warning("PartyJoin: User '{0}' tried to join party of non-existent leader.", client.Account.Id);
				Send.PartyJoinR(creature, PartyJoinResult.Full);
				return;
			}

			var party = partyLeader.Party;

			// Check if creature can join
			if (creature.IsInParty || partyLeader != party.Leader || !party.HasFreeSpace)
			{
				Log.Warning("PartyJoin: User '{0}' tried to join party illicitly.", client.Account.Id);
				Send.PartyJoinR(creature, PartyJoinResult.Full);
				return;
			}

			// Check space
			if (!party.HasFreeSpace)
			{
				Send.PartyJoinR(creature, PartyJoinResult.Full);
				return;
			}

			// Check password
			if (!party.CheckPassword(password))
			{
				Send.PartyJoinR(creature, PartyJoinResult.WrongPass);
				return;
			}

			// Add
			party.AddMember(creature);

			// Response and party update
			Send.PartyJoinR(creature, PartyJoinResult.Success);
			Send.PartyCreateUpdate(creature);
		}

		/// <summary>
		/// Sent when trying to leave party.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.PartyLeave)]
		public void LeaveParty(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Check if in party
			if (!creature.IsInParty)
			{
				Log.Warning("PartyLeave: User '{0}' tried to leave party without being in one.", client.Account.Id);
				Send.PartyLeaveR(creature, false);
				return;
			}

			// Check if able to leave
			// TODO: Check if there are other regions or situations in which you cannot leave a party.
			if (creature.Region.IsDynamic)
			{
				Log.Warning("PartyLeave: User '{0}' tried to leave party in a dynamic region.", client.Account.Id);
				Send.PartyLeaveR(creature, false);
				return;
			}

			// Remove member
			creature.Party.RemoveMember(creature);

			// Response
			Send.PartyLeaveR(creature, true);
		}

		/// <summary>
		/// Request to remove someone from the party.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.PartyRemove)]
		public void RemoveFromParty(ChannelClient client, Packet packet)
		{
			var targetEntityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			var party = creature.Party;

			// Check creature
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to leave party in a dynamic region.", client.Account.Id);
				Send.PartyRemoveR(creature, false);
				return;
			}

			// Check target
			var target = party.GetMember(targetEntityId);
			if (target == null || target == creature)
			{
				Log.Warning("PartyLeave: User '{0}' tried to remove invalid member from party.", client.Account.Id);
				Send.PartyRemoveR(creature, false);
				return;
			}

			// Check region
			if (creature.Region.IsDynamic)
			{
				Log.Warning("PartyLeave: User '{0}' tried to remove member in dynamic region.", client.Account.Id);
				Send.PartyRemoveR(creature, false);
				return;
			}

			// Remove
			party.RemoveMember(target);
			Send.PartyRemoved(target); // TODO: Send from RemoveMember?

			// Response
			Send.PartyRemoveR(creature, true);
		}

		/// <summary>
		/// Sent when changing the party's password.
		/// </summary>
		/// <example>
		/// 001 [................] String : 2345
		/// </example>
		[PacketHandler(Op.PartyChangePassword)]
		public void PartyChangePassword(ChannelClient client, Packet packet)
		{
			var password = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check if creature can change password
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party password illicitly.", client.Account.Id);
				Send.PartyChangePasswordR(creature, false);
				return;
			}

			// Change password
			party.SetPassword(password);

			// Response
			Send.PartyChangePasswordR(creature, true);
		}

		/// <summary>
		/// Sent when changing the leader in the party window.
		/// </summary>
		/// <example>
		/// 001 [0010000000000004] Long   : 4503599627370500
		/// </example>
		[PacketHandler(Op.PartyChangeLeader)]
		public void PartyChangeLeader(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check if creature can change leader
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party leader illicitly.", client.Account.Id);
				Send.PartyChangeLeaderR(creature, false);
				return;
			}

			// Set leader
			// Is there any instance in which you're not allowed to change party leader?
			var success = party.SetLeader(entityId);

			// Response
			Send.PartyChangeLeaderR(creature, success);

		}

		/// <summary>
		/// Sent when deactivating "Member Wanted".
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PartyWantedHide)]
		public void PartyWantedClose(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check if creature can change wanted status
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change ad setting illicitly.", client.Account.Id);
				Send.PartyWantedClosedR(creature, false);
				return;
			}

			// Close ad
			party.Close();

			// Response
			Send.PartyWantedClosedR(creature, true);
		}

		/// <summary>
		/// Sent when activating "Member Wanted".
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PartyWantedShow)]
		public void PartyWantedOpen(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check if creature can change wanted status
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change ad setting illicitly.", client.Account.Id);
				Send.PartyWantedOpenR(creature, false);
				return;
			}

			// Start wanted ad
			party.Open();

			// Response
			Send.PartyWantedOpenR(creature, true);
		}

		/// <summary>
		/// Sent when changing the finishing rule in the party window.
		/// </summary>
		/// <example>
		/// 001 [................] Int : 1
		/// </example>
		[PacketHandler(Op.PartyChangeFinish)]
		public void PartyChangeFinish(ChannelClient client, Packet packet)
		{
			var rule = (PartyFinishRule)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check rule
			if (!Enum.IsDefined(typeof(PartyFinishRule), rule))
			{
				Log.Warning("PartyChangeFinish: Creature '{0:X16}' tried to use unknown rule '{1}'.", creature.EntityId, rule);
				Send.PartyChangeFinishR(creature, false);
				return;
			}

			// Check if creature can change finish rule
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party finish setting illigitly.", client.Account.Id);
				Send.PartyChangeFinishR(creature, false);
				return;
			}

			// Change rule
			party.ChangeFinish(rule);

			// Response
			Send.PartyChangeFinishR(creature, true);
		}

		/// <summary>
		/// Sent when changing the exp rule in the party window.
		/// </summary>
		/// <example>
		/// 001 [................] Int : 1
		/// </example>
		[PacketHandler(Op.PartyChangeExp)]
		public void PartyChangeExp(ChannelClient client, Packet packet)
		{
			var rule = (PartyExpSharing)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check rule
			if (!Enum.IsDefined(typeof(PartyExpSharing), rule))
			{
				Log.Warning("PartyChangeExp: Creature '{0:X16}' tried to use unknown rule '{1}'.", creature.EntityId, rule);
				Send.PartyChangeFinishR(creature, false);
				return;
			}

			// Check if creature can change exp sharing rule
			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party exp setting illigitly.", client.Account.Id);
				Send.PartyChangeExpR(creature, false);
				return;
			}

			// Change rule
			party.ChangeExp(rule);

			// Response
			Send.PartyChangeExpR(creature, true);
		}

		/// <summary>
		/// Request for list of open parties, sent when opening party board.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PartyBoardRequest)]
		public void PartyBoardRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Dummy, until we have party boards.
			var parties = new List<Party>();

			Send.PartyBoardRequestR(creature, parties);
		}

		/// <summary>
		/// Sent when a party quest is selected.
		/// </summary>
		/// <example>
		/// 001 [006000CCB3841234] Long   : 27022476949328436
		/// </example>
		[PacketHandler(Op.PartySetQuest)]
		public void PartySetQuest(ChannelClient client, Packet packet)
		{
			var uniqueQuestId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);
			var quest = creature.Quests.GetSafe(uniqueQuestId);
			var min = ChannelServer.Instance.Conf.World.PartyQuestMinSize;

			// Check party
			if (!creature.IsInParty)
			{
				Log.Warning("PartySetQuest: Creature '{0:X16}' tried to set party quest without being in a party.", creature.EntityId);
				Send.PartySetQuestR(creature, false);
				return;
			}

			// Check party members
			if (creature.Party.MemberCount < min)
			{
				Send.MsgBox(creature, Localization.Get("Your party must consist of {0} or more members\nto register a party quest."), min);
				Send.PartySetQuestR(creature, false);
				return;
			}

			creature.Party.SetPartyQuest(quest);
			Send.PartySetQuestR(creature, true);
		}

		/// <summary>
		/// Sent when resetting party quest.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.PartyUnsetQuest)]
		public void PartyUnsetQuest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// Check party
			if (!creature.IsInParty)
			{
				Log.Warning("PartyUnsetQuest: Creature '{0:X16}' tried to unset party quest without being in a party.", creature.EntityId);
				Send.PartySetQuestR(creature, false);
				return;
			}

			// Try to unset, fails if no quest set.
			var success = creature.Party.UnsetPartyQuest();

			Send.PartyUnsetQuestR(creature, success);
		}
	}
}
