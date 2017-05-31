//--- Aura Script -----------------------------------------------------------
// Music and Reading (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank E Musical Knowledge
// Completes at Rank A Musical Knowledge, rewards AP and a book required
// to get a keyword to learn Rank 9 Musical Knowledge
//---------------------------------------------------------------------------

public class MusicalKnowledgeTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200023);
		SetName(L("Music and Reading"));
		SetDescription(L("I am 'Loeiz', the author of 'The History of Erinn Music' volumes 1 and 2. Thank you for buying my book. It would be a great delight to me if you read my book and increase your music skills. I don't know when it will be, but if you train your Musical Knowledge skill to rank A, I will give you my new book and another present. - Loeiz -"));

		SetIcon(QuestIcon.Music);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.MusicalKnowledge, SkillRank.RE));

		AddObjective("learn_musical_knowledge_rA", L("Learn Musical Knowledge Rank A"), 0, 0, 0, ReachRank(SkillId.MusicalKnowledge, SkillRank.RA));

		AddReward(AP(5));
		AddReward(Item(1030)); // Musicians of Erinn
	}
}

