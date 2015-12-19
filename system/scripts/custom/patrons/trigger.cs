//--- Aura Script -----------------------------------------------------------
// Trigger
//--- Description -----------------------------------------------------------
// Patreon reward NPC for Trigger, thank you for your support!
//---------------------------------------------------------------------------

public class TriggerPatronScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("Trigger");
		SetFace(skinColor: 16, eyeType: 30, eyeColor: 52, mouthType: 2);

		EquipItem(Pocket.Face, 4909, 16);
		EquipItem(Pocket.Hair, 4006, 0x1000004C);
		EquipItem(Pocket.Head, 18036, 0x1A2D70, 0x0FAFFF, 0x1A2D70);
		EquipItem(Pocket.Armor, 13031, 0x1A2D70, 0x0FAFFF, 0x0FAFFF);
		EquipItem(Pocket.Glove, 16013, 0x1A2D70, 0x1A2D70, 0x1A2D70);
		EquipItem(Pocket.Shoe, 17523, 0x1A2D70, 0x1A2D70, 0x1A2D70);
		EquipItem(Pocket.RightHand1, 40284, 0x1A2D70, 0x0FAFFF, 0x1A2D70);
		EquipItem(Pocket.RightHand2, 40038, 0xCC0000, 0xFFA90A, 0xFFA90A);
		EquipItem(Pocket.LeftHand2, 40817, 0xCC0000, 0xFFFFFF, 0xFFFFFF);

		SetLocation(1, 4496, 33096, 229);

		AddPhrase("Magic only ever gets stronger.");
		AddPhrase("Specialize first. Hybridize later.");
		AddPhrase("Icebolt is for beginners only. Don't rely on it forever.");
		AddPhrase("Thunder is the king of the spells.");
		AddPhrase("There is no such thing as an \"intermediate\" spell.");
		AddPhrase("Efficiency and speed trump raw power.");
		AddPhrase("Don't be just a \"fire mage\" or an \"ice mage\". You're just limiting your options.");

		NPC.StateEx |= CreatureStatesEx.RoyalAlchemist;
	}

	protected override async Task Talk()
	{
		Msg("I'm Trigger, the master of magic.<br/>Here for some pointers?");
		await Conversation();
		End();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("I was once a wizardry instructor; I've traveled extensively to study magic, and I can cast any spell in Erinn with ease.<br/>If you need to know how magic works, just ask.");
				break;

			case "rumor":
				Msg("Lassar's school is rather basic.<br/>Don't rely on formal training too much; basic education is good, but practical experience makes wizards out of apprentices.");
				break;

			case "about_skill":
				Msg("Lassar can teach you some basic spells, but to get the stronger stuff, you'll need to travel.<br/>The Dunbarton School should be your next stop, after you've finished Lassar's lessons of course.");
				break;

			case "about_arbeit":
				Msg("Just about everyone in town has something that needs doing, and they're all paying.<br/>Take special notice of Endelyon up at the church; she's offering the Holy Water of Lymilark to her helpers.");
				break;

			case "about_study":
				Msg("Lassar has some introductory magic courses for the aspiring mage.<br/>Just head inside the school building and ask her.");
				break;

			case "shop_misc":
				Msg("The general shop is back up the hill, in the town square.<br/>Malcolm's always got some basic tools and clothing in stock.");
				break;

			case "shop_grocery":
				Msg("Caitin's grocery is right across from the general shop.<br/>Take the path up the hill; the grocery will be on your right as you enter town.");
				break;

			case "shop_healing":
				Msg("Did Ranald get his students all bruised again?<br/>You'll want to see Dilys; her house is north of the town square, on the path to Alby Dungeon.<br/>Say hello for me, will you?");
				break;

			case "shop_inn":
				Msg("Sweet Nora and her uncle Piaras run the Tir Chonaill Inn.<br/>Head straight out the school gates, past the reservoir, and cross the first bridge you come to.<br/>The inn will be on your right.");
				break;

			case "shop_bank":
				Msg("Ah, the local branch of the Erskin Bank.<br/>Just head up the hill to the town square.<br/>You can't miss it; it's the best-looking building in town.<br/>Ask for Bebhinn.");
				break;

			case "shop_smith":
				Msg("The local blacksmith, Ferghus, has no skill at all.<br/>Never trust Ferghus with anything you don't want to see broken.<br/>In fact, go straight to Dunbarton and have Nerys do your repairs.<br/>Edern down in Bangor is even better with hammer and anvil, if you've got the coins to spare for his exceptional services.");
				break;

			case "skill_rest":
				Msg("Nora over at the inn can teach you how to recover from injury more effectively.<br/>Ask her about it sometime.");
				break;

			case "skill_range":
				Msg("Once the undisputed champions of combat, the bow and crossbow fell out of favor as new techniques were developed for other weapons.<br/>If you're curious about the old ways, show Chief Duncan a bow or a crossbow and ask him if he remembers the skills to use it.<br/>He'll get you started.");
				break;

			case "skill_instrument":
				Msg("Just pick up an instrument and start getting a feel for its sound.<br/>You'll get better with time and practice.<br/>Personally I prefer the lyre, the guitar, and the violin, but you can also use drums, chimes, flutes, and all manner of other music makers.");
				break;

			case "skill_composing":
				Msg("If you'd like to write your own music, there's a book that's been floating around the bookshops: Introduction to Music Composition.<br/>Malcolm should have a few copies on hand.");
				break;

			case "skill_tailoring":
				Msg("Learning how to work with cloth is a long, tedious process, but creating fashionable outfits is rather rewarding.<br/>Nora over at the inn has some tailoring kits and basic materials for you to start with.<br/>Good luck.");
				break;

			case "skill_magnum_shot":
				Msg("Ask Ranald about that.<br/>He'll teach you how it's done.");
				break;

			case "skill_counter_attack":
				Msg("The wise man learns from the experience of others.<br/>Trefor can teach you how to turn an attack back on your attacker so you won't have to learn the hard way.");
				break;

			case "skill_smash":
				Msg("I'm not here to do Ranald's job for him, you know.<br/>Finish his combat lessons and you'll learn all about that.");
				break;

			case "skill_gathering":
				Msg("If it's ingredients for mana potions you're looking for, you'll never find anything but the most basic herbs out in the open fields.<br/>The powerful stuff grows deep in dungeons and across the sea in the caverns of Iria.<br/>Good hunting!");
				break;

			case "square":
				Msg("I remember when the Tir Chonaill square was the busiest place in all of Uladh.<br/>Sometimes I wish we could return to those days.");
				break;

			case "pool":
				Msg("One of the best fishing spots on the whole continent is the reservoir right outside the school gates.<br/>You should try it out sometime.");
				break;

			case "farmland":
				Msg("Barley and wheat grow in abundance just south of here, in the fields near the windmill.<br/>I hear Endelyon could use some of that grain.");
				break;

			case "windmill":
				Msg("The old windmill keeps turning faithfully, grinding all our grain<br/>under Alissa's watchful eye.<br/>If you need to grind some grain of your own, Alissa can help you out.");
				break;

			case "brook":
				Msg("It flows down from the mountains right through Tir Chonaill.<br/>The windmill pumps some of the water into the local reservoir.");
				break;

			case "shop_headman":
				Msg("Old Chief Duncan's house is behind the great tree in the town square.<br/>Have a chat with the chief sometime; he knows quite a bit.");
				break;

			case "temple":
				Msg("Meven and Endelyon run the local Church of Lymilark, west of the town square.<br/>It's a good place to obtain the Holy Water of Lymilark, which is useful for blessing tools and equipment so they'll last longer.");
				break;

			case "school":
				Msg("You're already here.<br/>Why are you asking?");
				break;

			case "skill_windmill":
				Msg("If you've learned the Defense skill already, talk to Ranald.<br/>He can explain.");
				break;

			case "skill_campfire":
				Msg("The shepherd boy, Deian, knows a thing or two about campfires.<br/>Try asking him.");
				break;

			case "shop_restaurant":
				Msg("Tir Chonaill has no restaurants, but there's some fine cuisine elsewhere on this continent.<br/>Loch Lios in Emain Macha has some truly tasty dishes, and the Bean Rua club in the same city serves the best drinks in the whole kingdom of Aliech.");
				break;

			case "shop_armory":
				Msg("Ferghus, that sorry excuse for a blacksmith, has some basic weapons and armor on hand at all times.<br/>If you're looking for something more exotic, you'll have to visit the larger towns and cities of Uladh, or even cross the sea into Iria.");
				break;

			case "shop_cloth":
				Msg("Malcolm's general shop keeps some clothes in stock, but the nearest real clothier is in Dunbarton.<br/>Simon is the best tailor on two continents; he'll set you up properly.");
				break;

			case "shop_bookstore":
				Msg("I used to browse Aeira's bookshop in Dunbarton just to have an excuse to see her.");
				Msg("...Did I say that out loud?");
				break;

			case "shop_goverment_office":
				Msg("Tir Chonaill doesn't have any official government buildings.<br/>Chief Duncan manages affairs around here; talk to him if you need to retrieve your lost possessions.");
				break;

			case "graveyard":
				Msg("The local graveyard is north of Chief Duncan's house.<br/>Don't loiter in that place; the spiders grow fairly large up there and will quite happily have you for lunch if you don't pay attention.");
				break;

			case "skill_magic_shield":
				Msg("Useful in very few situations.<br/>Generally, when it comes to magic, the best defense is a precise and efficient offense.<br/>Still, it never hurts to expand one's repertoire.<br/>I learned the shield spells from an old chieftain named Kousai, deep in the jungles of Iria.<br/>Ask him to teach you.");
				break;

			case "skill_fishing":
				Msg("There isn't really a wrong way to fish.<br/>Pick up a rod and some bait and you're set.<br/>With practice, you'll discover how to lure in the fish more quickly...and possibly find some long-lost treasure in the waters as well.<br/>The reservoir right outside the school is an excellent fishing spot.");
				break;

			case "bow":
				Msg("Check with Ferghus.<br/>He's got the basics.<br/>Nerys of Dunbarton has a wider selection.");
				break;

			case "lute":
				Msg("Everybody's first instrument, it seems.<br/>Malcolm should have some in his shop.");
				break;

			case "complicity":
				Msg("I have no use for petty quarrels.");
				break;

			case "tir_na_nog":
				Msg("Paradise is not what it seems.<br/>Keep both eyes -- and your mind -- open.");
				break;

			case "mabinogi":
				Msg("Many bards sing stories, or \"mabinogi\".<br/>They are tales of legends and heroes and godlike feats.<br/>Why bother listening to the deeds of others when you can perform great deeds of your own, though?");
				break;

			case "musicsheet":
				Msg("Malcolm sells specially-designed score paper for anyone with the ability to create music of their own.<br/>If you don't already know how to compose music, Malcolm also has a book that can teach you how.");
				break;

			case "nao_friend":
				Msg("Nao has many friends, but there are only two people I know of that she would refer to as such -- Ruairi and Tarlach.");
				break;

			case "nao_owl":
				Msg("Nao's pet owl, Petrock, delivers scrolls and letters to Nao, much like the other owls of Erinn deliver things to you and me.");
				break;

			case "nao_owlscroll":
				Msg("Erinn's owls carry all manner of things between people.<br/>They're quite intelligent.");
				break;

			case "Cooker_qualification_test":
				Msg("The qualifying rounds take place every Samhain in the Emain Macha square, and the contest itself is held on the last Samhain of the month; if you ever feel like trying your hand at serious cooking, that's the place to be.");
				Msg("You'd best hope I'm not competing, though.<br/>I've received a fair share of prizes for my fine cooking.<br/>Beware the blue apron!");
				break;

			case "ego_weapon":
				Msg("Speak with Tarlach up in Sidhe Sneachta.<br/>If you wish to imbue a weapon with a particular spirit, he's the only man for the job.");
				Msg("Take care, though; spirit weapons are always hungry.<br/>If you can't feed them often enough, they'll grow weak -- and when I say \"feed\", I mean weapons and armor and other such objects.<br/>Be sure you're prepared for the extra expense!");
				break;

			case "elf_vs_giant":
				Msg("I may be a powerful wizard, but I still consider the elves less than desirable companions.<br/>King Krug of the giants is nothing but hospitable, and his frozen lands are far more inviting than the scorching deserts of Connous.<br/>I am a staunch ally of the giants, and I shall remain so.");
				break;

			case "jewel":
				Msg("Gems are the absolute favorite \"food\" of all spirit weapons.<br/>They're also used for certain weapon and armor upgrades.<br/>Hang onto any gems you find; chances are they'll be useful to you.");
				break;

			case "ego_weapon_move":
				Msg("Tarlach is the only one who can help you with that.");
				break;

			case "icespear_elemental_pass":
				Msg("I used to visit that dungeon frequently.<br/>Speak with Nele in Emain Macha; he'll get you inside.");
				break;

			case "ego_weapon06":
				Msg("You'll need those fossils if you want to create a spirit weapon.<br/>Gilmore down in Bangor has exclusive access to the dungeon where they're found, and he'll let you in...for a price.");
				break;

			case "musical_know_a_nele_loeiz":
				Msg("I remember Loeiz.<br/>He's vehemently anti-magic; I never cared for him.<br/>Nele and I get along far better.");
				break;

			case "memorial_tower":
				Msg("The elven Memory Tower, when it was still standing, was a glorified panopticon.<br/>Have you met the Desert Ghosts of Connous before?<br/>If they had any memories left, I'm sure they'd love to tell you more.");
				break;

			case "magic_bean":
				Msg("I know a goblin who's an expert on magical plants.<br/>Talk to Goro in the Alby Arena Lobby -- and be sure to have one of those Fomor Beans with you when you do.");
				break;

			case "two_sword_skill":
				Msg("You prefer two swords to a sword and shield, do you?<br/>You should bring that up with Nicca over in Iria; he's got some information that should be useful to you.");
				break;

			case "burning_sword":
				Msg("I was once one of the lucky few to pull that burning sword from its resting place.<br/>If you're ever that fortunate, here's a tip: don't use the sword in the rain.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("If the continent across the sea is still a mystery to you, I won't spoil the surprise.<br/>Just go have a chat with Tarlach.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("Ships departing for Iria used to dock at Port Ceann, south of Bangor.<br/>Nowadays they sail from Port Cobh, east of Dunbarton.");
				break;

			case "exit_ffion_tutorial":
				Msg("Surely you've already been there...if you see Ffion, say hello for me, will you?");
				break;

			case "exit_vena_tutorial":
				Msg("Filia's a fantastic city.<br/>It's a shame the rest of Connous is such a dreadful bore.<br/>Give me snow and cold any day.");
				break;

			case "exit_meriel_tutorial":
				Msg("Vales may not look like much, but the giants' architecture is excellent, and the snow and ice make the whole place sparkle year-round.<br/>The giants are far more hospitable folk than the elves are, too.");
				break;

			case "making_dogcollar_of_rab":
				Msg("Fleta asked you to make a new collar for Rab, did she?<br/>She used to have me do that, too.<br/>Try asking Ferghus, or head down to Bangor and chat with Edern or his granddaughter Elen.");
				break;

			case "errand_of_fleta":
				Msg("What, do I look like a resident of Emain Macha?<br/>Ask the folks in that city about Fleta's errands.");
				break;

			case "nao_gift_collect_all":
				Msg("That's...obsessive.");
				break;

			case "g9s2_violet":
				Msg("There's a sad story behind that.<br/>Ask around Taillteann; I'm sure the residents will tell it to you.");
				break;

			case "nao_blacksuit":
			case "nao_cloth0":
			case "nao_cloth2":
			case "nao_cloth3":
			case "nao_cloth4":
			case "nao_cloth5":
			case "nao_cloth6":
			case "nao_cloth7":
			case "nao_cloth8":
			case "nao_cloth_summer":
			case "nao_cloth_shakespeare":
			case "nao_cloth_summer_2008":
			case "nao_cloth_farmer":
				Msg("I'm partial to Rua's dress, myself.");
				break;

			case "nao_cloth1":
				Msg("That dress is a work of art.<br/>A real crowd-pleaser, that one.");
				break;

			case "nao_cloth_santa":
				Msg("Ha, only for Christmas.<br/>Right back to Rua's dress after that.");
				break;

			case "present_to_nao":
				Msg("If you can convince Rua to part with her dress, I'm sure it'd look great on Nao -- not that I'd know from personal experience or anything.");
				break;

			case "Mini_Aeira_lunchbox01":
			case "Mini_Aeira_lunchbox02":
				Msg("If I didn't know what a terrible cook that girl was, I'd want her making those lunchboxes for me instead!");
				break;

			case "g1_tarlach1":
				Msg("Ah, him.<br/>You'd better speak with Chief Duncan.");
				break;

			case "g1_tarlach2":
				Msg("Some things not even Chief Duncan can remember.<br/>Stewart at the Dunbarton School should be able to fill you in.");
				break;

			case "g1_goddess":
				Msg("Morrighan is the first of the three Badhbh Catha, the goddesses of war.<br/>By herself, she is the goddess of vengeance and crows.");
				Msg("If you come across a copy of the book \"The Goddess Who Turned into Stone\", you can read the account of how Morrighan intervened during the Second Battle of Mag Tuireadh and saved the Tuatha de Dananns from annihilation at the hands of the Fomor army.<br/>Quite a tale, that is.<br/>I highly recommend reading it sometime.");
				break;

			case "g1_tarlach_of_lughnasadh":
				Msg("Tarlach is still alive, despite what most people think.<br/>He keeps to himself up north, in the snow of Sidhe Sneachta.<br/>Pay him a visit at night to hear his story first-hand.");
				break;

			case "g1_book1":
				Msg("I remember that book.<br/>I saw it in Aeira's shop down in Dunbarton once, when I was...visiting her.");
				break;

			case "g1_paradise":
				Msg("Paradise doesn't exist.<br/>Don't obsess over it.");
				break;

			case "g1_medal_of_fomor":
				Msg("Looks like one of the emblems worn by the priests of Lymilark.");
				break;

			case "g1_voucher_of_priest":
				Msg("Fomorian writing on the back?<br/>You'll want to talk to Goro.<br/>He's in Alby Arena; Ranald can sell you a coin so you can get inside.");
				break;

			case "g1_dulbrau1":
			case "g1_dulbrau2":
				Msg("I don't know Fomorian.<br/>Can't help you with that.");
				break;

			case "g1_succubus":
				Msg("Kristell used to be a succubus, and she still wants Tarlach.<br/>If you didn't see that coming, I don't know what to say.");
				break;

			case "g1_message_of_kristell":
				Msg("No, no, no.<br/>I don't want to hear about Kristell's unrequited love.<br/>You take that message straight to Tarlach and don't waste anyone else's time with it.");
				break;

			case "g1_black_rose":
				Msg("Lassar's right inside.<br/>Talk to her.");
				break;

			case "g1_mores":
				Msg("Mores Gwydion was one of the greatest wizards to ever live.<br/>He had an uncanny knack for spellcasting.<br/>I suggest you ask Chief Duncan about him; he's more familiar with the historical accounts than I am.");
				break;

			case "g1_mores_gwydion":
				Msg("Ah, that's right, the Second Battle of Mag Tuireadh.<br/>I'd forgotten.<br/>What a triumph that was, and in no small part thanks to Mores.<br/>I wouldn't believe for a moment that he was an enemy of the Tuatha de Danann.");
				break;

			case "g1_memo_of_lost_thing":
				Msg("Eavan runs the town office in Dunbarton.<br/>Just ask her about anything Mores may have left behind.");
				break;

			case "g1_goddess_morrighan1":
			case "g1_goddess_morrighan2":
				Msg("Don't believe everything you see.");
				break;

			case "g1_memo_of_parcelman":
				Msg("Ask my lovely Aeira in Dunbarton.");
				break;

			case "g1_glasgavelen":
				Msg("Glas Ghaibhleann is a monster of terrifying power.<br/>It towers over the tallest giants, eats gold for food, and breathes fiery light from its mouth.<br/>I've fought it many times.<br/>It never stays dead.");
				break;

			case "g1_book_of_glasgavelen":
				Msg("Duncan must be growing forgetful in his old age.<br/>The book's actual title is \"The Embodiment of Destruction, Glas Ghaibhleann\".<br/>I used to keep a copy on hand.<br/>Bryce has the only one I know of at the moment, though.");
				break;

			case "g1_bone_of_glasgavelen":
				Msg("Adamantium is a phenomenal substance that totally nullifies all magic.<br/>Fortunately Glas Ghaibhleann's adamantium is treated with a compound that counteracts the nullification effect, or the monster would be even more difficult to destroy.");
				break;

			case "g1_request_from_goddess":
				Msg("If Glas Ghaibhleann's body is being rebuilt yet again, I'd advise you to prepare yourself well.<br/>When it awakens, and it inevitably will, you'll be in for quite a fight.");
				break;

			case "g1_way_to_tirnanog1":
			case "g1_way_to_tirnanog2":
				Msg("Only a Fomor can get you in.<br/>Ask Kristell.");
				break;

			case "g1_cichol":
				Msg("I told you not to believe everything you see.<br/>That advice doesn't stop here.<br/>Things are not always as simple as they appear to be.");
				break;

			case "g1_revive_of_glasgavelen":
				Msg("<br/>Be prepared.<br/>Nothing but death will stop it from coming to Erinn.<br/>Get your strongest weapons; bring your strongest allies.<br/>You will need them.");
				break;

			case "g1_KnightOfTheLight":
				Msg("I know you have very little reason to believe this, but I'll tell you anyway.<br/>Don't trust Morrighan.<br/>She does not have your best interest at heart.");
				break;

			case "g2_02_Paladin":
				Msg("Self-righteous, holier-than-thou moralizers, the lot of them.<br/>Don't let that stop you from becoming one, though.<br/>Rise above the rest.");
				break;

			case "g2_10_KnightOfTheLight_Lough":
				Msg("He's one of the heroes of the Second Battle of Mag Tuireadh.<br/>He defeated Cichol's predecessor, Balor.<br/>Aeira has a book on Lugh in her shop, if you'd like to read more about the man who was once king of Aliech.");
				break;

			case "g2_10_Ridire":
				Msg("One of the best Paladins since Lugh...if you believe the stories.");
				break;

			case "g2_12_LoughGhost":
				Msg("Price has secrets.<br/>That's nothing unusual; everyone has secrets.<br/>Price's secrets are just more complicated than most.");
				Msg("Stories may be interesting to tell and to listen to, but they rarely get everything right.<br/>Keep your ears open.");
				break;

			case "g2_13_DeathOfRidire":
				Msg("Just a man.<br/>Nothing more.<br/>No paragon of virtue is immune to his own mortal failings.");
				break;

			case "g2_14_PaladinNDarkKnight":
				Msg("A band of pretenders forming a flimsy defense against a power they are incapable of comprehending?<br/>I'm not impressed.");
				break;

			case "g2_16_GoodDeed01":
			case "g2_16_GoodDeed02":
			case "g2_16_GoodDeed03":
			case "g2_16_GoodDeed04":
			case "g2_16_GoodDeed05":
			case "g2_16_GoodDeed06":
			case "g2_16_GoodDeed07":
			case "g2_16_GoodDeedEnd":
				Msg("Don't be another two-faced \"Paladin\" with a god-sized ego.<br/>Erinn has plenty of those already.<br/>Be genuine.<br/>Embrace your own obscurity.<br/>Do nothing for fame.<br/>Aid the downtrodden.<br/>Comfort the weary.");
				Msg("Remember always that the path of the true Paladin is the path easiest for others, not for yourself.");
				break;

			case "g2_19_MeaningOfPaladin":
				Msg("If you become a Paladin, you'll have the powers of men, spirits, and gods at your back.<br/>Don't let that go to your head like so many others have.");
				break;

			case "g2_21_Spirit":
				Msg("No better person to ask about spirits than Tarlach.");
				break;

			case "g2_22_SpiritOfWaterAer":
				Msg("I've spoken with Aer on many occasions.<br/>She's on Ceo Island, deep underground.<br/>Watch out for the golems.");
				break;

			case "g2_22_IdealAppearance":
				Msg("Aeira.<br/>Tell her I said so, too.");
				break;

			case "g2_24_MythrilMine":
				Msg("That stingy Gilmore is the only one who can get you into the Mythril Mine.<br/>You'll have to ask him.");
				break;

			case "g2_27_MythrilArmor":
				Msg("Take that straight to Aer and have her bless it.<br/>It's no good without a spirit's blessing.");
				break;

			case "g2_29_SpiritOfPaladin":
				Msg("The Spirit of Order allows its user to temporarily assume the form of a Paladin.<br/>Simple as that.");
				break;

			case "g2_36_PurposeOfEsras":
				Msg("You're about to accuse a powerful figurehead of some serious crimes.<br/>Prepare well before you do so.");
				break;

			case "g3_01_BrokenSeal":
				Msg("This was to be expected after Glas Ghaibhleann's defeat.<br/>Speak with Tarlach immediately.");
				break;

			case "g3_02_dungeonpass":
				Msg("Why are you asking me?<br/>Gilmore is the only one who has the pass you will need to reach your destination.<br/>Ask him instead.");
				break;

			case "g3_04_GoddessMacha":
				Msg("Macha is the second of the three Badhbh Catha, the goddesses of war.<br/>By herself, she is the goddess of destruction and chaos.");
				Msg("The city of Emain Macha, or \"Macha's Incarnation\", is named after the goddess.<br/>The book \"Macha According to Emain Macha\" can tell you more about the legend that inspired the name.");
				break;

			case "g3_07_DorcaFeadhain":
				Msg("That's a Fomorian phrase.<br/>Simply translated, it means \"army of darkness\", but there's more nuance to the words than just the literal meaning.");
				break;

			case "g3_08_LiaFail":
				Msg("Lia Fail is a powerful artifact capable of raising a mortal to the plane of the deities.<br/>It is not to be trifled with.");
				break;

			case "g3_11_DungeonPass":
				Msg("I haven't got any of those.<br/>Only Morc does.");
				break;

			case "g3_12_CrommCruaich":
				Msg("Cromm Cruaich is a great and ancient dragon from a far-off land.<br/>If he is involved in whatever your business is, you had best be on your guard.");
				break;

			case "g3_14_PreventionOfResurrection":
				Msg("Get your best gear and gather your most trustworthy companions.<br/>Baol Dungeon is not a friendly place, and you'll have precious little assistance there.<br/>I hope you're ready.");
				break;

			case "ego_eyedrop":
				Msg("Unfortunately, since I'm not a spirit, I can't get you one of those.");
				break;

			case "g3_DarkKnight":
				Msg("The people of Erinn are needlessly afraid of the Dark Knights.<br/>The real threat is the creeping corruption of those who claim to be righteous.<br/>Even if the Dark Knights were inherently evil, which they are not, open evil is far better than evil masquerading as justice.");
				break;

			case "G3_DarkKnight_Obtain_Armor":
				Msg("Sorry, I don't have any of those passes.<br/>Ask Tarlach.");
				break;

			case "G3_DarkKnight_Rumor_Armor":
				Msg("A Dark Knight's armor is an extraordinarily powerful magical suit.<br/>Piaras down at the local inn should have a book on the subject that can explain the details far better than I can.");
				break;

			case "G3_DarkKnight_Dark_Power":
				Msg("Take great care when using Dark Knight armor.<br/>It has a mind of its own, and it will drain its wearer to maintain its own power.");
				break;

			case "G3_DarkKnight_Armor_Last":
				Msg("Kristell should have some of those.<br/>Ask her.");
				break;

			case "G3_DarkKnight_Temptation":
				Msg("Gilmore keeps all manner of oddities lying around.<br/>I'm sure he has what you're looking for.");
				break;

			case "G3_DarkKnight_BlackWizard":
				Msg("The Black Wizard is in Another World, deep in Albey Dungeon.<br/>Price can help you get to him.");
				Msg("No one else will tell you this, so I will.<br/>Become a Dark Knight.<br/>All great men eventually succumb to hypocrisy and ego.<br/>Help cleanse the corruption.<br/>Don't contribute to it.");
				break;

			case "jungle_ruins":
				Msg("The Cor villagers know much more about that than I do.<br/>I've only visited those ruins a handful of times.");
				break;

			case "courcle_heart":
				Msg("It's an artifact from Falias, the city of the gods.<br/>Irinid used it to create the continent of Iria and everything in it.");
				break;

			case "Elf_oblivion":
				Msg("The once-famous Memory Tower of Filia could be used to directly manipulate the memories of the elves connected to it.<br/>I know for a fact that Castanea has abused this power more than once.");
				break;

			case "Red_Dragon":
				Msg("I only know of one dragon that matches that description, and you won't like him any more than I do.");
				break;

			case "holy_knights":
				Msg("Lucas has business with them.<br/>I suggest you ask him.");
				break;

			case "NoitarArat":
				Msg("That's a Fomorian phrase.<br/>Loosely translated, it means \"world of darkness\".<br/>Berched can explain further.");
				break;

			case "g9s2_andras_secret":
				Msg("What, you couldn't tell she was an elf just by how haughty she is?");
				break;

			case "about_Neamhain":
				Msg("Neamhain is the third of the three Badhbh Catha, the goddesses of war.<br/>By herself, she is the goddess of light.");
				Msg("In ancient times, Neamhain went by the name Irinid.<br/>She's the one who created Iria and everything in it.<br/>The residents of Tara should have some bits and pieces of that legend to share with you.");
				break;

			case "uroboros":
				Msg("The snake devouring its own tail, symbol of eternity.<br/>It's also a real creature that wields powerful magic.<br/>Should you ever run into one, take great care.");
				break;

			case "g12_lileas_set":
				Msg("Anything that delicious needs to belong to me immediately.<br/>Unfortunately I've got no money with me at the moment.");
				break;

			default:
				Msg("Hm?");
				break;
		}
	}
}
