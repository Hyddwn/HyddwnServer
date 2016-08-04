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
		/// Dummy handler for EntrustedEnchant.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		[PacketHandler(Op.EntrustedEnchant)]
		public void EntrustedEnchant(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();
			var unkByte = packet.GetByte();
			var unkLong = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check feature
			if (!AuraData.FeaturesDb.IsEnabled("EnchantEntrust"))
			{
				Send.Notice(creature, Localization.Get("Requesting enchantments isn't possible yet."));
				Send.EntrustedEnchantR(creature, false);
				return;
			}

			// Check partner
			// Client also does a check for any party members.
			// No party members: "There is no party member available to ask for an Enchantment."
			var partner = creature.Party.GetMember(entityId);
			if (partner == null)
			{
				Send.Notice(creature, Localization.Get("There is no party member available to ask for an Enchantment."));
				Send.EntrustedEnchantR(creature, false);
				return;
			}

			// Check skill
			if (!partner.Skills.Has(SkillId.Enchant, SkillRank.RF))
			{
				Send.Notice(creature, Localization.Get("You cannot request the enchantment."));
				Send.EntrustedEnchantR(creature, false);
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
				Send.EntrustedEnchantR(creature, false);
				return;
			}

			// Initiate
			var entrustment = new Entrustment(creature, partner);
			entrustment.Initiate();

			// Response
			Send.EntrustedEnchantR(creature, true, partner);
		}

		/// <summary>
		/// Sent when closing the entrustment window.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EntrustedEnchantCancel)]
		public void EntrustedEnchantCancel(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			// check active...

			Send.EntrustedEnchantClose(creature);
		}
	}
}
