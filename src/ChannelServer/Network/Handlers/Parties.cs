// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
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

			// Needs check like the below, but do homesteads count as dynamic
			// regions? I know you can't make a party in the shadow realm
			// (you can in dungeons), but are they the only places?
			if (creature.IsInParty /*|| creature.Region.IsDynamic */)
			{
				Log.Warning("PartyCreate: User '{0}' tried to create a party while already being in one.", client.Account.Id);
				Send.CreatePartyR(creature, null);
				return;
			}

			creature.Party = Party.Create(creature, type, name, dungeonLevel, info, password, maxSize);

			Send.CreatePartyR(creature, creature.Party);

			creature.Party.Open();
		}

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

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party settings illicitly.", client.Account.Id);
				return;
			}

			party.ChangeSettings(type, name, dungeonLevel, info, password, maxSize);

			if (partyBoard)
			{
				// TODO: Party board
			}

			Send.PartyChangeSettingR(creature);
		}

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
			partyLeader.Party.AddMember(creature, password);

			Send.PartyJoinR(creature, PartyJoinResult.Success);
			Send.PartyCreateUpdate(creature);
		}

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

			creature.Party.RemoveMember(creature);

			Send.PartyLeaveR(creature, true);
		}

		[PacketHandler(Op.PartyRemove)]
		public void RemoveFromParty(ChannelClient client, Packet packet)
		{
			var targetEntityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			var canRemove = !creature.Region.IsDynamic;
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

			party.RemoveMember(target);
			Send.PartyRemoved(target);

			Send.PartyRemoveR(creature, true);
		}

		[PacketHandler(Op.PartyChangePassword)]
		public void PartyChangePassword(ChannelClient client, Packet packet)
		{
			var password = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party password illicitly.", client.Account.Id);
				Send.PartyChangePasswordR(creature, false);
				return;
			}

			party.SetPassword(password);

			Send.PartyChangePasswordR(creature, true);
		}

		[PacketHandler(Op.PartyChangeLeader)]
		public void PartyChangeLeader(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party leader illicitly.", client.Account.Id);
				Send.PartyChangeLeaderR(creature, false);
				return;
			}

			// IS there any instance in which you're NOT allowed to change party leader?
			var success = party.SetLeader(entityId);

			Send.PartyChangeLeaderR(creature, success);

		}

		[PacketHandler(Op.PartyWantedHide)]
		public void PartyWantedClose(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change ad setting illicitly.", client.Account.Id);
				Send.PartyWantedClosedR(creature, false);
				return;
			}

			party.Close();

			Send.PartyWantedClosedR(creature, true);
		}

		[PacketHandler(Op.PartyWantedShow)]
		public void PartyWantedOpen(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change ad setting illicitly.", client.Account.Id);
				Send.PartyWantedOpenR(creature, false);
				return;
			}

			party.Open();

			Send.PartyWantedOpenR(creature, true);
		}

		[PacketHandler(Op.PartyChangeFinish)]
		public void PartyChangeFinish(ChannelClient client, Packet packet)
		{
			var rule = (PartyFinishRule)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party finish setting illigitly.", client.Account.Id);
				Send.PartyChangeFinishR(creature, false);
				return;
			}

			party.ChangeFinish(rule);

			Send.PartyChangeFinishR(creature, true);
		}

		[PacketHandler(Op.PartyChangeExp)]
		public void PartyChangeExp(ChannelClient client, Packet packet)
		{
			var rule = (PartyExpSharing)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			if (!creature.IsInParty || creature != party.Leader)
			{
				Log.Warning("PartyLeave: User '{0}' tried to change party exp setting illigitly.", client.Account.Id);
				Send.PartyChangeExpR(creature, false);
				return;
			}

			party.ChangeExp(rule);

			Send.PartyChangeExpR(creature, true);
		}
	}
}