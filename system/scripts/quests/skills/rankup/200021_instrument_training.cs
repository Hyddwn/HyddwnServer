//--- Aura Script -----------------------------------------------------------
// Music Study (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank F Playing Instrument
//---------------------------------------------------------------------------

public class PlayingInstrumentTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200021);
		SetName(L("Music and Reading"));
		SetDescription(L("I'm Dilys. I heard you are interested in music. I would like to help you... If you [train your Playing Instrument skill to Rank D], I will give you a book about music. - Dilys -"));

		SetIcon(QuestIcon.Music);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.PlayingInstrument, SkillRank.RF));

		AddObjective("learn_playing_instrument_rD", L("Learn Playing Instrument Skill Rank D"), 0, 0, 0, ReachRank(SkillId.PlayingInstrument, SkillRank.RD));

		AddReward(Item(1018)); // The History of Music in Erinn (1)
	}
}

