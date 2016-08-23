//--- Aura Script -----------------------------------------------------------
// Big Order of Milk
//--- Description -----------------------------------------------------------
// PTJ reward quest to gather a lot of milk.
//--- Notes -----------------------------------------------------------------
// Normally the player is provided bottles upon receiving this quest.
// To compensate, the player will be reimbursed for
// purchasing 15 bottles on quest completion.
//
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible (if QuestScript is updated).
//---------------------------------------------------------------------------

public class BigOrderMilkQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40004);
		SetScrollId(70023);
		SetName(L("Big Order of Milk"));
		SetDescription(L("This job is to gather food ingredients. Today's ingredient is [15 Milk Bottles].  Use empty bottles to collect milk from the Cows. You can buy some empty bottles at any General Store. Don't worry, you will be reimbursed for the cost."));
		//SetDescription(L("This job is to gather food ingredients. Today's ingredient is [15 Milk Bottles].  Use the provided empty bottles to collect milk from the Cows."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Gather 15 Milk Bottles"), 0, 0, 0, Collect(50021, 15)); // Milk
		//AddObjective("collect1", L("Gather 15 Milk Bottles"), 0, 0, 0, Collect(63023, 15)); // Milk (Part-Time Job)

		AddReward(Exp(800));
		AddReward(Gold(2000 + 400 * 15));
		//             Base + BottleCost | Player reimbursed for now.
	}
}