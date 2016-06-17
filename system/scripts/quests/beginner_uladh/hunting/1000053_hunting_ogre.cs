//--- Aura Script -----------------------------------------------------------
// Hunt 1 Ogre
//--- Description -----------------------------------------------------------
// Twentieth  hunting quest beginner quest series, started automatically
// after completing Hunt 1 Troll or Reaching Level 60.
//---------------------------------------------------------------------------

public class OgreQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000053);
		SetName(L("Hunt 1 Ogre"));
		SetDescription(L("The Ogre that has destructive power flowing out of its large figure, is the boss of the Fomors. It's not easy to face an ogre but please hunt the ogre. - Eavan -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000052), ReachedTotalLevel(60)));
		
		AddObjective("kill_ogre", L("Hunt 1 Ogre"), 30, 19958, 67791, Kill(1, "/ogre/"));

		AddReward(Exp(15000));
		AddReward(Item(40305, 1));
	}
}

