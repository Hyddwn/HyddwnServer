//--- Aura Script -----------------------------------------------------------
//  Goblin Archer Kid AI
//--- Description -----------------------------------------------------------
//  AI for Goblin Archer Kids.
//---------------------------------------------------------------------------

[AiScript("goblinarcherkid")]
public class GoblinArcherKidAi : AiScript
{
	public GoblinArcherKidAi()
	{
		SetAggroRadius(1000); // angle 120 audio 500
		Hates("/pc/", "/pet/");
		SetAggroLimit(AggroLimit.One);
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
		Do(Wait(500));
		Do(SwitchTo(WeaponSet.First));
		Do(Wait(500));
		Do(KeepDistance(800, false, 2000));
		Do(Circle(800, 1000, 1000, false);
		var rndAggro = Random();
		if (rndAggro < 30)
		{
			Do(RangedAttack());
		}
		else if (rndAggro < 60)
		{
			Do(RangedAttack());
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
			Do(Wait(500)); // These "extra" waits are needed on Aura, otherwise you would not see the animation of weapon swapping which you do on NA
			Do(SwitchTo(WeaponSet.Second));
			Do(Wait(500));
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else
		{
			Do(Wait(500));
			Do(SwitchTo(WeaponSet.Second));
			Do(Wait(500));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000)); // Would need a wait too normally but I have one right after Aggro starts. Otherwise it would swap in smash animation and a bow attack sound would play.
		}
	}
}