//--- Aura Script -----------------------------------------------------------
// Goblin Bandit AI
//--- Description -----------------------------------------------------------
// Used for Goblin Bandit.
//---------------------------------------------------------------------------

[AiScript("goblinbandit")]
public class GoblinBanditAi : AiScript
{
	public GoblinBanditAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.Two);

		Doubts("/pc/", "/pet/");
		HatesNearby(7000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.KnockDown, SkillId.Counterattack, OnCounterKnockDown);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Wander(100, 500));
			Do(Wait(2000, 5000));
		}
		else if (Case(40))
		{
			Do(Wander(300, 500, false));
			Do(Wait(4000, 7000));
		}
		else if (Case(40))
		{
			Do(Wait(2000, 5000));
		}
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(5))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(40))
		{
			if (Random() < 70)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (Case(45))
		{
			Do(Circle(500, 1000, 4000));

		}
		else if (Case(10))
		{
			Do(Circle(500, 1000, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		Do(KeepDistance(400, true, 2000));
		Do(Circle(300, 1000, 1000, false));

		SwitchRandom();
		if (Case(60))
		{
			Do(Attack(3, 5000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, true, 5000));
			Do(Attack(1, 4000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnHit()
	{
		if (Random() < 20)
		{
			Do(KeepDistance(10000, true, 2000));
		}
		else
		{
			Do(Attack(3, 4000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(20))
		{
			if (Random() < 50)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(100, false, 4000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Timeout(4000, Wander(100, 500)));
				Do(CancelSkill());
			}
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
			Do(CancelSkill());
		}
		else if (Case(30))
		{
			Do(Attack(3, 5000));
		}
		else if (Case(30))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(500));
			Do(CancelSkill());
			Do(Attack(3, 5000));
		}
	}

	private IEnumerable OnCounterKnockDown()
	{
		Do(SwitchArmor(25004, 25006));
	}
}
