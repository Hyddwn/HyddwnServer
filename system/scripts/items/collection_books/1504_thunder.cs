//--- Aura Script -----------------------------------------------------------
// Thunder Collection Book
//--- Description -----------------------------------------------------------
// Rewards Thunder skill upon completion.
//---------------------------------------------------------------------------

[CollectionBook(1504)]
public class ThunderCollectionBookScript : CollectionBookScript
{
	private readonly int[] items = new[] { 64511, 64512, 64513, 64514, 64515 };

	public override int GetIndex(Item item)
	{
		return Array.IndexOf(items, item.Info.Id);
	}

	public override void OnAdd(Creature creature, Item book, Item item)
	{
		creature.PlaySound("data/sound/emotion_success.wav");
		creature.Notice(L("{0} collection book's {1} has been collected."), L(book.Data.Name), L(item.Data.Name));
	}

	public override void OnComplete(Creature creature, Item book)
	{
		creature.PlaySound("data/sound/emotion_success.wav");
		creature.Notice(L("{0}'s items have all been collected."), L(book.Data.Name));
	}

	public override void OnReward(Creature creature, Item book)
	{
		creature.GiveSkill(SkillId.Thunder, SkillRank.Novice);
	}
}
