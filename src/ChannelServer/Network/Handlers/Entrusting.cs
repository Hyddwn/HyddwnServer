// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
using Aura.Channel.World.Inventory;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent after selecting someone to entrust enchanting or burning to.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		[PacketHandler(Op.Entrustment)]
		public void Entrustment(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var type = (EntrustmentType)packet.GetByte();
			var propEntityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check feature
			if (!AuraData.FeaturesDb.IsEnabled("EnchantEntrust"))
			{
				Send.Notice(creature, Localization.Get("Requesting enchantments isn't possible yet."));
				Send.EntrustmentR(creature, false);
				return;
			}

			if (type != EntrustmentType.Enchant)
			{
				Send.Notice(creature, Localization.Get("Entrusting burns isn't supported yet."));
				Send.EntrustmentR(creature, false);
				return;
			}

			// Check partner
			// Client also does a check for any party members.
			// No party members: "There is no party member available to ask for an Enchantment."
			var partner = creature.Party.GetMember(entityId);
			if (partner == null)
			{
				Send.Notice(creature, Localization.Get("There is no party member available to ask for an Enchantment."));
				Send.EntrustmentR(creature, false);
				return;
			}

			// Check skill
			if (!partner.Skills.Has(SkillId.Enchant, SkillRank.RF))
			{
				Send.Notice(creature, Localization.Get("You cannot request the enchantment."));
				Send.EntrustmentR(creature, false);
				return;
			}

			// Check magic powder
			// Getting here is possible if you drop the powder before
			// confirming the party member for the enchant.
			var items = creature.Inventory.GetItems(a => a.HasTag("/enchant/powder/") && !a.HasTag("/powder05/"), StartAt.BottomRight);
			if (items.Count == 0)
			{
				// Unofficial
				Send.Notice(creature, Localization.Get("You don't have the necessary items."));
				Send.EntrustmentR(creature, false);
				return;
			}

			// Initiate
			var entrustment = new Entrustment(creature, partner);
			entrustment.Initiate();

			// Response
			Send.EntrustmentR(creature, true, partner);
		}

		/// <summary>
		/// Sent when closing the entrustment window.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EntrustmentCancel)]
		public void EntrustmentCancel(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Temp.ActiveEntrustment == null)
			{
				Log.Warning("EntrustmentCancel: User '{0}' tried to cancel entrustment without being in one.", client.Account.Id);
				return;
			}

			creature.Temp.ActiveEntrustment.Cancel();
		}

		/// <summary>
		/// Sent when canceling entrustment from the client that got
		/// the request.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EntrustmentRefuse)]
		public void EntrustmentRefuse(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Temp.ActiveEntrustment == null)
			{
				Log.Warning("EntrustmentRefuse: User '{0}' tried to cancel entrustment without being in one.", client.Account.Id);
				return;
			}

			creature.Temp.ActiveEntrustment.Cancel();
		}

		/// <summary>
		/// Sent when clicking request in the entrustment window.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EntrustmentFinalizeRequest)]
		public void EntrustmentFinalizeRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Temp.ActiveEntrustment == null)
			{
				Log.Warning("EntrustmentFinalizeRequest: User '{0}' tried to cancel entrustment without being in one.", client.Account.Id);
				Send.EntrustmentFinalizeRequestR(creature, false);
				return;
			}

			var ready = creature.Temp.ActiveEntrustment.IsReady;
			Send.EntrustmentFinalizeRequestR(creature, ready);

			if (ready)
				creature.Temp.ActiveEntrustment.Ready();
		}

		/// <summary>
		/// Sent when entrusted creature accepts the request.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EntrustmentAcceptRequest)]
		public void EntrustmentAcceptRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Temp.ActiveEntrustment == null)
			{
				Log.Warning("EntrustmentFinalizeRequest: User '{0}' tried to cancel entrustment without being in one.", client.Account.Id);
				Send.EntrustmentFinalizeRequestR(creature, false);
				return;
			}

			var ready = creature.Temp.ActiveEntrustment.IsReady;
			Send.EntrustmentAcceptRequestR(creature, ready);

			if (ready)
				creature.Temp.ActiveEntrustment.Accept();
		}
	}
}
