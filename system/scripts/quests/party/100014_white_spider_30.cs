//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt White Spiders (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class WhiteSpider30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100014);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt White Spiders"));
		SetDescription(L("The White Spiders are a threat for people trying to collect cobwebs. I will reward you for hunting [30 White Spiders]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 White Spiders"), 0, 0, 0, Kill(30, "/whitespider/"));

		AddReward(Exp(195));
		AddReward(Gold(603));
	}
}
