//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Werewolf (Barri Basic)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BarriBasicPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100082);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Werewolf");
		SetDescription("Please offer [Barri Basic Fomor Pass] on the altar of Barri Dungeon. [Werewolf] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 5 Werewolves", 0, 0, 0, Kill(5, "/werewolf/normalwerewolf/"));
		AddObjective("obj2", "Clear Barri Basic Dungeon", 0, 0, 0, ClearDungeon("Bangor_Barri_Low_Dungeon"));

		AddReward(Exp(4000));
		AddReward(Gold(10000));
	}
}
