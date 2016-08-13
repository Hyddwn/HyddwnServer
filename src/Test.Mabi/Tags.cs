// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using Xunit;

namespace Aura.Tests.Mabi
{
	public class TagsTests
	{
		[Fact]
		public void TagsMatchTestGeneral()
		{
			var tags = new Tags("/test1/test2/test3/");

			Assert.True(tags.Matches("*"));
			Assert.True(tags.Matches("/test1/"));
			Assert.True(tags.Matches("/test2/"));
			Assert.True(tags.Matches("/test3/"));
			Assert.True(tags.Matches("/test*/"));
			Assert.True(tags.Matches("/*test*/$"));
			Assert.True(tags.Matches("^/test*/"));
			Assert.True(tags.Matches("/test1/&/test2/"));
			Assert.True(tags.Matches("/test2/|/test3/"));
			Assert.True(tags.Matches("/test4/|(/test1/*/test3/)"));
			Assert.True(tags.Matches("/test4/|(/test1/&/test3/)"));
			Assert.True(tags.Matches("/test5/|(/test4/|/test3/)"));
			Assert.True(tags.Matches("/test1/&(/test2/|/test3/)"));
			Assert.True(tags.Matches("/test1/&(/test2/|/test4/)"));
			Assert.True(tags.Matches("((/*1/|/test4/)&(/test2/&/test3/))"));
			Assert.True(tags.Matches("((/*1/&/test*/)&(/test2/&/test3/))"));
			Assert.True(tags.Matches("/test[^5]/test[^5]/test[^2]/"));

			Assert.False(tags.Matches("*/*test_*/*"));
			Assert.False(tags.Matches("*/*fest*/*"));
			Assert.False(tags.Matches("/test4/"));
			Assert.False(tags.Matches("/test1/&(/test2/&/test4/)"));
			Assert.False(tags.Matches("((/*1/&/test4/)&(/test2/&/test3/))"));
			Assert.False(tags.Matches("/test[^5]/test[^5]/test[^3]/"));
		}

		[Fact]
		public void TagsMatchTestRaces()
		{
			var tags = new Tags("/animal/wolf/direwolf/beast/whitedirewolf/");

			Assert.True(tags.Matches("/wolf/"));
			Assert.True(tags.Matches("*/wolf/*"));
			Assert.True(tags.Matches("/wolf/*/beast/"));
			Assert.True(tags.Matches("/wolf/|/beast/"));
			Assert.True(tags.Matches("/wolf/&/beast/"));
			Assert.True(tags.Matches("/*direwolf/"));

			Assert.False(tags.Matches("/skeleton/"));
			Assert.False(tags.Matches("/blackdirewolf/"));
		}

		[Fact]
		public void TagsMatchTestEquipment()
		{
			var tags = new Tags("/equip/armor/lightarmor/leather/smith_repairable/");

			Assert.True(tags.Matches("*/equip/* & */armor/* & */lightarmor/*"));
			Assert.True(tags.Matches("*/cloth/*|/armor/"));

			Assert.False(tags.Matches("*/equip/* & */armor/* & */cloth/*"));
			Assert.False(tags.Matches("*/cloth/*"));
			Assert.False(tags.Matches("*/equip/righthand/weapon/bow/wood/bow03/Long_Bow/*"));
			Assert.False(tags.Matches("*/UpMax1Shield/*"));
		}

		[Fact]
		public void TagsMatchTestEnchants()
		{
			var tags = new Tags("/equip/armor/heavyarmor/steel/smith_repairable/");
			Assert.True(tags.Matches("/equip/"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/powder/|/fishing/bait/"));

			tags = new Tags("/equip/armor/heavyarmor/steel/smith_repairable/");
			Assert.True(tags.Matches("/equip/"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/powder/|/fishing/bait/"));

			tags = new Tags("/equip/armor/heavyarmor/steel/smith_repairable/");
			Assert.False(tags.Matches("/equip/&/weapon/"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/righthand/weapon/edged/steel/blade/01/Dagger/smith_repairable/twin_sword/weapontype_combat/");
			Assert.True(tags.Matches("/equip/&/weapon/"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/armor/agelimit_cloth/human_elf_only/");
			Assert.True(tags.Matches("/equip/&/cloth/|(/equip/armor/agelimit_cloth/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/armor/cloth/human_elf_only/");
			Assert.True(tags.Matches("/equip/&/cloth/|(/equip/armor/agelimit_cloth/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/armor/lightarmor/leather/smith_repairable/");
			Assert.False(tags.Matches("/equip/&/cloth/|(/equip/armor/agelimit_cloth/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/righthand/weapon/wand/lightning_wand/");
			Assert.True(tags.Matches("/equip/righthand/weapon/wand/|(/equip/twohand/weapon/staff/)|(/knuckle/staff/knuckle_staff/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/twohand/weapon/knuckle/staff/knuckle_staff/smith_repairable/");
			Assert.True(tags.Matches("/equip/righthand/weapon/wand/|(/equip/twohand/weapon/staff/)|(/knuckle/staff/knuckle_staff/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/righthand/weapon/crossbow/steel/wood/Arbalest/");
			Assert.False(tags.Matches("/equip/righthand/weapon/wand/|(/equip/twohand/weapon/staff/)|(/knuckle/staff/knuckle_staff/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/foot/agelimit_armorboots/steel/magicsmith_repairable/human_only/");
			Assert.True(tags.Matches("/equip/&/armorboots/|(/equip/&/agelimit_armorboots/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/foot/armorboots/steel/magicsmith_repairable/human_giant_only/");
			Assert.True(tags.Matches("/equip/&/armorboots/|(/equip/&/agelimit_armorboots/)"));
			Assert.False(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));

			tags = new Tags("/equip/lefthand/weapon/arrow/wood/arrow01/stack_item/not_dyeable/not_enchantable/");
			Assert.False(tags.Matches("/equip/&/armorboots/|(/equip/&/agelimit_armorboots/)"));
			Assert.True(tags.Matches("/bolt/|/arrow/|/robe/|/powder/|/guild/pendant/|/fishing/bait/"));
		}

		[Fact]
		public void TagsConversionTest()
		{
			var st = "/animal/wolf/direwolf/beast/whitedirewolf/";

			var tags1 = new Tags(st);

			string str = tags1;
			Assert.Equal(st, str);

			Tags tags2 = str;
			Assert.Equal(st, tags2.ToString());
		}
	}
}
