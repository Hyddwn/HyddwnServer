//--- Aura Script -----------------------------------------------------------
// Collect the Coyote's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class CoyoteScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71050);
		SetScrollId(70124);
		SetName(L("Collect the Coyote's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Coyote Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Coyote Fomor Scrolls"), 0, 0, 0, Collect(71050, 10));

		AddReward(Gold(3700));
	}
}
