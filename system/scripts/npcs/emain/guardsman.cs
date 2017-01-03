//--- Aura Script -----------------------------------------------------------
// Emain Macha Royal Guards
//--- Description -----------------------------------------------------------
// Script for the royal guard npcs in Emain Macha
// guardsman07 is different from the other guards and says lots of gossip
//---------------------------------------------------------------------------

public abstract class GuardsmanBaseScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetBody(height: 1.17f);
		SetFace(skinColor: 15, eyeType: 9, eyeColor: 29, mouthType: 0);
		SetStand("monster/anim/ghostarmor/natural/ghostarmor_natural_stand_friendly");

		EquipItem(Pocket.Armor, 13025, 0x008C8C8C, 0x00808080, 0x00FFFFFF);
		EquipItem(Pocket.Head, 18522, 0x00646464, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.RightHand2, 40012, 0x00FFFFFF, 0x006C7050, 0x00FFFFFF);

	}
	
	protected override async Task Talk()
	{
		RndMsg(
			"My back hurts",
			"My feet hurt...",
			"No talking is allowed during duty",
			"I cannot talk to you while on duty!",
			"...We're the proud guards of Emain Macha!<br/>As long as we're here, there's no need to fear!",
			"<title name='NONE' />(A guard with shining armor is standing post.)Everything here is in order!!",
			"When can I go home...",
			"Everything here is in order!",
			"......<p/>......<p/>......<p/>?<p/>I, I didn't fall asleep!"
		);
	}
	
	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			default:
				Msg(L("...Is that for us?"));
				break;
		}
	}
}

