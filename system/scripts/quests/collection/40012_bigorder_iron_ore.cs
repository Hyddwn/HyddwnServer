//--- Aura Script -----------------------------------------------------------
// Big Order of Iron Ore
//--- Description -----------------------------------------------------------
// PTJ reward quest to collect a lot of iron ore.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class BigOrderIronOreQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40012);
		SetScrollId(70023);
		SetName(L("Big Order of Iron Ore"));
		SetDescription(L("This task involves going into the mines to mine out Iron Ore. Today, the task is mining out [20 Iron Ore]. Mining out the ore will require a Pickaxe, so make sure to bring that before heading over to the mines."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 20 Iron Ore"), 0, 0, 0, Collect(64002, 20));

		AddReward(Exp(800));
		AddReward(Gold(2000));
	}
}
