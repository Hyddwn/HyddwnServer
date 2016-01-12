//--- Aura Script -----------------------------------------------------------
// Goblin AI
//--- Description -----------------------------------------------------------
// AI for Goblin Types
//---------------------------------------------------------------------------

[AiScript("goblin")]
public class GoblinAi : AiScript
{
	public GoblinAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.One);

		Hates("/pc/", "/pet/");
		Hates("/ahchemy_golem/");
		Hates("/rp/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(300, 500));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(KeepDistance(400, false, 2000));
		Do(Circle(300, 700, 1000, false));

		SwitchRandom();
		if (Case(60))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(Attack(2, 5000));
			Do(Wait(500));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, true, 5000));
			Do(Attack(1, 5000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(200, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}