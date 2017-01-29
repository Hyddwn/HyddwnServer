//--- Aura Script -----------------------------------------------------------
// Basic Combat Classes
//--- Description -----------------------------------------------------------
// Class taught by Ranald, to teach players about various combat skills.
// This script handles all classes and their quests.
//---------------------------------------------------------------------------

public class SchoolCombatRanaldQuestScript : GeneralScript
{
	public override void Load()
	{
		AddHook("_ranald", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "about_study")
			return HookResult.Continue;

		var stateName = "RanaldClassState";
		var lastName = "RanaldClassLast";
		var state = (int)npc.Player.Vars.Perm.Get(stateName, 0);
		var last = (string)npc.Player.Vars.Perm.Get(lastName, "never");
		var now = ErinnTime.Now;
		var today = now.ToString("yyyy-MM-dd");
		var start = 7;
		var end = 23;
		var remaintime = end - now.Hour;
		var lastState = 10;

		if (npc.Player.QuestInProgress(200040) || npc.Player.QuestInProgress(200041) || npc.Player.QuestInProgress(200028))
		{
			npc.Msg(L("Are you working on the assignment I gave you?<br/>You can't proceed to the next class unless you complete the assignment first."));
		}
		else if (state > lastState)
		{
			npc.Msg(L("I've got nothing more to teach you.<br/>But don't forget that you're still learning. Maintain your focus and keep up the good work."));
		}
		else if (last == today)
		{
			npc.Msg(L("Today's class is over.<br/>You can spend the rest of the day as you wish."));
		}
		else if (now.Hour < start || now.Hour >= end)
		{
			npc.Msg(L("This is not the time for class. Come back tomorrow morning."));
		}
		else
		{
			var exp = 0;
			var cost = 0;
			var msg = "";
			var name = "";
			var title = L("A class for learning combat theory.");
			var paydesc = "";
			var reward = "";
			var desc = "";
			var func = (Func<NpcScript, Task>)null;

			switch (state)
			{
				case 0:
					exp = 30;
					cost = 300;
					msg = L("Are you interested in the combat class?<br/>If you're sick and tired of battles run by simple mouse clicks,<br/>my class is definitely worth spending some time and money on.");
					name = L("Basic Combat 1-1");
					paydesc = L("3-day course. Total tuition of 300G");
					reward = L("* STR Increase<br/>* EXP Reward");
					desc = L("A class for the basics of combat. The complete class is 3 hours long with a simple assignment in the last hour.");
					func = Class1_1;
					break;

				case 1:
					exp = 30;
					msg = L("Uh? Hey, you're in my class, aren't you?<br/>Ready for the next class?<br/>The tuition was already paid last time, so you don't owe anything.");
					name = L("Basic Combat 1-2");
					paydesc = L("Total 300G for Tuition of 3-day Coursework (Already paid)");
					reward = L("* STR Increase<br/>* EXP Reward");
					desc = L("A class for learning basic combat. Today is our second class of the 3 hour course.");
					func = Class1_2;
					break;

				case 2:
					exp = 100;
					msg = L("Today is the last day of Basic Combat 1.<br/>Do you want to take the class?");
					name = L("Basic Combat 1-3");
					paydesc = L("Total 300G for Tuition of 3-day Coursework (Already paid)");
					reward = L("* STR Increase<br/>* EXP Reward");
					desc = L("A class for learning basic combat. It's the last class of the 3. There will be an assignment at the end.");
					func = Class1_3;
					break;

				case 3:
					exp = 50;
					cost = 500;
					msg = L("This is the second class on Basic Combat.<br/>Are you ready to take it?");
					name = L("Basic Combat 2-1");
					paydesc = L("3-day course. Total tuition of 500G");
					reward = L("* Defense Skill<br/>* STR Increase, EXP Reward");
					desc = L("A class for learning about the Defense skill. The complete class is comprised of 3 sessions with a simple assignment at the end.");
					func = Class2_1;
					break;

				case 4:
					exp = 50;
					msg = L("Did you watch a fox defending itself?<br/>Then, let's start the next class.");
					name = L("Basic Combat 2-2");
					paydesc = L("Total 500G for Tuition of 3-day Coursework (Already paid)");
					reward = L("* Defense Skill<br/>* STR Increase, EXP Reward");
					desc = L("A class for learning about the Defense skill. Today is the second class of the 3. You will learn how to use the Defense skill.");
					func = Class2_2;
					break;

				case 5:
					exp = 150;
					msg = L("Today is the last day of Basic Combat 2.<br/>Do you want to take the class?");
					name = L("Basic Combat 2-3");
					paydesc = L("Total 500G for Tuition of 3-day Coursework (Already paid)");
					reward = L("* Defense Skill<br/>* STR Increase, EXP Reward");
					desc = L("A class for learning about the Defense skill. It's the last class of the 3. There will be an assignment at the end.");
					func = Class2_3;
					break;

				case 6:
					exp = 100;
					cost = 1000;
					msg = L("This is the last class on Basic Combat.<br/>Are you Interested?");
					name = L("Basic Combat 3-1");
					paydesc = L("5-day course. Total tuition of 1000G");
					reward = L("* Smash skill<br/>* Will Increase, STR Increase, EXP Reward");
					desc = L("A class for learning comprehensive elementary combat. The complete class is comprised of 5 sessions with an assignment at the end.");
					func = Class3_1;
					break;

				case 7:
					exp = 100;
					msg = L("It was Basic Combat 3, right?<br/>Are you ready to start?");
					name = L("Basic Combat 3-2");
					paydesc = L("Total 1000G for Tuition of 5-day Coursework (Already paid)");
					reward = L("* Smash skill<br/>* Will Increase, STR Increase, EXP Reward");
					desc = L("A class for learning comprehensive elementary combat. This is the second class of the 5, and you will learn about the Smash skill.");
					func = Class3_2;
					break;

				case 8:
					exp = 100;
					msg = L("You're having troubles fighting alone?<br/>Then, today's class will help. Listen carefully.");
					name = L("Basic Combat 3-3");
					paydesc = L("Total 1000G for Tuition of 5-day Coursework (Already paid)");
					reward = L("* Smash skill<br/>* Will Increase, STR Increase, EXP Reward");
					desc = L("A class for learning comprehensive elementary combat. This is the third class of the 5, and you will learn about the warrior party combination.");
					func = Class3_3;
					break;

				case 9:
					exp = 100;
					msg = L("Remember the last class about the warrior-warrior party?<br/>Let's talk about a different type of party play.");
					name = L("Basic Combat 3-4");
					paydesc = L("Total 1000G for Tuition of 5-day Coursework (Already paid)");
					reward = L("* Smash skill<br/>* Will Increase, STR Increase, EXP Reward");
					desc = L("A class for learning comprehensive elementary combat. This is the fourth class of the 5. We will now focus on the warrior and archer party combination.");
					func = Class3_4;
					break;

				case 10:
					exp = 100;
					msg = L("Today is the last day of Basic Combat 3.<br/>Do you want to take the class?");
					name = L("Basic Combat 3-5");
					paydesc = L("Total 1000G for Tuition of 5-day Coursework (Already paid)");
					reward = L("* Smash skill<br/>* Will Increase, STR Increase, EXP Reward");
					desc = L("A class for learning comprehensive elementary combat. This is the last class of the 5. There is an assignment waiting at the end of the class.");
					func = Class3_5;
					break;
			}

			var school = GetSchoolTag(name, title, paydesc, reward, desc, remaintime);

			npc.Msg(msg + school);
			if (await npc.Select() == "@accept")
			{
				if (cost > 0)
				{
					if (npc.Gold < cost)
					{
						npc.Msg(L("Oh, you're short of money for the class.<br/>I'm sorry, but come back later when you can pay the tuition."));
						npc.End();
						return HookResult.End;
					}

					npc.Gold -= cost;
				}

				await func(npc);

				if (exp > 0)
					npc.Player.GiveExp(exp);

				npc.Player.Vars.Perm[stateName] = state + 1;
				npc.Player.Vars.Perm[lastName] = today;
			}
			else
			{
				npc.Msg(L("Not interested in my class? Well, then."));
			}
		}

		await npc.Conversation();
		npc.End();

		return HookResult.End;
	}

