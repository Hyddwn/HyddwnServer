//--- Aura Script -----------------------------------------------------------
// Ice Sprite AI
//--- Description -----------------------------------------------------------
// AI for Ice Sprites.
//---------------------------------------------------------------------------

[AiScript("icesprite")]
public class IceSpriteAi : AiScript
{
	public IceSpriteAi()
	{
		SetVisualField(1200, 120);
		SetAggroRadius(800);

		Hates("/pc/", "/pet/");
		//HatesAttacking("/elemental/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		if (Random() < 50)
			Do(Wander(300, 500));
		else
			Do(Wait(2000, 4000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Wait(1000, 2000));
		}
		else if (Case(30))
		{
			Do(Wait(1000, 4000));
			Do(Circle(600, 2000, 2000));
		}

		Do(PrepareSkill(SkillId.Icebolt, Rnd(1, 2)));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(10))
		{
			if (Random() < 50)
				Do(Wander(100, 200, false));

			Do(Attack(3, 4000));

			SwitchRandom();
			if (Case(60))
			{
				Do(Say("!!!"));
				Do(StackAttack(SkillId.Icebolt));

			}
			else if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}
			Do(Wait(500, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));

			if (Random() < 60)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(60))
				Do(Circle(400, 2000, 2000));
			else if (Case(20))
				Do(Follow(400, false, 5000));
			else if (Case(20))
				Do(KeepDistance(1000, false, 5000));
		}
		else if (Case(70))
		{
			Do(StackAttack(SkillId.Icebolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));
			Do(Attack(3, 4000));

			if (Random() < 50)
			{
				Do(StackAttack(SkillId.Icebolt));
				Do(Attack(3, 4000));

				if (Random() < 40)
				{
					Do(StackAttack(SkillId.Icebolt));
					Do(Attack(3, 4000));
				}
			}

			Do(Wait(500, 2000));
		}
	}

	private IEnumerable OnHit()
	{
		if (Random() < 50)
			Do(KeepDistance(1000, false, 2000));
		else
			Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		if (Random() < 50)
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 60)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else
		{
			Do(Attack(3, 8000));
			if (Random() < 40)
			{
				Do(StackAttack(SkillId.Icebolt));
				Do(Wait(1000, 2000));
			}
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3, 4000));

		if (Random() < 40)
		{
			Do(Say("!"));
			Do(StackAttack(SkillId.Icebolt));
			Do(Wait(1000, 2000));
			Do(KeepDistance(1000, false, 2000));
		}
	}
}
