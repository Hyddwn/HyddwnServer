//--- Aura Script -----------------------------------------------------------
// Grizzlybear AI
//--- Description -----------------------------------------------------------
// AI for bears.
//---------------------------------------------------------------------------

[AiScript("grizzlybear")]
public class GrizzlybearAi : AiScript
{
	public GrizzlybearAi()
	{
		SetVisualField(700, 90);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.One);

		Doubts("/pc/", "/pet/");
		HatesNearby(4000);
		//HatesAttacking("/grizzlybearkid/"); duration="500"

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 400));
		Do(Wait(2000, 5000));
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
				Do(Circle(400, 1000, 5000));
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
			Do(Circle(400, 1000, 4000));
		}
		else if (Case(10))
		{
			Do(Circle(500, 1000, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50)
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 6000));
				Do(CancelSkill());
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
				Do(Wait(3000, 8000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else
		{
			Do(Attack(3, 5000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(15))
		{
			Do(KeepDistance(100, true, 2000));
		}
		else if (Case(15))
		{
			Do(Timeout(2000, Wander()));
		}
		else if (Case(70))
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
				Do(Follow(100, true, 4000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Timeout(4000, Wander()));
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
			Do(Timeout(500, PrepareSkill(SkillId.Defense)));
			Do(CancelSkill());
			Do(Attack(3, 5000));
		}
	}
}