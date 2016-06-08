//--- Aura Script -----------------------------------------------------------
// Titles
//--- Description -----------------------------------------------------------
// Rewards titles to creatures when certain things happen.
//---------------------------------------------------------------------------

public class TitleRewardingScript : GeneralScript
{
	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
	}

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

		// the Reborn 
		// Enable if creature rebirthed.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(39) && creature.Has(CreatureStates.JustRebirthed))
			creature.Titles.Enable(39);
	}

	[On("CreatureAged")]
	public void OnCreatureAged(Creature creature, int prevAge)
	{
		// the Adult
		// Enable if creature reaches age 18.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(44) && creature.Age >= 18)
			creature.Titles.Enable(44);

		// the Reborn
		// Show if creature reaches age 25.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(39) && creature.Age >= 25)
			creature.Titles.Show(39);

		// the All-Knowing
		// Show if creature reaches age 25.
		// ------------------------------------------------------------------
		if (!creature.Titles.Knows(45) && creature.Age >= 25)
			creature.Titles.Show(45);

		// the All-Knowing
		// Enable if creature reaches age 30.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(45) && creature.Age >= 30)
			creature.Titles.Enable(45);
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
			{
				creature.Titles.Enable(33);
				creature.Titles.Show(35);
			}
		}

		// the Super Diligent 
		// Enable if creature completed a PTJ 10,000 times.
		// ------------------------------------------------------------------
		if (!creature.Titles.IsUsable(35))
		{
			var trackRecord = creature.Quests.GetPtjTrackRecord(type);
			if (trackRecord.Done >= 10000)
				creature.Titles.Enable(35);
		}
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		// the Lazy 
		// Enable if talking to Duncan about rumors while having the title
		// "the Neighboring Part-timer".
		// ------------------------------------------------------------------
		if (!npc.Player.Titles.IsUsable(34))
		{
			var keyword = args[0] as string;
			if (keyword == "rumor" && npc.Player.Titles.SelectedTitle == 20000) // the Neighboring Part-timer
			{
				npc.Msg(L("I thought you were doing a part-time job.<br/>What brings you here?<br/>If you slack on your work, people won't approve...<br/>So be responsible."));
				npc.Player.Titles.Enable(34);
			}
		}

		return HookResult.Continue;
	}

	[On("CreatureAttack")]
	public void OnCreatureAttacked(TargetAction tAction)
	{
		// who experienced Death
		// Show when knocked out by a 500+ dmg attack, enable when
		// surviving a 500+ dmg attack.
		// ------------------------------------------------------------------
		if (tAction.Damage < 500)
			return;

		if (!tAction.Creature.Titles.IsUsable(37))
		{
			if (tAction.Creature.IsDead)
			{
				if (!tAction.Creature.Titles.Knows(37))
					tAction.Creature.Titles.Show(37);
			}
			else
				tAction.Creature.Titles.Enable(37);
		}

		// who transcended Death
		// Show when knocked out by a 1000+ dmg attack, enable when
		// surviving a 1000+ dmg attack.
		// ------------------------------------------------------------------
		if (tAction.Damage < 1000)
			return;

		if (!tAction.Creature.Titles.IsUsable(38))
		{
			if (tAction.Creature.IsDead)
			{
				if (!tAction.Creature.Titles.Knows(38))
					tAction.Creature.Titles.Show(38);
			}
			else
				tAction.Creature.Titles.Enable(38);
		}
	}
}
