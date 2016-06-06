//--- Aura Script -----------------------------------------------------------
// Titles
//--- Description -----------------------------------------------------------
// Rewards titles to creatures when certain things happen.
//---------------------------------------------------------------------------

public class TitleRewardingScript : GeneralScript
{
	[On("CreatureFished")]
	public void OnCreatureFished(Creature creature, Item item)
	{
		if (item == null)
			return;

		// the Fishing Legend
		// ------------------------------------------------------------------
		var cm = (item.MetaData1.GetFloat("SCALE") * item.Data.BaseSize);
		if (cm > 95)
			creature.Titles.Show(19);

		if (item.Info.Id == 50216) // Carnivorous Fish
			creature.Titles.Enable(19);
	}

	[On("CreatureFinishedProductionOrCollection")]
	public void OnCreatureFinishedProductionOrCollection(Creature creature, bool success)
	{
		// the Butterfingers
		// Show if failed collecting or production something 5 times in a
		// rown, give it at 10 fails in a row.
		// ------------------------------------------------------------------
		if (creature.Titles.IsUsable(20))
			return;

		if (success)
		{
			creature.Vars.Temp["ButterfingerFailCounter"] = 0;
			return;
		}

		if (creature.Vars.Temp["ButterfingerFailCounter"] == null)
			creature.Vars.Temp["ButterfingerFailCounter"] = 0;

		var count = (int)creature.Vars.Temp["ButterfingerFailCounter"];
		count++;

		if(count >= 10)
		{
			creature.Titles.Enable(20);
			count = 0;
		}
		else if(count >= 5)
		{
			creature.Titles.Show(20);
		}

		creature.Vars.Temp["ButterfingerFailCounter"] = count;
		Log.Debug(count);
	}
}
