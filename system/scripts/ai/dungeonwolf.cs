//--- Aura Script -----------------------------------------------------------
// Dungeon Wolf AI
//--- Description -----------------------------------------------------------
// AI for Wild Boars, Giant Jackals, Wood Jackals, Giant Wood Jackals,
// Blue Dire Wolves, and Burgundy Dire Wolves.
//--- Notes -----------------------------------------------------------------
// Missing Wolf Support.
//---------------------------------------------------------------------------

[AiScript("dungeonwolf")]
public class DungeonWolfAi : AiScript
{
	protected int WanderRadius = 500;

	public DungeonWolfAi()
	{
		SetVisualField(1600, 180);
		SetAggroRadius(1600);

		Hates("/pc/", "/pet/");
		Hates("/sheep/");
		Hates("/dog/");
		Doubts("/cow/");
		SetAggroLimit(AggroLimit.None);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(40))
		{
			if (Random() < 70)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(WanderRadius, 1000, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (Case(5))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(45))
		{
			Do(Circle(WanderRadius, 1000, 4000));
		}
		else if (Case(10))
		{
			Do(Circle(WanderRadius, 500, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50)
		{
			SwitchRandom();
			if (Case(25))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(WanderRadius, 1000, 5000));
				Do(CancelSkill());
			}
			else if (Case(37))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
			else if (Case(38))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
			}
		}
		else
		{
			Do(Attack(3, 5000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(15))
		{
			Do(KeepDistance(1000, true, 2000));
		}
		else if (Case(15))
		{
			Do(Timeout(2000, Wander()));
		}
		else if (Case(70))
		{
			Do(Attack(3));
			Do(Wait(4000, 4000));
		}
	}
}

[AiScript("dungeonwolf2")]
public class DungeonWolfAi2 : DungeonWolfAi
{
	public DungeonWolfAi2()
	{
		SetVisualField(1000, 180);
		SetAggroRadius(1000);

		SetAggroLimit(AggroLimit.One);
	}
}