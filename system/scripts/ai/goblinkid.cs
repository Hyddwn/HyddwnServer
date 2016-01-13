//--- Aura Script -----------------------------------------------------------
// Young Goblin AI
//--- Description -----------------------------------------------------------
// AI for Young Goblins and Poison Goblins.
// Custom Wander and Circle Values, official Values are insane & buggy.
//---------------------------------------------------------------------------

[AiScript("goblinkid")]
public class YoungGoblinAi : AiScript
{
	public YoungGoblinAi()
	{
		SetVisualField(600, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.One);

		Hates("/pc/", "/pet/");

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
		Do(CancelSkill());

		SwitchRandom();
		if (Case(60))
		{
			Do(Wait(1000, 3000));
			Do(SwitchTo(WeaponSet.First));
			Do(Attack(2, 5000));
		}
		else if (Case(15))
		{
			Do(Say("!!!"));
			Do(PrepareSkill(SkillId.Smash));
			Do(Wander(300, 500));
			Do(Attack(1, 5000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(500, 1000, 1000, false));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(Say("..."));
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(3000, 5000));
			Do(CancelSkill());
		}
		Do(Wait(2000, 3000));
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Wander(300, 500));
		Do(Attack());
		Do(Wait(2000, 5000));
	}
}