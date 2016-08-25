//--- Aura Script -----------------------------------------------------------
// Big Order of Milk
//--- Description -----------------------------------------------------------
// PTJ reward quest to gather a lot of milk.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class BigOrderMilkQuestScript : QuestScript
{
	public override void OnReceive(Creature creature)
	{
		creature.Inventory.InsertStacks(63022, 15); // Empty Bottle (Part-Time Job)
	}

	public override void Load()
	{
		SetId(40004);
		SetScrollId(70023);
		SetName(L("Big Order of Milk"));
		SetDescription(L("This job is to gather food ingredients. Today's ingredient is [15 Milk Bottles].  Use the empty bottles to collect milk from the Cows."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Gather 15 Milk Bottles"), 0, 0, 0, Collect(63023, 15)); // Milk (Part-Time Job)

		AddReward(Exp(800));
		AddReward(Gold(2000));
	}
}