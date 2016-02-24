//--- Aura Script -----------------------------------------------------------
// Gremlin AI
//--- Description -----------------------------------------------------------
// AI for Gremlin, Poison Gremlin and Gold Gremlin.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
// Missing some of the Chat
//---------------------------------------------------------------------------

[AiScript("gremlin")]
public class GremlinAi : AiScript
{
	readonly string[] GremlinChat = new[]
	{
		L("..."),
		L("....."),
		L("Grrr.."),
		L("Grr.."),
		"",
		"",
		"",
		"",
		"",
	};

	public GremlinAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.One);

		Doubts("/pc/", "/pet/");
		HatesNearby(4000);
		HatesBattleStance(1000);

		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 500));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(50))
		{
			Do(Say(GremlinChat));
			Do(Circle(500, 1000, 5000));
		}
		else if (Case(50))
		{
			Do(Say(GremlinChat));
			Do(Circle(500, 1000, 5000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		Do(Say(GremlinChat));
		Do(Circle(300, 800, 1000, false));

		SwitchRandom();
		if (Case(70))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(Attack(1, 4000));
			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
			Do(Wait(500, 2000));
		}
		else if (Case(5))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(5))
		{
			Do(SwitchTo(WeaponSet.First));
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(3000, 5000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		if (Creature.Life < Creature.LifeMax * 0.40f)
		{
			Do(Say(GremlinChat));
			Do(KeepDistance(1600, false, 10000));
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(4000, 6000));
		}
		else
		{
			Do(Say(GremlinChat));
			SwitchRandom();
			if (Case(30))
			{
				Do(KeepDistance(1200, false, 6000));
			}
			else if (Case(40))
			{
				Do(Attack(1, 8000));
				Do(Wait(500, 2000));
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
			}
		}
	}
}
