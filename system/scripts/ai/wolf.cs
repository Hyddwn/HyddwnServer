//--- Aura Script -----------------------------------------------------------
// Wolf AI
//--- Description -----------------------------------------------------------
// AI for "normal" wolves.
//---------------------------------------------------------------------------

[AiScript("wolf")]
public class WolfAi : AiScript
{
	public WolfAi()
	{
		SetVisualField(650, 120);
		SetAggroRadius(400);

		Doubts("/pc/", "/pet/");
		Doubts("/cow/");
		Hates("/sheep/");
		Hates("/dog/");
		HatesBattleStance(3000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		if (Random() < 50)
		{
			if (Random() < 50)
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
		else
		{
			Do(Circle(400, 1000, 5000));
			Do(Wait(1000, 5000));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50)
		{
			SwitchRandom();
			if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
				Do(Wait(3000, 8000));
			}
			else if (Case(40))
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
			Do(KeepDistance(1000, true, 2000));
		}
		else if (Case(15))
		{
			Do(Timeout(2000, Wander()));
		}
		else if (Case(70))
		{
			Do(Attack(3));
			Do(Wait(4000, 4000));
		}
	}
}
