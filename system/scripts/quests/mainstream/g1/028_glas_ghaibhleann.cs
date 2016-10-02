//--- Aura Script -----------------------------------------------------------
// G1 028: Glas Ghaibhleann
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Getting the item for the last dungeon from Dougal and killing Glas.
// See "g1_39_tirnanog_dungeon" for the rest of G1.
// 
// Wiki:
// - Requirement: 1~3-persons party 
// - Instruction: Defeat Glas Ghaibhleann.
//---------------------------------------------------------------------------

public class GlasGhaibhleannQuest : GeneralScript
{
	private const int PendantOfTheGoddess = 73026;
	private const int PendantOfTheGoddessBind = 73029;

	public override void Load()
	{
		AddHook("_dougal", "before_keywords", DougalBeforeKeywords);
	}

	public async Task<HookResult> DougalBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_revive_of_glasgavelen")
		{
			if (npc.HasKeyword("g1_37"))
			{
				npc.RemoveKeyword("g1_37");
				npc.GiveKeyword("g1_37_2");

				npc.Msg(L("There's a sudden change in the sky.<br/>What's happened?"));
				npc.Msg(L("...<p/>You finally rescued the Goddess.<br/>Congratulations."));
				npc.Msg(Hide.Name, L("(You tell Dougal what you heard from the Goddess.)"));
				npc.Msg(L("...<p/>The Goddess told you that?<br/>But why are you telling me this?<p/>...<p/>...<p/>Ahh. I see.<p/>Heh. Hahaha.<p/>This explains quite a few things."));
				npc.Msg(L("I didn't realize that the 'Added Soul Effect'<br/>that I had told you in order to make you stay here<br/>could have the same effect on me."));
				npc.Msg(L("But you already seem to have guessed it.<br/>No, you must already know about it since that is essentially why you came here,<br/>to tell me this, right?"));
				npc.Msg(L("Right...<br/>I'm also a soul from another world just like yourself.<br/>I'm probably known to<br/>your people as Glas Ghaibhleann."));
				npc.Msg(L("I came here to reclaim my body<br/>that was summoned by the people<br/>of this world against my will,"));
				npc.Msg(L("but I ended up in the body<br/>of the last remaining human in this world.<br/>My host body is weak.<br/>I cannot even walk properly in this weak Human form."));
				npc.Msg(L("Now that you have learned all about me,<br/>I will proceed to pursue my original goal.<br/>The Goddess probably sent you to me fully<br/>aware of what my intentions are."));
				npc.Msg(L("My goal?<br/>It's obvious, isn't it? I wish to reclaim the freedom of my real body.<br/>I need to free my body into the world I originally belonged to<br/>by breaking away from these shackles."));
				npc.Msg(L("The Goddess Pendant is a key into another world.<br/>If you want, I can inscribe the pattern into<br/>the pendant so it will move you to my body, which is calling me."));
				npc.Msg(L("However, my body that's being<br/>controlled by the summoner would be powerful enough to blow away mere humans.<br/>I'm not sure if I should trust you with this task."));
				npc.Msg(L("Can you make me a promise?<br/>Can you promise you will defeat my body?<br/>And return it to where it belongs?"), npc.Button(L("Yes"), "@yes"), npc.Button(L("No"), "@no"));
			}
			else if (npc.HasKeyword("g1_37_2"))
			{
				npc.Msg(L("Can you promise you will defeat my body?<br/>And return it to where it belongs?"), npc.Button(L("Yes"), "@yes"), npc.Button(L("No"), "@no"));
			}
			else if (npc.HasKeyword("g1_38"))
			{
				if (!npc.HasItem(PendantOfTheGoddessBind))
					npc.GiveItem(PendantOfTheGoddessBind);

				npc.Msg(L("Go to Albey Dungeon and offer the pendant there.<br/>If you succeed,<br/>it will only be a matter time before I free myself from being an Added Soul."));

				return HookResult.Break;
			}
			else
			{
				return HookResult.Continue;
			}

			if (await npc.Select() != "@yes")
			{
				npc.Msg(L("Oh..."));
				return HookResult.Break;
			}
			else if (!npc.HasItem(PendantOfTheGoddess))
			{
				npc.Msg(L("You don't have the pendant on you?"));
				return HookResult.Break;
			}

			npc.RemoveKeyword("g1_37");
			npc.RemoveKeyword("g1_37_2");
			npc.GiveKeyword("g1_38");

			npc.RemoveItem(PendantOfTheGoddess);
			npc.GiveItem(PendantOfTheGoddessBind);
			npc.GiveItem(Item.CreateWarpScroll(63009, "tirnanog_dungeon"));

			npc.Msg(L("Okay, I'll trust you.<br/>I guess it might not be a bad idea to trust you since<br/>you rescued the Goddess."));
			npc.Msg(L("Go to Albey Dungeon and offer this pendant there.<br/>You know how to use the Red Wings of a Goddess, right?<br/>If you succeed,<br/>it will only be a matter time before I free myself from being an Added Soul."));
		}

		return HookResult.Continue;
	}
}