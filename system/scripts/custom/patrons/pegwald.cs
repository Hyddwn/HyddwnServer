//--- Aura Script -----------------------------------------------------------
// Waldon
//--- Description -----------------------------------------------------------
// Patreon reward NPC for Pegwald, thank you for your support!
//---------------------------------------------------------------------------

public class PegwaldPatronScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("Waldon");
		SetFace(skinColor: 24, eyeType: 57, eyeColor: 27, mouthType: 2);
		SetBody(height: 1.3f);
		SetStand("data/gfx/char/chapter4/human/anim/social_motion/male_2014_springdress");
		SetLocation(31, 14799, 8219, 167);

		EquipItem(Pocket.Face, 4900, 16);
		EquipItem(Pocket.Hair, 4006, 0x000000);
		EquipItem(Pocket.Head, 18940, 0x000000, 0x000000, 0x000000);
		EquipItem(Pocket.Armor, 15508, 0x000000, 0x000000, 0x333333);
		EquipItem(Pocket.Glove, 16094, 0x000000, 0x000000, 0x000000);
		EquipItem(Pocket.Shoe, 17942, 0x000000, 0x000000, 0x000000);

		AddPhrase("*sigh*");
		AddPhrase("What? Why is the rum gone?");
		AddPhrase("I miss my crew.");
		AddPhrase("This place lack quite a few customers, I say");
		AddPhrase("Oh really? You can repair accessories?");
		AddPhrase("We'll take a cup o' kindness yet, for auld lang syne");
	}

	protected override async Task Talk()
	{
		await Intro("You can feel a presence of mystery as you approach the man dressed in black from head to toe.<br/>His hair gives you an impression he's been far away from home quite a while.<br/>His deep and sleepy brown eyes greet yours and he begins to speak.");

		Msg("Hey kid, is something bothering you?");
		Msg("Tell me what's on your mind!");
		await Conversation();
		Close(Hide.None, "We shall meet again sometime.");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("You should think that I walked hand in hand with death sometimes<br/>by the fact that I've been so close to it.<br/>Countless times, I tell you!");
				Msg("Last time was only a few days ago when I was forced to abandon my own ship and crew.<br/>They didn't believe me when I told them about how dangerous the idea of opposing \"the keeper\" was.");
				Msg("I swam for nearly a day before I could finally feel the shore under my feet.<br/>That's how I got here by the way. The rest of the crew?<br/>I am already afraid that the depths have claimed their bodies.");
				break;

			case "rumor":
				Msg("So far I feel very welcome here. I'm always happy to give out<br/>a hand or two too if that's needed.<br/>I only have one problem however...");
				Msg("... There's a severe lack of rum. The rum is gone.<br/>Why is the rum gone?");
				break;

			case "about_skill":
				Msg("I am an excellent swimmer, but I suppose no landlubber<br/>like you have such interests.");
				break;

			case "about_arbeit":
				Msg("A part-time job? Do you have one for me?");
				break;

			case "about_study":
				Msg("I am no teacher I'm afraid, but if there's one thing life<br/>has taught me it's telling a girl to calm down works<br/>about as well as trying to baptize a cat.");
				break;

			case "shop_misc":
				Msg("Try talking to that old geezer with the glasses over there.<br/>I think he runs a general store.<br/>Just keep in mind he's very grumpy and stiff when it<br/>comes to running this business.");
				break;

			case "shop_grocery":
				Msg("Not in this town, but Lady Jennifer across this desk might<br/>be able to help you out!<br/>(Unless you've got certain rum-issues like me.)");
				break;

			case "shop_healing":
				Msg("We don't have anything like that. If you need treatment I'm affraid<br/>you've got to head over to the closest town from here.");
				Msg("Comgan over there, by the way, he sells potions and bandages<br/>in case you're looking for some.");
				break;

			case "shop_inn":
				Msg("Sorry kiddo, I don't think we have anything like that here.");
				break;

			case "shop_bank":
				Msg("Right over there! Talk to Bryce, he'll help you out...<br/>unless you have other intentions.<br/>*cracks fingers*");
				Msg("You don't look like the type for it, but try anything<br/>naughty and ol' Waldon will whoop your socks off<br/>and prove to you he's a master ninja!");
				Msg("What? Did you expect me to be a pirate?");
				break;

			case "shop_smith":
				Msg("If you don't mind the topless old man over there sweating<br/>it off at that piece of blade, I'm pretty sure he will offer you some good<br/>deals if you came looking for armor and weapons.");
				Msg("His grand daughter over there however, should not be trusted with repairs.<br/>Leave that to Mr. Hulk Hogan.");
				break;

			case "shop_armory":
				Msg("It's somewhere over there, I think.");
				break;

			case "skill_rest":
				Msg("Have a seat if you want!");
				break;

			case "skill_range":
				Msg("Did I tell you about the time someone nearly killed<br/>me with one of those bow and arrows?<br/>The poor guy was a terrible shot and missed a bear cub.");
				Msg("The arrow flew right past and got me right here.");
				Msg(Hide.Both, "(He pulls out a charm hanging from around his<br/>neck from the inside of his jacket.<br/>It is a thick ring-shaped stone that and<br/>has deep cuts around the center.)");
				Msg("I really hope he was just a terrible shot and did not intend to hit me.");
				break;

			case "skill_composing":
				Msg("Look at me. Do I look like some great composer to you?");
				break;

			case "skill_tailoring":
				Msg("Tailoring? Well don't look at me, I've never in my life known how to sew clothes.");
				break;

			case "skill_counter_attack":
				Msg("A sneaky skill to trick your enemies and taking the advantage of battle.<br/>Don't rely on it too much, however.");
				break;

			case "skill_smash":
				Msg("A lethal blow that will sometimes slay foes in a single blow.<br/>Not the most graceful technique, but it works.");
				break;

			case "skill_gathering":
				Msg("If you go into Barri dungeon you can gather a lot of<br/>minerals that you can use to make ingots and such.");
				break;

			case "windmill":
				Msg("Spin to win, am I right? It's a very useful technique that<br/>makes you very dizzy if you use it too much!");
				Msg("Wait- You were actually talking about a windmill, weren't you?<br/>Sorry I don't know about any.");
				break;

			case "temple":
				Msg("Church? Comgan over there mentioned he wants to build one soon.<br/>I don't know much about them, but I'll be there to help him with it!");
				Msg("I was actually raised in a monastery back in the day.<br/>Huh? Why's that funny? I'm actually not a pirate!");
				break;

			case "school":
				Msg("I think that's sort of like a monastery, am I right?");
				Msg("I stayed at one for years to become a master at shadow and martial arts,<br/>but one day a band of murderers came along and killed everyone I knew.");
				Msg("I believe so at least, because I was the only one<br/>to make it out alive... Ani...");
				Msg(Hide.Both, "(You're not sure what he's thinking about but you can feel a sort of sadness around him.)");
				break;

			case "skill_windmill":
				Msg("Spin to win, am I right? It's a very useful technique that<br/>makes you very dizzy if you use it too much!");
				break;

			case "skill_campfire":
				Msg("Fire is such a beautiful element, but yet so wild and chaotic.<br/>It's so mesmerizing...");
				break;

			case "shop_restaurant":
				Msg("Ahh, I would do anything to enjoy myself a good meal right now.<br/>... No, not anything, but... Oh you get it.");
				break;

			case "shop_cloth":
				Msg("We only have our old geezer over there who sells a few hats and shoes,<br/>I believe. Don't try to haggle with him.");
				break;

			case "shop_bookstore":
				Msg("Nope, we don't have anything like that. You have to go to<br/>Dunbarton if you want some good literature.");
				break;

			case "skill_fishing":
				Msg("Do you even know where you are right now?");
				break;

			case "bow":
				Msg("Need a bow? Check out Edern's or Elen's blacksmith shop over there.");
				break;

			case "lute":
				Msg("I don't mind lutes. A good song will always cheer me up.");
				break;

			case "complicity":
				Msg("What?");
				break;

			case "musicsheet":
				Msg("You can find those in any general shop,<br/>even Gilmore has a few in stock.");
				break;

			case "nao_friend":
				Msg("A friend of who, nao? (laughs.)");
				break;

			case "nao_blacksuit":
				Msg("Uhh... Why are you talking to me about this?");
				break;

			case "present_to_nao":
				Msg("I don't even know who this Nao is.");
				break;

			case "graveyard":
				Msg("I always pay my respects and try to memory<br/>all my dear and loved ones when I'm at sea.<br/>They say all sources of life comes from the sea.");
				Msg("My experience is that the sea only claims it.<br/>The surface is the line that draws the difference of it.<br/>Life and death.");
				break;

			case "skill_instrument":
				Msg("I'd love me a good song or two!<br/>Can you play me one?");
				break;

			case "tir_na_nog":
				Msg("I am not familiar with this term, but I hear people<br/>refer to it as paradise. Of all those places I've been,<br/>I have not yet been to some Tir Na Nog.<br/>Do you think such place exists?");
				break;

			case "mabinogi":
				Msg("I don't like lullabies.");
				break;

			default:
				Msg("Do you take me for someone that knows absolutely everything?<br/>I dare say that I am not! Try to ask someone else.");
				break;
		}
	}
}
