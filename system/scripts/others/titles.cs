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

		if (count >= 10)
		{
			creature.Titles.Enable(20);
			count = 0;
		}
		else if (count >= 5)
		{
			creature.Titles.Show(20);
		}

		creature.Vars.Temp["ButterfingerFailCounter"] = count;
	}

	[On("CreatureGotLuckyFinish")]
	public void CreatureGotLuckyFinish(Creature creature, LuckyFinish finish, int amount)
	{
		// the Lucky
		// Enable on lucky finish over 2k gold.
		// ------------------------------------------------------------------
		if (creature.Titles.IsUsable(23) || finish == LuckyFinish.None)
			return;

		if (amount >= 2000)
			creature.Titles.Enable(23);
	}

	[On("SkillRankChanged")]
	public void OnSkillRankChanged(Creature creature, Skill skill)
	{
		// the Elemental Apprentice
		// Enable if creature has all basic bolts.
		// ------------------------------------------------------------------
		if (creature.Titles.IsUsable(28) || !skill.Is(SkillId.Icebolt, SkillId.Firebolt, SkillId.Lightningbolt))
			return;

		if (creature.Skills.Has(SkillId.Icebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Firebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Lightningbolt, SkillRank.RF))
			creature.Titles.Enable(28);
	}

	[On("PlayerLoggedIn")]
	public void OnPlayerLoggedIn(Creature creature)
	{
		// the Elemental Apprentice
		// Enable if creature has all basic bolts. Fallback for players
		// who already have all bolts. TODO: Remove.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(28) && creature.Skills.Has(SkillId.Icebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Firebolt, SkillRank.RF) && creature.Skills.Has(SkillId.Lightningbolt, SkillRank.RF))
			creature.Titles.Enable(28);
	}

	[On("CreatureStartedPtj")]
	public void OnCreatureStartedPtj(Creature creature, PtjType type)
	{
		// the Diligent
		// Show if creature starts any PTJ.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(33))
			creature.Titles.Show(33);
	}

	[On("CreatureCompletedPtj")]
	public void OnCreatureCompletedPtj(Creature creature, PtjType type)
	{
		// the Diligent
		// Enable if creature completed a PTJ 100 times.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(33))
		{
			var trackRecord = creature.Quests.GetPtjTrackRecord(type);
			if (trackRecord.Done >= 100)
				creature.Titles.Enable(33);
		}
		}
	}
}
