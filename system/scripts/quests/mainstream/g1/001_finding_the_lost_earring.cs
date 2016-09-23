//--- Aura Script -----------------------------------------------------------
// G1 001: Finding the Lost Earring
//--- Description -----------------------------------------------------------
// Getting the lost earring from the snowman and bringing it to Duncan.
// 
// Wiki:
// - Requirement: Level 5 or higher
// - Instruction: Find the earring.
//--- Notes -----------------------------------------------------------------
// This version of G1 follows the descriptions on the MW-Wiki:
// http://wiki.mabinogiworld.com/index.php?title=Generation_1:_Advent_of_the_Goddess&oldid=61442
//---------------------------------------------------------------------------

public class FindingTheLostEarringQuest : QuestScript
{
	private const int Earring = 73001;

	public override void Load()
	{
		SetId(210001);
		SetName(L("Finding the Lost Earring"));
		SetDescription(L("You will find the road to Sidhe Sneachta on the way to Alby Dungeon, north of town. An earring was lost while making a snowman. Can you look for it? - Duncan -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("deliver", L("Find the earring."), 1, 15409, 38310, Talk("duncan"));

		AddReward(Exp(450));
		AddReward(Gold(170));

		AddHook("_duncan", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "deliver") && npc.HasItem(Earring))
		{
			npc.RemoveItem(Earring);
			npc.CompleteQuest(this.Id);

			npc.Msg(Hide.Name, L("(You have given Lost Earring to Duncan.)"));
			npc.Msg(L("Ahh, that's the earring. Well done.<br/>I don't suppose it was... stuck in a snowman, was it?<br/>Ha, I figured as much. This isn't my earring, you see."));
			npc.Msg(L("A... friend lost it, making all those snowmen.<br/>They mark the passing of an important druid, you know."));
			npc.Msg(L("You saw this structure up there, right?<br/>My friend knows much about it.<br/>It leads to a magical place, if he's to be believed."), npc.Image("G1_Ch01_sidhesnechta"));
			npc.Msg(L("There's a story about that druid who vanished.<br/>It is said he became a bear, and hid in a magical place.<br/>If you can traverse that gateway, perhaps you can find him."));
			npc.Msg(L("They say the bear loves Mana Herbs.<br/>Best take some with you if you go.<br/>You can find them in dungeons."), npc.Image("G1_Ch01_manaherb"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	[On("PlayerLoggedIn")]
	public void PlayerLoggedIn(Creature creature)
	{
		if (!IsEnabled("MainStreamG1"))
			return;

		// Already done or in progress?
		if (creature.Keywords.Has("g1_complete") || creature.Keywords.Has("g1"))
			return;

		// If human > level 5
		if (creature.Level >= 5 && (creature.IsHuman || IsEnabled("NonHumanChapter1")))
		{
			Cutscene.Play("G1_0_a_Morrighan", creature, cutscene =>
			{
				creature.Keywords.Give("g1");
				creature.Keywords.Give("g1_01");
				creature.Quests.SendOwl(this.Id);
			});
		}
	}
}
