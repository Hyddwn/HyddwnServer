//--- Aura Script -----------------------------------------------------------
// Snake AI
//--- Description -----------------------------------------------------------
// AI for snakes.
//---------------------------------------------------------------------------

[AiScript("snake")]
public class SnakeAi : AiScript
{
	public SnakeAi()
	{
		SetVisualField(600, 90);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Alert()
	{
		if (Random() < 25)
			Do(PrepareSkill(SkillId.Defense));
		Do(Circle(600, 1000, 3000));
		Do(Wait(2000, 4000));
		Do(CancelSkill());
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 75)
			Do(Attack());
		else
			Do(PrepareSkill(SkillId.Defense));

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
		Do(Attack(3));
		Do(Wait(3000));
	}
}
