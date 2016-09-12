//--- Aura Script -----------------------------------------------------------
// G1 025: Bind Magic
//--- Description -----------------------------------------------------------
// Killing a certain amount of zombies, to revive in TNN instead of Barri
// in the future.
// 
// Wiki:
// - Investigation of Added Soul Phenomenon
//---------------------------------------------------------------------------

public class BindMagicQuest : QuestScript
{
	private const int OwlDelay = 5 * 60;

	public override void Load()
	{
		SetId(210011);
		SetName(L("Bind Magic"));
		SetDescription(L("Come see me if you're worried about returning to your world upon dying. - Dougal -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		var amount = 50;
		if (IsEnabled("EasyBinding1"))
			amount = 10;
		if (IsEnabled("EasyBinding2"))
			amount = 1;

		AddObjective("talk1", L("Talk to Dougal."), 35, 15354, 38361, Talk("dougal"));
		AddObjective("kill", string.Format(LN("Eliminate {0} Zombie.", "Eliminate {0} Zombies.", amount), amount), 35, 20000, 44600, Kill(amount, "/zombie/undead/tirnanog/"));
		AddObjective("talk2", L("Report to Dougal."), 35, 15354, 38361, Talk("dougal"));

		AddReward(Exp(2200));
		AddReward(Gold(624));

		AddHook("_dougal", "after_intro", DougalAfterIntro);
	}

	public async Task<HookResult> DougalAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.HasQuest(this.Id) && !npc.HasKeyword("g1_37_1"))
		{
			npc.SendOwl(this.Id, OwlDelay);
			npc.GiveKeyword("g1_37_1");

			npc.Msg(L("You're quite brave to come to a place like this. I'm Dougal.<br/>I'm here... alone."));
			npc.Msg(L("I see that you're not from around here.<br/>I'm Dougal, the last remaining human here.<br/>I was pretty lonely. It's been a while since I've seen anyone."));
			npc.Msg(L("Why am I left here alone?<br/>It's complicated, but I didn't stay behind<br/>because I wanted to."));
			npc.Msg(L("..."));
			npc.Msg(L("What?<br/>This is Tir Na Nog?<br/>You mean the legendary paradise, Tir Na Nog?"));
			npc.Msg(L("Hah!<br/>Hahaha. I didn't expect to hear such a foolish statement from you."));
			npc.Msg(L("Look, if this truly is Tir Na Nog, the world where sickness and death don't exist,<br/>why would I have a leg like this?"));
			npc.Msg(L("Also, this town has turned into a wasteland<br/>after being invaded by the Fomors."));
			npc.Msg(L("You probably heard a false rumor somewhere.<br/>I'm sorry but this isn't the place you think it is."));
			npc.Msg(L("This is just another world that has become a land of the Fomors."));
			npc.Msg(L("But since you're here, take a look<br/>around as much as you want.<br/>Who knows?<br/>You might find this place similar to a place<br/>you are familiar with. Haha."));

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "talk1"))
		{
			npc.FinishQuest(this.Id, "talk1");

			npc.Msg(L("You've come... I was waiting for you.<br/>Because you're not from around here,<br/>I thought that losing the freedom of your body<br/>might end up getting you into big trouble."));
			npc.Msg(L("...True... Nobody but your party can help you<br/>in here.<br/>Even if your life ends and you pass away..."));
			npc.Msg(L("When you can't move...<br/>You return to your world.<br/>I do think you have to endure that fact, only because you are a human of another world..."));
			npc.Msg(L("...I've called you because a good plan has come up."));
			npc.Msg(L("Do you...know about the added soul phenomenon?<br/>It is the phenomenon where the spirit doesn't go away from one's body...<br/>I thought maybe if you were to use this trick<br/>you could let your spirit stay near here."));
			npc.Msg(L("Yes... If your spirit does not go away from your fallen body and stays near it<br/>there wouldn't be such a thing as having to resurrect back from the world you were in<br/>even if you were to lose the freedom of your body."));
			npc.Msg(L("But... dealing with spirits is also a dangerous thing...<br/>With the slightest provocation you can turn into one of those zombies behind the graveyard..."));
			npc.Msg(L("...So, I'll have to see if my thoughts are correct.<br/>If you knock down the spiritless zombies<br/>I will measure the added soul phenomenon that happens around the zombies<br/>and see if you can resurrect in this place."));
			npc.Msg(L("What do you think? Would you like to give it a try?"));

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "talk2"))
		{
			npc.CompleteQuest(this.Id);

			npc.RemoveKeyword("g1_37_1");
			npc.GiveKeyword("g1_bind");

			npc.Msg(L("It's just as I'd thought. We can use the added soul phenomenon<br/>and have your spirit stay here.<br/>If you believe me, from now on,<br/>you won't ever be an undead even if you collapse here."));
			npc.Msg(L("...Yes. My body and soul<br/>are exchanging with you the amount of strength<br/>you need to resurrect here..."));
			npc.Msg(L("Oh no... Are you uncomfortable about exchanging such things with me?<br/>Ha ha, no need to act calm.<br/>But..."));
			npc.Msg(L("Even when you can't move<br/>you can come near me.<br/>You will make it through this world with more ease if you remember this."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
