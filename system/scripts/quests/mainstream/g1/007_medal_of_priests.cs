//--- Aura Script -----------------------------------------------------------
// G1 007: Medal of Priests
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Talk to Comgan, Kristell, Endelyon, and Meven about the Fomor Medal,
// to be directed to Goro.
// 
// Wiki:
// - Instruction: Talk to Comgan in Bangor, Kristell in Dunbarton,
//                and Endelyon in Tir Chonaill about "Evil Medal",
//                then ask Meven.
//---------------------------------------------------------------------------

public class MedalOfPriestsQuest : GeneralScript
{
	private const int FomorMedal = 73021;

	public override void Load()
	{
		AddHook("_comgan", "before_keywords", ComganBeforeKeywords);
		AddHook("_kristell", "before_keywords", KristellBeforeKeywords);
		AddHook("_endelyon", "before_keywords", EndelyonBeforeKeywords);
		AddHook("_meven", "before_keywords", MevenBeforeKeywords);
	}

	public async Task<HookResult> ComganBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_medal_of_fomor")
			return HookResult.Continue;

		if (npc.HasItem(FomorMedal))
		{
			if (npc.HasKeyword("g1_11"))
			{
				npc.RemoveKeyword("g1_paradise");
				npc.RemoveKeyword("g1_11");
				npc.GiveKeyword("g1_11_1");

				npc.Msg(L("Fomor... Medal? There's such a thing?<br/>Could I see it?"));
				npc.Msg(L("Strange... This is just a medal Priests use..."), npc.Image("g1_ch11_12_fomormedal01"));
				npc.Msg(L("Every Priest has one because they say it<br/>contains the principles that created this world..."));
				npc.Msg(L("I wonder if a Fomor found one that was lost by a Priest...<br/>Hmm...I have mine here with me...<br/>Did you speak to any of the other Priests?"));
			}
			else if (npc.HasKeyword("g1_11_2"))
			{
				npc.RemoveKeyword("g1_11_1");
				npc.RemoveKeyword("g1_11_2");
				npc.GiveKeyword("g1_12");
				npc.RemoveKeyword("g1_medal_of_fomor");
				npc.GiveKeyword("g1_voucher_of_priest");

				npc.Msg(L("(Missing dialog: Talking to Comgan after talking to Meven."));
			}
			else
			{
				npc.Msg(L("Did you speak to any of the other Priests?"));
			}
		}
		else
		{
			npc.Msg(L("Fomor... Medal? There's such a thing?<br/>Could I see it?"));
		}

		return HookResult.Break;
	}

	public async Task<HookResult> KristellBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_medal_of_fomor")
			return HookResult.Continue;

		npc.Msg(L("That's...a medal worn by Priests.<br/>If you look on the back, you can find a token with<br/>the date it was given and the individual's name..."));
		if (npc.HasItem(FomorMedal))
		{
			npc.Msg(L("What? A Fomor had this? Impossible..."));
			npc.Msg(Hide.Name, L("(Kristell checked the back of the medal.)"));
			npc.Msg(L("...!"), npc.Image("g1_ch11_12_fomormedal02"));
			npc.Msg(L("Oh... it's nothing.<br/>I'm quite busy, I should get going..."));
		}

		return HookResult.Break;
	}

	public async Task<HookResult> EndelyonBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_medal_of_fomor")
			return HookResult.Continue;

		npc.Msg(L("(Missing dialog: Information about Fomor Medal."));

		return HookResult.Break;
	}

	public async Task<HookResult> MevenBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_medal_of_fomor")
			return HookResult.Continue;

		if (npc.HasItem(FomorMedal))
		{
			if (npc.HasKeyword("g1_11"))
			{
				npc.RemoveKeyword("g1_paradise");
				npc.RemoveKeyword("g1_11");
				npc.RemoveKeyword("g1_11_1");
				npc.GiveKeyword("g1_11_2");

				npc.Msg(L("Medal...? Could...I see it?"));
				npc.Msg(L("That's a Priest's Token."), npc.Image("g1_ch11_12_fomormedal01"));
				npc.Msg(L("...Hmm...<br/>You got this from a Fomor?<br/>I don't believe this..."));
				npc.Msg(L("Wait...The material and the weight<br/>are slightly different from ones distributed by the Pontiff's Court."));
				npc.Msg(L("There's some writing on the back...but I can't read it.<br/>It looks just like Fomors writing..."), npc.Image("g1_ch11_12_fomormedal02"));
				npc.Msg(L("There's no way humans can read this...<br/>Maybe a Fomor could..."));
			}
			else if (npc.HasKeyword("g1_11_1"))
			{
				npc.RemoveKeyword("g1_11_1");
				npc.RemoveKeyword("g1_11_2");
				npc.GiveKeyword("g1_12");
				npc.RemoveKeyword("g1_medal_of_fomor");
				npc.GiveKeyword("g1_voucher_of_priest");

				npc.Msg(L("Medal...? Could...I see it?"));
				npc.Msg(L("That's a Priest's Token."), npc.Image("g1_ch11_12_fomormedal01"));
				npc.Msg(L("...Hmm...<br/>You got this from a Fomor?<br/>I don't believe this..."));
				npc.Msg(L("Wait...The material and the weight<br/>are slightly different from ones distributed by the Pontiff's Court."));
				npc.Msg(L("There's some writing on the back...but I can't read it.<br/>It looks just like Fomors writing..."), npc.Image("g1_ch11_12_fomormedal02"));
				npc.Msg(L("There's no way humans can read this...<br/>Maybe a Fomor could..."));
			}
			else
			{
				npc.Msg(L("There's no way humans can read this...<br/>Maybe a Fomor could..."));
			}
		}
		else
		{
			npc.Msg(L("Medal...? Could...I see it?"));
		}

		return HookResult.Break;
	}
}
