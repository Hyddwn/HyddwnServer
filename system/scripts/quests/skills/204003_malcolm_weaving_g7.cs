//--- Aura Script -----------------------------------------------------------
// Gathering Wool
//--- Description -----------------------------------------------------------
// Malcolm asks you to gather some wool, in exchange for teaching you
// the Weaving skill. Up until G7, Malcolm gave you the skill directly,
// with a quest to try it out, creating Thin Thread Balls for him.
//---------------------------------------------------------------------------

public class GatheringWoolMalcolmScript : QuestScript
{
	public override void Load()
	{
		SetId(204003);
		SetName("Gathering Wool");
		SetDescription("I'm Malcolm from the General Shop. I have a backlog of fabric orders, but I'm all out of thread. I need wool so I can make more. Think you can grab me [5 Bundles of Wool]?");
		SetAdditionalInfo("How to Gather Wool\n1. Press <hotkey name='InventoryView'/> to open your Inventory.\n2. Equip a [Gathering Knife]. If you don't have one, you may buy one from Deian at the Pasture in the northeast part of town.\n3. With the [Gathering Knife] equipped, click on a Sheep to shear its wool.\n4. If the Sheep moves, the shearing will fail.");

		AddObjective("equip_knife", "Equip Gathering Knife", 0, 0, 0, Equip("/Gathering_Knife/"));
		AddObjective("shear", "Sheared 5 Sheep", 1, 27574, 40958, Gather(60009, 5)); // Wool
		AddObjective("talk_malcolm", "Delivered 5 Bundles of Wool to Malcolm", 1, 13150, 36392, Talk("malcolm"));

		AddReward(Exp(700));
		AddReward(Item(40759, 5)); // Stamina 30 Potion
		AddReward(Skill(SkillId.Weaving, SkillRank.Novice));

		AddHook("_malcolm", "after_intro", IntroHook);
		AddHook("_malcolm", "before_keywords", KeywordsHook);
	}

	public async Task<HookResult> IntroHook(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_malcolm"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(60009, 5)) // 5 Wool
			return HookResult.Continue;

		npc.Player.Inventory.Remove(60009, 5); // 5 Wool
		Send.Notice(npc.Player, L("You have given Wool to Malcolm."));
		npc.FinishQuest(this.Id, "talk_malcolm");

		npc.Msg("Thank you.<br/>Thanks to you I was able to complete the fabric orders.<br/>I will teach you the Weaving skill as promised.");

		return HookResult.Break;
	}

	public async Task<HookResult> KeywordsHook(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword != "about_skill" || npc.HasSkill(SkillId.Weaving))
			return HookResult.Continue;

		if (!npc.QuestActive(this.Id))
		{
			npc.Msg("Have you heard of the Weaving skill?<br/>It is a skill of spinning yarn from natural materials and making fabric.");
			npc.Msg("Do you want to learn the Weaving skill?<br/>Actually, I'm out of thick yarn and can't meet all the orders for fabric...<br/>If you get me some wool, I'll teach you the Weaving skill in return.<br/>An owl will deliver you a note on how to find wool if you wait outside.");
			await npc.Select();
			npc.StartQuest(this.Id);
			npc.Close();
		}
		else
		{
			// Unofficial
			npc.Msg("If you get me some wool, I'll teach you the Weaving skill in return.");
		}

		return HookResult.End;
	}
}
