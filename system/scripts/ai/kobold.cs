//--- Aura Script -----------------------------------------------------------
// Kobold AI
//--- Description -----------------------------------------------------------
// AI for Kobold, Poison Kobold and Gold Kobold.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
//---------------------------------------------------------------------------

[AiScript("kobold")]
public class KoboldAi : AiScript
{
	public KoboldAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.One);

		Hates("/pc/", "/pet/");
		//HatesAttacking("/koboldarcher/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 500));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(KeepDistance(400, true, 2000));
		Do(Circle(300, 800, 1000, false));

		SwitchRandom();
		if (Case(60))
		{
			Do(Attack(3, 5000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, true, 5000));
			Do(Attack(1, 4000));
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
