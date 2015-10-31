// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Test.Data
{
	public class ColorMapTests
	{
		[Fact]
		public void GetColors()
		{
			var db = new ColorMapDb();
			db.Load("../../../../system/db/colormap.dat", true);

			var rnd = new Random(1039548676);

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

			Assert.Equal((uint)0x87ADB9, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x635FCC, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xF0DCC0, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x089E8C, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x70CCD6, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x655BAA, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xE1F2D8, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x534E4B, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x0BAECE, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x36156D, db.GetRandom(1, rnd));
			Assert.Equal((uint)0x465078, db.GetRandom(1, rnd));
			Assert.Equal((uint)0xEA9ABB, db.GetRandom(1, rnd));
		}
	}
}
