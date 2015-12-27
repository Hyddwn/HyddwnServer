//--- Aura Script -----------------------------------------------------------
// Raccoon AI
//--- Description -----------------------------------------------------------
// AI for racoons.
//---------------------------------------------------------------------------

[AiScript("racoon")]
public class RaccoonAi : AiScript
{
	public RaccoonAi()
	{
		Doubts("/pc/", "/pet/");
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

		var num = Random();
		if (num < 50) // 50%
			Do(Circle(400, 800, 800));
		else if (num < 75) // 25%
			Do(KeepDistance(500, true, 800));
		else // 25%
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

		var num = Random();
		if (num < 10) // 20%
			Do(Circle(400, 800, 800));
		else if (num < 45) // 25%
			Do(KeepDistance(500, true, 800));
		else if (num < 70) // 25%
			Do(Follow(100, true, 800));
		else // 30%
			Do(Wait(2000, 3000));

		Do(CancelSkill());
	}

	private IEnumerable OnHit()
	{
		Do(Wait(1000, 2000));
	}

	private IEnumerable OnKnockDown()
	{
		var num = Random();
		if (num < 10) // 20%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say("..."));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
		else if (num < 70) // 60%
		{
			Do(Wait(7000, 8000));
		}
		else // 30%
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