	private string GetSchoolTag(string name, string title, string paydesc, string reward, string desc, int remaintime)
	{
		return string.Format(
			"<school><name>{0}</name><title>{1}</title><values paydesc='{2}' remaintime='{3}' history='0'/><rewards><reward>{4}</reward></rewards><desc>{5}</desc></school>",
			name, title, paydesc, remaintime, reward, desc
		);
	}

	public async Task Class1_1(NpcScript npc)
	{
		npc.Msg(L("Thank you for taking my class.<br/>You don't have to pay the tuition every day. What you paid today covers the entire class.<br/>The class normally lasts 3 days, but it may take several more days if you don't keep up."));
		npc.Msg(L("It's mostly a theory class,<br/>but you may get some homework or questions. So stay alert."));
		npc.Msg(L("For your information, I am strict in the classroom. So don't make a mistake!"));
		npc.Msg(L("Learning skills is not the only thing you can do at the School.<br/>You can improve several stats like Intelligence, Strength and Will without bending your back on leveling up,<br/>and even gain additional EXP."));
		npc.Msg(L("You are going to learn the very basics of combat today.<br/>If you have experience fighting an animal or monster,<br/>you've probably realized your attack doesn't do anything to your enemy<br/>when it wakes up after being knocked down."));
		npc.Msg(L("At that moment, your enemy has an advantage status.<br/>It's very dangerous to attack then because the target is positioned lower than the attacker.<br/>When the two hit each other at the same time, they end up exchanging counterattacks."));
		npc.Msg(L("When you attack an opponent that has an advantage status,<br/>the enemy will hit you instead or both will hit each other."));
		npc.Msg(L("That doesn't necessarily mean you just sit and watch the enemy on the ground.<br/>You can either prepare and launch your skill or change position.<br/>You can use that time to think of strategies that will help you in the battle."));
		npc.Msg(L("Watch other people engaged in battles closely and try to see how they fight.<br/>Well, that's a all for today."));
	}

