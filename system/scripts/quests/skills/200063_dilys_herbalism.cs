//--- Aura Script -----------------------------------------------------------
// Gather Base Herb (Dilys)
//--- Description -----------------------------------------------------------
// Dilys asks you to gather 1 Base Herb for her, in exchange for teaching
// you Herbalism.
//---------------------------------------------------------------------------

public class GatherBaseHerbDilysQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200063);
		SetName("Gather Base Herb");
		SetDescription("Didn't you say you were interested in Herbalism? If you get me [1 base herb] from Ciar Dungeon, I will teach you the Herbalism skill as a reward. - Dilys");

		AddObjective("gather", "Gather 1 Base Herb", 0, 0, 0, Gather(51104, 1)); // 1 Base Herb
		AddObjective("talk", "Give 1 Base Herb to Dilys", 6, 1107, 1050, Talk("dilys"));

		AddReward(Exp(500));
		AddReward(Skill(SkillId.Herbalism, SkillRank.Novice));

		AddHook("_dilys", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(51104, 1)) // 1 Base Herb
			return HookResult.Continue;

		Send.Notice(npc.Player, "You have given Base Herb to Dilys.");
		npc.Player.Inventory.Remove(51104, 1); // 1 Base Herb
		npc.FinishQuest(this.Id, "talk");

		npc.Msg("You've come, <username/>.<br/>And with the base herb. Well done.");
		npc.Msg("Okay, I'll tell you about Herbalism.<br/>Herbalism is the knowledge to be able<br/>to classify the physiological effect of eating herbs.");
		npc.Msg("Some herbs create a specific effect.<br/><image name='bloody_herb'/>This herb is known as Bloody Herb for its red color.<br/>It contains an element that strengthens your life force.");
		npc.Msg("<image name='sunlight_herb'/>This herb is known as Sunlight Herb for its yellow color.<br/>It helps increase endurance.<br/>Mostly used in making tonics.");
		npc.Msg("<image name='base_herb'/>The Base Herb functions as a stabilizer to stabilize the virtue of several herbs mixed altogether,<br/>so this herb is often used when making medicine.");
		npc.Msg("<image name='G1_Ch01_manaherb'/>The Mana Herb is a plant that grows on the mana of Eweca.<br/>If properly processed, this herb can be used to make medicine that recovers mana.");
		npc.Msg("This processing step would be the Potion Making skill, right?<br/>Buy a Potion Making Kit and try it out for yourself. Ha ha. I just happen to have some with me...");
		npc.Msg("So now, <username/>, you are an Herbalism beginner .<br/>From now on, please help me out with the work in my store.");

		return HookResult.Break;
	}
}
