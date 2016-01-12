//--- Aura Script -----------------------------------------------------------
// Hellhound AI
//--- Description -----------------------------------------------------------
// AI for Hellhounds.
//---------------------------------------------------------------------------

[AiScript("hellhound")]
public class HellhoundAi : AiScript
{
	public HellhoundAi()
	{
		SetVisualField(2000, 120);
		SetAggroRadius(800);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(50))
			Do(Wander(300, 500, Random() < 50));
		else if (Case(30))
			Do(PrepareSkill(SkillId.Firebolt, Rnd(1, 2, 3)));
		else if (Case(20))
			Do(Wait(1000, 2000));

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(30))
		{
			Do(PrepareSkill(SkillId.Firebolt, Rnd(1, 1, 1, 1, 1, 1, 1, 1, 2, 3)));
			Do(Wait(3000));
		}
		else if (Case(30))
		{
			Do(StackAttack(SkillId.Firebolt, Rnd(1, 1, 1, 1, 1, 1, 2, 3, 4, 5), 10000));
			if (Random() < 50)
				Do(StackAttack(SkillId.Firebolt, 1, 5000));
			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(Attack(3, 5000));
		}
		else if (Case(6))
		{
			Do(Circle(400, 2000, 2000, false));
		}
		else if (Case(2))
		{
			Do(Follow(400, false, 2000));
		}
		else if (Case(2))
		{
			Do(KeepDistance(1000, false, 2000));
		}
		else if (Case(6))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(400, 5000, 5000));
			Do(CancelSkill());
		}
		else if (Case(2))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(5000));
			Do(CancelSkill());
		}
		else if (Case(2))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
		}
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(70))
			Do(Attack(3, 4000));
		else if (Case(15))
			Do(KeepDistance(1000, false, 2000));
		else if (Case(15))
			Do(Wander(500, 500, false));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(40))
		{
			SwitchRandom();
			if (Case(30))
			{
				Do(Circle(400, 4000, 4000, false));
			}
			else if (Case(10))
			{
				Do(Follow(400, false, 3000));
			}
			else if (Case(60))
			{
				Do(KeepDistance(1200, false, 5000));
			}
		}
		else if (Case(25))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 50)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(KeepDistance(1200, false, 7000));
			Do(PrepareSkill(SkillId.Firebolt, Rnd(1, 1, 1, 1, 1, 1, 1, 1, 2, 3)));
			Do(Wait(3000));
		}
		else if (Case(10))
		{
			Do(Wait(3000));
			Do(Attack(3, 4000));

			if (Random() < 50)
			{
				Do(StackAttack(SkillId.Firebolt, 1, 5000));
				Do(Wait(1000, 2000));
			}
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
