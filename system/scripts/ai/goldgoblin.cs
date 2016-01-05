//--- Aura Script -----------------------------------------------------------
// Goblin AI
//--- Description -----------------------------------------------------------
// AI for GoldGoblin & Poison Goblin Types
//---------------------------------------------------------------------------

[AiScript("goldgoblin")]
public class GoldGoblinAi : AiScript
{
	public GoldGoblinAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.None);

		Hates("/pc/", "/pet/");
		//Fears("/hippopotamus/");
		//HatesAttacking("goblinarcher");

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
		if (Case(30))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(Timeout(5000, Attack()));
			Do(Wait(500));
		}
		else if (Case(45))
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