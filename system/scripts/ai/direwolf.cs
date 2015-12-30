//--- Aura Script -----------------------------------------------------------
// Direwolf AI
//--- Description -----------------------------------------------------------
// AI for direwolf creatures.
//--- Missing ---------------------------------------------------------------
// Fear, HatesAttacking, Aggro over time, Visual Angle and Audio Aggro
// Hates Battlestance delay
//---------------------------------------------------------------------------

[AiScript("direwolf")]
public class DirewolfAi : AiScript
{
	public DirewolfAi()
	{
		Doubts("/pc/", "/pet/", "/cow/");
		Hates("/dog/", "/sheep/");
		//Fear ("/bear/");
		//HatesAttacking("/direwolfkid/"); duration="500"
		HatesBattleStance(); // needs a 3000 delay
		SetAggroRadius(650); // 400 range audio missing
		//SetAggroDelay(6000);
		SetAggroLimit(AggroLimit.One);
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
		var rndAlert = Random();
		if (rndAlert < 5)
		{
			Do(Attack(3, 4000));
		}
		else if (rndAlert < 50)
		{
			if (Random() < 60)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else // 40%
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (rndAlert < 90)
		{
			Do(Circle(500, 1000, 5000));
		}
		else
		{
			Do(Circle(500, 1000, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50)
		{
			var rndAggro = Random();
			if (rndAggro < 25)
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
			else if (rndAggro < 62)
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000, 5000));
				Do(CancelSkill());
			}
			else
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
		var rndOH = Random();
		if (rndOH < 15)
		{
			Do(KeepDistance(1000, true, 2000));
		}
		else if (rndOH < 30)
		{
			Do(Wander());
		}
		else
		{
			Do(Attack(3));
			Do(Wait(4000, 4000));
		}
	}
}
