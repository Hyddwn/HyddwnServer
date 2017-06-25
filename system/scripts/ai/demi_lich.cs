//--- Aura Script -----------------------------------------------------------
// Demi Lich AI
//--- Description -----------------------------------------------------------
// AI for Demi Lich.
//---------------------------------------------------------------------------

[AiScript("demi_lich")]
public class DemiLichAi : AiScript
{
	public DemiLichAi()
	{
		SetVisualField(1800, 120);
		SetAggroRadius(1800);
		SetAggroLimit(AggroLimit.Three);

		Hates("/pc/", "/pet/");
		
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.CriticalKnockDown, OnCriticalKnockDown);
	}

	protected override IEnumerable Idle()
	{
		if (Random(100) < 60)
			Do(Wander());
		else
			Do(Wait(500, 3000));
	}

	protected override IEnumerable Aggro()
	{
		Do(Summon(17605, 8, 100, 500)); // Summoned Ghost
		SwitchRandom();
		if (Case(15))
		{
			Do(Attack(Rnd(1, 2, 2, 3, 3), 3000));
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
			//Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, true, 5000));
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
			SwitchRandom();
			if (Case(25))
			{
				Do(StackAttack(SkillId.Thunder));
			}
			else if (Case(25))
			{
				Do(PrepareSkill(SkillId.Fireball));
				Do(UseSkill());
			}
			else if (Case(20))
			{
				Do(StackAttack(SkillId.Thunder));
				Do(PrepareSkill(SkillId.Fireball));
				Do(UseSkill());
			}
			else if (Case(20))
			{
				Do(StackAttack(SkillId.Thunder));
				Do(PrepareSkill(SkillId.Fireball));
				Do(UseSkill());
				Do(StackAttack(SkillId.Thunder));
			}
			else if (Case(20))
			{
				Do(StackAttack(SkillId.Thunder));
				Do(PrepareSkill(SkillId.Fireball));
				Do(UseSkill());
				Do(StackAttack(SkillId.Thunder));
				Do(PrepareSkill(SkillId.Fireball));
				Do(UseSkill());
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
			Do(Attack(Rnd(1, 2, 3), 3000));
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
