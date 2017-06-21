//--- Aura Script -----------------------------------------------------------
// Ghost AI
//--- Description -----------------------------------------------------------
// AI for Ghost.
//---------------------------------------------------------------------------

[AiScript("ghost")]
public class GhostAi : AiScript
{
	public GhostAi()
	{
		SetVisualField(800, 120);
		SetAggroRadius(800);

		Hates("/pc/", "/pet/");
		
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.CriticalKnockDown, OnCriticalKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(500, 3000));
	}

	protected override IEnumerable Aggro()
	{
		if (Random(100) < 20)
		{
			Do(Circle(500, 1000, 1000, false));
		}
		SwitchRandom();
		if (Case(15))
		{
			Do(Attack(Rnd(2, 2, 2, 3, 3), 3000));
		}
		else if (Case(20))
		{
			Do(Attack(1, 700));
			Do(Wait(1000));
			Do(Attack(1, 700));
			Do(Wait(1000));

			if (Random(100) < 50)
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
				Do(Wait(1000, 3000));
				Do(CancelSkill());
			}
		}
		else if (Case(5))
		{
			Do(Attack(Rnd(1, 2, 3), 3000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(200, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			if (HasSkill(SkillId.Thunder))
			{
				Do(StackAttack(SkillId.Thunder));
			}
			else if (HasSkill(SkillId.Fireball))
			{
				Do(PrepareSkill(SkillId.Fireball));
				Do(UseSkill());
				Do(CancelSkill());
			}
		}	
	}

	private IEnumerable OnHit()
	{
		if (Random(100) < 70)
		{
			Do(Attack(Rnd(2, 2, 3), 3000));
			Do(KeepDistance(3000, false, 3000));
		}
		else
		{
			Do(PrepareSkill(SkillId.DarkLord));
			Do(Attack(Rnd(1, 2, 2, 2, 3), 3000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		Do(Wait(500, 1000));
		Do(PrepareSkill(SkillId.DarkLord));
		SwitchRandom();
		if (Case(50))
		{
			Do(Attack(Rnd(1, 2, 3), 3000));
		}
		else if (Case(25))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(25))
		{
			Do(KeepDistance(1200, false, 5000));
		}
	}

	private IEnumerable OnCriticalKnockDown()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(40))
		{
			Do(Attack(3, 3000));
		}
		else if (Case(40))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(UseSkill());
			Do(Wait(1000, 3000));
			Do(CancelSkill());
		}
	}
}
