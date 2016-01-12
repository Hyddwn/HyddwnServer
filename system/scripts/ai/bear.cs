//--- Aura Script -----------------------------------------------------------
// Bear AI
//--- Description -----------------------------------------------------------
// General AI for bears.
//---------------------------------------------------------------------------

[AiScript("bear")]
public class BearAi : AiScript
{
	public BearAi()
	{
		SetVisualField(700, 90);
		SetAggroRadius(400);

		Doubts("/pc/", "/pet/");
		HatesNearby(5000);

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(45))
		{
			Do(Circle(400, 2000, 4000));
		}
		else if (Case(40))
		{
			if (Random() < 70)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(400, 2500, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (Case(15))
		{
			Do(Circle(400, 500, 1000));
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
				Do(Circle(400, 3000, 6000));
				Do(CancelSkill());
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
				Do(CancelSkill());
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

	private IEnumerable OnHit()
	{
		if (Random() < 70)
		{
			Do(Attack(3, 4000));
		}
		else
		{
			Wander(500, 500, false);
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 50)
			{
				Follow(100, true, 4000);
			}
			else
			{
				Wander(500, 500, true);
			}
			Do(CancelSkill());
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

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
