//--- Aura Script -----------------------------------------------------------
// Traveler A AI
//--- Description -----------------------------------------------------------
// AI for the NPC you're rescuing in Trefor's RP. There are two versions of
// this, one hating Goblins, that is used when you fail the RP, to make it
// easier.
//--- Notes -----------------------------------------------------------------
// Essencially a copy of the Goblin AI.
//---------------------------------------------------------------------------

[AiScript("traveler_a")]
public class TravelerAAi : AiScript
{
	public TravelerAAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.One);

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

[AiScript("traveler_a2")]
public class TravelerA2Ai : TravelerAAi
{
	public TravelerA2Ai()
	{
		Hates("/goblin/");
	}
}
