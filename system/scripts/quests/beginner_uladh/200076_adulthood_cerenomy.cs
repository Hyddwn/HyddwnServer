//--- Aura Script -----------------------------------------------------------
// Adulthood Ceremony
//--- Description -----------------------------------------------------------
// Duncan congratulates you for becoming old.
//---------------------------------------------------------------------------

public class AdulthoodCeremonyQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200076);
		SetName("Adulthood Ceremony");
		SetDescription("I'm Duncan. You are about to come of age now. I have something to give you, can you drop by sometime?");

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedAge(20));

		AddObjective("talk", "Talk with Chief Duncan", 1, 15409, 38310, Talk("duncan"));

		AddReward(Exp(200));
		AddReward(Item(52024)); // Bouquet

		AddHook("_duncan", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");

			npc.Msg("Oh, so finally you are here. <username/>.<br/>Now you are a respectable adult too.<br/>And you must have earned necessary titles for your age...");
			npc.Msg("Up till now, the age limit for adulthood was 18,<br/>but in Erinn, the druid gives a ceremony for youths<br/>of the age like you and this special event is so they remember<br/>the taboo 'geis' which should never be violated.");
			npc.Msg("During the 2nd Mag Tuired War kids not even in their 20s<br/>fought as soldiers and the meaning of the adulthood ceremony was lost...");
			npc.Msg("Nowadays the adulthood ceremony reminds people<br/>to take responsibility for their own actions.<br/>And also to congratulate them for growing up to be<br/>healthy and respectable adults.");
			npc.Msg("Under these circumstances, I congratulate you once again.<br/>Believe in yourself and never regret the decisions you make as<br/>you walk your way through adulthood.<br/>This is about all I have to say for now.");
			npc.Msg("By the way, Nao forgot to give you this and asked me to give you this gift.<br/>Press the Mission Complete button to receive it.<br/>Nao seems to be looking after you. Ha ha...");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
