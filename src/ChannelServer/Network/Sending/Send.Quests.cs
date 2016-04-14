// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World.Quests;
using System.Collections;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Channel.World;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		///  Sends NewQuest to creature's client.
		/// </summary>
		/// <param name="character"></param>
		/// <param name="quest"></param>
		public static void NewQuest(Creature character, Quest quest)
		{
			var packet = new Packet(Op.NewQuest, character.EntityId);
			packet.AddQuest(quest);

			character.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="quest"></param>
		public static void QuestUpdate(Creature creature, Quest quest)
		{
			var packet = new Packet(Op.QuestUpdate, creature.EntityId);
			packet.AddQuestUpdate(quest);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts QuestUpdate in party.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="quest"></param>
		public static void QuestUpdate(Party party, Quest quest)
		{
			var packet = new Packet(Op.QuestUpdate, 0);
			packet.AddQuestUpdate(quest);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Broadcasts QuestOwlNew in range of creature.
		/// </summary>
		/// <remarks>
		/// Effect of an owl delivering the new quest.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="questId"></param>
		public static void QuestOwlNew(Creature creature, long questId)
		{
			var packet = new Packet(Op.QuestOwlNew, creature.EntityId);
			packet.PutLong(questId);

			// Creature don't have a region in Soul Stream.
			if (creature.Region != Region.Limbo)
				creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts QuestOwlComplete in range of creature.
		/// </summary>
		/// <remarks>
		/// Effect of an owl delivering the rewards.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="questId"></param>
		public static void QuestOwlComplete(Creature creature, long questId)
		{
			var packet = new Packet(Op.QuestOwlComplete, creature.EntityId);
			packet.PutLong(questId);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends QuestClear to creature's client.
		/// </summary>
		/// <remarks>
		/// Removes quest from quest log.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="questId"></param>
		public static void QuestClear(Creature creature, long questId)
		{
			var packet = new Packet(Op.QuestClear, creature.EntityId);
			packet.PutLong(questId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CompleteQuestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void CompleteQuestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.CompleteQuestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends CompleteQuestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GiveUpQuestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GiveUpQuestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestStartPtj to creature's client, which starts the clock.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="uniqueQuestId"></param>
		public static void QuestStartPtj(Creature creature, long uniqueQuestId)
		{
			var packet = new Packet(Op.QuestStartPtj, creature.EntityId);
			packet.PutLong(uniqueQuestId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestEndPtj to creature's client, which stops the clock.
		/// </summary>
		/// <param name="creature"></param>
		public static void QuestEndPtj(Creature creature)
		{
			var packet = new Packet(Op.QuestEndPtj, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends QuestUpdatePtj to creature's client, updates the char info window.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="type"></param>
		/// <param name="done"></param>
		/// <param name="success"></param>
		public static void QuestUpdatePtj(Creature creature, PtjType type, int done, int success)
		{
			var packet = new Packet(Op.QuestUpdatePtj, creature.EntityId);
			packet.PutInt((int)type);
			packet.PutInt(done);
			packet.PutInt(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends SetQuestTimer to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="time">Time in milliseconds</param>
		/// <param name="task">Task to fulfill</param>
		/// <param name="deadline">Deadline msg (include {0} where the timer should appear)</param>
		/// <param name="counter">Counter msg (e.g. "Remaining Sheep: 20")</param>
		public static void SetQuestTimer(Creature creature, int time, string task, string deadline, string counter, int amount)
		{
			var packet = new Packet(Op.SetQuestTimer, MabiId.Broadcast);
			packet.PutInt(time);
			packet.PutString(task);
			packet.PutString(deadline);
			packet.PutString(counter, amount);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends RemoveQuestTimer to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void RemoveQuestTimer(Creature creature)
		{
			var packet = new Packet(Op.RemoveQuestTimer, MabiId.Broadcast);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends UpdateQuestTimerCounter to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="counter">Counter msg (e.g. "Remaining Sheep: 20")</param>
		public static void UpdateQuestTimerCounter(Creature creature, string counter, int amount)
		{
			var packet = new Packet(Op.UpdateQuestTimerCounter, MabiId.Broadcast);
			packet.PutString(counter, amount);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PartySetQuestR to creature's client.
		/// </summary>
		/// <param name="party"></param>
		/// <param name="success"></param>
		public static void PartySetQuestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartySetQuestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PartyUnsetQuestR to creature's client.
		/// </summary>
		/// <param name="party"></param>
		/// <param name="success"></param>
		public static void PartyUnsetQuestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyUnsetQuestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts PartySetActiveQuest in party.
		/// </summary>
		/// <param name="party"></param>
		/// <param name="uniqueQuestId"></param>
		public static void PartySetActiveQuest(Party party, long uniqueQuestId)
		{
			var packet = new Packet(Op.PartySetActiveQuest, 0);
			packet.PutLong(uniqueQuestId);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Broadcasts PartyUnsetActiveQuest in party.
		/// </summary>
		/// <param name="party"></param>
		/// <param name="uniqueQuestId"></param>
		public static void PartyUnsetActiveQuest(Party party, long uniqueQuestId)
		{
			var packet = new Packet(Op.PartyUnsetActiveQuest, 0);
			packet.PutLong(uniqueQuestId);
			packet.PutByte(1);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Sends SpecialUnitInfoRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void SpecialUnitInfoRequestR(Creature creature)
		{
			var packet = new Packet(Op.SpecialUnitInfoRequestR, 0);

			packet.PutInt(0); // count
			{
				//002 [............0001] Short  : 1
				//003 [........00000005] Int    : 5
				//004 [............0001] Short  : 1
			}

			packet.PutInt(0); // count
			{
				//093 [............0001] Short  : 1
				//094 [................] String : g20_mk_kanna
				//095 [........00000001] Int    : 1
				//096 [..............01] Byte   : 1
				//097 [............0003] Short  : 3
				//098 [............0003] Short  : 3
				//099 [............0002] Short  : 2
				//100 [............0001] Short  : 1
				//101 [............0001] Short  : 1
				//102 [............0001] Short  : 1
				//103 [............0001] Short  : 1
				//104 [................] String : I-I'm sorry. I would like some counseling before I begin my mission. Would that be okay?|I'm sorry, captain. I really need some counseling. I'm so exhausted.
				//105 [........00000005] Int    : 5
				//106 [............0001] Short  : 1
				//107 [........0000001E] Int    : 30
				//108 [................] String : Hee hee, all clear, sir!
				//109 [................] String : I will report on the mission!
				//110 [............0002] Short  : 2
				//111 [........00000064] Int    : 100
				//112 [................] String : Hee hee, all clear, sir! No wait, not all clear! I need your help!
				//113 [................] String : I will report on the mission!
				//114 [............0003] Short  : 3
				//115 [........00000046] Int    : 70
				//116 [................] String : Ahaha, sorry. I kind of broke something... So I'm helping them out in return. Ahaha.|Ahh, what do I do? I was practicing my sword strokes at the inn, and split the bed in two!
				//117 [................] String : I will report on the mission!
				//118 [............0004] Short  : 4
				//119 [........00000064] Int    : 100
				//120 [................] String : Ahaha, I left for my mission without bringing my wallet... So I've been sleeping rough the whole time.|Gahh... Captain, have you seen my sword? Where is it? Where did I leave it?
				//121 [................] String : I will report on the mission!
				//122 [............0005] Short  : 5
				//123 [........00000064] Int    : 100
				//124 [................] String : Hee hee hee, that's it for missions! Don't worry. I'm completely focused on my duties so I can see you that much sooner!|Heh, what do you know? Doing things your way makes everything go much smoother, captain.
				//125 [................] String : I will report on the mission!
			}

			packet.PutInt(0); // count
			{
				//292 [........0009EB75] Int    : 650101
				//293 [............0001] Short  : 1
				//294 [............0000] Short  : 0
				//295 [..............01] Byte   : 1
				//296 [..............00] Byte   : 0
				//297 [............0000] Short  : 0
				//298 [............0000] Short  : 0
				//299 [000000000020F580] Long   : 2160000
				//300 [................] String : Sharp teeth from mutant animals were found near Ciar Dungeon.
				//301 [................] String : All dungeons around Tir Chonaill were checked, but no pollutant was found.
				//302 [............0001] Short  : 1
				//303 [............0000] Short  : 0
				//304 [............0000] Short  : 0
				//305 [............0001] Short  : 1
				//306 [............0000] Short  : 0
				//307 [............0000] Short  : 0
				//308 [............0000] Short  : 0
				//309 [................] String : Check Foul Plume
				//310 [................] String : Mutant animals appear around Ciar Dungeon time to time. Investigate to see if there are any remaining traces of mutant animals.
				//311 [................] String : * [Reward] Basic Baltane Seal x1\n* Command EXP 5\n* Training Points 3\n
				//312 [........00000000] Int    : 0
			}

			packet.PutLong(604800000);
			packet.PutLong(2160000);
			packet.PutShort(2);
			packet.PutShort(30);
			packet.PutInt(1);
			packet.PutShort(1);
			packet.PutShort(1);

			creature.Client.Send(packet);
		}
	}
}
