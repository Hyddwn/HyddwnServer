//--- Aura Script -----------------------------------------------------------
// Black Raccoon AI
//--- Description -----------------------------------------------------------
// Same as Raccoon, but hates players. Used for Black Racoon Field Boss.
//---------------------------------------------------------------------------

[AiScript("blackraccoon")]
public class BlackRaccoonAi : AiScript
{
	public BlackRaccoonAi()
	{
		SetVisualField(700, 90);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");
		Hates("/chicken/");

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
		if (Random() < 25)
			Do(PrepareSkill(SkillId.Defense));
		Do(Wait(2000, 4000));

		SwitchRandom();
		if (Case(50))
			Do(Circle(400, 800, 800));
		else if (Case(25))
			Do(KeepDistance(500, true, 800));
		else if (Case(25))
			Do(Follow(300, true, 800));

		Do(Wait(2000, 4000));
		Do(CancelSkill());
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 10)
			Do(PrepareSkill(SkillId.Defense));
		else
			Do(Attack(Rnd(1, 1, 1, 1, 1, 1, 2, 2, 3, 3), 4000));

		SwitchRandom();
		if (Case(20))
			Do(Circle(400, 800, 800));
		else if (Case(25))
			Do(KeepDistance(500, true, 800));
		else if (Case(25))
			Do(Follow(100, true, 800));
		else if (Case(30))
			Do(Wait(2000, 3000));

		Do(CancelSkill());
	}

	private IEnumerable OnHit()
	{
		Do(Wait(1000, 2000));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say("..."));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
		else if (Case(60))
		{
			Do(Wait(7000, 8000));
		}
		else if (Case(30))
		{
			Do(Wait(1000));
			Do(Attack(1, 4000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Wait(1000));
		Do(Attack(3));
		Do(Wait(3000));
	}
}
