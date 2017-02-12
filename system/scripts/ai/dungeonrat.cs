//--- Aura Script -----------------------------------------------------------
// Dungeon Rat AI
//--- Description -----------------------------------------------------------
// AI for Giant Forest Rats.
//---------------------------------------------------------------------------

[AiScript("dungeonrat")]
public class DungeonRatAi : AiScript
{
	public DungeonRatAi()
	{
		SetVisualField(1600, 180);
		SetAggroRadius(1600);

		Hates("/pc/", "/pet/");
		SetAggroLimit(AggroLimit.None);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 4000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(500, 500, 4000));
			Do(CancelSkill());
		}
		else if (Case(80))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(10))
		{
			Do(Circle(500, 500, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 33)
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(500, 500, 4000));
			Do(CancelSkill());
		}
		else
		{
			Do(Attack(3));
			Do(Wait(3000));
		}
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
