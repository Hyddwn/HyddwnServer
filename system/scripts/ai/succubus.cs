//--- Aura Script -----------------------------------------------------------
// Succubus AI
//--- Description -----------------------------------------------------------
// AI for Succubi.
//--- History ---------------------------------------------------------------
// 1.0 Official Succubus AI
//---------------------------------------------------------------------------

[AiScript("succubus")]
public class SuccubusAi : AiScript
{
	readonly string[] Alert1 = new[]
	{
		L("Who's there?"),
		L("Introduce yourself."),
		L("Who is it?"),
	};

	readonly string[] Alert2 = new[]
	{
		L("Why have you come here?"),
		L("Why have you come to see me?"),
		L("What brought you here?"),
		L("How come you are here?"),
	};

	readonly string[] Alert3 = new[]
	{
		L("Do not be afraid."),
		L("=)"),
		L("I'll come closer."),
	};

	readonly string[] Alert4 = new[]
	{
		L("We look alike yet different."),
		L("You're very cute."),
		L("Your eye color is beautiful."),
	};

	readonly string[] Alert5 = new[]
	{
		L("Quite cute..."),
		L("=)"),
		L("Have you come to see me?"),
	};

	readonly string[] Alert6 = new[]
	{
		L("Talk about anything that comes to your mind."),
		L("Am I that good looking?"),
		L("What are you thinking about?"),
	};

	readonly string[] Alert7 = new[]
	{
		L("Haha!"),
		L("Smile!"),
		L("The sound of your heartbeat is scary."),
	};

	readonly string[] Alert8 = new[]
	{
		L("Please wait..."),
		L("Do not be afraid."),
		L("Do not be afraid."),
	};

	readonly string[] Alert9 = new[]
	{
		L("Did not recognize you because it was too dark. (Laughter)"),
		L("Brigthened up a bit? (Laughter)"),
	};

	readonly string[] Alert10 = new[]
	{
		L("It ran out... (Laughter)"),
		L("It ran out... (Laughter)"),
		L("Ah, it's off... (Laughter)"),
		L("=)"),
	};

	readonly string[] Alert11 = new[]
	{
		L("You have scary Eyes."),
		L("=)"),
		L("You are gorgeous."),
		L("Ah..."),
		L("I'm starting to have interest in you."),
	};

	readonly string[] Alert12 = new[]
	{
		L("ha ha"),
		L("I would like to have a closer look."),
		L("What do you have in your hand?"),
		L("Are you looking at me?"),
	};

	readonly string[] Alert13 = new[]
	{
		L("You don't seem like an ordinary person."),
		L("I would like to see you from the side."),
		L("How come you look different?"),
	};

	readonly string[] AggroAttack = new[]
	{
		L("Here I come!"),
		L("Over here!"),
		L("Ah!"),
		L("Attack!"),
		L("Like this?"),
		L("Ho ho"),
		L("Haven't you used your skills?"),
		L("Will you be still?"),
		L("Will it work?"),
		L("Why aren't you fightning back?"),
	};

	readonly string[] AfterAttack = new[]
	{
		L("I am so happy today!"),
		L("This is fun!"),
		L(":-)"),
		L("Don't stand up."),
	};

	readonly string[] Defense = new[]
	{
		L("What are you looking at?"),
		L("Prepare yourself!"),
		L("Stand Still."),
		L("Please, don't."),
		L("Punch with love"),
	};

	readonly string[] WalkDefense = new[]
	{
		L("I'm leaving now."),
		L("Stand still."),
		L("Are you coming along?"),
		L("Don't follow me!"),
		L("I'm starting to have interest in you."),
	};

	readonly string[] AfterDefense = new[]
	{
		L("Did you know?"),
		L("You let me down."),
	};

	readonly string[] Smash = new[]
	{
		L("Here comes my smash!"),
		L("Prepare yourself!"),
		L("Punch with love"),
		L("Get ready! :-)"),
		L("Ah!"),
		L("Smash on the go!"),
	};

	readonly string[] AfterSmash = new[]
	{
		L("Just like this?"),
		L("Still the same?"),
		L("=)"),
		L("Not this time?"),
	};

	readonly string[] FakeSmash = new[]
	{
		L("Don't stand up."),
		L("Oops!"),
	};

	readonly string[] Counter = new[]
	{
		L("Don't follow me!"),
		L("Here I come!"),
		L("It's embarrassing..."),
	};