public class Guardsman07Script : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_guardsman07");
		SetBody(height: 1.17f);
		SetFace(skinColor: 15, eyeType: 9, eyeColor: 29, mouthType: 0);
		SetStand("monster/anim/ghostarmor/natural/ghostarmor_natural_stand_friendly");
		SetLocation(52, 40387, 58006, 63);

		EquipItem(Pocket.Face, 4900, 0x006F7048, 0x00FCB39B, 0x000D94CE);
		EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1);
		EquipItem(Pocket.Armor, 13025, 0x008C8C8C, 0x00808080, 0x00FFFFFF);
		EquipItem(Pocket.Head, 18522, 0x00646464, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.RightHand2, 40012, 0x00FFFFFF, 0x006C7050, 0x00FFFFFF);

		AddPhrase("If I wear this armor, then no one can tell that I'm asleep...");
		AddPhrase("What is that?");
		AddPhrase("When do I get off my shift...");
		AddPhrase("I miss my wife...");
		AddPhrase("Good thing no one can tell who I am under this armor.");
		AddPhrase("Dungeons... They're no picnics...");
		AddPhrase("Why is my replacement not coming?");
		AddPhrase("Are my eyes failing me?");
		AddPhrase("People treat us like machines...");
		AddPhrase("Hmm! Hmmm!");
		AddPhrase("I better report what's going on.");
		AddPhrase("Who's the streaker there!");
		AddPhrase("Another code red in the dungeon...?");
		AddPhrase("Hey, who's throwing away trash!");
		AddPhrase("Hmmm... my legs hurt...");
	}
	
	protected override async Task Talk()
	{
		RndMsg(
			"Hey... Have you heard...?<p/>...You know Lucas, that guy that looks like a hoodlum...?<br/>I think he likes Delen...<br/>Is he trying to break another poor girl's heart or what...",
			"Hey... Have you heard...?<p/>...I heard the young lady that sells flowers at the Town Square has a twin. I think our Captain is interested in one of them...",
			"Hey... Have you heard...?<p/>...I don't know since when, but there are some people who claimed to have rescued the Goddess.<br/>I wonder what a person like that looks like...",
			"...We are the proud Guards of Emain Macha!<br/>As long as we're here, there is no need to fear any monsters!",
			"Hey... Have you heard...?<p/>...I heard this town doesn't have a School...<br/>I heard there was one before, but for some reason, they got rid of it...",
			"Hey... Have you heard...?<p/>...I heard Osla, who sells weapons,<br/>is actually from a faraway town.<br/>Aren't her parents worried<br/>after sending their daughter all the way over here all alone?",
			"Hey... Have you heard...?<p/>...You know Lucas, that club owner...?<br/>I heard he likes to collect all different types of bows.<br/>I guess he makes a lot of money from the club.",
			"Hey... Have you heard...?<p/>...I think Esras and the Lord go way back.<br/>Apparently, she used to take care of him ever since he was a little boy.<p/>It's almost like she's his mom.<br/>I see why Esras would sacrifice so much for him.",
			"Hey... Have you heard...?<p/>...Supposedly there's someone who walks around dropping money at the Town Square.<br/>...Do they think money grows on trees or something...",
			"Hey... Have you heard...?<p/>...The red moon in the sky, I heard it's the moon of Humans...<br/>Well, I'm sure it's just an old fable that's been passed down...",
			"Hey... Have you heard...?<p/>...Apparently there is quite a bit of fish even in the lake so you can fish there.<br/>I want to check it out once I'm off duty",
			"Hey... Have you heard...?<p/>...Apparently there is a club, northeast from the town, that you can only go to at night...<br/>Strange...<p/>Should we check it out after work...?",
			"Hey... Have you heard...?<p/>...I heard that Agnes, the Healer, thinks she's really pretty...<br/>Well, it is true...<p/>No one can deny that...",
			"Hey... Have you heard...?<p/>...You know Fraser who works at the Restaurant...?<br/>He always gets in trouble by Gordon,<br/>but I heard he is becoming better and better.<br/>I think Gordon secretly has high expectations of Fraser...",
			"Hey... Have you heard...?<p/>...You know Nele, that Bard...?<br/>You know no one has ever heard him actually sing?<br/>Maybe he's really tone deaf...",
			"Don't talk to me right now. The Captain might come by.",
			"Hey... Have you heard...?<p/>...You know the Restaurant over there...<br/>Loch Lios...?<br/>I heard Shena, who is one of the workers there...<br/>...Doesn't have a father...<p/>...That's so unfortunate... So sad...",
			"Hey... Have you heard...?<p/>...About Craig, the Paladin leader?<br/>Even though he is a very strict officer now,<br/>I heard he used to be<br/>the complete opposite.<p/>Although, no one has dared to<br/>ask him if this is true...",
			"Hey... Have you heard...?<p/>...You know how sometimes random items come out of the lake?<br/>Some people have even reeled up bright red shields.</p>Maybe you'll get 3 times stronger if you equip it...<p/>No... I'm just saying that would be cool...",
			"Hey... Have you heard...?<p/>...You know Gordon, the owner of the Restaurant, Loch Lios...?<br/>I heard he was in the army before.<p/>He probably made the best army food.",
			"Hey... Have you heard...?<p/>...I heard the previous Lord's son was exceptional with the sword<br/>that he was even able to compete with Captain Aodhan at one point.<p/>Wait, if he is the previous Lord's son, isn't he the current Lord?<br/>Why does he look frail now...?",
			"Hey... Have you heard...?<p/>...You know how Paladin students<br/>always go to Barri dungeon to train?<p/>I heard the real reason they go there is to claim the rights to the gold mine under Emain Macha.<p/>I didn't know our Lord was that clever.<br/>But isn't Bangor out of our jurisdiction...?",
			"Hey... Have you heard...?<p/>...I heard there's a Corn field north of the city.<br/>There are some soldiers<br/>that go there to get some corn while they are off duty...<p/>...If Captain Aodhan found out, they would be in serious trouble...",
			"Hey... Have you heard...?<p/>...If you go southwest from here,<br/>there's a small island called Ceo Island,<br/>but it's hard to get there by boat because of the thick fog...<p/>Apparently you can only get there through the Moon Gate...<p/>But don't even think about going there...<br/>There's a rumor that Golems live in that island...",
			"Hey...<p/>...Have you been eating...?",
			"Hey... Have you heard...?<p/>...You know that Chief that lives in that countryside village far up north from here?<br/>I heard there's a cat that lives in his house that shows you strange pictures.<p/>Also, I heard it looks so hideous<br/>that people who've seen the cat say it felt like they went to hell and back.<p/>What kind of cat is this?",
			"Hey... Have you heard...?<p/>...There used to be a statue of some hero next to the Magic school,<br/>but I think they took it down when they got rid of the School.<br/>I wonder who the statue was made after...",
			"Hey... Have you heard...?<p/>...There's a rumor that our Lord<br/>is not an only child but that he has a sibling.<p/>But I guess the previous Lord was not happy with the Lord's older brother<br/>and banished him from the Kingdom.<p/>But only the elders seem to know about this rumor<br/>so I don't know if it's true or not.<p/>Then maybe the Lord's son<br/>who was the exceptional swordsman was that son...",
			"Hey... Have you heard...?<p/>...I heard there's a small little village called Tir Chonaill up north, on the outskirts of the Kingdom.<br/>Supposedly, the people that live there are really behind...",
			"Hey... Have you heard...?<p/>...They say the food at Loch Lios is so good that<br/>one bite will take you to heaven and back...<br/>If it wasn't so pricey I would check it out myself...",
			"Hey... Have you heard...?<p/>...You know Craig, the Paladin leader...?<br/>I heard he's the same age as our Captain.<p/>They are very repectful towards each other in public<br/>but I heard they are really informal and casual when they are by themselves.<p/>I can't believe he's only in his late 20's,<br/>I guess you age fast when you have a kid.",
			"Hey... Have you heard...?<p/>...The whole town knows about you gossiping because you spent so much time talking to me...<p/>Great, there goes my promotion...",
			"Everything here's in order!",
			"Hey... Have you heard...?<p/>...You know the beggar near the Town Square? I heard all he wants is money",
			"Hey... Have you heard...?<p/>...You know Galvin...? I heard he collects armor. What a strange hobby...",
			"Hey... Have you heard...?<p/>...I heard the food at Loch Lios is so good<br/>that even Chefs-in-training go there to taste the food.<br/>You can learn just by tasting...?<br/>The food there must be amazing.",
			"Hey... Have you heard...?<p/>...You know the Clothing shop, Tre'imhse Cairde...?<br/>I heard it wasn't originally owned by Ailionoa.<br/>Someone else was the original owner,<br/>but apparently there was some scandal and Ailionoa took over after.<p/>It's a fairly expensive shop... If they just handed it down to her,<br/>something big must've happened, don't you think?",
			"Hey... Have you heard...?<p/>...They say you not only catch fish in the lake<br/>but really expensive items as well.<br/>If that's true, anyone can make a lot of moeny if they just get lucky.<br/>Maybe I should go fishing when I'm off duty...",
			"Hey... Have you heard...?<p/>...Although he's a Priest,<br/>I heard James is quite a skilled fighter...<br/>...Things like that make me wonder about his past sometimes...",
			"Hey... Have you heard...?<p/>...You know that Clothing shop over there...<br/>Tre'imhse Cairde...? The lady owner there seems to be desperately looking for someone...<p/>...<br/>...She was looking for someone named...Sammon? Well...it was something close to that...",
			"Hey... Have you heard...?<p/>...You know those sign posts you see here and there that only show faces...<br/>...I heard a man named Lucas made them...<br/>What was he thinking...?",
			"Hey... Have you heard...?<p/>...You know the Restaurant over there...<br/>Loch Lios...?<br/>There's a guy named Fraser who interns there<br/>who says he can tell twins apart...<p/>...That's some talent...",
			"Hey... Have you heard...?<p/>...Supposedly there's someone who walks around dropping money at the Town Square.<br/>If they're going to do that, at least drop a lot... Or give me some...",
			"Hey... Have you heard...?<p/>...I heard Lucas, the Club owner, has a different girlfriend every time you see him.<br/>He's broken a lot of girls' hearts..."
		);
	}	

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{		
			default:
				Msg(L("...Is that for me? Thank you."));
				break;
		}
	}
}

