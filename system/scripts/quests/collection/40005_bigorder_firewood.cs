//--- Aura Script -----------------------------------------------------------
// Big Order of Firewood
//--- Description -----------------------------------------------------------
// PTJ reward quest to collect a lot of firewood.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class BigOrderFirewoodQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40005);
		SetScrollId(70023);
		SetName(L("Big Order of Firewood"));
		SetDescription(L("To gather firewood, you will need [a gathering axe]. Please collect [50 pieces of firewood]."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 50 Pieces of Firewood"), 0, 0, 0, Collect(63002, 50));

		AddReward(Exp(900));
		AddReward(Gold(1000));
	}
}