	readonly string[] AfterCounter = new[]
	{
		L("Just like that?"),
		L("..."),
		L("Sudden attack!"),
		L("Smile!"),
		L("I'm disappointed"),
	};

	readonly string[] AggroLB1 = new[]
	{
		L("Smile!"),
		L("Still left."),
		L("Great posture."),
		L("Watch out!"),
		L("It's a UFO!"),
		L("Wait a moment..."),
		L("Quiet!"),
		L("You have a phone call!!!"),
	};

	readonly string[] AggroLB2 = new[]
	{
		L("More, more!"),
		L("Last one!"),
		L("Still more to go."),
	};

	readonly string[] AggroFB2 = new[]
	{
		L("Still more to go."),
		L("Last one!"),
		L("Do you want more heat?"),
		L("Firebolt!"),
		L("Fire"),
	};

	readonly string[] AggroIB1 = new[]
	{
		L("Icebolt!"),
		L("Ice"),
		L("That was an Icebolt."),
		L("Cool, isn't it?"),
	};

	readonly string[] OnDefHit = new[]
	{
		L("Why would you want to hit me?"),
		L("Do you want to hit me?"),
		L("Do you want to strike me?"),
		L("Are you going to hit me?"),
		L("You're too harsh!"),
		L("You don't like me?"),
	};

	readonly string[] AfterDefFlurry = new[]
	{
		L("It was so much fun!!"),
		L("Success?"),
	};

	readonly string[] OnHitSay = new[]
	{
		L("..."),
		L("I'm disappointed"),
		L("Shoot"),
		L("You want me to leave?"),
	};

	readonly string[] OnKDwn = new[]
	{
		L("Oh wow."),
		L("Ouch..."),
		L("You have guts."),
		L("Ahhhhh!"),
		L("Gah!"),
		L("I admire you."),
	};

