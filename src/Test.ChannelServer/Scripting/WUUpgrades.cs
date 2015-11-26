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
			Assert.Equal(wu.Unknown, 7);
			Assert.Equal(wu.CastingSpeed, 8);
			Assert.Equal(wu.MagicDamage, 9);
		}

		[Fact]
		public void WUModification()
		{
			var test1 = "12345603070809";
			var test2 = "65432723374859";
			var test3 = "100012fffefdfc";

			var wu = new WUUpgrades(test1);
			Assert.Equal(test1, wu.ToString());

			wu.ChainCastSkillId = 65432;
			wu.ChainCastLevel += 0x1;
			wu.ManaUse += 0x20;
			wu.Unknown += 0x30;
			wu.CastingSpeed += 0x40;
			wu.MagicDamage += 0x50;

			Assert.Equal(test2, wu.ToString());
			Assert.Equal(wu.ChainCastSkillId, 65432);
			Assert.Equal(wu.ChainCastLevel, 7);
			Assert.Equal(wu.ManaUse, 0x23);
			Assert.Equal(wu.Unknown, 0x37);
			Assert.Equal(wu.CastingSpeed, 0x48);
			Assert.Equal(wu.MagicDamage, 0x59);

			wu.ChainCastSkillId = 10001;
			wu.ChainCastLevel = 2;
			wu.ManaUse = -1;
			wu.Unknown = -2;
			wu.CastingSpeed = -3;
			wu.MagicDamage = -4;

			Assert.Equal(test3, wu.ToString());
			Assert.Equal(wu.ChainCastSkillId, 10001);
			Assert.Equal(wu.ChainCastLevel, 2);
			Assert.Equal(wu.ManaUse, -1);
			Assert.Equal(wu.Unknown, -2);
			Assert.Equal(wu.CastingSpeed, -3);
			Assert.Equal(wu.MagicDamage, -4);
		}

		[Fact]
		public void WUExceptions()
		{
			var wu = new WUUpgrades();

			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { wu.ChainCastSkillId = 1; });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { wu.ChainCastLevel = 10; });
			Assert.DoesNotThrow(() => { wu.ChainCastSkillId = 10000; });
			Assert.DoesNotThrow(() => { wu.ChainCastLevel = 9; });
		}
	}
}