	public async Task Class1_2(NpcScript npc)
	{
		npc.Msg(L("Let me see... Where were we last time...<br/>Hmm... Yes, we learned about the advantage status in the last class."));
		npc.Msg(L("Today is about Stamina.<br/>If you run out of Stamina, you can't do anything.<br/>A sound body is supposed to be filled with Stamina."));
		npc.Msg(L("<image name='Stamina'/>Stamina is the yellow bar in the status window. When it is down to zero, you can't do anything.<br/>Keep in mind that Stamina is used not just for physical activities but also for reading and other mental activities as well."));
		npc.Msg(L("<image name='Stamina'/>You will now see a dark region in the Stamina bar.<br/>This is your second day, so you probably know everything you need to know about this,<br/>but that shows how hungry you are."));
		npc.Msg(L("When you get hungry, the maximum Stamina will drop and you can't really move or work as usual.<br/>But I haven't seen anyone starving to death.<br/>It seems the gauge stops at around 50%."));
		npc.Msg(L("The Resting skill can easily recover the Stamina... Even when walking or running too.<br/>You can recover the hunger gauge only when you eat some food.<br/>You can buy it from a shop or get some from other players at a nearby campfire."));
		npc.Msg(L("That'll be it for today.<br/>Study how to use Stamina well."));
	}

	public async Task Class1_3(NpcScript npc)
	{
		npc.Msg(L("Then let's begin the class. I need you to sit up and focus.<br/>HP is the basic parameter.<br/>What happens when you run out of HP?<br/>You lose consciousness. But don't worry. You are not dead yet..."));
		npc.Msg(L("<image name='HP'/>Just like Stamina, the HP bar also has a black region that cannot be recovered by resting.<br/>That represents how serious your wound is."));
		npc.Msg(L("<image name='HP'/>Some enemies may injure you with their attacks.<br/>Bats and Mimics especially can cause injuries very easily. You need to be careful when you meet them.<br/>So, what if you ignored the wounds and fought on?"));
		npc.Msg(L("<image name='Die'/>That's what happens. You would even lose some EXP. It hurts a lot."));
		npc.Msg(L("Wounds can be cured by simply using the Resting skill or the First Aid skill.<br/>In other words, make sure to take a rest whenever you get wounded."));
		npc.Msg(L("But resting too much in a dungeon may backfire.<br/>What do you think will happen if you just take a quick break after a battle without even checking your HP bar and face another monster right away?"));
		npc.Msg(L("<image name='Die'/>Again, this would happen."));
		npc.Msg(L("You can't cure your wounds by resting in a dungeon. Probably because of the cold temperature inside.<br/>But a campfire will help you recover. Someone who knows how to start a campfire may be good company for you."));
		npc.Msg(L("Now, like I said, I'm giving you homework.<br/>Gather nails that are dangerously protruding from objects around the town.<br/>You must have seen many people hitting a variety of objects to get small gems."));
		npc.Msg(L("Anyone learning how to fight should first think about how to serve others.<br/>There's no way you can follow them one by one and stop them from doing stupid things,<br/>but you can help them by removing nails from those objects they hit.<br/>At least, they won't get hurt by them. Just hit the objects a few times and get the nails from them."));
		npc.Msg(L("Get me 10 nails and you're done for the day.<br/>Cheating is not allowed. Nails from other people don't count.<br/>Get them on your own.<br/>Now, go!"));

		npc.Player.StartQuest(200040); // Basic Combat 1 Mission
	}

