//--- Aura Script -----------------------------------------------------------
// Hollowknight AI
//--- Description -----------------------------------------------------------
// AI for Hollowknight.
//---------------------------------------------------------------------------

[AiScript("hollowknight")]
public class HollowknightAi : AiScript
{
	public HollowknightAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);
		SetAggroLimit(AggroLimit.Three);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		// Kind of a hack job, removes shield if Hollow Knight is using a two-hander
		if (HasEquipped("/twohand/") && HasEquipped("/shield/"))
		{
			var shield = this.Creature.Inventory.GetItemAt(Pocket.LeftHand1, 0, 0);
			this.Creature.Inventory.Remove(shield);
		}
		
		SwitchRandom();
		if (Case(80))
		{
			Do(Wander(300, 500, Random(100) < 50));
		}
		else if (Case(20))
		{
			Do(Wait(3000, 6000));
		}
	}

	protected override IEnumerable Alert()
	{
		Do(Wait(1000, 3000));
		Do(Circle(600, 2000, 2000, Random(100) < 50));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(KeepDistance(800, false, 2000));
			if (Random() < 50)
			{
				Do(Circle(800, 1000, 1000, false));
			}
			Do(SwitchTo(WeaponSet.Second));
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.ArrowRevolver));
				Do(RangedAttack(3000));
				Do(RangedAttack(3000));
				Do(RangedAttack(3000));
				if (Random() < 50)
				{
					Do(RangedAttack(3000));
					Do(RangedAttack(3000));
				}
				Do(CancelSkill());
			}
			else if (Case(40))
			{
				Do(RangedAttack(4000));
				Do(RangedAttack(4000));
				Do(RangedAttack(4000));
				Do(RangedAttack(4000));
			}
			else if (Case(20))
			{
				Do(PrepareSkill(SkillId.MagnumShot));
				Do(RangedAttack(3000));
			}
			if (Random() < 25)
			{
				Do(SwitchTo(WeaponSet.First));
				Do(PrepareSkill(SkillId.Defense));
				SwitchRandom();
				if (Case(35))
				{
					Do(Circle(800, 1000, 5000));
				}
				else if (Case(40))
				{
					Do(Circle(800, 1000, 2000));
					Do(CancelSkill());
					Do(PrepareSkill(SkillId.Windmill));
					Do(UseSkill());
					Do(Wait(1000, 3000));
					Do(CancelSkill());
				}
				else if (Case(40))
				{
					Do(Circle(800, 1000, 2000));
					Do(CancelSkill());
					Do(Attack(Rnd(1, 1, 2, 3), 3000));
				}
			}
		}
		else if (Case(60))
		{
			Do(SwitchTo(WeaponSet.First));
			if (Random() < 25)
			{
				Do(Attack(3, 4000));
			}
			SwitchRandom();
			if (Case(30))
			{
				Do(Attack(Rnd(1, 2, 2, 2, 3, 3), 3000));
			}
			else if (Case(30))
			{
				Do(Attack(1, 700));
				Do(Wait(1000));
				Do(Attack(1, 700));
				Do(Wait(1000));
				if (Random(100) < 50)
				{
					Do(PrepareSkill(SkillId.Windmill));
					Do(UseSkill());
					Do(Wait(1000, 3000));
					Do(CancelSkill());
				}
			}
			else if (Case(15))
			{
				Do(PrepareSkill(SkillId.Smash));
				if (Random(100) < 50)
				{
					Do(Follow(200, true, 5000));
					Do(CancelSkill());
					Do(Attack(Rnd(1, 1, 2, 3), 3000));
				}
				else
				{
					Do(Attack(1, 5000));
				}
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
	}

	private IEnumerable OnDefenseHit()
	{
		Do(SwitchTo(WeaponSet.First));
		Do(Attack(3));
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(SwitchTo(WeaponSet.First));
		SwitchRandom();
		if (Case(33))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(34))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(UseSkill());
			Do(Wait(2000, 4000));
		}
		else if (Case(33))
		{
			Do(Attack(Rnd(1, 3), 3000));
		}
	}
}
