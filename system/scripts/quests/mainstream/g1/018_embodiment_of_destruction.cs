//--- Aura Script -----------------------------------------------------------
// G1 018: Embodiment of Destruction, Glas Ghaibhleann
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Learning about Glas Ghaibhleann from the Duncan and Tarlach.
// 
// Wiki:
// - Investigation of Glas Ghaibhleann
//---------------------------------------------------------------------------

public class EmbodimentOfDestructionQuest : GeneralScript
{
	private const int NextQuestId = 210024;

	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_bone_of_glasgavelen")
		{
			npc.Msg(L("Of course, Glas Ghaibhleann's Bones..."));
			npc.Msg(L("After such a long time some bones will be missing or broken,<br/>and must be replaced with something...<br/>like Adamantium..."));
			npc.Msg(L("I don't know much about this topic, maybe Tarlach has more information."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_bone_of_glasgavelen" && npc.HasKeyword("g1_28"))
		{
			if (!npc.QuestActive(NextQuestId))
			{
				npc.StartQuest(NextQuestId); // Obtain the Magic Powder of Preservation
				npc.GiveWarpScroll(63009, "fiodth_dungeon");

				npc.Msg(L("...!<p/>That's it! In order to revive Glas Ghaibhleann,<br/>his bones must be exacavated and re-assembled.<br/>Any missing bones must be replaced with Adamantium."));
				npc.Msg(L("But Adamantium is difficult to shape. It must<br/>be melted at very high temperatures first. And<br/>there's a special ingredient that must be applied to the bone."));
				npc.Msg(L("It won't be enough to simply destroy the bones before it's<br/>completed. Once the Adamantium hardens, you can't destroy it. I have no<br/>doubt the Formors have already gathered all the bones<br/>and prepared their magical reagents, too."));
				npc.Msg(L("It's only a matter of time before the Fomors invade Erinn..."));
				npc.Msg(L("How do I know all this? I learned it<br/>in the land of Fomors, Tir Na Nog.<br/>Tir Na Nog is a reflection of Erinn...or perhaps it's the other way around?<br/>At any rate, the worlds are quite similar."));
				npc.Msg(L("Perhaps it would be better for you to see it first hand.<br/>Can you do me a favor?"));
				npc.Msg(L("These glasses carry the memories of my time there."), npc.Image("g1_ch25_glasses"));
				npc.Msg(L("I can't say for certain if the memories<br/>are intact, but if you wish to see what I saw through<br/>these glasses, you'll need them.<br/>Please find me some Magic Powder of Preservation."), npc.Image("g1_ch25_glasses"));
				npc.Msg(L("The powder will prevent the glasses from breaking further.<br/>This will also sustain the memories contained within,<br/>If you get me the powder, I'll preserve the glasses."));
				npc.Msg(L("You can find Magic Powder of Preservation<br/>deep inside Fiodh Dungeon."));
			}
			else
			{
				npc.Msg(L("You can find Magic Powder of Preservation<br/>deep inside Fiodh Dungeon.<br/>If you get me the powder, I'll preserve the glasses."));
			}

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
