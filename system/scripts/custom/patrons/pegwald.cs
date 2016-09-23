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

		EquipItem(Pocket.Face, 4909, 16);
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
				Msg("So far I feel very welcome here. I feel very happy to make<br/>my presence here and help out those who need a hand or two.<br/>There is only one problem.");
				Msg("There is something terribly wrong with this rum.<br/>Reason? There is no rum.");
				break;

			case "about_skill":
				Msg("Heh, I am very good at swimming, but I don't expect<br/>a land crab like you to have such interests.");
				break;

			case "about_arbeit":
				Msg("A part-time job? Do you have one for me?");
				break;

			case "about_study":
				Msg("You should try asking a teacher about that.<br/>I had a very good master who taught me a lot of things.<br/>Yes, I might not look the type, but I was a very good student.<br/>Alas, the poor man died too young when raiders came along and<br/>caused hell upon the temple.");
				Msg("What? You thought I was I pirate? No, I'm a ninja.");
				break;

			case "shop_misc":
			case "shop_bank":
			case "shop_smith":
			case "shop_armory":
				Msg("It's somewhere over there, I think.");
				break;

			case "temple":
				Msg("Church? Is that like a temple? A shrine?<br/>I don't know, kid.");
				break;

			case "school":
				Msg("I never went to school,<br/>I was raised in a temple.");
				break;

			case "shop_restaurant":
				Msg("This place is not a restaurant.<br/>It's a pub. A rum-less pub.");
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
