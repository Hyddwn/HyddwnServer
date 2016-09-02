//--- Aura Script -----------------------------------------------------------
// G1 002: The Three Missing Warriors
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Getting the Tarlach keyword from Bear Tarlach, the 3 Missing Warriors
// keyword from Duncan, and Tarlach's Locket from Stewart for the first
// RP quest.
// 
// Wiki:
// - Requirement: Level 10 or Higher and Mana Herb x 2~5
// - Instruction: Give Mana Herbs to the bear and use necessary keywords.
//---------------------------------------------------------------------------

public class TheThreeMissingWarriorsQuest : GeneralScript
{
	private const int TarlachsLocket = 73002;

	public override void Load()
	{
		AddHook("_tarlachbear", "before_gift", TarlachBearBeforeGift);
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_stewart", "before_keywords", StewartBeforeKeywords);
	}

	public async Task<HookResult> TarlachBearBeforeGift(NpcScript npc, params object[] args)
	{
		if (!npc.HasKeyword("g1_01") || npc.Favor < 15)
			return HookResult.Continue;

		npc.RemoveKeyword("g1_01");
		npc.GiveKeyword("g1_02");
		npc.GiveKeyword("g1_tarlach1");

		npc.Msg(Hide.Name, L("(The bear is writing something in the snow.)"));
		npc.Msg(Hide.Name, L("(Tar...)<p/>(Tar... la... ch.)<p/>(The bear writes the word 'Tarlach' and stares at you.)<p/>(Tarlach...)<p/>(It seems to be someone's name.)"));

		return HookResult.Break;
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword == null || keyword != "g1_tarlach1")
			return HookResult.Continue;

		npc.RemoveKeyword("g1_tarlach1");
		npc.GiveKeyword("g1_tarlach2");

		npc.Msg(L("Tar...lach? Did you say Tarlach?<br/>One of the three missing warriors..."));
		npc.Msg(Hide.Name, L("(Duncan seems shocked.)"));
		npc.Msg(L("I've always wondered about that bear.<br/>I never thought it could truly be Tarlach..."));
		npc.Msg(L("Long ago, three adventurers actually went to Tir Na Nog<br/>to rescue the black-winged Goddess<br/>and bring paradise to this world.<br/>But none of them ever returned."), npc.Image("G1_Ch03_3warriors"));
		npc.Msg(L("Hence they were known as the three missing warriors.<br/>Tarlach was one of them...<br/>It's been so long... could it really be him?<br/>Talk to Stewart at the school in Dunbarton. He knows the legend better."));

		return HookResult.Break;
	}

	public async Task<HookResult> StewartBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword == null || keyword != "g1_tarlach2")
			return HookResult.Continue;

		if (npc.HasKeyword("g1_02"))
		{
			npc.RemoveKeyword("g1_02");
			npc.GiveKeyword("g1_03");

			npc.GiveItem(TarlachsLocket);
			npc.GiveWarpScroll(63009, "alby_dungeon");

			npc.Msg(L("The three missing Warriors? The ones who are said to have gone to Tir Na Nog?"));
			npc.Msg(L("I see...<br/>There was a time when scholars debated back and forth regarding the validity of that legend."));
			npc.Msg(L("From what I remember, there were stories of Tir Na Nog circulating via word of mouth<br/>but there weren't any written records of it.<br/>In the end, it was concluded that it was only a rumor."));
			npc.Msg(L("So it's just a legend..."));
			npc.Msg(L("...What do I think...?<br/>I believe in the legend of three missing Warriors. Haha...<br/>I'm no fool though..."));
			npc.Msg(L("I'm not sure if what I just gave you will be of any help.<br/>It's a locket. You know, an accessory that you put pictures in."), npc.Image("G1_Ch04_locket01"));
			npc.Msg(L("...Don't be surprised...<br/>This is actually a memento that belonged to<br/>Tarlach, the mighty Wizard, one of the<br/>three missing Warriors."), npc.Image("G1_Ch04_locket02"));
			npc.Msg(L("Yes, the picture of the boy in the locket is Tarlach.<br/>This was probably from when he was young.<br/>Judging from how old she is, the pretty girl next to him is probably his sister."), npc.Image("G1_Ch04_locket02"));
			npc.Msg(L("That's not all, though...<br/>This locket...it's a type of a Memorial item that allows you<br/>to experience the life of the person who used to own it."));
			npc.Msg(L("Tarlach spent his entire life trying to rescue the goddess<br/>who holds the secrets of Tir Na Nog.<br/>His desires and memories remain in this locket."));
			npc.Msg(L("Try placing this on the altar to the goddess inside Alby Dungeon."));
			npc.Msg(L("Drop the locket on the Alby Dungeon altar.<br/>And get two more friends to go with you."));

			return HookResult.Break;
		}
		else if (npc.HasKeyword("g1_03"))
		{
			// Give locket again if lost
			if (!npc.HasItem(TarlachsLocket))
				npc.GiveItem(TarlachsLocket);

			npc.Msg(L("Drop the locket on the Alby Dungeon altar.<br/>And get two more friends to go with you."));

			return HookResult.Break;
		}
		else
		{
			return HookResult.Continue;
		}
	}
}
