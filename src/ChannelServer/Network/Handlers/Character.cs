// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Mabi.Const;
using Aura.Channel.World.Entities.Creatures;
using Aura.Mabi.Network;
using Aura.Channel.World.Dungeons;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when changing titles.
		/// </summary>
		/// <example>
		/// 0001 [............0001] Short  : 1
		/// 0002 [............0002] Short  : 2
		/// </example>
		[PacketHandler(Op.ChangeTitle)]
		public void ChangeTitle(ChannelClient client, Packet packet)
		{
			var titleId = packet.GetUShort();
			var optionTitleId = packet.GetUShort();

			var creature = client.GetCreatureSafe(packet.Id);

			var titleSuccess = creature.Titles.ChangeTitle(titleId, false);
			var optionSuccess = creature.Titles.ChangeTitle(optionTitleId, true);

			Send.ChangeTitleR(creature, titleSuccess, optionSuccess);
		}

		/// <summary>
		/// Request for receiving the available revive methods.
		/// </summary>
		/// <remarks>
		/// Sent automatically after dying and when toggling the window
		/// afterwards.
		/// </remarks>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.DeadMenu)]
		public void DeadMenu(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (!creature.IsDead)
			{
				Send.DeadMenuR(creature, null);
				return;
			}

			Send.DeadMenuR(creature, creature.DeadMenu);
		}

		/// <summary>
		/// Revive request (from dead menu).
		/// </summary>
		/// <example>
		/// Town
		/// 001 [........00000001] Int    : 1
		/// 
		/// ArenaWaitingRoom
		/// 001 [........00000001] Int    : 21
		/// </example>
		[PacketHandler(Op.Revive)]
		public void Revive(ChannelClient client, Packet packet)
		{
			var option = (ReviveOptions)(1 << (packet.GetInt() - 1));

			var creature = client.GetCreatureSafe(packet.Id);

			if (!creature.IsDead || !creature.DeadMenu.Has(option))
			{
				Send.Revive_Fail(creature);
				return;
			}

			var dungeonRegion = creature.Region as DungeonRegion;

			switch (option)
			{
				case ReviveOptions.WaitForRescue:
				case ReviveOptions.NaoStone:
				case ReviveOptions.NaoStoneRevive:
				case ReviveOptions.Here:
				case ReviveOptions.HereNoPenalty:
					creature.Revive(option);
					return;

				case ReviveOptions.Town:
					creature.Warp(creature.LastTown);
					creature.Revive(option);
					return;

				case ReviveOptions.TirNaNog:
					creature.Warp("Ula_Tirnanog/_Uladh_Tirnanog/Tirna_from_Cross");
					creature.Revive(option);
					return;

				case ReviveOptions.BarriLobby:
					creature.Warp("Ula_DgnHall_Bangor_before1/_Ula_DgnHall_Bangor_before1/DG_BangorBefore1_Outer_Spawn");
					creature.Revive(option);
					return;

				case ReviveOptions.DungeonEntrance:
					if (dungeonRegion == null || creature.DungeonSaveLocation.RegionId == 0)
					{
						Log.Warning("Dungeon revive outside of dungeon or without save location (Creature: {0:X16})", creature.EntityId);
						break;
					}

					creature.Warp(dungeonRegion.Dungeon.Data.Exit);
					creature.Revive(option);
					return;

				case ReviveOptions.StatueOfGoddess:
					if (dungeonRegion == null || creature.DungeonSaveLocation.RegionId == 0)
					{
						Log.Warning("Dungeon revive outside of dungeon or without save location (Creature: {0:X16})", creature.EntityId);
						break;
					}

					creature.Warp(creature.DungeonSaveLocation);
					creature.Revive(option);
					return;

				default:
					Log.Unimplemented("ReviveOption '{0}'", option);
					Send.ServerMessage(creature, "Unimplemented: ReviveOption '{0}'", option);
					break;
			}

			Send.ServerMessage(creature, "Revive failed.");
			Send.Revive_Fail(creature);
		}
	}
}
