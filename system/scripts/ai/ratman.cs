//--- Aura Script -----------------------------------------------------------
// Rat Man AI
//--- Description -----------------------------------------------------------
// AI for Rat Man.
//---------------------------------------------------------------------------

[AiScript("ratman")]
public class RatManAi : AiScript
{
	readonly string[] DistanceChat = new[] { "This way", "Follow me~" };
	readonly string[] SmashChat = new[] { "Prepare for a heavy blow.", "Here I come.", "I have found a blind side." };
	readonly string[] AttackChat = new[] { "Hahahaha", "A hit, it's a hit!"};

	public RatManAi()
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
		SwitchRandom();
		if (Case(30))
		{
			Do(Say(DistanceChat));
			Do(KeepDistance(1000, false, 2000));
			Do(Circle(600, 1000, 2000));
		}
		else if (Case(20))
		{
			Do(CancelSkill());
			Do(Say(AttackChat));
			Do(Attack(3));
		}
		else if (Case(20))
		{
			Do(Say(SmashChat));
			Do(PrepareSkill(SkillId.Smash));			
			Do(CancelSkill());
			Do(Say(AttackChat));
			Do(Attack(3));
		}
		else if (Case(20))
		{
			Do(Say(SmashChat));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true));
			Do(CancelSkill());
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			//Do(Follow(600, true));
			Do(CancelSkill());
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
			if (Case(40))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Windmill));
				//Do(Wait(4000, 4000));
				Do(UseSkill());
				
			}
			else if (Case(30))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if(Case(30))
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
		}
	}
}
