//--- Aura Script -----------------------------------------------------------
// Rab's Dog Food [Dog Biscuit]
//--- Description -----------------------------------------------------------
// Deliver Dog food to Rab. Obtained by giving Fleta Rab's empty plate
// Rab's empty plate is earned by talking to Rab while logged on as a pet
//---------------------------------------------------------------------------

public class RabFood02QuestScript : RabFood01QuestScript
{
	public override int DogFood { get { return 50213; } } // Dog Biscuit

	public override void Load()
	{
		SetId(60042);
		SetScrollId(70069);
		SetName("Rab's Dog Food");
		SetDescription("Rab looks like he wants some Dog Biscuit. Seriously.. is he spoiled or what... Anyway, Dog Biscuit can be made by mixing butter biscuit and chocolatechip cookies. I think they sell those ingredients in Emain Macha. You can make it, or you can ask someone else to make it, or whatever. If you can just get it for me, I'd appreciate it. - Fleta -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		AddObjective("talk_rab", "Deliver Dog Biscuit to Rab", 0, 0, 0, Talk("rab"));
		AddObjective("talk_fleta", "Talk to Fleta.", 53, 104616, 110159, Talk("fleta"));

		AddReward(Item(52038)); // Fleta Upgrade Coupon for Heavy Armor for 1

		AddHook("_rab", "after_intro", RabIntro);
		AddHook("_fleta", "after_intro", FletaIntro);
	}
}
