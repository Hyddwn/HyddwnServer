//--- Aura Script -----------------------------------------------------------
// Imp AIs
//--- Description -----------------------------------------------------------
// AIs for Imps and Giant Imps.
//---------------------------------------------------------------------------

[AiScript("imp")]
public class ImpAi : AiScript
{
	readonly string[] ImpIdle = new[]
	{
		L("Booyah!"),
		L("Ha ha"),
		L("Hahaha"),
		L("Growl.."),
		L("What?"),
	};

	readonly string[] ImpAlert = new[]
	{
		L("Why have you come?"),
		L("What's your business..."),
		L("Are you human?"),
		L("What are you?"),
		L("Set your items."),
		L("Go get lost."),
		L("My mouse got dirty."),
		L("Rubbish..."),
		"",
		"",
		"",
		"",
	};

	readonly string[] ImpAttack = new[]
	{
		L("Attack!"),
		L("Here I come!"),
		L("Fool."),
		L("Ha ha"),
		L("Hahaha"),
		"",
		"",
		"",
	};

	readonly string[] ImpDefense = new[]
	{
		L("Do you know how to use the Smash skill?"),
		L("Do you know how to use magic?"),
		L("Let's see what you've got!"),
		"",
		"",
	};

	readonly string[] ImpCounter = new[]
	{
		L("Do you know how to use the Smash skill?"),
		L("Do you know how to attack?"),
		L("Let's see what you've got!"),
		"",
		"",
	};

	readonly string[] ImpChargeLB1 = new[]
	{
		L("There is something under the keyboard!"),
		L("You like that?"),
		L("Please, I need guidance."),
		L("Do you know how to use this?"),
		L("Please wait."),
		L("Hey, wait."),
		L("Ran out of cash..."),
		"",
		"",
		"",
		"",
	};

	readonly string[] ImpChargeLB2 = new[] 
	{
		L("What is your IP?"),
		"",
		"",
	};

	readonly string[] ImpSmash = new[] 
	{
		L("Imp Smash!"),
		L("Here comes a Smash!"),
		L("Why don't you start over again."),
		L("Can I really use the Smash skill?"),
		"",
		"",
	};

	readonly string[] ImpOnHit = new[] 
	{
		L("Umph..."),
		L("Mmph!"),
		L("Gah!"),
		"",
		"",
	};

	readonly string[] ImpOnKnockDown = new[] 
	{
		L("Ouch!"),
		L("Are you a noob?"),
		L("It hurts."),
		"",
		"",
	};

	public ImpAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(600);
		SetAggroLimit(AggroLimit.Two);

		Doubts("/pc/", "/pet/");
		HatesNearby(2000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(Say(ImpIdle));
		}
		else if (Case(10))
		{
			Do(Wander(100, 500));
		}
		else if (Case(30))
		{
			Do(Wander(100, 500, false));
		}
		else if (Case(20))
		{
			Do(Wait(4000, 6000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
		}
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(CancelSkill());

		SwitchRandom();
		if (Case(20))
		{
			Do(Say(ImpAlert));
			Do(Wait(1000, 2000));
		}
		else if (Case(30))
		{
			Do(Say(ImpAlert));
			Do(Wait(1000, 4000));
			Do(Say(ImpAlert));
			Do(Circle(600, 1000, 2000));
		}

		Do(PrepareSkill(SkillId.Lightningbolt, Rnd(1, 2)));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		Do(StackAttack(SkillId.Lightningbolt));
		Do(CancelSkill());

		SwitchRandom();
		if (Case(10))
		{
			if (Random() < 50)
				Do(Wander(100, 200, false));

			Do(Say(ImpAttack));
			Do(Attack(3, 4000));
			Do(Wait(1000, 2000));

			SwitchRandom();
			if (Case(60))
			{
				Do(Say(ImpChargeLB1));
				Do(StackAttack(SkillId.Lightningbolt));
			}
			else if (Case(20))
			{
				Do(Say(ImpDefense));
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(Say(ImpChargeLB2));
			Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));

			if (Random() < 80)
			{
				Do(Say(ImpAttack));
				Do(Attack(3, 4000));
				Do(Say(ImpAttack));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(ImpSmash));
				Do(Attack(1, 4000));
				Do(Say(ImpAttack));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(ImpSmash));
				Do(CancelSkill());
				Do(Attack(3, 4000));
				Do(Say(ImpAttack));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Say(ImpDefense));
				Do(Wait(2000, 7000));
				Do(Say(ImpDefense));
			}
			Do(Wait(1000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say(ImpDefense));

			if (Random() < 60)
				Do(Circle(400, 1000, 2000));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(60))
			{
				Do(Say(ImpAlert));
				Do(Circle(400, 1000, 2000, false));
			}
			else if (Case(20))
			{
				Do(Follow(400, false, 5000));
			}
			else if (Case(20))
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Say(ImpCounter));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
			Do(Say(ImpAlert));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say(ImpAlert));
		Do(Attack(3, 4000));
		if (Random() < 40)
		{
			Do(Say(ImpChargeLB1));
			Do(StackAttack(SkillId.Lightningbolt));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say(ImpOnHit));

		SwitchRandom();
		if (Case(20))
		{
			Do(KeepDistance(1000, false, 2000));
		}
		else if (Case(50))
		{
			Do(Attack(3, 4000));
			Do(Wait(2000, 4000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say(ImpOnKnockDown));

		SwitchRandom();
		if (Case(25))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(50))
		{
			Do(PrepareSkill(SkillId.Defense));

			if (Random() < 60)
				Do(Circle(400, 1000, 2000));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(25))
		{
			Do(Attack(3, 4000));
			Do(Wait(1000));
			if (Random() < 40)
			{
				Do(Say(ImpChargeLB1));
				Do(StackAttack(SkillId.Lightningbolt));
				Do(Wait(1000, 2000));
			}
		}
	}
}

[AiScript("giantimp")]
public class GiantImpAi : ImpAi
{
	public GiantImpAi()
	{
		SetVisualField(1500, 120);
		SetAggroRadius(600);
		SetAggroLimit(AggroLimit.None);
	}
}
