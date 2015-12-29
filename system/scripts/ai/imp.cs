//--- Aura Script -----------------------------------------------------------
// Imp AI
//--- Description -----------------------------------------------------------
// AI for Imps.
//---   Missing   -----------------------------------------------------------
// Magic Charges and Magic Attack
// Do(Wait(1000, 2000)); Do(Attack(1, 4000)); are not official.
// Without the wait the AI would be WAY to fast.
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
		SetAggroRadius(950); // Angle 120 Audio 400
		//SetAggroDelay(2000);
		Doubts("/pc/", "/pet/");
		SetAggroLimit(AggroLimit.Two);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		var rndidle = Random();
		if (rndidle < 10) // 10%
		{
			Do(Say(ImpIdle));
		}
		else if (rndidle < 20) // 10%
		{
			Do(Wander(100, 500));
		}
		else if (rndidle < 50) // 30%
		{
			Do(Wander(100, 500, false));
		}
		else if (rndidle < 70) // 20%
		{
			Do(Wait(4000, 6000));
		}
		else if (rndidle < 80) // 10%
		{
			Do(PrepareSkill(SkillId.Lightningbolt)); // Just prepare - NO ATTACK
		}
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(CancelSkill());
		var rndalert = Random();
		if (rndalert < 20) // 20%
		{
			Do(Say(ImpAlert));
			Do(Wait(1000, 2000));
		}
		else if (rndalert < 50) // 30%
		{
			Do(Say(ImpAlert));
			Do(Wait(1000, 4000));
			Do(Say(ImpAlert));
			Do(Circle(600, 1000, 2000));
		}
		if (Random() < 50)
		{
			Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge NO attack
		}
		else
		{
			Do(PrepareSkill(SkillId.Lightningbolt)); // 2 charges NO attack
		}
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge and then attacks
		Do(Wait(1000, 2000));
		Do(Attack(1, 4000));
		Do(CancelSkill());
		var rndagr = Random();
		if (rndagr < 10) // 10%
		{
			if (Random() < 50)
			{
				Do(Wander(100, 200, false));
			}
			Do(Say(ImpAttack));
			Do(Attack(3, 4000));
			Do(Wait(1000, 2000));
			var rndagr2 = Random();
			if (rndagr2 < 60) // 60%
			{
				Do(Say(ImpChargeLB1));
				Do(PrepareSkill(SkillId.Lightningbolt));
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
			}
			else if (rndagr2 < 80) // 20%
			{
				Do(Say(ImpDefense));
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}
			Do(Wait(500, 2000));
		}
		else if (rndagr < 30) // 20%
		{
			Do(Say(ImpChargeLB2));
			var charge = Random();
			if (charge < 56)
			{
				Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge ?? Only attacks with Do(Attack(3, 4000)); that follows later
				Do(Wait(1000));
				Do(Attack(1, 4000));
			}
			else if (charge < 67)
			{
				Do(PrepareSkill(SkillId.Lightningbolt)); // 2 charges
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
			}
			else if (charge < 78)
			{
				Do(PrepareSkill(SkillId.Lightningbolt)); // 3 charges
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
			}
			else if (charge < 89)
			{
				Do(PrepareSkill(SkillId.Lightningbolt)); // 4 charges
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
			}
			else
			{
				Do(PrepareSkill(SkillId.Lightningbolt)); // 5 charges
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
			}
			if (Random() < 80)
			{
				Do(Say(ImpAttack));
				Do(Attack(3, 4000));
				Do(Say(ImpAttack));
			}
			Do(Wait(500, 2000));
		}
		else if (rndagr < 50) // 20%
		{
			var rndagr3 = Random();
			if (rndagr3 < 40)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(ImpSmash));
				Do(Attack(1, 4000));
				Do(Say(ImpAttack));
			}
			else if (rndagr3 < 70)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(ImpSmash));
				Do(CancelSkill());
				Do(Attack(3, 4000));
				Do(Say(ImpAttack));
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Say(ImpDefense));
				Do(Wait(2000, 7000));
				Do(Say(ImpDefense));
			}
			Do(Wait(1000, 2000));
		}
		else if (rndagr < 60) // 10%
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say(ImpDefense));
			var rndagr4 = Random();
			if (rndagr4 < 60)
			{
				Do(Circle(400, 1000, 2000));
			}
			else
			{
				Do(Follow(400, true, 5000));
			}
			Do(CancelSkill());
		}
		else if (rndagr < 70) // 10%
		{
			var rndagr5 = Random();
			if (rndagr5 < 60)
			{
				Do(Say(ImpAlert));
				Do(Circle(400, 1000, 2000, false));
			}
			else if (rndagr5 < 80)
			{
				Do(Follow(400, false, 5000));
			}
			else
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
		else if (rndagr < 80) // 10%
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
			Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge
			Do(Wait(1000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnHit()
	{
		var ohrdn = Random();
		Do(Say(ImpOnHit));
		if (ohrdn < 20)
		{
			Do(KeepDistance(1000, false, 2000));
		}
		else if (ohrdn < 70)
		{
			Do(Attack(3, 4000));
			Do(Wait(2000, 4000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say(ImpOnKnockDown));
		var knockd = Random();
		if (knockd < 25)
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (knockd < 75)
		{
			Do(PrepareSkill(SkillId.Defense));
			var knockd2 = Random();
			if (knockd2 < 60)
			{
				Do(Circle(400, 1000, 2000));
			}
			else
			{
				Do(Follow(400, true, 5000));
			}
			Do(CancelSkill());
		}
		else
		{
			Do(Attack(3, 4000));
			Do(Wait(1000));
			if (Random() < 40)
			{
				Do(Say(ImpChargeLB1));
				Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
				Do(Wait(1000, 2000));
			}
		}
	}
}