	public async Task Class2_1(NpcScript npc)
	{
		npc.Msg(L("Alright! Basic Combat! Let's get on with the second part of the course!"));
		npc.Msg(L("We covered HP and recovery in the last class, didn't we?<br/>Then I need to start the second class with foxes."));
		npc.Msg(L("<image name='Skill_preparing'/>Check the fox closely. It may sometimes stop and stand still in a flash.<br/>Have you hit a fox at that time?"));
		npc.Msg(L("If you have, you must have been knocked out by it.<br/>I can imagine... What was it like to be hit by a fox again and again?"));
		npc.Msg(L("The fox used the Defense skill.<br/>Before learning about it, just take a good look at what a fox can do.<br/>Study how a fox defends itself by tomorrow.<br/>That's it for today."));
	}

	public async Task Class2_2(NpcScript npc)
	{
		npc.Msg(L("Hmm... Where were we... I'm a little lost here..."));
		npc.Msg(L("Hey, don't give me that look.<br/>Students are supposed to remember that,<br/>not the teacher. How can a teacher remember everything?"));
		npc.Msg(L("Right. I told you to watch a fox defending itself...<br/>So, you did you observe them?"));
		npc.Msg(L("Good. Then you can learn how to use the Defense skill today.<br/>If you don't know how, I can teach you tomorrow. Don't worry about it.<br/>This is the time you should watch, listen, and learn."));
		npc.Msg(L("....<br/>Don't fall asleep! Do you think your laid-back attitude can help you learn the Defense skill?<br/>If you want to become a fearful warrior, you should learn how to respect your master first."));
		npc.Msg(L("Then let's begin the class.<br/>Keep the right posture throughout the class!"));
		npc.Msg(L("<image name='knockdown'/>When is the best time to use the Defense skill?<br/>It's when either you or your enemy is being knocked out and blown away.<br/>That's when you should use it."));
		npc.Msg(L("If you're hit and knocked out, you can defend yourself from the next attack as your enemy waits for you to get up.<br/>If it was your enemy being knocked down, you can defend yourself from the enemy as it arises to hit you back."));
		npc.Msg(L("Here's something important.<br/>If your enemy noticed you're defending yourself,<br/>you should abort the skill immediately.<br/>What if your enemy strikes down on your head with a skill like Smash while you're counting on your Defense skill alone to protect you?"));
		npc.Msg(L("<image name='Die'/>Again, this is what happens. Clean and neat, huh?"));
		npc.Msg(L("I guess that was enough for the day. Don't forget what you learned today."));
	}

	public async Task Class2_3(NpcScript npc)
	{
		npc.Msg(L("Hmm... Is it already our last class?<br/>I'll explain today to you why the Defense skill is so important."));
		npc.Msg(L("You've probably seen a stat called Defense Rate.<br/>It's particularly difficult to upgrade this stat."));
		npc.Msg(L("Clothes can add only 1 or 2 defense points at the best.<br/>What you can do is to train yourself hard enough to improve your critical defense."));
		npc.Msg(L("You may not agree that the Defense Rate is so critical in the beginning, but it will become more and more important for your survival."));
		npc.Msg(L("<image name='whitespider'/>Here's the assignment for the class.<br/>Go to Alby Dungeon and defeat 5 spiders.<br/>Dismissed!"));

		npc.Player.StartQuest(200041); // Basic Combat 2 Mission
	}

	public async Task Class3_1(NpcScript npc)
	{
		npc.Msg(L("Oh... You've got some guts... Good!<br/>No pain, no gain! The more you sweat off today, the more achievements you can make tomorrow!"));
		npc.Msg(L("This class is all about the Smash skill.<br/>Do not be worried if you know nothing about it.<br/>You will learn how to use the skill at the end of this session.<br/>Learning it in advance wouldn't hurt anyone, though."));
		npc.Msg(L("<image name='Smash'/>Smash is a very powerful skill.<br/>A single blow can knock down an enemy with significant damage.<br/>Definitely one of the best skills for a surprise attack.<br/>When the enemy is disoriented, that's when you hit them with the Smash skill!"));
		npc.Msg(L("But there's one thing you must remember.<br/>Normal attacks always beat this skill."));
		npc.Msg(L("OK, I guess that was enough for today."));
	}

