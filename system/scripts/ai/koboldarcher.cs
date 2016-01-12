//--- Aura Script -----------------------------------------------------------
// Kobold Archer AI
//--- Description -----------------------------------------------------------
// AI for Kobold Archers and Red Kobold Archers.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
//---------------------------------------------------------------------------

[AiScript("koboldarcher")]
public class KoboldArcherAi : AiScript
{
	public KoboldArcherAi()
	{
		SetVisualField(1000, 120);
		SetAggroRadius(500);
		SetAggroLimit(AggroLimit.Two); // Support One

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 500));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(SwitchTo(WeaponSet.First));
		Do(KeepDistance(800, false, 2000));
		Do(Circle(800, 800, 1000, false));

		SwitchRandom();
		if (Case(30))
		{
			Do(RangedAttack());
		}
		else if (Case(30))
		{
			Do(RangedAttack());
			Do(RangedAttack());
		}
		else if (Case(40))
		{
			Do(PrepareSkill(SkillId.MagnumShot));
			Do(RangedAttack());
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(30))
		{
			Do(SwitchTo(WeaponSet.Second));
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(30))
		{
			Do(SwitchTo(WeaponSet.Second));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}
