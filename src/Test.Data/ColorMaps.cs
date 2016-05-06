// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Aura.Tests.Data
{
	public class ColorMapTests
	{
		[Fact]
		public void GetColors()
		{
			var db = new ColorMapDb();
			db.Load("../../../../system/db/colormap.dat", true);

			var rnd = new TestRandom(1039548676);

			Assert.Equal((uint)0xA02522, db.GetAt(10, 0, 0));
			Assert.Equal((uint)0xFFD800, db.GetAt(32, 1, 1));

			Assert.Equal((uint)0xF6D1BD, db.GetAt(1, 0, 0));
			Assert.Equal((uint)0xCDD742, db.GetAt(1, 25, 25));
			Assert.Equal((uint)0x087189, db.GetAt(1, 50, 50));
			Assert.Equal((uint)0x0B8EBE, db.GetAt(1, 75, 75));
			Assert.Equal((uint)0xF1ACC9, db.GetAt(1, 100, 100));
			Assert.Equal((uint)0x9C7B6B, db.GetAt(1, 125, 125));
			Assert.Equal((uint)0xC1CBCF, db.GetAt(1, 150, 150));
			Assert.Equal((uint)0xEBE3E1, db.GetAt(1, 175, 175));
			Assert.Equal((uint)0x546C70, db.GetAt(1, 200, 200));
			Assert.Equal((uint)0xC0E2AF, db.GetAt(1, 225, 225));
			Assert.Equal((uint)0x8B2208, db.GetAt(1, 250, 250));

			Assert.Equal((uint)0xFFD800, db.GetRandom(32, rnd));

			Assert.Equal((uint)0xA4657E, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x822E00, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xFFFFFF, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x3CBE95, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x8E7580, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xD64B95, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x805D57, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x806C7C, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x9DD6E7, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xDECFB5, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x84C15E, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xF37333, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x707070, db.GetRandom(1, rnd));


		}

		public class TestRandom : Random
		{
			private int _val = 0;

			public TestRandom()
			{
			}

			public TestRandom(int seed)
			{
				_val = seed;
			}

			public override int Next()
			{
				return (_val = _val * 0x08088405 + 1);
			}

			public override int Next(int maxValue)
			{
				return this.Next() % maxValue;
			}

			public override int Next(int minValue, int maxValue)
			{
				return minValue + this.Next() % (maxValue - minValue);
			}
		}
	}
}
