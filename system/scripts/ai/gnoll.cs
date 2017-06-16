//--- Aura Script -----------------------------------------------------------
// Gnoll AI
//--- Description -----------------------------------------------------------
// AI for Gnoll.
//---------------------------------------------------------------------------

[AiScript("gnoll")]
public class GnollAi : AiScript
{
	readonly string[] DistanceChat = new[] { "", "", "" };
	readonly string[] SmashChat = new[] { "", "", "" };
	readonly string[] AttackChat = new[] { "", "", "" };
	readonly string[] WindmillChat = new[] { "", "", "" };

	public GnollAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(Say(DistanceChat));
		if (Random() < 30)
		{
			Do(KeepDistance(500, false, 2000));
		}
		Do(Circle(300, 1000, 2000));
		SwitchRandom();
		if (Case(20))
		{
			Do(CancelSkill());
			Do(Say(AttackChat));
			Do(Attack(3, 4000));
			if (Random() < 30)
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(Say(WindmillChat));
				Do(UseSkill());
			}
		}
		else if (Case(20))
		{
			Do(Say(SmashChat));
			Do(PrepareSkill(SkillId.Smash));			
			Do(CancelSkill());
			Do(Say(AttackChat));
			Do(Attack(3, 4000));
		}
		else if (Case(20))
		{
			Do(Say(SmashChat));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 2000));
			if (Random() < 30)
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
			}
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 5000));
			Do(CancelSkill());
			if (Random() < 80)
			{
				Do(Attack(3,4000));
			}
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			//Do(Follow(600, true));
			Do(CancelSkill());
			if (Random() < 30)
			{
				Do(Say(AttackChat));
				Do(Attack(3,4000));
			}
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		if (Creature.Life < Creature.LifeMax * 0.20f)
		{
			
			if (Random() < 50)
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
			else 
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
		}
		else
		{
			SwitchRandom();
			if (Case(80))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Windmill));
				//Do(Wait(3000, 3000));
				Do(UseSkill());		
			}
			else if (Case(10))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if(Case(10))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
		}
	}
}
