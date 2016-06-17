// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Quests;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using System;
using System.Linq;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class QuestHelper
	{
		public static void AddQuest(this Packet packet, Quest quest)
		{
			if (quest.Data == null)
				throw new Exception("AddQuest: Missing quest data for '" + quest.Id.ToString() + "'.");

			packet.PutLong(quest.UniqueId);
			packet.PutByte(0);

			packet.PutLong(quest.QuestItem.EntityId);

			packet.PutByte((byte)quest.Data.Type); // 0 = blue icon, 2 = normal, 4 = exploration, 7 = shadow (changes structure slightly)
			// Client values that might make sense:
			// Delivery: 1? (id == 506401?)
			// Event: 1? ((this + 80) == 18?)
			// Homework: 1? ((this + 80) >= 10000?)
			// Exploration: 4|5
			// Escort: 6
			// Shadow: 7|8
			// Bingo: 9
			// GameQuest: 2|4|5
			// GuildQuest: 0 (id >= 110001 < 120000?)
			// PartyQuest: 0 (id >= 100000 < 110000?), blue icon with party symbol, turns yellow if active

			packet.PutInt(quest.Id); // Range is important for the tabs.
			// 201000~201999 : event
			// 202000~209999 : normal
			// 210000~239999 : goddess
			// 240000~289999 : normal
			// 290000~290599 : alchemist
			// 290600~291999 : normal
			// 292000~292599 : alchemist
			// 292600~292999 : normal
			// 293000~293599 : alchemist
			// 293600~293999 : shakespear (default)
			// 294000~294599 : shakespear (hamlet)
			// 294600~294999 : shakespear (default)
			// 295000~295599 : shakespear (romeo and juliet)
			// 295600~295999 : shakespear (default)
			// 296000~296599 : shakespear (merchant)
			// 296600~296999 : shakespear (default)
			// 297000~297599 : shakespear (macbeth)
			// 297600~______ : normal

			packet.PutString(quest.Data.Name);
			packet.PutString(quest.Data.Description);
			packet.PutString(quest.Data.AdditionalInfo);

			packet.PutInt(1);
			packet.PutInt(quest.QuestItem.Info.Id);
			packet.PutByte(quest.Data.Cancelable);
			packet.PutByte(0);
			packet.PutByte(0); // 1 = blue icon
			packet.PutByte(0);

			// [180300, NA166 (18.09.2013)] ?
			{
				packet.PutByte(0);
				packet.PutByte(0);
				packet.PutByte(0);
			}

			// [190200, NA203 (22.04.2015)] ?
			{
				packet.PutByte(0);
			}

			// [190200, NA223 (2016-03-17)] ?
			{
				packet.PutByte(0);
				packet.PutByte(0);
			}

			// [200100, NA226 (2016-04-14)] ?
			{
				packet.PutByte(0);
			}

			// [200100, NA229 (2016-06-16)] ?
			{
				packet.PutByte(0);
				packet.PutInt(3); // tab, 0=tutorial, 1=event, 2=repeat, 3=basic, 4=skill, 5=the divine knights, 6=saga, 7=shakespeare, 8=alchemist, 9=iria, 10=advent of the goddess, 11=squires, 12=field, 13=midsummer mission
				packet.PutInt(0); // [category], 0=none, 1=adventure, 2=close combat, 3=magic, 4=archery, 5=mercantile, 6=battle alchemy, 7=martial arts, 8=music, 9=puppetry, 10=lance, 11=holy arts, 12=transmutation, 13=cooking, 14=smithing, 15=tailoring, 16=medicine, 17=caprentry, 18=gunslinger, 19=ninja, 20=merlin, 21=starlet, 22=professor j, 23=culinary artist, 24=treasure hunter, 25=event, 26=guide, 27=normal, 28=life, 29=common, 30=action, 31=paladin, 32=hidden talent, 33=contents, 34=exploration, 35=partner, 36=guild, 37=daily, 38=party, 39=tutorial, 40=part-time job, 41=return incentive, 42=daily, 43=weekly, 44=monthly
				packet.PutInt(0); // icon, 0=normal, 1=blue, 2=money, 3=theater mission, 4=movie, 5=blue party, 6=exp, 7=seal, 8=pink book, 9=yellow party, 10=chest, 11=romeo and juliet, 12=the divine knights, 13=gold pouch, 14=scroll, 15=shakespeare side quest, 16=blue board, 17=normal, 18=mainstream book, 19=Merchant of Venice, 20=sword, 21=exploration, 22=chapter3, 23=macbeth, 24=bow, 25=exploration cap, 26=shadow mission, 27=light blue party, 28=hexagram, 29=beginner, 30=hamlet, ...
			}

			packet.PutString(""); // data\gfx\image\gui_temporary_quest.dds
			packet.PutInt(0);     // 4, x y ?
			packet.PutInt(0);
			packet.PutString(""); // <xml soundset="4" npc="GUI_NPCportrait_Lanier"/>
			packet.PutString(quest.MetaData.ToString());

			switch (quest.Data.Type)
			{
				case QuestType.Deliver:
					packet.PutInt((int)quest.Data.PtjType);
					packet.PutInt(quest.Data.StartHour);
					packet.PutInt(quest.Data.ReportHour);
					packet.PutInt(quest.Data.DeadlineHour);
					packet.PutLong(quest.Deadline);
					break;

				default:
					packet.PutInt(0);
					packet.PutInt(0);
					break;
			}

			packet.PutInt(quest.Data.Objectives.Count);
			foreach (var objectiveData in quest.Data.Objectives)
			{
				var objective = objectiveData.Value;
				var progress = quest.GetProgress(objectiveData.Key);

				// Objective
				packet.PutByte((byte)objective.Type);
				packet.PutString(objective.Description);
				packet.PutString(objective.MetaData.ToString());

				// 3  - TARGECHAR:s:shamala;TARGETCOUNT:4:1; - Ask Shamala about collecting transformations
				// 14 - TARGETITEM:4:40183;TARGETCOUNT:4:1; - Break a nearby tree
				// 1  - TGTSID:s:/brownphysisfoxkid/;TARGETCOUNT:4:10;TGTCLS:2:0; - Hunt 10 Young Brown Physis Foxes
				// 9  - TGTSKL:2:23002;TGTLVL:2:1;TARGETCOUNT:4:1; - Combat Mastery rank F reached
				// 19 - TGTCLS:2:3906;TARGETCOUNT:4:1; - Win at least one match in the preliminaries or the finals of the Jousting Tournament.
				// 18 - TGTCLS:2:3502;TARGETCOUNT:4:1; - Read the Author's Notebook.
				// 4  - TARGECHAR:s:duncan;TARGETITEM:4:75473;TARGETCOUNT:4:1; - Deliver the Author's Notebook to Duncan.
				// 15 - TGTLVL:2:15;TARGETCOUNT:4:1; - Reach Lv. 15
				// 2  - TARGETITEM:4:52027;TARGETCOUNT:4:10;QO_FLAG:4:1; - Harvest 10 Bundles of Wheat
				// 22 - TGTSID:s:/ski/start/;TARGETITEM:4:0;EXCNT:4:0;TGITM2:4:0;EXCNT2:4:0;TARGETCOUNT:4:1; - Click on the Start Flag.
				// 47 - TARGETCOUNT:4:1;TGTCLS:4:730205; - Clear the Monkey Mash Mission.
				// 52 - QO_FLAG:b:true;TARGETCOUNT:4:1; - Collect for the Transformation Diary
				// 50 - TARGETRACE:4:9;TARGETCOUNT:4:1; - Transform into a Kiwi.
				// 54 - TARGETRACE:4:9;TARGETCOUNT:4:1; - Collect Frail Green Kiwi perfectly.
				// 31 - TARGETCOUNT:4:1;TGTSID:s:/Gathering_Knife/; - Equip Gathering Knife
				// 32 - TARGETCOUNT:4:5;TARGETITEM:4:60009;TGTSID:s:/Gathering_Knife/; - Sheared 5 Sheep
				// 13 - TARGETCOUNT:4:1;TGTCLS:s:TirCho_Ciar_Low_Dungeon; - Clear Ciar Basic Dungeon

				// Type theory:
				// 1  : Kill x of y
				// 2  : Collect x of y
				// 3  : Talk to x
				// 4  : Bring x to y
				// 9  : Reach rank x on skill y
				// 13 : Clear dungeon
				// 14 : ?
				// 15 : Reach lvl x
				// 18 : Do something with item x ?
				// 19 : Clear something, like jousting or a dungeon?
				// 31 : Equip item, matching tag X
				// 32 : Gather item x, gather equipment doesn't seem to matter

				// Progress
				packet.PutInt(progress.Count);
				// [180600, NA187 (25.06.2014)] ?
				{
					packet.PutFloat(0);
				}
				packet.PutByte(progress.Done);
				packet.PutByte(progress.Unlocked);

				// Target location
				packet.PutByte(objective.RegionId > 0);
				if (objective.RegionId > 0)
				{
					packet.PutInt(objective.RegionId);
					packet.PutInt(objective.X);
					packet.PutInt(objective.Y);
				}
			}

			// Rewards
			packet.PutByte((byte)quest.Data.RewardGroups.Count);
			foreach (var group in quest.Data.RewardGroups.Values.OrderBy(a => a.Id))
			{
				// Group id has to be !0 for client to display rewards for PTJs
				packet.PutByte((byte)group.Id);
				packet.PutByte((byte)group.Type);
				packet.PutByte(group.PerfectOnly);

				packet.PutByte((byte)group.Rewards.Count);
				foreach (var reward in group.Rewards)
				{
					packet.PutByte((byte)reward.Type);
					packet.PutString(reward.ToString());
					packet.PutByte((byte)reward.Result);
					packet.PutByte(reward.Visible);

					// If false, displays "(시간초과시)" (timeout) after the
					// reward. Probably not used anymore, maybe players could
					// still get certain rewards after a timeout at some
					// point?
					packet.PutByte(true);
				}
			}

			packet.PutByte(0);
		}

		public static void AddQuestUpdate(this Packet packet, Quest quest)
		{
			var progress = quest.GetList();

			packet.PutLong(quest.UniqueId);
			packet.PutByte(1);
			packet.PutInt(progress.Count);
			foreach (var p in progress)
			{
				packet.PutInt(p.Count);
				// [180600, NA187 (25.06.2014)] ?
				{
					packet.PutFloat(0);
				}
				packet.PutByte(p.Done);
				packet.PutByte(p.Unlocked);
			}
			packet.PutByte(0);
			packet.PutByte(0);
		}
	}
}
