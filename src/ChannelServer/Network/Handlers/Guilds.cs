// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when choosing to convert play points to guild points.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GuildConvertPlayPoints)]
		public void GuildConvertPlayPoints(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("GuildConvertPlayPoints: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildConvertPlayPointsR(creature, false, 0);
				return;
			}

			Send.GuildConvertPlayPointsR(creature, true, creature.PlayPoints);
		}

		/// <summary>
		/// Sent when confirming the conversion of play points to guild points.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GuildConvertPlayPointsConfirm)]
		public void GuildConvertPlayPointsConfirm(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("GuildConvertPlayPointsConfirm: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildConvertPlayPointsConfirmR(creature, false);
				return;
			}

			ChannelServer.Instance.GuildManager.ConvertPlayPoints(creature, creature.Guild);

			Send.GuildConvertPlayPointsConfirmR(creature, true);
		}

		/// <summary>
		/// Sent when clicking OK on the guild gold donation window.
		/// </summary>
		/// <example>
		/// 001 [........00000159] Int    : 345
		/// 002 [0000000000000000] Long   : 0
		/// </example>
		[PacketHandler(Op.GuildDonate)]
		public void GuildDonate(ChannelClient client, Packet packet)
		{
			var amount = packet.GetInt();
			var checkId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("ConvertGpConfirm: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}

			// Check
			if (checkId != 0)
			{
				var check = creature.Inventory.GetItem(checkId);
				if (check == null)
				{
					Log.Warning("GuildDonate: User '{0}' tried to donate non-exitent check.", client.Account.Id);
					Send.GuildDonateR(creature, false);
					return;
				}
				else if (!check.HasTag("/check/"))
				{
					Log.Warning("GuildDonate: User '{0}' tried to donate invalid check ({1}).", client.Account.Id, check.Info.Id);
					Send.GuildDonateR(creature, false);
					return;
				}

				ChannelServer.Instance.GuildManager.DonateItem(creature, creature.Guild, check);
			}
			// Gold
			else
			{
				if (amount == 0)
				{
					Send.GuildDonateR(creature, false);
					return;
				}
				else if (creature.Inventory.Gold < amount)
				{
					Send.MsgBox(creature, Localization.Get("You don't have enough gold."));
					Send.GuildDonateR(creature, false);
					return;
				}

				ChannelServer.Instance.GuildManager.DonateGold(creature, creature.Guild, amount);
			}

			Send.GuildDonateR(creature, true);
		}

		/// <summary>
		/// Sent choosing to destroy the guild stone.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GuildDestroyStone)]
		public void GuildDestroyStone(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("GuildDestroyStone: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}
			else if (creature.GuildMember.Rank != GuildMemberRank.Leader)
			{
				Log.Warning("GuildDestroyStone: User '{0}' tried to destroy stone without being leader.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}

			ChannelServer.Instance.GuildManager.DestroyStone(creature.Guild);
		}

		/// <summary>
		/// Sent when applying at a guild.
		/// </summary>
		/// <example>
		/// 001 [0300000000500002] Long   : 216172782119026690
		/// 002 [................] String : aaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeffffffffffgggggggggghhhhhhhhhhjjjjjjjjjjiiiiiiiiii
		/// </example>
		[PacketHandler(Op.GuildApply)]
		public void GuildApply(ChannelClient client, Packet packet)
		{
			var guildId = packet.GetLong();
			var application = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check creature's guild
			if (creature.Guild != null)
			{
				Log.Warning("GuildApply: User '{0}' is already in a guild.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}

			// Check guild
			var guild = ChannelServer.Instance.GuildManager.GetGuild(guildId);
			if (guild == null)
			{
				Send.MsgBox(creature, Localization.Get("Guild not found."));
				Send.GuildApplyR(creature, false);
				return;
			}

			// Check application
			if (application.Length > 100)
			{
				Log.Warning("GuildApply: User '{0}' sent an application that was >100 characters, truncated.", client.Account.Id);
				application = application.Substring(0, 100);
			}

			// Check members
			if (guild.MemberCount >= guild.MaxMembers)
			{
				Send.MsgBox(creature, Localization.Get("The guild's maximum amount of members has been reached."));
				Send.GuildApplyR(creature, false);
				return;
			}

			ChannelServer.Instance.GuildManager.Apply(creature, guild, application);

			Send.GuildMessage(creature, guild, Localization.Get("Your application has been accepted.\nPlease wait for the Guild Leader to make the final confirmation."));
			Send.GuildApplyR(creature, true);
		}

		/// <summary>
		/// Sent choosing to change the look of the guild stone.
		/// </summary>
		/// <example>
		/// 001 [........00000001] Int    : 1
		/// </example>
		[PacketHandler(Op.GuildChangeStone)]
		public void GuildChangeStone(ChannelClient client, Packet packet)
		{
			var stoneType = (GuildStoneType)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check guild
			var guild = creature.Guild;
			if (guild == null)
			{
				Log.Warning("GuildChangeStone: User '{0}' is not in a guild.", client.Account.Id);
				return;
			}

			// Check rank
			if (!creature.GuildMember.IsLeader)
			{
				Log.Warning("GuildChangeStone: User '{0}' is not the guild's leader.", client.Account.Id);
				return;
			}

			// Check guild level
			var level = GuildLevel.Beginner;
			switch (stoneType)
			{
				case GuildStoneType.Hope: level = GuildLevel.Advanced; break;
				case GuildStoneType.Courage: level = GuildLevel.Grand; break;
			}

			if (guild.Level < level)
			{
				Log.Warning("GuildChangeStone: User '{0}' tried to change stone without having the necessary guild level.", client.Account.Id);
				return;
			}

			// Check resources
			var points = 0;
			var gold = 0;
			switch (stoneType)
			{
				case GuildStoneType.Hope: points = 10000; gold = 100000; break;
				case GuildStoneType.Courage: points = 100000; gold = 1000000; break;
			}

			if (guild.Points < points || guild.Gold < gold)
			{
				Send.MsgBox(creature, Localization.Get("You need {0:n0} Guild Points and {1:n0} Gold to change the guild stone."), points, gold);
				return;
			}

			// Update
			guild.Points -= points;
			guild.Gold -= gold;

			ChannelServer.Instance.Database.UpdateGuildResources(guild);
			ChannelServer.Instance.GuildManager.ChangeStone(creature.Guild, stoneType);
		}

		/// <summary>
		/// Sent choosing to change the look of the guild stone.
		/// </summary>
		/// <example>
		/// 001 [..............01] Byte   : 1
		/// 002 [........000C3500] Int    : 800000
		/// </example>
		[PacketHandler(Op.GuildWithdrawGold)]
		public void GuildWithdrawGold(ChannelClient client, Packet packet)
		{
			var getCheck = packet.GetBool();
			var amount = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check guild
			var guild = creature.Guild;
			if (guild == null)
			{
				Log.Warning("GuildWithdrawGold: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildWithdrawGoldR(creature, false);
				return;
			}

			// Check rank
			if (!creature.GuildMember.IsLeader)
			{
				Log.Warning("GuildWithdrawGold: User '{0}' is not the guild's leader.", client.Account.Id);
				Send.GuildWithdrawGoldR(creature, false);
				return;
			}

			// Check availablility
			if (guild.WithdrawMaxAmount == 0)
			{
				Send.MsgBox(creature, Localization.Get("A total agreememt from the Guild is required to withdraw the Gold from the guild safe. Please set up a poll in the guild homepage."));
				Send.GuildWithdrawGoldR(creature, false);
				return;
			}

			// Check date
			if (DateTime.Now > guild.WithdrawDeadline)
			{
				Send.MsgBox(creature, Localization.Get("Withdrawal period expired. Guild funds may only be withdrawn for up to one week after winning the majority vote."));
				Send.GuildWithdrawGoldR(creature, false);
				return;
			}

			// Check requested amount
			var maxAmount = guild.WithdrawMaxAmount;
			if (maxAmount < amount)
			{
				Log.Warning("GuildWithdrawGold: User '{0}' requested more than they should be able to.", client.Account.Id);
				Send.GuildWithdrawGoldR(creature, false);
				return;
			}

			// Check needed amount
			var needed = amount;
			if (getCheck)
			{
				var fee = (int)Math.Max(500, amount * 0.05f);
				needed = fee;
			}

			if (maxAmount < needed)
			{
				Send.MsgBox(creature, Localization.Get("Unable to pay the transaction fee due to lack of budget."));
				Send.GuildWithdrawGoldR(creature, false);
				return;
			}

			// Withdraw
			guild.Gold -= needed;
			ChannelServer.Instance.Database.UpdateGuildResources(guild);

			if (getCheck)
			{
				var check = Item.CreateCheck(amount);
				creature.Inventory.Add(check, true);
			}
			else
			{
				creature.Inventory.AddGold(amount);
			}

			Send.GuildWithdrawGoldR(creature, true);

			// Sending panel again to reset variables, unofficial,
			// we're missing a log for withdrawing.
			Send.GuildPanel(creature, guild);
		}

		/// <summary>
		/// Sent clicking "Invite to join guild" in character right-click menu.
		/// </summary>
		/// <example>
		/// 001 [0010000000000021] Long   : 4503599627370529
		/// </example>
		[PacketHandler(Op.GuildInvite)]
		public void GuildInvite(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check feature
			if (!AuraData.FeaturesDb.IsEnabled("InviteGuild"))
			{
				Send.MsgBox(creature, Localization.Get("This feature hasn't been enabled yet."));
				return;
			}

			// Check guild
			var guild = creature.Guild;
			if (guild == null)
			{
				Log.Warning("GuildInvite: User '{0}' is not in a guild.", client.Account.Id);
				return;
			}

			// Check rank
			if (!creature.GuildMember.IsMember)
			{
				Log.Warning("GuildInvite: User '{0}' is not a full member.", client.Account.Id);
				return;
			}

			// Check availablility
			if (guild.MemberCount >= guild.MaxMembers)
			{
				Send.MsgBox(creature, Localization.Get("The guild's maximum amount of members has been reached."));
				return;
			}

			// Check character
			var other = creature.Region.GetCreature(entityId);
			if (other == null || !other.GetPosition().InRange(creature.GetPosition(), Region.VisibleRange))
			{
				Send.MsgBox(creature, Localization.Get("Character not found."));
				return;
			}
			else if (other.Guild != null)
			{
				Send.MsgBox(creature, Localization.Get("{0} is already a member of a guild."), other.Name);
				return;
			}

			// Send info as an invite? We're actually lacking a log of what's
			// supposed to happen here, but this works.
			Send.GuildInfoNoGuild(other, guild);
		}

		/// <summary>
		/// Sent when using guild creation permit.
		/// </summary>
		/// <remarks>
		/// This packet is presumably only to check the item, after a
		/// successful response the client checks if the party has
		/// 5 members itself, so non-fiver guilds aren't possible this way.
		/// The "Nothing" result can be used to send custom error msgs,
		/// as nothing will be displayed, but the client will accept the
		/// response.
		/// </remarks>
		/// <example>
		/// 001 [005000CC9DBC40C3] Long   : 22518876956541123
		/// </example>
		[PacketHandler(Op.GuildPermitCheck)]
		public void GuildPermitCheck(ChannelClient client, Packet packet)
		{
			var itemEntityId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);
			var item = creature.Inventory.GetItemSafe(itemEntityId);

			// Check feature
			if (!AuraData.FeaturesDb.IsEnabled("SystemGuild"))
			{
				Send.MsgBox(creature, Localization.Get("This feature hasn't been enabled yet."));
				Send.GuildPermitCheckR(creature, GuildPermitCheckResult.Nothing, itemEntityId);
				return;
			}

			// Check item
			if (item.Info.Id != 63040) // Guild Formation Permit
			{
				Log.Warning("GuildPermitCheck: User '{0}' tried to use invalid item.", client.Account.Id);
				Send.GuildPermitCheckR(creature, GuildPermitCheckResult.Nothing, itemEntityId);
				return;
			}

			// Check guild
			if (creature.GuildId != 0)
			{
				Send.MsgBox(creature, Localization.Get("You are already a member of a guild."));
				Send.GuildPermitCheckR(creature, GuildPermitCheckResult.Nothing, itemEntityId);
				return;
			}

			Send.GuildPermitCheckR(creature, GuildPermitCheckResult.Success, itemEntityId);
		}

		/// <summary>
		/// Sent to check if guild name is valid and available.
		/// </summary>
		/// <example>
		/// 001 [................] String : Name
		/// </example>
		[PacketHandler(Op.GuildCheckName)]
		public void GuildCheckName(ChannelClient client, Packet packet)
		{
			var name = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check name
			if (!Regex.IsMatch(name, @"^[\w]{4,12}$") || ChannelServer.Instance.Database.GuildNameExists(name))
			{
				Send.GuildCheckNameR(creature, false);
				return;
			}

			Send.GuildCheckNameR(creature, true);
		}

		/// <summary>
		/// Sent when attempting to create a guild.
		/// </summary>
		/// <example>
		/// 001 [005000CC9DBC40C3] Long   : 22518876956541123
		/// 002 [................] String : Researcher
		/// 003 [........00000001] Int    : 1
		/// 004 [........00000001] Int    : 1
		/// </example>
		[PacketHandler(Op.GuildCreateRequest)]
		public void GuildCreateRequest(ChannelClient client, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var name = packet.GetString();
			var type = (GuildType)packet.GetInt();
			var visibility = (GuildVisibility)packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);
			var item = creature.Inventory.GetItemSafe(itemEntityId);

			// Check item
			if (item.Info.Id != 63040) // Guild Formation Permit
			{
				Log.Warning("GuildCreateRequest: User '{0}' tried to use invalid item.", client.Account.Id);
				Send.GuildCreateRequestR(creature, false);
				return;
			}

			// Check types
			if (!Enum.IsDefined(typeof(GuildType), type) || !Enum.IsDefined(typeof(GuildVisibility), visibility))
			{
				Log.Warning("GuildCreateRequest: User '{0}' sent invalid type or visibility ({1}, {2}).", client.Account.Id, type, visibility);
				Send.GuildCreateRequestR(creature, false);
				return;
			}

			// Check name
			if (!Regex.IsMatch(name, @"^[\w]{4,12}$") || ChannelServer.Instance.Database.GuildNameExists(name))
			{
				Send.MsgBox(creature, Localization.Get("That name is not valid or is already in use."));
				Send.GuildCreateRequestR(creature, false);
				return;
			}

			// Check guild
			if (creature.GuildId != 0)
			{
				Send.MsgBox(creature, Localization.Get("You are already a member of a guild."));
				Send.GuildCreateRequestR(creature, false);
				return;
			}

			// Check party
			var party = creature.Party;
			var size = 5;
			if (!creature.IsInParty || party.MemberCount != size || party.GetMembers().Any(a => a.GuildId != 0) || party.Leader != creature)
			{
				Send.MsgBox(creature, Localization.GetPlural("You must be the leader of a party of {0} to form a guild.", "You must be the leader of a party of {0} to form a guild.", size), size);
				Send.GuildCreateRequestR(creature, false);
				return;
			}

			Send.GuildCreateRequestR(creature, true);

			party.GuildNameToBe = name;
			party.GuildTypeToBe = type;
			party.GuildVisibilityToBe = visibility;
			party.GuildNameVoteRequested = true;
			party.GuildNameVoteCount = 1;
			party.GuildNameVotes = 1;

			// Send vote requests
			var partyMembers = party.GetMembers();
			foreach (var member in partyMembers.Where(a => a != party.Leader))
				Send.GuildNameAgreeRequest(member, creature.EntityId, name);
		}

		/// <summary>
		/// Sent when voting for guild name.
		/// </summary>
		/// <example>
		/// 001 [001000000019AD26] Long   : 4503599629053222
		/// 002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.GuildNameVote)]
		public void GuildNameVote(ChannelClient client, Packet packet)
		{
			var leaderEntityId = packet.GetLong();
			var vote = packet.GetBool();

			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check Party
			if (!party.GuildNameVoteRequested)
			{
				Log.Warning("GuildNameVote: User '{0}' sent vote without request.", client.Account.Id);
				Send.GuildCreateRequestR(creature, false);
				return;
			}
			else if (party.Leader.EntityId != leaderEntityId)
			{
				Log.Warning("GuildNameVote: User '{0}' sent invalid leader id.", client.Account.Id);
				Send.GuildCreateRequestR(creature, false);
				return;
			}

			// Send votes
			var partyMembers = party.GetMembers();
			foreach (var member in partyMembers)
				Send.GuildNameVote(member, creature.Name, vote);

			// Update
			creature.Party.GuildNameVoteCount++;
			if (vote) creature.Party.GuildNameVotes++;

			// Inform leader about result
			if (creature.Party.GuildNameVoteCount == party.MemberCount)
			{
				if (creature.Party.GuildNameVotes == party.MemberCount)
					Send.GuildCreationConfirmRequest(party.Leader);
			}
		}

		/// <summary>
		/// Sent when confirming the guild creation.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GuildCreationConfirmation)]
		public void GuildCreationConfirmation(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			var party = creature.Party;

			// Check guild
			if (creature.GuildId != 0)
			{
				Send.MsgBox(creature, Localization.Get("You are already a member of a guild."));
				return;
			}

			// Check party
			var size = 5;
			if (!creature.IsInParty || party.MemberCount != size || party.GetMembers().Any(a => a.GuildId != 0) || party.Leader != creature || party.GuildNameVoteCount != party.MemberCount)
			{
				Send.MsgBox(creature, Localization.GetPlural("You must be the leader of a party of {0} to form a guild.", "You must be the leader of a party of {0} to form a guild.", size), size);
				return;
			}

			var name = party.GuildNameToBe;
			var type = party.GuildTypeToBe;
			var visibility = party.GuildVisibilityToBe;

			ChannelServer.Instance.GuildManager.CreateGuild(party, name, type, visibility);

			creature.Inventory.Remove(63040); // Guild Formation Permit
			creature.Inventory.Add(new Item(63041), true); // Guild Stone Installation Permit
		}

		/// <summary>
		/// Sent when clicking request join on guild list.
		/// </summary>
		/// <example>
		/// 001 [0300000000500423] Long   : 216172782119027747
		/// </example>
		[PacketHandler(Op.GuildListJoinRequest)]
		public void GuildListJoinRequest(ChannelClient client, Packet packet)
		{
			var guildId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check feature
			if (!AuraData.FeaturesDb.IsEnabled("GuildListBoard"))
			{
				Send.MsgBox(creature, Localization.Get("This feature hasn't been enabled yet."));
				return;
			}

			// Check guilds
			if (creature.GuildId != 0)
			{
				Send.MsgBox(creature, Localization.Get("You are already member of a guild."));
				return;
			}

			// Check guild
			var guild = ChannelServer.Instance.GuildManager.GetGuild(guildId);
			if (guild == null)
			{
				Send.MsgBox(creature, Localization.Get("Guild not found."));
				return;
			}

			// Check availablility
			if (guild.MemberCount >= guild.MaxMembers)
			{
				Send.MsgBox(creature, Localization.Get("The guild's maximum amount of members has been reached."));
				return;
			}

			// Send info as an invite? We're actually lacking a log of what's
			// supposed to happen here, but this works.
			Send.GuildInfoNoGuild(creature, guild);
		}

		/// <summary>
		/// Sent when finishing guild robe creation.
		/// </summary>
		/// <example>
		/// 001 [..............2D] Byte   : 45
		/// 002 [..............18] Byte   : 24
		/// 003 [..............06] Byte   : 6
		/// 004 [........006EAD5C] Int    : 7253340
		/// 005 [..............1B] Byte   : 27
		/// 006 [..............14] Byte   : 20
		/// 007 [..............19] Byte   : 25
		/// 008 [..............17] Byte   : 23
		/// </example>
		[PacketHandler(Op.GuildCreateGuildRobe)]
		public void GuildCreateGuildRobe(ChannelClient client, Packet packet)
		{
			var emblemMark = packet.GetByte();
			var emblemOutline = packet.GetByte();
			var stripes = packet.GetByte();
			var robeColor = packet.GetUInt();
			var badgeColor = packet.GetByte();
			var emblemMarkColor = packet.GetByte();
			var emblemOutlineColor = packet.GetByte();
			var stripesColor = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);
			var guild = creature.Guild;
			var member = creature.GuildMember;

			// Check guild and rank
			if (guild == null || member.Rank != GuildMemberRank.Leader)
			{
				Send.MsgBox(creature, Localization.Get("You need to be leader of a guild to create a Guild Robe."));
				goto L_Fail;
			}

			// Check resources
			var gp = 1000;
			var gold = 50000;

			if (guild.Points < gp)
			{
				Send.MsgBox(creature, Localization.Get("Your guild needs 1,000 GP and 50,000 Gold in order to design a Guild Robe."));
				goto L_Fail;
			}

			// Check color
			if (creature.Vars.Temp["GuildRobeColor"] == null)
			{
				Send.MsgBox(creature, Localization.Get("Invalid robe color."));
				goto L_Fail;
			}

			robeColor = (uint)creature.Vars.Temp["GuildRobeColor"];

			// Update resources
			guild.Points -= gp;
			guild.Gold -= gold;
			ChannelServer.Instance.Database.UpdateGuildResources(guild);

			// Update robe
			guild.Robe = new GuildRobe();
			guild.Robe.EmblemMark = emblemMark;
			guild.Robe.EmblemOutline = emblemOutline;
			guild.Robe.Stripes = stripes;
			guild.Robe.RobeColor = robeColor;
			guild.Robe.BadgeColor = badgeColor;
			guild.Robe.EmblemMarkColor = emblemMarkColor;
			guild.Robe.EmblemOutlineColor = emblemOutlineColor;
			guild.Robe.StripesColor = stripesColor;
			ChannelServer.Instance.Database.UpdateGuildRobe(guild);

			// Response
			Send.GuildCreateGuildRobeUpdate(creature, emblemMark, emblemOutline, stripes, robeColor, badgeColor, emblemMarkColor, emblemOutlineColor, stripesColor, true);
			Send.GuildCreateGuildRobeR(creature, true);
			return;

		L_Fail:
			Send.GuildCreateGuildRobeUpdate(creature, emblemMark, emblemOutline, stripes, robeColor, badgeColor, emblemMarkColor, emblemOutlineColor, stripesColor, false);
			Send.GuildCreateGuildRobeR(creature, false);
		}
	}
}
