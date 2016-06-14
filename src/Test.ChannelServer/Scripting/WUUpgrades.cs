// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using System;
using Xunit;

namespace Aura.Tests.Channel.Scripting
{
	public class WUUpgradesTests
	{
		[Fact]
		public void WUParsing()
		{
			var test = "12345603070809";
			var wu = new WUUpgrades(test);

			Assert.Equal(test, wu.ToString());
			Assert.Equal(wu.ChainCastSkillId, 12345);
			Assert.Equal(wu.ChainCastLevel, 6);
			Assert.Equal(wu.ManaUse, 3);
			Assert.Equal(wu.ManaBurn, 7);
			Assert.Equal(wu.CastingSpeed, 8);
			Assert.Equal(wu.MagicDamage, 9);

			var test2 = "00006504030201";
			var wu2 = new WUUpgrades(test2);

			Assert.Equal(test2, wu2.ToString());
			Assert.Equal(wu2.ChainCastSkillId, 6);
			Assert.Equal(wu2.ChainCastLevel, 5);
			Assert.Equal(wu2.ManaUse, 4);
			Assert.Equal(wu2.ManaBurn, 3);
			Assert.Equal(wu2.CastingSpeed, 2);
			Assert.Equal(wu2.MagicDamage, 1);
		}

		[Fact]
		public void WUSizeFix()
		{
			var test1_1 = "0504030201";
			var test1_2 = "00000504030201";
			var wu1 = new WUUpgrades(test1_1);

			Assert.Equal(test1_2, wu1.ToString());
			Assert.Equal(wu1.ChainCastSkillId, 0);
			Assert.Equal(wu1.ChainCastLevel, 5);
			Assert.Equal(wu1.ManaUse, 4);
			Assert.Equal(wu1.ManaBurn, 3);
			Assert.Equal(wu1.CastingSpeed, 2);
			Assert.Equal(wu1.MagicDamage, 1);
		}

		[Fact]
		public void WUModification()
		{
			var test1 = "12345603070809";
			var test2 = "65432723374859";
			var test3 = "100012fffefdfc";
			var test4 = "00000504030201";

			var wu = new WUUpgrades(test1);
			Assert.Equal(test1, wu.ToString());

			wu.ChainCastSkillId = 65432;
			wu.ChainCastLevel += 0x1;
			wu.ManaUse += 0x20;
			wu.ManaBurn += 0x30;
			wu.CastingSpeed += 0x40;
			wu.MagicDamage += 0x50;

			Assert.Equal(test2, wu.ToString());
			Assert.Equal(wu.ChainCastSkillId, 65432);
			Assert.Equal(wu.ChainCastLevel, 7);
			Assert.Equal(wu.ManaUse, 0x23);
			Assert.Equal(wu.ManaBurn, 0x37);
			Assert.Equal(wu.CastingSpeed, 0x48);
			Assert.Equal(wu.MagicDamage, 0x59);

			wu.ChainCastSkillId = 10001;
			wu.ChainCastLevel = 2;
			wu.ManaUse = -1;
			wu.ManaBurn = -2;
			wu.CastingSpeed = -3;
			wu.MagicDamage = -4;

			Assert.Equal(test3, wu.ToString());
			Assert.Equal(wu.ChainCastSkillId, 10001);
			Assert.Equal(wu.ChainCastLevel, 2);
			Assert.Equal(wu.ManaUse, -1);
			Assert.Equal(wu.ManaBurn, -2);
			Assert.Equal(wu.CastingSpeed, -3);
			Assert.Equal(wu.MagicDamage, -4);

			wu.ChainCastSkillId = 0;
			wu.ChainCastLevel = 5;
			wu.ManaUse = 4;
			wu.ManaBurn = 3;
			wu.CastingSpeed = 2;
			wu.MagicDamage = 1;

			Assert.Equal(test4, wu.ToString());
			Assert.Equal(wu.ChainCastSkillId, 0);
			Assert.Equal(wu.ChainCastLevel, 5);
			Assert.Equal(wu.ManaUse, 4);
			Assert.Equal(wu.ManaBurn, 3);
			Assert.Equal(wu.CastingSpeed, 2);
			Assert.Equal(wu.MagicDamage, 1);
		}

		[Fact]
		public void WUExceptions()
		{
			var wu = new WUUpgrades();

			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { wu.ChainCastLevel = 10; });
			Assert.DoesNotThrow(() => { wu.ChainCastSkillId = 10000; });
			Assert.DoesNotThrow(() => { wu.ChainCastLevel = 9; });
		}
	}
}
