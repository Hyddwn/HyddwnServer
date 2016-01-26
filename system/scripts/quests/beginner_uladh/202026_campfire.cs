//--- Aura Script -----------------------------------------------------------
// Campfire
//--- Description -----------------------------------------------------------
// Quest to learn Campfire.
//---------------------------------------------------------------------------

public class CampfireQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202026);
		SetName("Campfire");
		SetDescription("Have you ever been to an Inn before? I have something to tell you that you may find helpful. Please drop by and talk to me, while checking out the place. - Piaras -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202003)); // Save my Sheep
		AddPrerequisite(NotSkill(SkillId.Campfire));

		AddObjective("get_keyword", "Learn about the Campfire skill from Piaras", 7, 1344, 1225, GetKeyword("skill_campfire"));
		AddObjective("use_skill", "Learn how to use the Campfire skill from Deian", 1, 27953, 42287, UseSkill(SkillId.Campfire));

		AddReward(Exp(1300));
	}
}
