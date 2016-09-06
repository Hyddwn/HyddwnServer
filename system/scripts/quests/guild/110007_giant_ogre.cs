//--- Aura Script -----------------------------------------------------------
// Eliminate Giant Ogre
//--- Description -----------------------------------------------------------
// Guild quest to kill a Giant Ogre.
//---------------------------------------------------------------------------

public class GiantOgreGuildQuest : QuestScript
{
	public override void Load()
	{
		SetId(110007);
		SetScrollId(70152);
		SetName(L("Eliminate Giant Ogre"));
		SetDescription(L("Defeat [Giant Ogre] that sometimes appears in Gairech."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Kill 1 Giant Ogre"), 0, 0, 0, Kill(1, "/ogre/giantogre2/"));

		AddReward(Exp(9500));
		AddReward(Gold(13800));
	}
}
