// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using System;
using Xunit;

namespace Aura.Tests.Channel.World.Entities
{
	public class CreatureTests
	{
		private const int AvgNum = 100000;
		private const int AvgErrMargin = 8;

		[Fact]
		public void GetRndMagicBalance()
		{
			for (int j = 40; j <= 60; j += 10)
			{
				var expected = j;
				var creature = new TestCreature();
				creature.IntBase = expected * 4 + 10;

				var avg = 0;
				for (int i = 0; i < AvgNum; ++i)
					avg += creature.GetRndMagicBalance(0);
				avg /= AvgNum;

				Assert.InRange(avg, expected - AvgErrMargin, expected + AvgErrMargin);
			}

			for (int j = 40; j <= 60; j += 10)
			{
				var expected = j;
				var creature = new TestCreature();
				creature.IntBase = 0;

				var avg = 0;
				for (int i = 0; i < AvgNum; ++i)
					avg += creature.GetRndMagicBalance(expected);
				avg /= AvgNum;

				Assert.InRange(avg, expected - AvgErrMargin, expected + AvgErrMargin);
			}
		}

		[Fact]
		public void GetRndBalance()
		{
			for (int j = 40; j <= 60; j += 10)
			{
				var expected = j;
				var creature = new TestCreature();
				creature.DexBase = expected * 4 + 10;

				var avg = 0;
				for (int i = 0; i < AvgNum; ++i)
					avg += creature.GetRndBalance(0);
				avg /= AvgNum;

				Assert.InRange(avg, expected - AvgErrMargin, expected + AvgErrMargin);
			}

			for (int j = 40; j <= 60; j += 10)
			{
				var expected = j;
				var creature = new TestCreature();
				creature.DexBase = 0;

				var avg = 0;
				for (int i = 0; i < AvgNum; ++i)
					avg += creature.GetRndBalance(expected);
				avg /= AvgNum;

				Assert.InRange(avg, expected - AvgErrMargin, expected + AvgErrMargin);
			}
		}

		private class TestCreature : Creature
		{
			public override bool Warp(int regionId, int x, int y)
			{
				throw new NotImplementedException();
			}

			protected override bool ShouldSurvive(float damage, Creature from, float lifeBefore)
			{
				throw new NotImplementedException();
			}

			public override void Aggro(Creature target)
			{
				throw new NotImplementedException();
			}
		}
	}
}
