//--- Aura Script -----------------------------------------------------------
// Basic Sorcery Classes
//--- Description -----------------------------------------------------------
// Class taught by Lassar, to teach players about magic and magic skills.
// This script handles all classes and their quests.
//---------------------------------------------------------------------------

public class SchoolMagicLassarQuestScript : GeneralScript
{
	public override void Load()
	{
		AddHook("_lassar", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "about_study")
			return HookResult.Continue;

		var stateName = "LassarClassState";
		var lastName = "LassarClassLast";
		var state = (int)npc.Player.Vars.Perm.Get(stateName, 0);
		var last = (string)npc.Player.Vars.Perm.Get(lastName, "never");
		var now = ErinnTime.Now;
		var today = now.ToString("yyyy-MM-dd");
		var start = 7;
		var end = 23;
		var remaintime = end - now.Hour;
		var lastState = 8;

		if (npc.QuestActiveUncompleted(200029))
		{
			npc.Msg(L("Haven't you finished the assignment I gave you?<br/>Well... Don't tell me you haven't received the assignment yet?<br/>It's been a long time since I sent it by an owl!"));
			npc.Msg(L("Well, this assignment has no deadline,<br/>so you can take your time."));
		}
		else if (state > lastState)
		{
			npc.Msg(L("<username/>, you have already completed the Basic Sorcery.<br/>From now on, you should find ways to study on your own."));
		}
		else if (last == today)
		{
			npc.Msg(L("Hmm... Now is not the right time to talk about it.<br/>Today's magic classes are all finished.<br/>Come back later. Shall we say... around when the shadow points north, northwest?"));
		}
		else if (now.Hour < start || now.Hour >= end)
		{
			npc.Msg(L("Class has started long ago.<br/>You should come back later."));
		}
		else
		{
			var exp = 0;
			var cost = 0;
			var msg = "";
			var name = "";
			var title = L("The Basics of Magic and Understanding of Mana");
			var paydesc = "";
			var reward = "";
			var desc = "";
			var func = (Func<NpcScript, Task>)null;

			switch (state)
			{
				case 0:
					exp = 30;
					cost = 4000;
					msg = L("Are you going to take a magic class?<br/>Then, how about this course?<br/>Tuition is a lump sum that includes three days of lessons including today.<br/>This tuition covers up to the end of Basic Sorcery Chapter One.");
					name = L("Basic Sorcery 1-1");
					paydesc = L("Total tuition for three days is 4000G.");
					reward = L("* Icebolt Spell<br/>* EXP Reward");
					desc = L("The first thing you learn about and use in the three classes in Basic Sorcery Chapter One is Mana, which is the most fundamental element in magic.");
					func = Class1_1;
					break;

				case 1:
					exp = 30;
					msg = L("Hee hee.<br/>You are here for a class. <username/>?<br/>How was the class yesterday?<br/>We'll start as soon as we're ready.");
					name = L("Basic Sorcery 1-2");
					paydesc = L("Already paid.");
					reward = L("* Icebolt Spell<br/>* EXP Reward");
					desc = L("The second of the three classes in Basic Sorcery Chapter One, you learn how to distinguish magic by its domain and learn about the Elementals.");
					func = Class1_2;
					break;

				case 2:
					exp = 100;
					msg = L("You're already here for the last class of the first chapter, <username/>.<br/>The class is not only for gaining spells.<br/>One of the advantages of taking classes is gaining knowledge and experiences to use magic correctly and effectively<br/>without additional training. Do you want to take the class?");
					name = L("Basic Sorcery 1-3");
					title = L("Icebolt Magic Practice");
					paydesc = L("Already paid.");
					reward = L("* Icebolt Spell<br/>* EXP Reward");
					desc = L("The last thing you learn in the three classes of Basic Sorcery Chapter One is how to cast Icebolt magic.");
					func = Class1_3;
					break;

				case 3:
					exp = 50;
					cost = 4000;
					msg = L("Oh. You took the first chapter of Basic Sorcery before.<br/>Was the class helpful?<br/>I hope you do well in this chapter too.<br/>To learn this chapter, you have to pay the tuition now. Hee hee.");
					name = L("Basic Sorcery 2-1");
					title = L("How to Cast Icebolt Consecutively");
					paydesc = L("Total tuition for three days is 4000G.");
					reward = L("* Firebolt Spell<br/>* EXP Reward");
					desc = L("In the first lesson of the three classes in Basic Sorcery Chapter Two, designed to improve the understanding of Ice elemental magic, you learn how to cast Icebolt magic consecutively and what to watch out for.");
					func = Class2_1;
					break;

				case 4:
					exp = 50;
					msg = L("Hee hee. You came here to take the class, right?");
					name = L("Basic Sorcery 2-2");
					title = L("Strategic Use of Icebolt Magic");
					paydesc = L("Already Paid.");
					reward = L("* Firebolt Spell<br/>* EXP Reward");
					desc = L("In the second lesson of the three classes in Basic Sorcery chapter two, you learn how to employ Icebolt spells effectively in real combat situations.");
					func = Class2_2;
					break;

				case 5:
					exp = 50;
					msg = L("It is already the last class of chapter two.<br/>Please remain focused until the end.");
					name = L("Basic Sorcery 2-3");
					title = L("StrategicApplications to Other Elemental Magic");
					paydesc = L("Already Paid.");
					reward = L("* Firebolt Spell<br/>* EXP Reward");
					desc = L("In the last lesson of the three classes in Basic Sorcery Chapter Two, you learn Elemental magic of other domains by utilizing the already-learned Icebolt spell.");
					func = Class2_3;
					break;

				case 6:
					exp = 100;
					cost = 7000;
					msg = L("Hee hee. You've come to learn Basic Sorcery Chapter Three.<br/>I knew, <username/>, that<br/>you would do this.<br/>So good to see you. I was right about you, it seems.");
					name = L("Basic Sorcery 3-1");
					title = L("How to Cast Firebolt Consecutively");
					paydesc = L("Total tuition for three days is 7000G.");
					reward = L("* Lightningbolt Spell<br/>* EXP Reward, Intelligence Increase");
					desc = L("They say that this chapter is fairly tough since it includes an assignment. In the first lesson of the three classes in Basic Sorcery Chapter Three, designed to improve the understanding of Fire elemental magic, you learn how to cast Firebolt consecutively and what to watch out for.");
					func = Class3_1;
					break;

				case 7:
					exp = 100;
					msg = L("I trust that your training is going well?<br/>I hope so. Haha.<br/>Let's start today's class, then.");
					name = L("Basic Sorcery 3-2");
					title = L("Strategic Use of Firebolt Magic");
					paydesc = L("Already Paid.");
					reward = L("* Lightningbolt Spell<br/>* EXP Reward, Intelligence Increase");
					desc = L("In the second lesson of the three classes in Basic Sorcery Chapter Three, you learn how to employ Firebolt spells effectively in real combat situations.");
					func = Class3_2;
					break;

				case 8:
					exp = 100;
					msg = L("Are you ready for the class?");
					name = L("Basic Sorcery 3-3");
					title = L("Using Icebolt and Firebolt in Combat");
					paydesc = L("Already Paid.");
					reward = L("* Lightningbolt Spell<br/>* EXP Reward, Intelligence Increase");
					desc = L("In the last lesson of the three classes in Basic Sorcery Chapter Three, you learn how to overcome the weaknesses of Firebolt and train in the casting of additional magic by doing assignments.");
					func = Class3_3;
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
						npc.Msg(L("Hmm.<br/>The Magic School tuition is expensive.<br/>If you want to take the class, you should save up for tuition."));
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
				npc.Msg(L("No one can force you to take a class you don't want to."));
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
		npc.Msg(L("Now, let's begin."));
		npc.Msg(L("As I said, the tuition you paid today<br/>would cover this whole chapter.<br/>Let's see...<br/>You could probably complete this chapter in three days including today."));
		npc.Msg(L("Since today is the first day, I will briefly discuss<br/>what we'll be learning, instead of proceeding with the lesson.<br/>It is about magic and Mana,<br/>the fundamental power of magic."));

		npc.Msg(L("Why do you want to learn magic?<br/><button title='Because it looks cool.' keyword='@reply1' /><button title='To help others' keyword='@reply2' /><button title='Because Lassar is pretty.' keyword='@reply3' /><button title='Because I have money to spend.' keyword='@reply4' />"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Haha. You are so funny.<br/>But magic is not for showing off."));
				npc.Msg(L("Although, it does look cool, too."));
				break;

			case "@reply2":
				npc.Msg(L("That's great.<br/>You are a saint."));
				npc.Msg(L("...<br/>But it's not good to lie."));
				break;

			case "@reply3":
				npc.Msg(L("Hee hee.<br/>You know beauty when you see it, don't you?<br/>Yes, I am gorgeous, if I may say so myself."));
				npc.Msg(L("But, you should concentrate on the lesson,<br/>not only on the teacher."));
				break;

			case "@reply4":
				npc.Msg(L("You do? Well, aren't you lucky? You and your extra money.Hmph."));
				break;
		}

		npc.Msg(L("Very well. Even though you may not be able to spell out everything that you feel,<br/>you probably have something in mind.<br/>Please keep that attitude<br/>as you study magic."));
		npc.Msg(L("<image name='icebolt_preparing'/>Let me explain the definition of magic first.<br/>Magic is all the technical activities<br/>which change the surrounding environment<br/>by using supplementary means based on Mana."));
		npc.Msg(L("Spelling it out this way<br/>may only be confusing to you. Hehehehe.."));
		npc.Msg(L("Simply put, it is understanding the order before a phenomenon<br/>and to change the phenomenon created by this order.<br/>As with everything else in the world...<br/>The only difference with magic is that it requires Mana and other incidental factors."));
		npc.Msg(L("Does that help?<br/>Now, then, let me explain Mana,<br/>arguably the most important factor in magic. Please pay attention."));
		npc.Msg(L("<image name='hp_mana_stamina'/>First, look to the left bottom side of the menu bar on your interface.<br/>See the three bar graphs?<br/>As you know, red means HP,<br/>and yellow at the bottom means Stamina."));
		npc.Msg(L("<image name='gauge_mana'/>And... the second blue bar<br/>is the Mana gauge.<br/>It shows the amount of Mana you have."));
		npc.Msg(L("Mana is the energy of the source of existence which human beings possess.<br/>It is also the power through which people can communicate with all things in the world.<br/>Without Mana, it is impossible to cast magic."));
		npc.Msg(L("This is different from Erg,<br/>the source of energy<br/>which comes from Palala, the shining sun."));
		npc.Msg(L("Erg is also a form of energy<br/>but, it is just one form of Mana,<br/>not Mana itself."));
		npc.Msg(L("While Mana can be transformed into Erg,<br/>Erg cannot be transformed into Mana.<br/>That is the irreversible rule of Mana."));
		npc.Msg(L("You seem lost... Let's move on."));
		npc.Msg(L("Now, is Mana for only a select few?<br/>Not necessarily.<br/>Everyone possesses Mana<br/>but the amount and efficiency varies from person to person."));
		npc.Msg(L("Some have a large amount of Mana,<br/>but spend their whole life without using it,<br/>while others have little Mana but<br/>use it effectively<br/>and cast great magic."));
		npc.Msg(L("Of course, you can increase<br/>your capacity for Mana and<br/>familiarize yourself with using magic<br/>by training yourself."));
		npc.Msg(L("That is, while it is important to increase the amount of Mana,<br/>keep in mind that it is even more important<br/>to use it efficiently and effectively."));
		npc.Msg(L("And, there is one more thing you should remember.<br/>As I said before,<br/>Mana is not limitless."));
		npc.Msg(L("<image name='gauge_mana_empty'/>If you use Mana until the Mana gauge reaches zero,<br/>you will not be able to use magic<br/>until you recover Mana."));
		npc.Msg(L("Then, how can you recover Mana?<br/>It is very simple.<br/>Just wait until nightfall."));
		npc.Msg(L("<image name='moon_eweca'/>It is because of Eweca, the moon of Erinn.<br/>Smart students would have understood that right away...<br/>Mana can only be recovered at night when Eweca rises."));
		npc.Msg(L("Hmm... I've talked too long.<br/>Let's call it a day.<br/>Come again tomorrow<br/>and we will proceed with the rest of the lesson."));
		npc.Msg(L("And... Let's see...<br/>Hmm. You came without any preparation.<br/>Today is fine<br/>but, from now on, you should bring a notebook."));
		npc.Msg(L("Swordsmen learn through their bodies<br/>but that is not the case for wizards."));
		npc.Msg(L("A wizard should get in the habit of recording.<br/>You should always be prepared to write down<br/>the changes you feel and the achievements you make<br/>if you are to improve your magic beyond a certain level."));
		npc.Msg(L("If you do not review what you've learned in the class and forget,<br/>it would be very tough<br/>to improve your magic beyond average.<br/>So, you should be prepared to jot down notes even if you don't want to."));
		npc.Msg(L("As you might have surmised from your enrollment already,<br/>it takes a lot of patience to learn magic.<br/>Consistent attendance is key."));
		npc.Msg(L("Then, we'll finish here."));
	}

	public async Task Class1_2(NpcScript npc)
	{
		npc.Msg(L("Let's see...<br/>First, let's see how much you remember<br/>from what I taught you yesterday, shall we?"));

		npc.Msg(L("The magic which transforms the environment by responding to the rule of the universe -<br/>the most important thing in this magic is in handling what?<button title='Mana' keyword='@reply1' /><button title='Male' keyword='@reply2' /><button title='Female' keyword='@reply3' />"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Yes. You paid attention during class."));
				break;

			case "@reply2":
				npc.Msg(L("...<br/>Correct!<br/>We have something in common!"));
				break;

			case "@reply3":
				npc.Msg(L("NOT!<br/>Please pay attention during class."));
				break;
		}

		npc.Msg(L("But you know anyone could have gotten that, right?<br/>*Laugh*"));

		npc.Msg(L("Next question...<br/>Of the three graphs at the bottom of the interface on the right,<br/>which color represents Mana?<button title='Red' keyword='@reply1' /><button title='Blue' keyword='@reply2' /><button title='Yellow' keyword='@reply3' />"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Haha. That's my favorite color.<br/>But red doesn't represent mana."));
				npc.Msg(L("Red is for HP.<br/>You must have been way too nervous.<br/>Blue is the color for Mana."));
				break;

			case "@reply2":
				npc.Msg(L("Correct!<br/>But don't be too proud of yourself.<br/>Even Endelyon's dog knows that."));
				break;

			case "@reply3":
				npc.Msg(L("Umm... wrong.<br/>Yellow represents Stamina.<br/>You should know this..."));
				npc.Msg(L("Are you color blind, by any chance? Yellow-blue color blind?<br/>Then my apologies. I picked the wrong question."));
				npc.Msg(L("Mana graph is the middle one.<br/>If you learn magic with this in mind, you shouldn't have trouble learning it."));
				npc.Msg(L("But if you are not color blind and picked this wrong answer,<br/>that is simply disappointing."));
				break;
		}

		npc.Msg(L("One more quick question.<br/>What would be helpful in recovering Mana?<button title='Drink MP Potion.' keyword='@reply1' /><button title='Wait until night.' keyword='@reply2' /><button title='Ask Manari' keyword='@reply3' />"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Hmm. I suppose I didn't mention this...<br/>You can recover Mana to a certain point<br/>by drinking MP Potion."));
				npc.Msg(L("But you should be careful.<br/>Too much reliance on potions<br/>would eventually<br/>destroy your capability to control your body and Mana."));
				break;

			case "@reply2":
				npc.Msg(L("Yes, correct."));
				break;

			case "@reply3":
				npc.Msg(L("Who is Manari?<br/>Now, that was a random answer..."));
				break;
		}

		npc.Msg(L("As I explained before,<br/>Mana can be recovered by the power of Eweca.<br/>That's why you should wait<br/>until Eweca rises."));

		npc.Msg(L("Well, I guess this is enough review.<br/>Let's start today's class."));
		npc.Msg(L("Today, I am going to talk about Elementals."));
		npc.Msg(L("As the main components comprising the world,<br/>I mentioned Mana and Erg yesterday.<br/>Mana and Erg are very important components,<br/>but the order of the universe cannot be established just with these two."));
		npc.Msg(L("No less important than those two are<br/>the elements comprising the world.<br/>The elements, created in accordance with the rule of the physical world,<br/>actually play an important role in enriching the universe."));
		npc.Msg(L("In theory, the power of Mana affects all the elements.<br/>This is also the will of<br/>Aton Cimeni, the ultimate God who created the world."));
		npc.Msg(L("There are various kinds of elements<br/>and they are so vast that the relevant theories alone<br/>would fill dozens of books."));
		npc.Msg(L("The Four Element Theory, the Five Element Theory, the Multi-layer Element Theory, Element Fusion Theory...<br/>Many scholars maintained numerous theories<br/>and many of them have been proven to be true."));
		npc.Msg(L("...But this class is not about element theories<br/>but about magic.<br/>Wizards should understand elements<br/>as a part of magic."));
		npc.Msg(L("Now, how do we do this? How can we<br/>understand elements as a part of magic?<br/>What is central here is the magic affinity of elements."));
		npc.Msg(L("Among various elements comprising the world,<br/>three elements in particular have the strongest magic affinity.<br/>They are Lightning, Fire and Ice."));
		npc.Msg(L("In Sorcery, we call these Lightning, Fire<br/>and Ice Elementals."));
		npc.Msg(L("Since these three elements have especially strong magic affinity,<br/>keep in mind that we are only dealing with these three<br/>when we talk about elements in magic."));
		npc.Msg(L("One interesting thing is<br/>that magic based on these elementals<br/>have unique relations with each other."));
		npc.Msg(L("First of all, Fire elemental magic<br/>is incompatible with Ice elemental magic.<br/>Fire elemental magic<br/>is irrelevant with Lightning elemental magic."));
		npc.Msg(L("And while Lightning elemental magic<br/>and Ice elemental magic<br/>have complementary relations,"));
		npc.Msg(L("they conflict with each other sometimes<br/>and have a synergy at other times."));
		npc.Msg(L("Umm... you look confused. It is understandable."));
		npc.Msg(L("When it comes to Lightning and Ice magic,<br/>rather than explain them in great detail,<br/>you can simply pick them up as you<br/>actually learn them, so don't worry too much."));
		npc.Msg(L("For now, it is enough<br/>to be aware of the relationship of the three."));
		npc.Msg(L("Now, what kind of forms would these elementals appear?"));
		npc.Msg(L("You can picture thunder or a ball of lightning<br/>for Lightning,<br/>a ball of fire or flame for Fire<br/>and snow or a mass of ice for Ice."));
		npc.Msg(L("<image name='3elemental_stone'/>But this is not always the case.<br/>When you use powerful Mana and treat it with advanced magic,<br/>you can turn elementals into crystals."));
		npc.Msg(L("If you have further interest in elementals,<br/>it would be helpful to read 'Understanding of Elementals' written by Leslie.<br/>It might not be so easy to find a copy, though..."));
		npc.Msg(L("But, if you happen to get one, you should definitely read it.<br/>It will be of great help."));
		npc.Msg(L("Because I'm talking a lot about elementals,<br/>you might get the impression that all magic works<br/>within the large frame of elementals.<br/>But that is actually not true."));
		npc.Msg(L("Many kinds of magic<br/>have nothing to do with elementals."));
		npc.Msg(L("The kind of magic that does not belong to the elemental domains<br/>is called Non-elemental Magic.<br/>You look confused without any examples.<br/>Hmm. Healing is a classic example."));
		npc.Msg(L("Healing magic is<br/>a classic Non-elemental magic."));
		npc.Msg(L("If you want to learn Healing magic,<br/>'Healing: Basics of Magic' written by Flann<br/>would be helpful.<br/>If you are interested, it would be nice to study it on the side."));
		npc.Msg(L("Now, let me summarize for you."));
		npc.Msg(L("First, magic is divided into<br/>Elemental and Non-elemental magic."));
		npc.Msg(L("Second, Elemental magic is further divided<br/>into Lightning, Fire and Ice.<br/>Each elemental<br/>has unique characteristics and relations."));
		npc.Msg(L("Well, then, shall we learn<br/>how to cast Elemental magic?"));
		npc.Msg(L("Wait... Is it time to finish already?<br/>We will continue next time."));

		npc.Msg(L("So, how are you doing? Are you following OK?<br/><button title='Yes.' keyword='@reply1'/><button title='No. A waste of tuition.' keyword='@reply2'/>"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Heeheehee... But you seem disgruntled.<br/>You should show some respect to your teacher.<br/>We are having a class after all..."));
				break;

			case "@reply2":
				npc.Msg(L("Sorry to hear that. But there is no refund.<br/>If you think it's unfair,<br/>I would recommend giving up on the magic class."));
				npc.Msg(L("But you've already started - why don't you just give it a chance?"));
				break;
		}

		npc.Msg(L("Hmm. Perhaps you lost interest because of the heavy focus on the theory so far.<br/>Please, be patient. You cannot learn magic overnight.<br/>Basic Sorcery course will be finished by the next class.<br/>Actually, we are preparing for practice."));
		npc.Msg(L("And if you are hoping to actually use magic,<br/>the next class will be very exciting for you.<br/>Please show up to the end."));
		npc.Msg(L("Bye."));
	}

	public async Task Class1_3(NpcScript npc)
	{
		if (!npc.Player.Skills.Has(SkillId.Icebolt, SkillRank.Novice))
		{
			npc.Player.Skills.Give(SkillId.Icebolt, SkillRank.Novice);
			npc.Player.Skills.Train(SkillId.Icebolt, SkillRank.Novice, 1);
		}

		npc.Msg(L("Good.<br/>First of all, let's check the new magic."));
		npc.Msg(L("<image local='true' name='magic_ice'/>Check the Skill window and press the 'Learn' button."));
		npc.Msg(L("As you know, the spell I've just taught you is Icebolt."));
		npc.Msg(L("Heehee... You want to use it already?<br/>What's the hurry?"));
		npc.Msg(L("You're making it difficult for me. Hehehe..."));
		npc.Msg(L("<image name='icebolt_fired'/>Now, first, you should know<br/>what kind of spell Icebolt is.<br/>Icebolt is an attack magic<br/>which shoots sharp ice fragments to the target at a rapid speed."));
		npc.Msg(L("In other words, it is very dangerous.<br/>So, you shouldn't use this magic on just anyone."));
		npc.Msg(L("You should know the principles, too.<br/>The principles behind Icebolt is very simple."));
		npc.Msg(L("First, collect the surrounding moisture with the power of Mana.<br/>Second, lower the temperature with the power of Mana.<br/>Third, create sharp ice fragments by controlling Mana.<br/>Fourth, shoot them to the target by using the force of repulsion."));
		npc.Msg(L("Easy, huh?"));
		npc.Msg(L("Of these four steps,<br/>how sharp the ice pieces are<br/>and how much repulsion it takes<br/>to shoot them to the target -"));
		npc.Msg(L("Balancing each of these two factors<br/>is the key to Icebolt magic."));
		npc.Msg(L("Ha ha. Simple, isn't it?<br/>It should look easy."));
		npc.Msg(L("But understanding with your head is one thing<br/>and connecting your body and will,<br/>and eventually being assimilated to the order of the universe,<br/>is quite another."));
		npc.Msg(L("Keep this in mind when you practice later."));
		npc.Msg(L("That is all I can explain to you.<br/>If you want<br/>more detail about<br/>how Icebolt magic was created,"));
		npc.Msg(L("'Icebolt Spell: Origin and Training'<br/>would be helpful."));
		npc.Msg(L("I hope that, with this magic,<br/>you would make a lot of people happy."));
		npc.Msg(L("And, if you have any questions<br/>or want more details about magic,<br/>please enroll in Basic Sorcery Chapter Two."));
		npc.Msg(L("Please keep practicing<br/>the magic I taught you today."));
		npc.Msg(L("Bye."));
		npc.Msg(L("Oh!<br/>Come to think of it, I didn't review what we learned yesterday."));

		npc.Msg(L("It's kind of late, but they have to be reviewed.<br/>Let me just ask some quick questions for review.<br/>Do you remember<br/>what you learned in the previous class, not what you've just learned?<button title='Elemental' keyword='@reply1' /><button title='Icebolt' keyword='@reply2' /><button title='Firebolt' keyword='@reply3' />"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Alright. You remember.<br/>But learning magic takes more than just memory.<br/>Don't let it go to your head."));
				break;

			case "@reply2":
				npc.Msg(L("Haha. You must have misunderstood me.<br/>That is what you've just learned."));
				npc.Msg(L("All you can think about is Icebolt right now, isn't it?"));
				break;

			case "@reply3":
				npc.Msg(L("Wow! You are a fast learner!<br/>Great!"));
				npc.Msg(L("...But don't tell me you've already learned firebolt."));
				break;
		}

		npc.Msg(L("Now, which one of the following does not belong to the Elementals?<button title='Lightning' keyword='@reply1' /><button title='Fire' keyword='@reply2' /><button title='Ice' keyword='@reply3' /><button title='Poison' keyword='@reply4' />"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Lightning belongs to the Elementals.<br/>If you picture a ball of lightning, it should be much easier to understand.<br/>Don't forget next time."));
				npc.Msg(L("Lightning, Fire, and Ice belong to the elementals but Poison does not."));
				break;

			case "@reply2":
				npc.Msg(L("Lightning belongs to the Elementals.<br/>If you picture a ball of lightning, it should be much easier to understand.<br/>Don't forget next time."));
				npc.Msg(L("Lightning, Fire, and Ice belong to the elementals but Poison does not."));
				break;

			case "@reply3":
				npc.Msg(L("Ice belongs to the elementals.<br/>If you picture ice, it should be much easier to understand.<br/>Don't forget next time."));
				npc.Msg(L("Lightning, Fire, and Ice belong to the elementals but Poison does not."));
				break;

			case "@reply4":
				npc.Msg(L("Correct. Poison means toxic substances.<br/>Poison simply means something<br/>created by a combination of different elements, and their state.<br/>It's far from the Elementals though."));
				npc.Msg(L("You seem to have studied a fair bit. Or was it a lucky guess? Haha."));
				break;
		}

		npc.Msg(L("Okay, today's class is over.<br/>Congratulations on your work so far."));
		npc.Msg(L("Do just as you learned today,<br/>and you, as a beginning wizard,<br/>will be able to adequately understand and use Icebolt<br/>wherever you may go."));
		npc.Msg(L("Haha. Congratulations on completing Chapter 1 of Basic Sorcery."));
	}

	public async Task Class2_1(NpcScript npc)
	{
		npc.Msg(L("How did you like getting trained in magic?<br/>Are you able to freely use Icebolt<br/>that I've taught you?"));
		npc.Msg(L("Let's see... Someone at your level, <username/>,<br/>should learn this first.<br/>How to cast Icebolt magic consecutively."));
		npc.Msg(L("Huh? You want to learn other magic first?<br/>Hmm. You may if you really want to."));
		npc.Msg(L("But, for now, it would probably be better for you to do as I say...<br/>You should learn to walk first before running, right?"));
		npc.Msg(L("When you learn something new,<br/>you should study and practice it diligently<br/>and eventually master it."));
		npc.Msg(L("If you learn several things at the same time without fully embracing each of them,<br/>you are bound to lose interest in everything."));
		npc.Msg(L("At the end of the day, you will end up not being able to learn anything more<br/>without excelling at anything."));
		npc.Msg(L("<image local='true' name='AP_points'/>Yes. In other words, you would lack AP, your personal capability.<br/>So, you will find it difficult to raise your rank and learn something new."));
		npc.Msg(L("You already know the Icebolt spell<br/>but, you need more training, <username/>."));
		npc.Msg(L("For now, trust your teacher<br/>and study and practice diligently what I teach you.<br/>It's never too late to learn something new after that."));
		npc.Msg(L("<image name='icebolt_loaded'/>Now, let's begin the lesson.<br/>We'll learn how to cast the Icebolt spell consecutively.<br/>Even if you have practiced it already, please listen carefully."));
		npc.Msg(L("You probably heard about it in the previous class.<br/>Charging the same spell multiple times<br/>to increase its power and then casting it<br/>is called consecutive casting."));
		npc.Msg(L("When you cast Icebolt consecutively,<br/>you can create several balls of ice,<br/>allowing you to make consecutive attacks.<br/>Please note that there are five balls of ice in the picture."));
		npc.Msg(L("Huh? You knew it all?<br/>Ha ha. You are always in a hurry.<br/>Then, let me ask you this."));

		npc.Msg(L("When you shoot balls of ice as a result of consecutive-casting,<br/>which ball of ice will cause the biggest damage?<button title='The first shot.' keyword='@reply1'/><button title='All are the same.' keyword='@reply2'/><button title='The last shot.' keyword='@reply3'/>"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Wrong! Wrong answer!<br/>All of the Icebolts in consecutive-casting cause the same amount of damage.<br/>If you had practiced Icebolt more, you might have gotten it right."));
				npc.Msg(L("It seems to me that, instead of being trained in magic step by step,<br/>you are in a rush to use various spells.<br/>You'd better focus more on the training."));
				break;

			case "@reply2":
				npc.Msg(L("Yes. Correct! They are all the same.<br/>You trained yourself a fair bit, didn't you? haha."));
				npc.Msg(L("I want to give you something as a reward but I have nothing suitable right now. Hehehehe."));
				break;

			case "@reply3":
				npc.Msg(L("Wrong.<br/>All of the icebolts in consecutive-casting cause the same amount of damage.<br/>If you had practiced icebolt more, you might have gotten it right."));
				npc.Msg(L("It seems to me that, instead of being trained in magic step by step,<br/>you are in a rush to use various spells.<br/>You'd better focus more on the training."));
				break;
		}

		npc.Msg(L("Then, let's continue the lesson.<br/>Consecutive-casting helps in magic training<br/>in that it is a way to directly control the amount of Mana.<br/>The same goes for other magic."));
		npc.Msg(L("But, practicing magic through consecutive-casting<br/>takes that much more Mana.<br/>So, it's not easy."));
		npc.Msg(L("That means<br/>the less Mana the spell consumes in consecutive-casting,<br/>the easier it will be to practice it."));
		npc.Msg(L("Hehehe... Someone who studied hard<br/>like you, <username/>,<br/>may have noticed it right away."));
		npc.Msg(L("The Elementals in the domain of ice<br/>spend less Mana<br/>as you use consecutive-casting more."));
		npc.Msg(L("That's why I taught you<br/>Icebolt first.<br/>It lessens the burden for beginning wizards<br/>who find it difficult to control the amount of Mana to use."));
		npc.Msg(L("How was that? Did you know that too?"));
		npc.Msg(L("Why don't I give you this, too?<br/>The more the consecutive-casting is practiced,<br/>the less additional Mana is required.<br/>That is the so-called the 'Law of Marginal Mana Decrease'."));
		npc.Msg(L("If spells like Icebolt which can be cast by using Mana alone<br/>are ruled by the law of marginal Mana decrease,<br/>they would be a perfect fit to practice controlling Mana."));
		npc.Msg(L("Now, do you see why I<br/>recommended Icebolt first?<br/>It is easy to learn, quick to be cast and convenient to do consecutive-casting.<br/>There is no better choice in starting to learn magic than this."));
		npc.Msg(L("Of course, the amount of damage by each iceball created by consecutive-casting<br/>does not increase as consecutive-casting is practiced.<br/>But, right now, learning how to use Mana more effectively<br/>is much more important than learning about the damage."));
		npc.Msg(L("Now, let me summarize for you what I taught you today."));
		npc.Msg(L("Some spells can be cast consecutively<br/>but, in the case of Ice Elemental spells,<br/>the spell is affected by the Law of Marginal Mana Decrease."));
		npc.Msg(L("When you cast Icebolt consecutively,<br/>the damage caused by each ice piece<br/>doesn't increase,<br/>but you can shoot sharp ice pieces consecutively."));
		npc.Msg(L("Let's call it a day here.<br/>Don't be late tomorrow."));
		npc.Msg(L("And one more thing - don't rush into it.<br/>I'd like to see you learn step-by-step. Ha ha."));
	}

	public async Task Class2_2(NpcScript npc)
	{
		npc.Msg(L("Yes. I have been waiting for you,<username/>.<br/>How is your magic training going?<br/>I enjoy teaching you as well, <username/>.<br/>I hope you do a good job in this chapter, too."));
		npc.Msg(L("Let's see...<br/>I taught you how to cast Icebolt consecutively yesterday, right?"));
		npc.Msg(L("There is one thing I forgot to mention in the previous class.<br/>It is about the limitation on consecutive-casting."));
		npc.Msg(L("Have you ever thought that,<br/>if you could continue consecutive-casting,<br/>eventually a spell with unlimited destructive power could be produced? Hahaha."));
		npc.Msg(L("This scenario doesn't seem impossible in theory,<br/>but no matter how great the wizard is,<br/>five times is actually the limit to consecutive-casting."));
		npc.Msg(L("Take note of this. The limit is five times.<br/>Even if you cast the spell more than five times in a row,<br/>it will not be applied to the effects of consecutive-casting.<br/>It will only waste precious Mana so be careful."));
		npc.Msg(L("Now, let's start the class."));
		npc.Msg(L("<image name='icebolt_preparing'/>Today's topic is<br/>how you can use<br/>Icebolt magic efficiently."));
		npc.Msg(L("You might remember from the last class session that,<br/>when you cast Icebolt magic consecutively,<br/>the damage caused by each iceball does not increase.<br/>Instead,"));
		npc.Msg(L("the number of iceballs<br/>you can shoot consecutively<br/>increases."));
		npc.Msg(L("Then, what can we do to use these iceballs more effectively?<br/>Let's think about that for a minute."));
		npc.Msg(L("Once you face physical or magical attacks,<br/>sometimes you cannot move at all due to the shock.<br/>Have you ever experienced it before?"));
		npc.Msg(L("If you have,<br/>it would be much easier for you to grasp<br/>the clues as to how to use the Icebolt spell effectively."));
		npc.Msg(L("<image name='icebolt_fired01'/>Since Icebolt is a magical long range attack,<br/>you can hit the target from a distance.<br/>That's obvious, though..."));
		npc.Msg(L("<image name='icebolt_fired02'/>What's more important for Icebolt is<br/>that you can hit the target consecutively<br/>at a rapid pace."));
		npc.Msg(L("That means,<br/>When you load five iceballs for consecutive casting<br/>and shoot at the target in a timely manner,<br/>the target will be hopelessly defeated."));
		npc.Msg(L("It is also possible to launch an unexpected attack on the enemy<br/>who is fighting, thus immobilizing him.<br/>Even Windmill<br/>cannot reverse this attack."));
		npc.Msg(L("<image name='icebolt_fired03'/>If you are a member of a party,<br/>you could support other fighters in a close quarter<br/>by using this skill, right?"));
		npc.Msg(L("This kind of swiftness<br/>is a unique advantage of Icebolt<br/>which other weapons cannot imitate."));
		npc.Msg(L("But, there is something you must be careful about<br/>when using Icebolt."));
		npc.Msg(L("<image name='icebolt_loaded'/>When you cast Icebolt and create an iceball,<br/>the iceball ready to be fired<br/>orbits around you in a circle."));
		npc.Msg(L("<image name='icebolt_knockdown'/>If you get attacked at this moment and fall down,<br/>the iceballs you've created thus far by using Icebolt<br/>will disappear altogether."));
		npc.Msg(L("To make the iceballs continue to float<br/>while maintaining the temperature requires a high level of concentration.<br/>But, when your concentration is disrupted, those iceballs would be gone."));
		npc.Msg(L("Therefore, you must be careful of external attacks<br/>when casting Icebolt consecutively."));
		npc.Msg(L("In other words, a moment of carelessness<br/>could result in losing hard-earned Mana.<br/>You must keep this in mind."));
		npc.Msg(L("Now, let's review what we learned today."));
		npc.Msg(L("<image name='icebolt_loaded'/>First, the limit on consecutive magic casting is five times.<br/>If you cast the spell beyond that, you will only waste Mana."));
		npc.Msg(L("Second, when Icebolt is cast,<br/>under the Law of Marginal Mana Decrease,<br/>only the number of iceballs you can shoot increases<br/>but the damage caused by each iceball doesn't increase."));
		npc.Msg(L("<image name='icebolt_knockdown'/>And finally, when you are ready to shoot iceballs,<br/>if you are attacked and knocked down,<br/>all of the iceballs<br/>will disappear."));
		npc.Msg(L("So, to use this Icebolt magic<br/>effectively,"));
		npc.Msg(L("...it is important<br/>to shoot at the target with these iceballs at the right moment,<br/>giving the enemy no time to counterattack."));
		npc.Msg(L("That is the summary of the class. Can you remember all that?"));
		npc.Msg(L("Ha ha. You still seem to<br/>want to learn other spells.<br/>Actually, I taught you<br/>everything I know about Icebolt magic."));
		npc.Msg(L("Whether you will use this magic well and effectively<br/>totally depends on you."));
		npc.Msg(L("Well, it is too late to teach something new.<br/>I will teach you new magic<br/>that has the opposite traits from Icebolt tomorrow.<br/>But, don't neglect to practice Icebolt, ok?"));
		npc.Msg(L("Then, let's call it a day here.<br/>You are more than halfway through this chapter already.<br/>Push on just a little farther."));
	}

	public async Task Class2_3(NpcScript npc)
	{
		if (!npc.Player.Skills.Has(SkillId.Firebolt, SkillRank.Novice))
		{
			npc.Player.Skills.Give(SkillId.Firebolt, SkillRank.Novice);
			npc.Player.Skills.Train(SkillId.Firebolt, SkillRank.Novice, 1);
		}

		npc.Msg(L("Now, let's take a look at the new magic first<br/>and then start the class."));
		npc.Msg(L("<image local='true' name='magic_ice_fire'/>Check the Skill window and press the 'Learn' button,<br/>You should be able to confirm the Firebolt magic."));
		npc.Msg(L("<image name='firebolt_fired'/>Firebolt is magic<br/>that creates flames and shoots them at the target,<br/>doing fire damage."));
		npc.Msg(L("To use this magic with ease,<br/>you have to remember<br/>what I taught you in the previous class."));
		npc.Msg(L("On that note, let's review what you learned.<br/>Let's see if you still remember<br/>what I taught you yesterday."));

		npc.Msg(L("How many times is the limit for consecutive casting?<br/><button title='4' keyword='@reply1'/><button title='5' keyword='@reply2'/><button title='6' keyword='@reply3'/><button title='108' keyword='@reply4'/>"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Hmm. Have you even tried consecutive-casting?<br/>Five times is the limit."));
				npc.Msg(L("That was an easy question.<br/>That's disappointing, <player name>."));
				break;

			case "@reply2":
				npc.Msg(L("Correct!<br/>But, anyone who tried it once<br/>would know this, right?"));
				break;

			case "@reply3":
				npc.Msg(L("Wrong. Five times is the limit.<br/>You may cast magic beyond that,<br/>but it won't make any difference."));
				npc.Msg(L("Hmm. If there is any difference,<br/>you might be taken to magic scholars<br/>to be dissected as a specimen."));
				npc.Msg(L("Watch your back."));
				break;

			case "@reply4":
				npc.Msg(L("What?? Wrong! Five times is the limit.<br/>Hmm. If there are any changes,<br/>you might be taken to magic scholars<br/>to be dissected as a specimen."));
				npc.Msg(L("Watch your back."));
				break;
		}

		npc.Msg(L("What is the most important thing<br/>in using the Icebolt magic effectively?<br/><button title='Timing' keyword='@reply1'/><button title='Sense' keyword='@reply2'/><button title='Mana' keyword='@reply3'/><button title='Robe' keyword='@reply4'/>"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Correct. You studied hard.<br/>It is rewarding to teach you."));
				npc.Msg(L("Not just for using the Icebolt magic,<br/>but for captivating a woman's heart also,<br/>timing is crucial.<br/>Have you thought about that?"));
				break;

			case "@reply2":
				npc.Msg(L("Ha ha. Having some sense would be good.<br/>But I didn't mention sense<br/>during the previous class."));
				npc.Msg(L("How can you give such an absent-minded answer<br/>in front of a beautiful lady like me?<br/>The answer is T.I.M.I.N.G! Don't forget."));
				break;

			case "@reply3":
				npc.Msg(L("Nope, that's incorrect.<br/>Mana is important for any magic,<br/>but it is not the most important factor<br/>in using the icebolt magic effectively..."));
				npc.Msg(L("Timing is the most important thing<br/>in using the icebolt magic."));
				break;

			case "@reply4":
				npc.Msg(L("What a disappointing answer...<br/>A robe does make a wizard look cool,<br/>but it has nothing to do with the Icebolt magic.<br/>You should focus more on timing."));
				npc.Msg(L("The strength of Icebolt<br/>which no other spell possesses<br/>is in the timing."));
				break;
		}

		npc.Msg(L("Hehehehe...It seems to be going into one ear and out the other<br/>because of your anticipation for new magic.<br/>Oddly disappointing."));
		npc.Msg(L("If you cannot get a good grasp on this,<br/>you will have a hard time understanding in the next class."));
		npc.Msg(L("You should remember that<br/>there is a time for learning,<br/>and always do your best at every moment."));
		npc.Msg(L("Now, I would like to explain the Firebolt magic.<br/>Please pay attention."));
		npc.Msg(L("<image name='campfire'/>First of all, let's briefly talk about fire.<br/>Fire means heat and light<br/>which an object emits as it gets hot."));
		npc.Msg(L("To make fire,<br/>the object to be burned and a high temperature to burn it are necessary.<br/>Without these two conditions met, it is impossible to make a fire."));
		npc.Msg(L("<image name='firebolt_loaded1'/>But, the story is different when using the power of Mana.<br/>Even without the object to burn,<br/>you can still make a fire by changing<br/>Mana and making it the center of heat."));
		npc.Msg(L("<image name='firebolt_fired'/>Firebolt is<br/>magic that shoots the fireball, reacted by using the power of Mana,<br/>to the target."));
		npc.Msg(L("Other than the fact that you shoot fire instead of ice,<br/>it is almost the same as Icebolt."));
		npc.Msg(L("<image name='firebolt_fired01'/>The enemy hit by the fireball<br/>would instantly feel the bodily combustion."));
		npc.Msg(L("Not only for the physical damage,<br/>but for the fear of experiencing his or her body set on fire,<br/>it is clearly a<br/>threatening magic..."));
		npc.Msg(L("Instead, since the Firebolt magic consumes more Mana<br/>compared to the Icebolt magic,<br/>you should not overuse it<br/>unless you are in an emergency situation."));
		npc.Msg(L("Now, let's learn how to cast<br/>Firebolt magic in earnest."));
		npc.Msg(L("The first step in learning the Firebolt magic is<br/>to create fireballs<br/>by using the power of Mana only."));
		npc.Msg(L("The most important part is<br/>to concentrate heat on a certain point by using Mana."));
		npc.Msg(L("As  you learn<br/>how to quickly adjust the process of<br/>absorbing heat by using the power of Mana, as with Icebolt,<br/>you should be able to easily pick it up."));
		npc.Msg(L("<image name='firebolt_loaded1'/>When you feel the concentrated<br/>Mana burning,<br/>try to create repulsive force in fireballs towards the target<br/>by using some of the Mana."));
		npc.Msg(L("<image name='firebolt_fired'/>Now, the fireball will fly towards the target.<br/>But the speed will not be that fast."));
		npc.Msg(L("Because a lot of energy is being concentrated<br/>to maintain the temperature of the fireball,<br/>even with repulsive force, it is difficult<br/>to pick up the speed as fast as Icebolt does."));
		npc.Msg(L("Well, that is all I can tell you.<br/>The rest, you should learn on your own through training and practicing."));
		npc.Msg(L("If you feel there is something missing in using the skill<br/>and want more information about the history<br/>and the development process of Firebolt magic,"));
		npc.Msg(L("go find and read 'How to Perform Firebolt Magic'<br/>written by Tarnwen."));
		npc.Msg(L("It is a bestseller that sold many copies.<br/>If you read it, it will be of great help.<br/>For the trainees' convenience,<br/>I sell the copies at wholesale price here. If you need one, just tell me."));
		npc.Msg(L("Well, now we've completed Basic Sorcery Chapter Two.<br/>I hope that, through the magic I taught you today,<br/>you will be able to better use Icebolt magic which you learned in Chapter One<br/>and be more proficient at using Mana."));
		npc.Msg(L("If you want to dig deeper on<br/>how to use the Firebolt magic,<br/>it would be good<br/>to enroll for Basic Sorcery  Chapter Three."));
		npc.Msg(L("Then, let's call it a day.<br/>Thank you for listening.<br/>This chapter is over, but please practice more."));
	}

	public async Task Class3_1(NpcScript npc)
	{
		npc.Msg(L("As you know, Basic Sorcery Chapter Three is about the application of Firebolt Magic.<br/>I believe that, when you complete this course<br/>you will no longer be a wannabe wizard, but a real bonafide wizard."));
		npc.Msg(L("After taking this class and practicing hard,<br/>I hope you will become a wizard<br/>who better understands the order of the universe."));
		npc.Msg(L("Especially, at the end of this chapter,<br/>you will have a tough assignment.<br/>So, you will have a difficult time<br/>if you don't practice hard. Ha ha."));
		npc.Msg(L("Then, let's start today's class."));
		npc.Msg(L("<image name='firebolt_loaded2'/>The third section of the Basic Sorcery is about the ways in which you can accumulate the Firebolt magic<br/>to make it stronger.<br/>I mentioned it before when I was teaching you<br/>the Icebolt magic."));
		npc.Msg(L("I'm talking about consecutive-casting.<br/>Yes, today's class is about consecutive-casting of Firebolt."));
		npc.Msg(L("Now, a question here as always:"));

		npc.Msg(L("I explained that there is an important law associated with Mana<br/>when casting the Icebolt magic consecutively.<br/>Do you remember what it was?<button title='The Law of Marginal Mana Decrease' keyword='@reply1'/><button title='The Law of Marginal Mana Increase' keyword='@reply2'/><button title='The Law of Marginal Mana Conservation' keyword='@reply3'/>"));
		switch (await npc.Select())
		{
			case "@reply1":
				npc.Msg(L("Yes, correct.<br/>It is the law that the amount of Mana required for each stage decreases gradually<br/>when casting magic consecutively."));
				break;

			case "@reply2":
				npc.Msg(L("Increase. Oh, gosh.<br/>When casting ice elemental magic consecutively,<br/>the amount of mana required for each stage<br/>decreases gradually.<br/>So, not increase, but decrease is the right answer."));
				npc.Msg(L("You probably confused it with the Law of Marginal Damage Increase."));
				break;

			case "@reply3":
				npc.Msg(L("Ha ha. You seem confused.<br/>Sorry, but incorrect."));
				npc.Msg(L("When casting Ice Elemental magic consecutively,<br/>the amount of mana required for each stage<br/>decreases gradually.<br/>So, decrease is the right answer."));
				npc.Msg(L("You probably confused it with the Law of Marginal Damage Conservation."));
				break;
		}

		npc.Msg(L("What I'm going to talk about today is<br/>a little different from the Law of Marginal Mana Decrease."));
		npc.Msg(L("Do you remember I said that<br/>the limit on consecutive-casting is up to five times?"));
		npc.Msg(L("If you practiced it,<br/>you might know that's the case<br/>for Firebolt as well."));
		npc.Msg(L("<image name='icebolt_loaded'/>When you cast the Icebolt spell consecutively,<br/>the number of iceballs<br/>orbiting around you in a circle increases..."));
		npc.Msg(L("<image name='firebolt_loaded1'/>But when you cast the Firebolt spell consecutively,<br/>due to fire attracting fire,<br/>the fireballs orbiting around you in a circle get bigger."));
		npc.Msg(L("The bigger the fireball is,<br/>the greater the damage will be."));
		npc.Msg(L("Ha ha. It seems like you know it all."));
		npc.Msg(L("But, do you know how much the damage will grow<br/>every time you cast this magic?"));
		npc.Msg(L("Probably not.<br/>Even for good recorders,<br/>it would be difficult to observe this kind of detail and practice it."));
		npc.Msg(L("In the case of Firebolt, every time you cast it consecutively,<br/>the damage caused by the fireball increases."));
		npc.Msg(L("The increment of the damage grows as well<br/>at each stage.<br/>This is the so-called Law of Marginal Damage Increase."));
		npc.Msg(L("The law commonly applies to<br/>Fire elemental magic."));
		npc.Msg(L("But, the amount of Mana required at each level<br/>of the consecutive casting isn't any different."));
		npc.Msg(L("It means that Firebolt cast consecutively<br/>can make a stronger attack<br/>than Icebolt at the same level."));
		npc.Msg(L("<image local='true' name='mana_empty'/>But, if you cast Firebolt<br/>the same way you do Icebolt,<br/>you would run out of Mana<br/>and get into trouble."));
		npc.Msg(L("Much of the Firebolt magic is similar<br/>to the Icebolt magic.<br/>But, if you are not careful,<br/>you won't be able to use it as you might have originally intended."));
		npc.Msg(L("Now, let's summarize the class.<br/>It is very simple."));
		npc.Msg(L("<image name='firebolt_loaded2'/>First, consecutive-casting of the Firebolt magic is<br/>similar to that of the Icebolt magic.<br/>But, it doesn't create several fireballs as with the Icebolt magic.<br/>Instead, the size of the fireball increases."));
		npc.Msg(L("Second, while the Law of Marginal Mana Decrease<br/>applies to the Icebolt spell,<br/>the Law of Marginal Damage increase applies<br/>to the Firebolt spell."));
		npc.Msg(L("This is simple stuff.<br/>But, you remember it<br/>since the consecutive-casting of the Icebolt and the Firebolt spell is different<br/>in these two ways."));
		npc.Msg(L("In the next class,<br/>I'm going to explain how to use the Firebolt magic effectively."));
		npc.Msg(L("Please come to the next class.<br/>The more you focus on the class, <username/>, the better you<br/>will understand magic.<br/>So, don't be anxious if your performance doesn't improve quickly."));
		npc.Msg(L("From now on, attending magic classes alone will not be enough<br/>to maintain your magic performance."));
		npc.Msg(L("If you don't review what you learned that day,<br/>you might end up forgetting everything later.<br/>So, you should review what you learned through practice<br/>after class."));
		npc.Msg(L("Well, that's all for today."));
	}

	public async Task Class3_2(NpcScript npc)
	{
		npc.Msg(L("Now, let's begin.<br/>As I said yesterday,<br/>today's class is about<br/>using the Firebolt magic effectively."));
		npc.Msg(L("You could say that this is Firebolt in practice. Ha ha."));
		npc.Msg(L("Have you tried to cast Firebolt<br/>to shoot a fireball at the target and see what happens?"));
		npc.Msg(L("Hee hee. If you saw it burning.<br/>It means you did not practice hard."));
		npc.Msg(L("<image name='firebolt_fired'/>If you saw<br/>the target thrown far away every time it was hit,<br/>that means you practiced hard."));
		npc.Msg(L("The difference is the essence of today's class."));
		npc.Msg(L("<image name='icebolt_fired'/>First, let's take a look at the Icebolt magic.<br/>Icebolt is a spell you can cast consecutively.<br/>If you attack a target consecutively with it,<br/>the target collapses by the third iceball."));
		npc.Msg(L("And, if you attack with a perfect timing,<br/>your enemy will not be given the chance to attack.<br/>I said up to this point."));
		npc.Msg(L("<image name='firebolt_fired'/>By contrast, Firebolt will allow you<br/>to attack only once regardless of how many times it is cast.<br/>And, this one time attack<br/>can make the enemy collapse."));
		npc.Msg(L("Then, what kind of attack<br/>would be effective with Firebolt?"));
		npc.Msg(L("Let's image a combat scene."));
		npc.Msg(L("<image name='firebolt_attacked'/>Since it takes time to cast a spell,<br/>if you hit the target after casting it only once,<br/>the target would start retaliating<br/>right away."));
		npc.Msg(L("<image name='firebolt_attacked'/>You shouldn't try to attack it again with a spell,<br/>because even before you could finish casting,<br/>the target would probably attack you and knock you down."));
		npc.Msg(L("In other words, what is important is<br/>to hit the target from a distance and<br/>incapacitate it before the target<br/>approaches you or retaliates."));
		npc.Msg(L("For this,<br/>consecutive-casting of the Firebolt spell is a good option."));
		npc.Msg(L("As I explained in the previous class,<br/>the Law of Marginal Damage Increase applies<br/>to the Firebolt magic<br/>and you can move freely after readying the spell."));
		npc.Msg(L("But, still, there is one thing you should be very careful about."));
		npc.Msg(L("<image name='firebolt_knockdown'/>It is the case of being attacked<br/>with fireballs orbiting around you in a circle<br/>after you cast the spell."));
		npc.Msg(L("Since maintaining the fireball<br/>requires a high level of concentration,<br/>if you are attacked and knocked down,<br/>you may lose part of the magical energy you prepared."));
		npc.Msg(L("<image name='icebolt_knockdown'/>Do you remember I said that, if you are attacked<br/>when you are ready to use the Icebolt magic,<br/>you lose all the iceballs you created?<br/>It's the same principle."));
		npc.Msg(L("Fortunately, however,<br/>if you are attacked and knocked down<br/>when you are ready to cast the spell after readying it with consecutive-casting,"));
		npc.Msg(L("you don't lose all of the fireballs.<br/>Instead, its size only decreases.<br/>To be more specific,<br/>the power of the fireball drops to the level of the previous casting."));
		npc.Msg(L("Ha ha. Don't you think strategic use of magic is pretty profound?<br/>It is an established academic field of study,<br/>so if you are interested,<br/>it would be good to study in this field later."));
		npc.Msg(L("Time's up, already.<br/>Studying with you, <username/>, is always so pleasant<br/>that time just flies by. Ha ha."));
		npc.Msg(L("The next class is already the last class of Chapter Three.<br/>You will have a tough assignment in the next class.<br/>If you do a good job, you will be richly rewarded."));
		npc.Msg(L("I can't emphasize enough for<br/>you to practice hard even after the classes."));
		npc.Msg(L("Well, then, work hard in your magic training."));
	}

	public async Task Class3_3(NpcScript npc)
	{
		npc.Msg(L("This is the last class already..."));
		npc.Msg(L("I recall the day when you, <username/>, first came to me to learn magic.<br/>Heeheehee... It seems like only yesterday. Time flies, doesn't it?"));
		npc.Msg(L("Now, let's start the class.<br/>You need to be extra focused since this is the last class of chapter three.<br/>There is an assignment after the class, too."));
		npc.Msg(L("Now, please pay attention. You will learn two things today."));
		npc.Msg(L("First, the delay time of magic.<br/>Second, how to secure the distance from the target<br/>with the help of allies."));
		npc.Msg(L("Let me first explain<br/>about delay time of magic.<br/>As I said before,<br/>any magic requires time for casting."));
		npc.Msg(L("But, if you are attacked while casting a spell,<br/>what would happen?<br/>Have you ever been in this kind of situation?"));
		npc.Msg(L("<image name='firebolt_attacked'/>If you are attacked while casting a spell,<br/>the Mana you have used so far becomes useless<br/>and the casting also fails."));
		npc.Msg(L("Even if you are not knocked down from the attack,<br/>the attack itself<br/>can make these things happen.<br/>So, you need to be extra careful."));
		npc.Msg(L("Now, then,<br/>you must be as careful as you can<br/>not to be attacked while you are casting a spell.<br/>Unfortunately, however, it is easier said than done."));
		npc.Msg(L("As you probably know from experience,<br/>making the time to cast while maintaining a certain distance from the enemy is nearly impossible."));
		npc.Msg(L("You probably know that<br/>you cannot move while casting a spell<br/>because you are concentrating, right?"));
		npc.Msg(L("When this happens, the best way<br/>to maintain the distance from the target is<br/>by getting some help from your friends."));
		npc.Msg(L("It is simple and easy<br/>to secure the distance from the target with the help of friends."));
		npc.Msg(L("If you have a melee-type ally,<br/>you can stand at a distance from him<br/>while your friend stands between the target and you,<br/>guarding you physically."));
		npc.Msg(L("Long range attack types... That is,<br/>when you have a party consisting of archers or wizards,<br/>you can make magic attacks on the target<br/>that is attacking your friend."));
		npc.Msg(L("Well, that's all for today.<br/>If you have a good understanding of the characteristics of Icebolt and Firebolt,<br/>today's class will be a big help."));
		npc.Msg(L("Let me briefly explain the assignment and finish this class.<br/>I will send details to you via an owl."));
		npc.Msg(L("The assignment is to enter Alby Dungeon<br/>and to defeat the Giant Spider.<br/>If you cannot do this alone,<br/>you can go there with others by forming a party."));
		npc.Msg(L("Don't be too scared.<br/>You are already a good wizard.<br/>Plus, there is no deadline for this assignment. So, you have plenty of time to do it."));
		npc.Msg(L("You can start the assignment as soon as the owl delivers it.<br/>Check it immediately after receiving the assignment."));
		npc.Msg(L("Now, then, I wish you good luck."));

		npc.StartQuest(200029); // Basic Sorcery 3 Mission
	}
}

public class BasicSorcery3MissionQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200029);
		SetName(L("Basic Sorcery 3 Mission"));
		SetDescription(L("Today's assignment is to clear Alby Dungeon. Drop an item on the Alby Dungeon altar, make your way to the Statue of Goddess, and then come back. You can go by yourself or with friends. - Lassar -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		AddObjective("obj1", L("Clear Alby Dungeon"), 13, 3190, 3200, ClearDungeon("tircho_alby_dungeon"));
		AddObjective("obj2", L("Talk to Lassar"), 9, 2020, 1537, Talk("lassar"));

		AddReward(Exp(100));
		AddReward(StatBonus(Stat.Int, 2));
		AddReward(Skill(SkillId.Lightningbolt, SkillRank.Novice, 1));

		AddHook("_lassar", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "obj2"))
		{
			npc.FinishQuest(this.Id, "obj2");
			npc.Msg(L("Ah, you are done with the sorcery assignment? I see you made it back in one piece."));
			npc.Msg(L("<image local='true' name='magic_lightning'/>Okay then, after you complete this quest,<br/>check your Skill window and press the 'Learn' button.<br/>You will see the Lightningbolt spell."));
			npc.Msg(L("If you paid attention in class<br/>you will easily know which part of the spell<br/>you need to practice."));
			npc.Msg(L("If you'd like to read more,<br/>I recommend the book [Basics of Lightning Magic: Lightningbolt]."));
			npc.Msg(L("Oh...right!<br/>Now that you know how to use the Lightningbolt spell,<br/>you are a fully privileged Elemental Apprentice.<br/>Check your title."));
			npc.Msg(L("The title proves<br/>that you have completed<br/>all three classes of Lassar's Magic School."));
			npc.Msg(L("That's all for Basic Sorcery 3...<br/>May the blessings of the great druid Uscias be with you..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
