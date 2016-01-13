//--- Aura Script -----------------------------------------------------------
// Bat AI
//--- Description -----------------------------------------------------------
// AI for bats.
//---------------------------------------------------------------------------

[AiScript("bat")]
public class BatAi : AiScript
{
	public BatAi()
	{
		SetVisualField(200, 120);
		SetAggroRadius(1000);

		Doubts("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
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
		Do(Circle(300, 1000, 3000));
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
		if (Case(20))
			Do(Circle(600, 1000, 3000));
		else if (Case(50))
			Do(KeepDistance(400, Random() < 60, 2000));
		else if (Case(30))
			Do(Wait(3000, 3000));

		Do(CancelSkill());
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(30))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
		else if (Case(40))
		{
			Do(Wait(7000, 8000));
		}
		else if (Case(30))
		{
			Do(Attack(1, 4000));
		}
	}
}
