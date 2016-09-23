//--- Aura Script -----------------------------------------------------------
// Werewolf AIs
//--- Description -----------------------------------------------------------
// AIs for werewolves. Gray and Blue Werewolves are the same as normal ones,
// the only difference are wander and aggro radii.
//---------------------------------------------------------------------------

[AiScript("werewolf")]
public class WerewolfAi : AiScript
{
	protected int WanderRadius = 500;

	public WerewolfAi()
	{
		SetVisualField(1500, 120);
		SetAggroRadius(1200);

		Hates("/pc/", "/pet/");
		SetAggroLimit(AggroLimit.One);

		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(WanderRadius, WanderRadius));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Circle(300, 2000, 2000, false, Random() < 50));
		}
		else if (Case(20))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(500, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(5000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(4000));
			Do(UseSkill());
		}

		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 6000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}

[AiScript("werewolf2")]
public class WerewolfAi2 : WerewolfAi
{
	public WerewolfAi2()
	{
		SetVisualField(1600, 120);
		SetAggroRadius(1600);

		WanderRadius = 1600;
	}
}
