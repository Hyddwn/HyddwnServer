//--- Aura Script -----------------------------------------------------------
// G1 026: Collecting Pieces of Black Orb
//--- Description -----------------------------------------------------------
// Collection quest, 4 fragments for the full Black Orb.
// 
// Wiki:
// - Requirement: Small Green Gem, Small Blue Gem, Small Red Gem,
//                Small Silver Gem
// - Instruction: Collect 4 Pieces of Black Orb in Albey Dungeon.
//---------------------------------------------------------------------------

public class BlackOrbCollectionQuest : QuestScript
{
	public override void Load()
	{
		SetId(210021);
		SetName(L("Collect the Black Orb Fragments"));
		SetDescription(L("The black orb is what you need to get to the place where the strength of evil has been sealed. Please gather the black orb broken into 4 fragments. If you put the green, blue, red, and silver orbs in order in Albey Dungeon, you can go the place where the strength of the 4 black orb fragments are sealed. If you gather all 4 of the fragments the black orb will find its original shape."));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("collect1", L("Use the Small Green Gem to collect a Black Orb fragment at Albey Dungeon."), 0, 0, 0, Collect(73027, 1));
		AddObjective("collect2", L("Use the Small Blue Gem to collect a Black Orb Fragment at Albey Dungeon."), 0, 0, 0, Collect(73030, 1));
		AddObjective("collect3", L("Use the Small Red Gem to collect a Black Orb Fragment at Albey Dungeon."), 0, 0, 0, Collect(73031, 1));
		AddObjective("collect4", L("Use the Small Silver Gem to collect a Black Orb Fragment at Albey Dungeon."), 0, 0, 0, Collect(73032, 1));

		AddReward(Item(73033)); // Black Orb
	}
}
