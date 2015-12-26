//--- Aura Script -----------------------------------------------------------
//  Coyote AI
//--- Description -----------------------------------------------------------
//  AI for Coyote type monster.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
// Missing: aggro over time, visual angle, wolf support, fear
//---------------------------------------------------------------------------

[AiScript("Coyote")]
public class CoyoteAi : AiScript
{
	public CoyoteAi()
	{
		SetAggroRadius(650); // audio 500 visual Angle 120°
		Doubts("/pc/", "/pet/");
		Doubts("/cow/");
		Hates("/sheep/");
		Hates("/dog/");
		//Fears("/junglewolf/")
		HatesBattleStance(); // 3000 delay
		// Aggro over time 10000

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 400));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		var rndAlert = Random();
		if (rndAlert < 40) // 40%
		{
			if (Random() < 70) // 70%
			{
				if (Random() < 50) // 50%
				{
					Do(PrepareSkill(SkillId.Defense));
					Do(Circle(500, 1000, 5000, true));
					Do(CancelSkill());
				}
				else // 50%
				{
					Do(PrepareSkill(SkillId.Defense));
					Do(Circle(500, 1000, 5000, false));
					Do(CancelSkill());
				}
			}
			else // 30%
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (rndAlert < 45) // 5%
		{
			Do(Attack(3, 4000));
		}
		else if (rndAlert < 90) // 45%
		{
			if (Random() < 50)
			{
				Do(Circle(500, 1000, 4000, true));
			}
			else
			{
				Do(Circle(500, 1000, 4000, false));
			}
		}
		else // 10%
		{
			if (Random() < 50)
			{
				Do(Circle(500, 1000, 5000, true, false));
			}
			else
			{
				Do(Circle(500, 1000, 5000, false, false));
			}
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 60) // 60%
		{
			var rndnum = Random();
			if (rndnum < 25) // 25%
			{
				Do(PrepareSkill(SkillId.Defense));
				if (Random() < 50)
				{
					Do(Circle(400, 1000, 5000, true));
					Do(CancelSkill());
				}
				else
				{
					Do(Circle(400, 1000, 5000, false));
					Do(CancelSkill());
				}
			}
			else if (rndnum < 50) // 25%
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
			else if (rndnum < 75) // 25%
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
			}
			else // 25%
			{
				if (Random() < 50)
				{
					Do(Circle(400, 1000, 1000, true, false));
				}
				else
				{
					Do(Circle(400, 1000, 1000, false, false));
				}
			}
		}
		else // 40%
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
		if (rndOH < 15) // 15%
		{
			Do(Timeout(2000, KeepDistance(1000, true)));
		}
		else if (rndOH < 30) // 15%
		{
			Do(Timeout(2000, Wander()));
		}
		else // 70%
		{
			Do(Attack(3));
			Do(Wait(4000, 4000));
		}
	}
}
