//--- Aura Script -----------------------------------------------------------
// Cat Sith AI
//--- Description -----------------------------------------------------------
// AI for one of the Cat Sith Knights.
//---------------------------------------------------------------------------

[AiScript("catsith4")]
public class CatSith4Ai : AiScript
{
	public CatSith4Ai()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.None);

		Doubts("/pc/", "/pet/");
		HatesNearby(3000);

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Say("", "", ""));

		SwitchRandom();
		if (Case(40))
		{
			Do(Wander(300, 500, Random(100) < 20));
		}
		else if (Case(20))
		{
			Do(Wait(4000, 6000));
		}

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(Say("", "", ""));

		Do(Circle(400, 2000, 3000));

		Do(Say("", "", ""));
	}

	protected override IEnumerable Aggro()
	{
		Do(SwitchTo(WeaponSet.First));
		Do(KeepDistance(250, false, 2000));

		SwitchRandom();
		if (Case(50))
		{
			Do(Say("", "", ""));
			Do(Attack(3, 4000));

			SwitchRandom();
			if (Case(30))
			{
				Do(Say("", "", ""));
				Do(Attack(3, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(1000));
				Do(CancelSkill());
			}

			if (Random() < 50)
				Do(Wait(500, 2000));
		}
		else if (Case(30))
		{
			SwitchRandom();
			if (Case(50))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(CancelSkill());
				Do(Say("", "", ""));
				Do(Attack(3, 4000));
			}

			Do(Wait(1000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));

			if (Random(100) < 60)
				Do(Circle(400, 2000));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(Say("", "", ""));

			SwitchRandom();
			if (Case(60))
			{
				Do(Say("", "", ""));
				Do(Circle(400, 2000));
			}
			else if (Case(20))
			{
				Do(Follow(400, true, 5000));
			}
			else if (Case(20))
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say("", "", ""));

		if (Random(100) < 80)
			Do(Attack(3, 4000));
		else
			Do(KeepDistance(10000, false, 2000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("", "", ""));

		SwitchRandom();
		if (Case(40))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(3000, 4000));
			Do(UseSkill());
		}
		else if (Case(55))
		{
			Do(Circle(400, 2000));
			Do(Say("", "", ""));
			Do(Attack(3, 4000));
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Say("", "", ""));
			Do(Attack(1, 4000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		if (Random(100) < 40)
		{
			Do(Say("", "", ""));
			Do(Attack(3, 4000));

			if (Random(100) < 50)
			{
				Do(Wait(1000, 2000));
			}
			else
			{
				Do(Say("", "", ""));
				Do(Attack(3, 4000));
			}
		}
		else
		{
			Do(Attack(1, 4000));
			Do(Say("", "", ""));
			Do(Wait(2000));
			Do(Attack(1, 4000));
			Do(Say("", "", ""));
			Do(Wait(2000));
			Do(Say("", "", ""));
			Do(Attack(1, 4000));
			Do(Wait(2000));
			Do(Say("", "", ""));
			Do(Attack(1, 4000));
		}
	}
}