	public SuccubusAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.One);

		Doubts("/pc/", "/pet/");
		HatesBattleStance(8000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.KnockDown, SkillId.Counterattack, OnCounterKnockDown);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(Say("La la"));
			Do(Wait(2000, 4000));
			Do(Say("Drop your chilvalrous sword when you return. (Serious tone)"));
			Do(Wait(4000, 6000));
			Do(Say("As I see the brilliance of your weapon in your hands (Laughter)"));
			Do(Wait(2000, 4000));
			Do(Say("In which my reflected doppelganger somberly stands (Normal)"));
			Do(Wait(2000, 4000));
			Do(Say("To fiery anguish I succumb. (Normal)"));
			Do(Wait(3000, 5000));
			Do(Wander(100, 500));
			Do(Say("La la"));
			Do(Wait(2000, 4000));
			Do(Say("The moment you entered my chamber (Love)"));
			Do(Wait(4000, 6000));
			Do(Say("Was not a moment of abstract stupefaction. (Normal)"));
			Do(Wait(2000, 4000));
			Do(Say("For deep inside my heart was an attraction. (Normal)"));
			Do(Wait(2000, 4000));
			Do(Say("To none other than yourself in highest caliber. (Love)"));
			Do(Wait(3000, 5000));
			Do(Wander(100, 500));
			Do(Say("La la... (*Laugh*)"));
			Do(Wait(2000, 4000));
			Do(Say("Never must you close your eyes in alarm. (Sorrow)"));
			Do(Wait(4000, 6000));
			Do(Say("In your lustrous eyes must confine my radiating charm. (Normal)"));
			Do(Wait(2000, 4000));
			Do(Say("For you are my master... (Normal)"));
			Do(Wait(2000, 4000));
			Do(Say("For you are forever the master of the Black Rose... (Wink)"));
			Do(Wait(4000, 6000));
			Do(Say("La la... (*Laugh*)"));
		}
		else if (Case(10))
		{
			Do(Say("..."));
		}
		else if (Case(30))
		{
			Do(Wander(100, 500));
		}
		else if (Case(20))
		{
			Do(Wander(100, 500, false));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Firebolt)); // Just prepare - NO ATTACK
		}
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(CancelSkill());

		SwitchRandom();
		if (Case(20))
		{
			Do(Say(Alert1));
			Do(Wait(2000, 4000));
			Do(Say(Alert2));
			Do(Wait(4000, 6000));
			Do(Say(Alert3));
			Do(Wait(2000, 4000));
			Do(Follow(100, true, 2000));
			Do(Wait(2000, 4000));
			Do(Say(Alert4));
			Do(Wait(3000, 5000));
			Do(Circle(100, 3000, 3000, true, true));
			Do(Wait(3000, 5000));
			Do(Circle(100, 3000, 3000, false, true));
			Do(Say(Alert5));
			Do(Wait(3000, 5000));
			Do(Say(Alert6));
			Do(Wait(4000, 7000));

			if (Random() < 70)
			{
				Do(KeepDistance(600, false, 2000));
				Do(Wait(1000, 2000));
				Do(Say(Alert7));
			}
			else
			{
				Do(Say("I'm about to blush."));
				Do(Attack(3, 4000));
				Do(Wait(500));
				Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge and then attacks
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
				Do(Wait(1000, 2000));
				Do(Say("I adore you."));
			}
		}
		else if (Case(20))
		{
			Do(Say(Alert8));
			Do(PrepareSkill(SkillId.Firebolt, Rnd(1, 2, 5)));
			Do(Wait(1000, 2000));
			Do(Say(Alert9));
			Do(Wait(3000, 4000));
			Do(Say(Alert2));
			Do(Wait(4000, 5000));

			if (Random() < 30)
			{
				Do(StackAttack(SkillId.Firebolt));
				Do(Wait(2000, 3000));
				Do(Say("You're not mad at me, are you? (Laughter)"));
			}
			else
			{
				Do(CancelSkill());
				Do(Say(Alert10));
			}
		}
		else if (Case(30))
		{
			Do(Say(Alert11));
			Do(Wait(1000, 4000));
			if (Random() < 50)
			{
				Do(Say(Alert12));
				Do(Circle(600, 2000, 2000, true, true));
			}
			else
			{
				Do(Say(Alert13));
				Do(Circle(600, 2000, 2000, false, true));
			}
		}
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		Do(CancelSkill());

		SwitchRandom();
		if (Case(10))
		{
			if (Random() < 50)
				Do(Wander(100, 200, false));

			Do(Say(AggroAttack));
			Do(Attack(3, 4000));
			Do(Wait(1000, 2000));

			SwitchRandom();
			if (Case(60))
			{
				Do(Say(AggroLB1));
				Do(PrepareSkill(SkillId.Lightningbolt));
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));

				SwitchRandom();
				if (Case(40))
				{
					Do(Say(AggroLB2));
					Do(PrepareSkill(SkillId.Lightningbolt));
					Do(Wait(1000, 2000));
					Do(Attack(1, 4000));
					Do(Wait(1000, 2000));
				}
				else if (Case(40))
				{
					Do(Say(AggroAttack));
					Do(Attack(3, 4000));
					Do(Say(AfterAttack));
					Do(Wait(1000, 2000));
				}
			}
			else if (Case(20))
			{
				Do(Say(Defense));
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}
			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(Say(AggroLB1));
			Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge
			Do(Wait(1000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(500));

			SwitchRandom();
			if (Case(10))
			{
				Do(Say(AggroLB2));
				Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
				Do(Wait(1000, 2000));
			}
			else if (Case(10))
			{
				Do(Say(AggroFB2));
				Do(PrepareSkill(SkillId.Firebolt)); // 1 charge
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
				Do(Wait(1000, 2000));
			}
			else if (Case(80))
			{
				Do(Say(AggroAttack));
				Do(Attack(3, 4000));
				Do(Say(AfterAttack));
			}
			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(Smash));
				Do(Attack(1, 4000));
				Do(Say(AfterSmash));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(Smash));
				Do(CancelSkill());
				Do(Attack(3, 4000));
				Do(Say(FakeSmash));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Say(Defense));
				Do(Wait(2000, 7000));
				Do(Say(AfterDefense));
			}
			Do(Wait(1000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say(Defense));

			if (Random() < 60)
			{
				Do(Circle(400, 1000, 2000));
			}
			else
			{
				Do(Follow(400, true, 5000));
			}
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(30))
			{
				Do(Say("What about this side?"));
				Do(Circle(400, 1000, 2000, true, false));
			}
			else if (Case(30))
			{
				Do(Say("The right side seems to be open...", "Which hand do you use to attack?"));
				Do(Circle(400, 1000, 2000, false, false));
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
			Do(Say(Counter));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
			Do(Say(AfterCounter));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(50))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Say(WalkDefense));
				Do(KeepDistance(1000, false, 3000));

				SwitchRandom();
				if (Case(20))
				{
					Do(CancelSkill());
					Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge
					Do(Wait(1000, 2000));
					Do(Attack(1, 4000));
					Do(Wait(500));
					Do(Say(AggroLB1));

					SwitchRandom();
					if (Case(10))
					{
						Do(Say(AggroLB2));
						Do(PrepareSkill(SkillId.Lightningbolt));
						Do(Wait(1000, 2000));
						Do(Attack(1, 4000));
						Do(Wait(1000, 2000));
					}
					else if (Case(10))
					{
						Do(Say(AggroFB2));
						Do(PrepareSkill(SkillId.Firebolt));
						Do(Wait(1000, 2000));
						Do(Attack(1, 4000));
						Do(Wait(1000, 2000));
					}
					else if (Case(80))
					{
						Do(Say(AggroAttack));
						Do(Attack(3, 4000));
						Do(Say(AfterAttack));
					}
				}
				else if (Case(20))
				{
					Do(CancelSkill());
					Do(PrepareSkill(SkillId.Firebolt));
					Do(Wait(1000, 2000));
					Do(Attack(1, 4000));
					Do(Say(AggroFB2));
					Do(Wait(1000, 2000));
				}
				else if (Case(20))
				{
					Do(CancelSkill());
					Do(PrepareSkill(SkillId.Icebolt));
					Do(Wait(1000, 2000));
					Do(Attack(1, 4000));
					Do(Say(AggroIB1));

					SwitchRandom();
					if (Case(10))
					{
						Do(Say(AggroLB2));
						Do(PrepareSkill(SkillId.Lightningbolt));
						Do(Wait(1000, 2000));
						Do(Attack(1, 4000));
						Do(Wait(1000, 2000));
					}
					else if (Case(10))
					{
						Do(Say(AggroFB2));
						Do(PrepareSkill(SkillId.Firebolt));
						Do(Wait(1000, 2000));
						Do(Attack(1, 4000));
						Do(Wait(1000, 2000));
					}
					else if (Case(80))
					{
						Do(Say(AggroAttack));
						Do(Attack(3, 4000));
						Do(Say(AfterAttack));
						Do(Wait(1000, 2000));
					}
				}
				else if (Case(40))
				{
					Do(Wait(3000, 4000));
					Do(Say("..."));
					Do(Wait(1000, 2000));
					Do(CancelSkill());
				}
			}
			else if (Case(20))
			{
				Do(Say(WalkDefense));
				Do(KeepDistance(1400, false, 3000));

				SwitchRandom();
				if (Case(20))
				{
					Do(CancelSkill());
					Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 2))); 
					Do(Wait(500));
					Do(Say("Flash!"));
				}
				else if (Case(20))
				{
					Do(CancelSkill());
					Do(PrepareSkill(SkillId.Firebolt));
					Do(Wait(1000, 2000));
					Do(Attack(1, 4000));
					Do(Say(AggroFB2));
					Do(Wait(1000, 2000));
				}
				else if (Case(20))
				{
					Do(CancelSkill());
					Do(PrepareSkill(SkillId.Icebolt));
					Do(Wait(1000, 2000));
					Do(Attack(1, 4000));
					Do(Say(AggroIB1));
				}
				else if (Case(40))
				{
					Do(Wait(3000, 4000));
					Do(Say("..."));
					Do(Wait(1000, 2000));
					Do(CancelSkill());
				}
			}
			else if (Case(30))
			{
				Do(Say(Counter));
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(3000, 7000));
				Do(Say(AfterCounter));
				Do(CancelSkill());
				Do(Wait(1000, 2000));
			}
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say(OnDefHit));

		if (Random() < 40)
		{
			Do(Attack(3, 4000));
			Do(Wait(500));
			if (Random() < 70)
			{
				Do(Say(AggroLB1));
				Do(PrepareSkill(SkillId.Lightningbolt)); // 1 charge
				Do(Wait(1000, 2000));
				Do(Attack(1, 4000));
				Do(Wait(1000, 2000));
			}
		}
		else
		{
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
			Do(Attack(1, 4000));
			Do(Say(AfterDefFlurry));
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say(OnHitSay));

		SwitchRandom();
		if (Case(20))
		{
			Do(KeepDistance(10000, false, 2000));
		}
		else if (Case(50))
		{
			Do(Attack(3, 4000));
			Do(Wait(500));
		}
	}

	private IEnumerable OnKnockDown()
	{
		Do(Wait(500));
		Do(Say(OnKDwn));
	}

	private IEnumerable OnCounterKnockDown()
	{
		Do(SwitchArmor(15046, 15047, 15048, 15049, 15050));
	}
}
