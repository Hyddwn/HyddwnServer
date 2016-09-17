//--- Aura Script -----------------------------------------------------------
// Coyote AI
//--- Description -----------------------------------------------------------
// AI for Coyote type monster.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
// Missing: wolf support, fear
//---------------------------------------------------------------------------

[AiScript("coyote")]
public class CoyoteAi : AiScript
{
	public CoyoteAi()
	{
		SetVisualField(650, 120);
		SetAggroRadius(500);

		Doubts("/pc/", "/pet/");
		Doubts("/cow/");
		Hates("/sheep/");
		Hates("/dog/");
		//Fears("/junglewolf/")
		HatesNearby(10000);
		HatesBattleStance(3000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 400));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(40))
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
		else if (Case(5))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(45))
		{
			Do(Circle(500, 1000, 4000));
		}
		else if (Case(10))
		{
			Do(Circle(500, 500, 1000));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 60)
		{
			SwitchRandom();
			if (Case(25))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(400, 1000, 5000));
				Do(CancelSkill());
			}
			else if (Case(25))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
			else if (Case(25))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
			}
			else if (Case(25))
			{
				Do(Circle(400, 1000, 1000, false));
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