	public async Task Class3_2(NpcScript npc)
	{
		npc.Msg(L("Today's class is about the relationship between Defense and Smash."));
		npc.Msg(L("Hey, don't just skim through the text! It was you who signed up for the class, remember?<br/>Who do you think I am explaining all these things to?<br/>You think you are smart enough now? Is that why you're just flipping through the pages?"));
		npc.Msg(L("Do you want to be a warrior or not? It's all about having the right attitude. Now I need you to stand straight and listen carefully!<br/>All right, let's start over. Today's class is about the relationship between Defense and Smash."));
		npc.Msg(L("What would you do if your enemy is using the Defense skill?<br/>Will you just sit and wait?<br/>If your enemy is using it, then you are supposed to get ready to Smash it."));
		npc.Msg(L("Like I said, normal attacks beat the Smash skill, but Smash ignores the Defense skill.<br/>When you use Smash, you first grab the enemy and then strike it with a powerful blow.<br/>The moment you grab it, your enemy's Defense skill will be canceled."));
		npc.Msg(L("The question is, how can I know when the enemy is using Defense?<br/>That's when you need to use your senses.  Just remember this. When an enemy uses Defense, it can't run.<br/>Then it won't be really hard to see if your enemy is using it."));
		npc.Msg(L("All right! Class dismissed."));
	}

	public async Task Class3_3(NpcScript npc)
	{
		npc.Msg(L("Today's class is about party play... Wow, you've come far!<br/>You've been doing very well. Keep it up to the last minute!"));
		npc.Msg(L("<image name='party_2sword'/>Have you heard of a warrior-warrior party?<br/>This type of party can be quite strong,<br/>although it's true any party can be strong as long as they have good teamwork."));
		npc.Msg(L("One of them first strikes the enemy with a normal attack.<br/>If you were in the middle of a triple hit, stop at the second one.<br/>And the other hits the enemy with the Smash skill.<br/>This Smash will succeed 100%."));
		npc.Msg(L("But there's one thing you should keep in mind.<br/>Smash is a powerful skill. But counterattacks and even normal attacks will beat it.<br/>If you think there's little chance of success, you'd better not use the skill at all."));
		npc.Msg(L("That's a wrap..."));
	}

	public async Task Class3_4(NpcScript npc)
	{
		npc.Msg(L("In this class, you'll learn how to work with an archer."));
		npc.Msg(L("<image name='party_sword_bow'/>A bow! A bow can make party play much easier.<br/>When a warrior is allied with only warriors, there's always pressure to make the first normal strike work.<br/>The enemy may hit back while you're trying to attack it with a normal blow."));
		npc.Msg(L("But a bow can make your battle tactics totally different.<br/>Attack an enemy from a distance with a bow, then the enemy will not be able to move or run for a while.<br/>So it becomes just a sitting duck for a 100% successful smash!<br/>Isn't it brilliant? That's what we call a warrior's ecstasy!"));
		npc.Msg(L("Once you succeed in knocking down the enemy,<br/>the archer in your party can keep it from charging the warrior<br/>and you can just repeat the same tactics over and over."));
		npc.Msg(L("A warrior and an archer, what an ideal pair.<br/>Well, that's all for today.."));
	}

