//--- Aura Script -----------------------------------------------------------
// Young Goblin Archer AI
//--- Description -----------------------------------------------------------
// AI for Younh Goblin Archers.
//---------------------------------------------------------------------------

[AiScript("goblinarcherkid")]
public class YoungGoblinArcherAi : AiScript
{
	public YoungGoblinArcherAi()
	{
		SetVisualField(1000, 120);
		SetAggroRadius(500);
		SetAggroLimit(AggroLimit.One);

		Hates("/pc/", "/pet/");
		//HatesAttacking("redgoblin");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(300, 500));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(SwitchTo(WeaponSet.First));
		Do(KeepDistance(800, false, 2000));
		Do(Circle(800, 1000, 1000, false));

		if (Random() < 60)
		{
			Do(RangedAttack());
			if (Random() < 50)
				Do(RangedAttack());
		}
		else
		{
			Do(PrepareSkill(SkillId.MagnumShot));
			Do(RangedAttack());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		if (Random() < 50)
		{
			Do(SwitchTo(WeaponSet.Second));
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else
		{
			Do(SwitchTo(WeaponSet.Second));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000)); // Would need a wait too normally but I have one right after Aggro starts. Otherwise it would swap in smash animation and a bow attack sound would play.
		}
	}
}