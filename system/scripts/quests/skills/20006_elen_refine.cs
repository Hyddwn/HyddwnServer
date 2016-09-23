//--- Aura Script -----------------------------------------------------------
// Elen's Request
//--- Description -----------------------------------------------------------
// Elen asks you to get 5 Iron Ore from Barri, as reward she teaches you
// Refining.
//---------------------------------------------------------------------------

public class ElensRequestQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(20006);
		SetName("Elen's Request");
		SetDescription("I need iron ore to refine iron ingots. Go inside Barri Dungeon and get me 5 Lumps of Iron Ore and I'll give you information regarding the Refining Skill. What do you say? -Elen-");

		SetIcon(QuestIcon.Smithing);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		AddObjective("talk_elen", "Deliver 5 Lumps of Iron Ore to Elen", 0, 0, 0, Talk("elen"));

		AddReward(Exp(20));
		AddReward(Skill(SkillId.Refining, SkillRank.Novice));

		AddHook("_elen", "after_intro", ElenIntro);
		AddHook("_elen", "before_keywords", ElenKeywords);
	}

	public async Task<HookResult> ElenIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(64002, 5)) // 5 Iron Ore
			return HookResult.Continue;

		Send.Notice(npc.Player, "You have given Iron Ore to Elen.");
		npc.Player.Inventory.Remove(64002, 5); // 5 Iron Ore
		npc.CompleteQuest(this.Id);

		return HookResult.Break;
	}

	public async Task<HookResult> ElenKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword != "about_skill" || npc.HasSkill(SkillId.Refining))
			return HookResult.Continue;

		if (!npc.QuestActive(this.Id))
		{
			npc.Msg("Did my grandpa send you over this way?<br/>Hehe.... Oh, nothing.<br/>Here, take this...");
			npc.Msg("Are you interested in refining by any chance?<br/>Refining is the first step in becoming a blacksmith...");
			npc.Msg("You can use ore in its raw form.<br/>You have to melt it to extract the pure metal from it.<br/>You can simply assume that you can use the ore as it is.");
			npc.Msg("We've been looking for more ore anyway,<br/>so why don't you go to Barri Dungeon and mine some ore for us?<br/>Bring some ore and I'll teach you how to refine metal. Tee hee...<br/>Of course, if you are going to mine, you will need at least a pickaxe.");
			await npc.Select();
			npc.StartQuest(this.Id);
			npc.Close();
		}
		else
		{
			npc.Msg("Did you forget what I asked you?");
			npc.Msg("You can find ore inside Barri Dungeon over there.<br/>Of course, you can't mine with bare hands. You will need a pickaxe for that.");
		}

		return HookResult.End;
	}
}