public class Guardsman01Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman01"); SetLocation(52, 34270, 45985, 222); EquipItem(Pocket.Face, 4900, 0x00460D00, 0x00471E00, 0x00EAB21C); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman02Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman02"); SetLocation(52, 34825, 46534, 221); EquipItem(Pocket.Face, 4900, 0x00F57B34, 0x0070A1D6, 0x00ACB361); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman05Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman05"); SetLocation(52, 32032, 48355, 223); EquipItem(Pocket.Face, 4900, 0x00673138, 0x00315934, 0x0099422C); EquipItem(Pocket.Hair, 4041, 0x00004040, 0x00004040, 0x00004040); } }
public class Guardsman06Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman06"); SetLocation(52, 32462, 48773, 225); EquipItem(Pocket.Face, 4900, 0x0024587B, 0x00F69063, 0x00DC1B40); EquipItem(Pocket.Hair, 4041, 0x00004040, 0x00004040, 0x00004040); } }
public class Guardsman08Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman08"); SetLocation(52, 41213, 58006, 63); EquipItem(Pocket.Face, 4900, 0x007B6C3C, 0x006E5E5A, 0x00E4E97E); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman13Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman13"); SetLocation(52, 31273, 49527, 226); EquipItem(Pocket.Face, 4900, 0x00616169, 0x00435467, 0x0000295D); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman15Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman15"); SetLocation(52, 30253, 46178, 32); EquipItem(Pocket.Face, 4900, 0x00F57046, 0x00B94E8F, 0x00004E22); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman16Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman16"); SetLocation(52, 34614, 50555, 165); EquipItem(Pocket.Face, 4900, 0x00272476, 0x003F6863, 0x00693200); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman51Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman51"); SetLocation(60, 5487, 6048, 201); EquipItem(Pocket.Face, 4900, 0x00F32D37, 0x00FFC380, 0x0056001D); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman52Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman52"); SetLocation(60, 5922, 6088, 194); EquipItem(Pocket.Face, 4900, 0x00E20048, 0x00FFF3D7, 0x00FBC75B); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman53Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman53"); SetLocation(66, 5460, 6346, 250); EquipItem(Pocket.Face, 4900, 0x0052BA61, 0x00AFC5E6, 0x00852957); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
public class Guardsman54Script : GuardsmanBaseScript { public override void Load() { base.Load(); SetName("_guardsman54"); SetLocation(66, 6545, 6346, 133); EquipItem(Pocket.Face, 4900, 0x00216F60, 0x000184C5, 0x002B0039); EquipItem(Pocket.Hair, 4030, 0x00FCF4D1, 0x00FCF4D1, 0x00FCF4D1); } }
