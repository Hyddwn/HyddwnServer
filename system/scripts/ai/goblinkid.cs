//--- Aura Script -----------------------------------------------------------
//  Goblin Kid AI
//--- Description -----------------------------------------------------------
//  AI for Goblin Kids and Poison Goblin Kids.
//	Custom Wander and Circle Values, Official Values are insane & buggy
//---------------------------------------------------------------------------

[AiScript("goblinkid")]
public class GoblinKidAi : AiScript
{
	public GoblinKidAi()
	{
		SetAggroRadius(600); // angle 120 audio 200
		Hates("/pc/", "/pet/");
		SetAggroLimit(AggroLimit.One);

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
		var rndAggro = Random();
		if (rndAggro < 60) // 60%
		{
			Do(Wait(1000, 3000));
			Do(SwitchTo(WeaponSet.First));
			Do(Attack(2, 5000));
		}
		else if (rndAggro < 75) // 15%
		{
			Do(Say("!!!"));
			Do(PrepareSkill(SkillId.Smash));
			Do(Wander(300, 500, false));
			Do(Attack(1, 5000));
		}
		else if (rndAggro < 90) // 15%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(500, 1000, 1000, false));
			Do(CancelSkill());
		}
		else // 10%
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