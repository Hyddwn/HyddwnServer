//--- Aura Script -----------------------------------------------------------
// Nightmare Humanoid AI
//--- Description -----------------------------------------------------------
// AI for the rainbow pony on two hoofs.
//---------------------------------------------------------------------------

[AiScript("nightmarehumanoid")]
public class NightmareHumanoidAi : AiScript
{
	public NightmareHumanoidAi()
	{
		SetVisualField(1500, 1200);
		SetAggroRadius(90);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected IEnumerable ChangeMode(int skinColor)
	{
		Do(SetSkinColor(skinColor));
		Do(RemoveSkills(SkillId.NaturalShieldPassive, SkillId.HeavyStanderPassive, SkillId.ManaDeflectorPassive));

		switch (skinColor)
		{
			case 0: break;
			case 4: Do(AddSkill(SkillId.HeavyStanderPassive, SkillRank.R1)); break;
			case 6: Do(AddSkill(SkillId.NaturalShieldPassive, SkillRank.R1)); break;
			case 9: Do(AddSkill(SkillId.ManaDeflectorPassive, SkillRank.R1)); break;
			case 15:
				Do(AddSkill(SkillId.HeavyStanderPassive, SkillRank.R1));
				Do(AddSkill(SkillId.NaturalShieldPassive, SkillRank.R1));
				Do(AddSkill(SkillId.ManaDeflectorPassive, SkillRank.R1));
				break;
		}

		yield break;
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(500, 500, true));
		Do(Wait(2000, 4000));

		SwitchRandom();
		if (Case(50))
		{
			Do(ChangeMode(4));
		}
		else if (Case(20))
		{
			Do(ChangeMode(9));
		}
		else if (Case(20))
		{
			Do(ChangeMode(6));
		}
		else if (Case(10))
		{
			Do(ChangeMode(0));
		}

		Do(Wait(3000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(45))
		{
			if (Random(100) < 80)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 7000));
			}
			else
			{
				Do(Attack(3, 7000));
			}
		}
		else if (Case(15))
		{
			Do(Say(L("Meet your worst nightmare."), L("You can't beat me."), L("Ha ha ha ha"), L("Ha ha ha"), L("You coward."), L("Are you afraid?"), L("Just waiting will not give you answers."), L("You wouldn't dare attack me."), "", ""));
			Do(Wait(500, 2000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(Wander(500, 500, false));
		}
		else if (Case(10))
		{
			Do(Say(L("True Night!!")));
			Do(ChangeMode(0));
		}
		else if (Case(5))
		{
			Do(Say(L("Jean Night!!")));
			Do(ChangeMode(15));
		}
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(70))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(15))
		{
			Do(Wander(300, 600, false));
		}
		else if (Case(15))
		{
			Do(KeepDistance(500, false, 2000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000, 3000));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(50))
		{
			Do(Say(L("Change Night One!!")));
			Do(ChangeMode(4));
		}
		else if (Case(20))
		{
			Do(Say(L("Change Night Two!!")));
			Do(ChangeMode(9));
		}
		else if (Case(20))
		{
			Do(Say(L("Change Night Three!!")));
			Do(ChangeMode(6));
		}
		else if (Case(10))
		{
			Do(Say(L("Jean Night!!")));
			Do(ChangeMode(15));
		}

		SwitchRandom();
		if (Case(35))
		{
			Do(Attack(3, 5000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(4000, 4000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
	}
}
