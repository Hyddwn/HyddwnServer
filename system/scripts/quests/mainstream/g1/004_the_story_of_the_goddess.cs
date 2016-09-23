//--- Aura Script -----------------------------------------------------------
// G1 004: The Story of the Goddess Turned into a Stone
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Talking to Duncan, Meven, and Tarlach about Morrighan.
// 
// Wiki:
// - Instruction: Learn details about the mysterious goddess.
//---------------------------------------------------------------------------

public class TheStoryOfTheGoddessQuest : GeneralScript
{
	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_meven", "before_keywords", MevenBeforeKeywords);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_goddess")
			return HookResult.Continue;

		npc.Msg(L("You want to know of the Goddess Morrighan?<br/>If it's about the Goddess, you're better off speaking to Meven..."));

		return HookResult.Break;
	}

	public async Task<HookResult> MevenBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_goddess")
			return HookResult.Continue;

		npc.GiveItem(73059); // Book: The Goddess Who Turned into Stone

		npc.RemoveKeyword("g1_04");
		npc.GiveKeyword("g1_05");
		npc.RemoveKeyword("g1_goddess");
		npc.GiveKeyword("g1_tarlach_of_lughnasadh");

		npc.Msg(Hide.Name, L("(Received The Goddess Who Turned into Stone from Meven.)"));
		npc.Msg(L("So you wish to know more about the black-winged Goddess of War and Vengeance.<br/>Please, take a look at this free reading material on Morrighan."));
		npc.Msg(L("You learned about Morrighan from Tarlach's memorial item, eh?<br/>In that case...there's something you should know."));
		npc.Msg(L("Tarlach is the only surviving member of the three warriors.<br/>I know, everyone claims he is dead...<br/>But he's actually alive. If you want to speak to him<br/>yourself, go to Sidhe Sneachta at night."));

		return HookResult.Break;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_tarlach_of_lughnasadh")
			return HookResult.Continue;

		npc.RemoveKeyword("g1_05");
		npc.GiveKeyword("g1_06");
		npc.RemoveKeyword("g1_tarlach_of_lughnasadh");
		npc.GiveKeyword("g1_book1");

		npc.Msg(L("Meven must have told you how to find me..."));
		npc.Msg(L("Hah. Sorry if I surprised you. Well, what do you think?<br/>Do you like Sidhe Sneachta at night?<br/>Yes, the bear you met earlier was me.<br/>Oh, I forgot to thank you for the Mana Herb."));
		npc.Msg(L("I have a...condition that requires me<br/>to constantly eat Mana Herb during the day...<br/>I'm allergic to it in human form.  Hence, the bear form."));
		npc.Msg(L("...You had a dream about the Goddess, didn't you?<br/>The one where she asks you to rescue her in Tir na Nog.<br/>Then you're here to find out how to get there..."));
		npc.Msg(L("Forget about it. That's no place for mortal kind."));
		npc.Msg(L("Forget it..."));
		npc.Msg(L("If you really must know, I can recommend a book to you.<br/>'Land of Eternity, Tir Na Nog.'<br/>You can find it at the Bookstore in Dunbarton."));
		npc.Msg(L("I hope that will sate your curiosity. I suggest you let the matter drop.<br/>And please, don't tell anyone I'm here.<br/>Please..."));

		return HookResult.Break;
	}
}
