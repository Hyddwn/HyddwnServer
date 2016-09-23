//--- Aura Script -----------------------------------------------------------
// G1 022: RP Quest - Shiela's Memory
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Playing through the Mores RP again, this time as Shiela's Ghost.
// 
// See dungeon script "g1rp_31_dunbarton_math_dungeon".
// 
// Wiki:
// - Requirement: 2-persons RP quest (Party leader = Shiela, Member = Mores)
// - Instruction: Clear the RP dungeon.
//---------------------------------------------------------------------------

public class ShielasMemoryQuest : GeneralScript
{
	private const int Torque = 73005;

	public override void Load()
	{
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
		AddHook("_eavan", "before_keywords", EavanBeforeKeywords);
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_cichol")
		{
			npc.GiveKeyword("g1_33_2");

			npc.Msg(L("Say what!? Someone is pretending to be Morrighan?<br/>White winged...and an obscured face?"));
			npc.Msg(L("I don't believe this! Damn it!<br/>I've been deceived.<br/>Why didn't I think of that?"));
			npc.Msg(L("It's Cichol.<br/>There's no doubt. The god of the Fomors.<br/>I can't forgive him.<br/>He dared deceive not only my friends, but also my master!"), npc.Image("g1_ch32_cichol"));
			npc.Msg(L("Even the Goddess couldn't escape from his power!<br/>No wonder the Goddess was acting so strange."));
			npc.Msg(L("Thank you for letting me know, <username/>.<br/>I was utterly oblivious.<br/>I'm ashamed that I doubted the grace of the Goddess.<br/>even with my power as a druid."));

			// Newer versions of G1 have the dream cutscene here.

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> EavanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_memorial4" && npc.HasKeyword("g1_34_1"))
		{
			if (!npc.HasItem(Torque))
				npc.GiveItem(Torque);

			npc.Msg(L("(Missing dialog: Eavan giving you back the lost Torque.)"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