	public async Task Class3_5(NpcScript npc)
	{
		npc.Msg(L("OK, today is your last day.<br/>You've done very well so far.<br/>Take a look at the Quest Scroll I gave you."));
		npc.Msg(L("Just do what it says.<br/>Take advantage of everything you've learned<br/>and make it to the end of Alby Dungeon and come back in one piece."));
		npc.Msg(L("You don't seem too enthused...<br/>Either do it or not. It's totally up to you.<br/>If you think you can't do it, then you can give it up."));
		npc.Msg(L("But if you're the same student with guts I saw before, this task should be a piece of cake, right?<br/>I'll wait for you here."));
		npc.Msg(L("<image name='alby_dungeon01'/>Here's our last class.<br/>Since you have the final assignment, I'll tell you how to fight in a dungeon."));
		npc.Msg(L("<br/>The key is to control your HP and Stamina."));
		npc.Msg(L("<image name='HP'/>First is Life.<br/>Just using the Resting skill alone cannot recover your HP in dungeons.<br/>You must rest near a campfire.<br/>I think I told you that before. Do you still remember?"));
		npc.Msg(L("Do not let yourself be injured or surrounded by monsters. You should avoid a situation like that at all costs.<br/>You need to be much more careful in a dungeon than when you fight in the field outside."));
		npc.Msg(L("Potions could be helpful,<br/>but they won't cure your wounds.<br/>In other words, when you're in a dungeon, your safety comes before defeating your opponent."));
		npc.Msg(L("<image name='Stamina'/>Next is Stamina. It's not really different.<br/>The best way to recover your Stamina is by food.<br/>But it's not easy to get something to eat in a dungeon unless you pack it for yourself.<br/>That means you need to save your Stamina as much as possible."));
		npc.Msg(L("Get rid of any unnecessary motions,<br/>and be efficient in every move you make.<br/>Don't forget."));
		npc.Msg(L("And last,<br/>there are some books<br/>about dungeons and exploration.<br/>Read them. They can only help you."));
		npc.Msg(L("All right, this is the end of Basic Combat.<br/>You've been a good student.<br/>Don't stop training. Become a great warrior."));
		npc.Msg(L("When you're done with your homework, make sure you come back and let me know.<br/>I'll be all ears for your whining if you make it through Alby Dungeon, hahaha.<br/>"));

		npc.Player.StartQuest(200028); // Basic Combat 3 Mission
	}
}

public class BasicCombat1MissionQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200040);
		SetName(L("Basic Combat 1 Mission"));
		SetDescription(L("Today's assignment is to remove [Large Nails] from objects so people don't get pricked. Just hit an object a few times and [Large Nails] will drop from them. Get me [10 Large Nails] and then you're done for the day. - Ranald -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		AddObjective("obj1", L("Deliver 10 Nails to Ranald"), 1, 4651, 32166, Talk("ranald"));

		AddReward(Exp(100));
		AddReward(StatBonus(Stat.Str, 1));

		AddHook("_ranald", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id))
		{
			if (npc.Player.HasItem(52003, 10)) // Large Nail
			{
				npc.Player.FinishQuestObjective(this.Id, "obj1");
				npc.Player.RemoveItem(52003, 10);
				npc.Player.Notice(L("You have given Large Nail to Ranald."));
				npc.Msg(L("Good job.<br/>Keep up the good work."));

				return HookResult.Break;
			}
		}

		return HookResult.Continue;
	}
}

public class BasicCombat2MissionQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200041);
		SetName(L("Basic Combat 2 Mission"));
		SetDescription(L("Today's assignment is to hunt monsters. Remember the importance of defense, then go to Alby Dungeon and knock down [5 White Spiders]. - Ranald -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		AddObjective("obj1", L("Hunt 5 White Spiders"), 0, 0, 0, Kill(5, "/whitespider/"));
		AddObjective("obj2", L("Talk to Ranald"), 1, 4651, 32166, Talk("ranald"));

		AddReward(Exp(150));
		AddReward(StatBonus(Stat.Str, 2));
		AddReward(Skill(SkillId.Defense, SkillRank.Novice, 1));

		AddHook("_ranald", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "obj2"))
		{
			npc.Player.FinishQuestObjective(this.Id, "obj2");
			npc.Msg(L("Good job.<br/>Keep up the good work."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}

public class BasicCombat3MissionQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200028);
		SetName(L("Basic Combat 3 Mission"));
		SetDescription(L("Today's assignment is to hunt dungeon monsters. Drop an item on Alby Dungeon alter and defeat the boss. You can go alone or with friends. - Ranald -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		AddObjective("obj1", L("Clear Alby Dungeon"), 13, 3190, 3200, ClearDungeon("tircho_alby_dungeon"));
		AddObjective("obj2", L("Talk to Ranald"), 1, 4651, 32166, Talk("ranald"));

		AddReward(Exp(100));
		AddReward(StatBonus(Stat.Str, 2));
		AddReward(StatBonus(Stat.Will, 2));
		AddReward(Skill(SkillId.Smash, SkillRank.Novice, 1));

		AddHook("_ranald", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "obj2"))
		{
			npc.Player.FinishQuestObjective(this.Id, "obj2");
			npc.Msg(L("Ah, did you complete the Alby Dungeon assignment?<br/>Good job!<br/>Keep on training.<br/>Okay, this is enough of the basic combat class for now."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
