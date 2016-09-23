//--- Aura Script -----------------------------------------------------------
// G1 009: Succubus
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Talking to Kristell about the fomorian phrase and running Tarlach RP.
// 
// See "g1rp_15_dunbarton_rabbie_dungeon" for keyword swap.
// 
// Wiki:
// - Instruction: Clear the Rabbie RP dungeon (as Tarlach)
//---------------------------------------------------------------------------

public class SuccubusRpQuest : GeneralScript
{
	private const int TarlachsGlassesPouch = 73022;
	private const int BookOfFomor = 73052;

	public override void Load()
	{
		AddHook("_kristell", "before_keywords", KristellBeforeKeywords);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
	}

	public async Task<HookResult> KristellBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_dulbrau2")
		{
			if (npc.HasKeyword("g1_14"))
			{
				npc.RemoveKeyword("g1_14");
				npc.GiveKeyword("g1_15");

				npc.GiveItem(TarlachsGlassesPouch);
				npc.GiveWarpScroll(63009, "rabbie_dungeon");

				npc.Msg(L("...How... how did you interpret...it...<br/>It means that the Goddess is sending Fomors here?<br/>Where did you hear that...?<br/>What...? The wizard...you heard from a Druid?"));
				npc.Msg(L("...Named Tarlach...?"));
				npc.Msg(L("Ahh, I see...<br/>I understand... So Tarlach is still alive after all."));
				npc.Msg(Hide.Name, L("(Kristell closes her eyes and smiles.)"));
				npc.Msg(L("Dul Brau Dairam Shanon.<br/>Truth is, I was the<br/>one who taught Tarlach the meaning of those words."));
				npc.Msg(L("Yes...I am Bondi Gordisse.<br/>I'm what you people call Fomor. A monster..."));
				npc.Msg(L("I don't look it, eh?<br/>I have become a human with the blessings of the gods..."));
				npc.Msg(L("Ah... But I've rambled on too much...<br/>I apologize."));
				npc.Msg(L("The words on that medal...<br/>It's true that it means to seek out the power of Morrighan."));
				npc.Msg(L("But...the the goddess isn't the one sending the Fomors.<br/>I can swear on my honor as a priestess.<br/>Tarlach misunderstood that part.<br/>The Goddess has always been looking after mankind, even as a statue."));
				npc.Msg(L("That's only an amulet for Fomors<br/>There's no meaning behind those words anymore..."));
				npc.Msg(L("...<br/>Can you tell me where Tarlach is...?"));
				npc.Msg(L("...You don't seem to trust me either.<br/>Is this because I told you I was a Fomor?"));
				npc.Msg(L("I see...<br/>Well, then..."));
				npc.Msg(L("What I just gave you is the only item I've ever received from Tarlach...<br/>It's an item that holds his memory."), npc.Image("g1_ch14_glassespocket"));
				npc.Msg(L("If you go to Rabbi dungeon alone and offer this on the altar of the Goddess...<br/>You will be able to trust me.<br/>I'll give you a Red Wing of the Goddess<br/>so you can get to Rabbie Dungeon easily."), npc.Image("g1_ch14_glassespocket"));
				npc.Msg(Hide.None, L("(You receive Tarlach's Spectacle Pouch from Kristell.)"));
			}
			else if (npc.HasKeyword("g1_15"))
			{
				if (!npc.HasItem(TarlachsGlassesPouch))
					npc.GiveItem(TarlachsGlassesPouch);

				npc.Msg(L("If you go to Rabbi dungeon alone and offer the pouch on the altar of the Goddess...<br/>You will be able to trust me."));
			}

			return HookResult.Break;
		}
		else if (keyword == "g1_succubus")
		{
			npc.RemoveKeyword("g1_16");
			npc.GiveKeyword("g1_17");
			npc.RemoveKeyword("g1_succubus");
			npc.GiveKeyword("g1_message_of_kristell");

			npc.Msg(L("...<p/>Hello, I see you're back...<br/>Do you believe me now...?"));
			npc.Msg(L("I'm not trying to hurt Tarlach or anything.<br/>I only want to know how he's doing."));
			npc.Msg(L("You still seem reluctant...<br/>Well then...I have a favor to ask."));
			npc.Msg(L("Tell Tarlach how I am doing<br/>and let him know that I really want to meet him...<br/>That I have something I want to tell him...<br/>...that should be okay...right?"));
			npc.Msg(L("...If you understand how hard it is for a woman to<br/>reveal her true intentions and her past..."));
			npc.Msg(L("...Please do this favor for me..."));

			return HookResult.Break;
		}
		else if (keyword == "g1_message_of_kristell")
		{
			npc.Msg(L("Tell Tarlach how I am doing<br/>...Please do this favor for me..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_message_of_kristell")
		{
			npc.RemoveKeyword("g1_17");
			npc.GiveKeyword("g1_17_1");
			npc.RemoveKeyword("g1_message_of_kristell");

			npc.GiveItem(BookOfFomor);
			npc.StartQuest(210003); // Translating the Book of Fomors

			npc.Msg(L("Kristell's in Dunbarton?<br/>How...troubling."));
			npc.Msg(L("I'm sorry, but I'm in no position to reciprocate her feelings...<br/>But if it really is her, perhaps<br/>she can translate this book."));
			npc.Msg(Hide.Name, L("(Tarlach pulls a book from his chest.)"));
			npc.Msg(L("Could you give this to her?<br/>Tell her it's my last request to her...<br/>If she could just translate this book..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
