//--- Aura Script -----------------------------------------------------------
// Worm AI
//--- Description -----------------------------------------------------------
// AI for Worm type monster.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
//---------------------------------------------------------------------------

[AiScript("worm")]
public class WormAi : AiScript
{
	public WormAi()
	{
		SetVisualField(600, 45);
		SetAggroRadius(400);

		Doubts("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
	}

	protected override IEnumerable Alert()
	{
		if (Random() < 25)
			Do(PrepareSkill(SkillId.Defense));

		Do(Circle(500, 1000, 3000));
		Do(Wait(2000, 4000));
		Do(CancelSkill());
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 75)
		{
			if (Random() < 40)
				Do(Attack(2, 10000));
			else
				Do(Attack(3, 10000));
		}
		else
		{
			Do(PrepareSkill(SkillId.Defense));
		}

		SwitchRandom();
		if (Case(40))
		{
			Do(KeepDistance(400, true, 3000));
		}
		else if (Case(30))
		{
			Do(KeepDistance(700, false, 3000));
		}
		else if (Case(30))
		{
			Do(Wait(3000));
		}
		Do(CancelSkill());
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
			Do(KeepDistance(1000, false, 2000));
		}
		else if (Case(15))
		{
			Do(Timeout(2000, Wander(100, 500, false)));
		}
		else if (Case(70))
		{
			Do(Attack(3, 4000));
		}
	}
}
