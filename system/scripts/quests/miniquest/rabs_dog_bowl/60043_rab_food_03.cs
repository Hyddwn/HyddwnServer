//--- Aura Script -----------------------------------------------------------
// Rab's Dog Food [Bacon and Potato Dog Food]
//--- Description -----------------------------------------------------------
// Deliver Dog food to Rab. Obtained by giving Fleta Rab's empty plate
// Rab's empty plate is earned by talking to Rab while logged on as a pet
//---------------------------------------------------------------------------

public class RabFood03QuestScript : RabFood01QuestScript
{
	public override int DogFood { get { return 50214; } } // Bacon and Potato Dog Food

	public override void Load()
	{
		SetId(60043);
		SetScrollId(70069);
		SetName("Rab's Dog Food");
		SetDescription("Rab looks like he wants some Bacon and Potato Dog Food. For a dog, he has quite a taste... Bacon and Potato Dog Food is literally a mix of bacon and potato. The ingredients should be available around Dunbarton, and you should gather up the potato yourself, right? Anyway, whatever you do, whether you cook it, buy it or anything else, it doesn't matter. Just get it for me and I'd appreciate it. - Fleta -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		AddObjective("talk_rab", "Deliver Dog Food to Rab", 0, 0, 0, Talk("rab"));
		AddObjective("talk_fleta", "Talk to Fleta.", 53, 104616, 110159, Talk("fleta"));

		AddReward(Item(52038)); // Fleta Upgrade Coupon for Heavy Armor for 1

		AddHook("_rab", "after_intro", RabIntro);
		AddHook("_fleta", "after_intro", FletaIntro);
	}
}
