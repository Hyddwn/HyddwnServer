//--- Aura Script -----------------------------------------------------------
// Direwolf AI
//--- Description -----------------------------------------------------------
// AI for direwolf creatures.
//--- Missing ---------------------------------------------------------------
// Fear, HatesAttacking
//---------------------------------------------------------------------------

[AiScript("direwolf")]
public class DirewolfAi : AiScript
{
	public DirewolfAi()
	{
		SetVisualField(650, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.One);

		Doubts("/pc/", "/pet/", "/cow/");
		Hates("/dog/", "/sheep/");
		//Fear ("/bear/");
		//HatesAttacking("/direwolfkid/", 500);
		HatesBattleStance(3000);
		HatesNearby(6000);

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
		if (Case(5))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(45))
		{
			if (Random() < 60)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (Case(40))
		{
			Do(Circle(500, 1000, 5000));
		}
		else if (Case(10))
		{
			Do(Circle(500, 1000, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50)
		{
			SwitchRandom();
			if (Case(25))
			{
				if (Random() < 50)
				{
					Do(PrepareSkill(SkillId.Defense));
					Do(Circle(400, 1000, 5000, true));
					Do(CancelSkill());
				}
				else
				{
					Do(PrepareSkill(SkillId.Defense));
					Do(Circle(400, 1000, 5000, false));
					Do(CancelSkill());
				}
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000, 5000));
				Do(CancelSkill());
			}
			else if (Case(35))
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
		else if (Case(30))
		{
			Do(Wander());
		}
		else if (Case(55))
		{
			Do(Attack(3));
			Do(Wait(4000, 4000));
		}
	}
}
