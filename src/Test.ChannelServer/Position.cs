// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World;
using Aura.Mabi;
using System;
using System.Drawing;
using Xunit;

namespace Aura.Tests.Channel
{
	public class PositionTests
	{
		[Fact]
		public void Instantiation()
		{
			var pos1 = new Position();
			Assert.Equal(0, pos1.X);
			Assert.Equal(0, pos1.Y);

			var pos2 = new Position(123, 456);
			Assert.Equal(123, pos2.X);
			Assert.Equal(456, pos2.Y);

			var locationId = 0x3000000100020003;
			var pos3 = new Position(locationId);
			Assert.Equal(2 * 20, pos3.X);
			Assert.Equal(3 * 20, pos3.Y);

			var pos4 = new Position(pos2);
			Assert.Equal(123, pos4.X);
			Assert.Equal(456, pos4.Y);
		}

		[Fact]
		public void GetDistance()
		{
			var pos1 = new Position(0, 0);
			var pos2 = new Position(0, 500);
			var pos3 = new Position(0, -500);
			var pos4 = new Position(500, 500);
			var pos5 = new Position(-500, -500);

			Assert.Equal(500, pos1.GetDistance(pos2));
			Assert.Equal(500, pos1.GetDistance(pos3));
			Assert.Equal(707, pos1.GetDistance(pos4));
			Assert.Equal(707, pos1.GetDistance(pos5));
			Assert.Equal(1414, pos4.GetDistance(pos5));
		}

		[Fact]
		public void InRange()
		{
			var pos1 = new Position(0, 0);
			var pos2 = new Position(0, 500);
			var pos4 = new Position(500, 500);
			var pos5 = new Position(-500, -500);

			Assert.Equal(true, pos1.InRange(pos2, 500));
			Assert.Equal(false, pos1.InRange(pos2, 499));
			Assert.Equal(true, pos4.InRange(pos5, 1415));
			Assert.Equal(false, pos4.InRange(pos5, 1413));
		}

		[Fact]
		public void InCone()
		{
			var centerPos = new Position(0, 0);
			var topPos = new Position(0, 500);
			var bottomPos = new Position(0, -500);
			var leftPos = new Position(-500, 0);
			var rightPos = new Position(500, 0);

			var up = MabiMath.DegreeToRadian(90);
			var down = MabiMath.DegreeToRadian(270);
			var left = MabiMath.DegreeToRadian(180);
			var right = MabiMath.DegreeToRadian(0);

			Assert.Equal(true, topPos.InCone(centerPos, up, 600, MabiMath.DegreeToRadian(10)));
			Assert.Equal(true, bottomPos.InCone(centerPos, down, 600, MabiMath.DegreeToRadian(10)));
			Assert.Equal(true, leftPos.InCone(centerPos, left, 600, MabiMath.DegreeToRadian(10)));
			Assert.Equal(true, rightPos.InCone(centerPos, right, 600, MabiMath.DegreeToRadian(10)));

			Assert.Equal(true, topPos.InCone(centerPos, up, 500, MabiMath.DegreeToRadian(10)));
			Assert.Equal(false, topPos.InCone(centerPos, up, 500, MabiMath.DegreeToRadian(180)));
			Assert.Equal(true, topPos.InCone(centerPos, up, 600, MabiMath.DegreeToRadian(180)));
			Assert.Equal(true, topPos.InCone(centerPos, up, 1000, MabiMath.DegreeToRadian(180)));

			Assert.Equal(true, topPos.InCone(centerPos, up, 1000, MabiMath.DegreeToRadian(190)));
			Assert.Equal(true, topPos.InCone(centerPos, up, 1000, MabiMath.DegreeToRadian(-190)));
			Assert.Equal(true, topPos.InCone(centerPos, up, 1000, MabiMath.DegreeToRadian(9999)));
			Assert.Equal(true, topPos.InCone(centerPos, up, 1000, MabiMath.DegreeToRadian(-9999)));

			Assert.Equal(false, topPos.InCone(centerPos, MabiMath.DegreeToRadian(45), 2000, MabiMath.DegreeToRadian(45)));
			Assert.Equal(true, topPos.InCone(centerPos, MabiMath.DegreeToRadian(68), 2000, MabiMath.DegreeToRadian(45)));
		}

		[Fact]
		public void GetRandomInRange()
		{
			var rnd = new Random(Environment.TickCount);
			var pos = new Position(10, 10);

			for (int i = 0; i < 10000; ++i)
			{
				var rndPos1 = pos.GetRandomInRange(10, rnd);
				Assert.InRange(pos.GetDistance(rndPos1), 0, 11);

				var rndPos2 = pos.GetRandomInRange(5, 10, rnd);
				Assert.InRange(pos.GetDistance(rndPos2), 4, 11);
			}
		}

		[Fact]
		public void GetRandomInRect()
		{
			var rnd = new Random(Environment.TickCount);
			var pos = new Position(10, 10);

			for (int i = 0; i < 10000; ++i)
			{
				var rndPos = pos.GetRandomInRect(10, 10, rnd);
				Assert.InRange(pos.X, 0, 20);
				Assert.InRange(pos.Y, 0, 20);
			}
		}

		[Fact]
		public void GetRelative()
		{
			var pos1 = new Position(100, 100);
			var pos2 = new Position(100, 200);

			Assert.Equal(new Position(100, 250), pos1.GetRelative(pos2, 50));
			Assert.Equal(new Position(100, 150), pos1.GetRelative(pos2, -50));
			Assert.Equal(new Position(100, -50), pos1.GetRelative(pos2, -250));

			var pos3 = new Position(50, 100);

			Assert.Equal(new Position(0, 100), pos1.GetRelative(pos3, 50));
			Assert.Equal(new Position(100, 100), pos1.GetRelative(pos3, -50));
			Assert.Equal(new Position(300, 100), pos1.GetRelative(pos3, -250));

			var up = MabiMath.DegreeToRadian(90);
			var down = MabiMath.DegreeToRadian(270);
			var left = MabiMath.DegreeToRadian(180);
			var right = MabiMath.DegreeToRadian(0);

			Assert.Equal(new Position(100, 200), pos1.GetRelative(up, 100));
			Assert.Equal(new Position(100, 0), pos1.GetRelative(down, 100));
			Assert.Equal(new Position(0, 100), pos1.GetRelative(left, 100));
			Assert.Equal(new Position(200, 100), pos1.GetRelative(right, 100));
		}

		[Fact]
		public void InPolygon()
		{
			var polygon = new[] { new Point(10, 10), new Point(20, 10), new Point(5, 20), new Point(15, 20) };

			Assert.Equal(false, new Position(6, 11).InPolygon(polygon));
			Assert.Equal(false, new Position(19, 19).InPolygon(polygon));
			Assert.Equal(true, new Position(17, 11).InPolygon(polygon));
			Assert.Equal(true, new Position(13, 19).InPolygon(polygon));
		}

		public void GetDirection()
		{
			var up = MabiMath.DegreeToRadian(90);
			var down = MabiMath.DegreeToRadian(270);
			var left = MabiMath.DegreeToRadian(180);
			var right = MabiMath.DegreeToRadian(0);

			var pos = new Position(100, 100);

			Assert.Equal(up, pos.GetDirection(new Position(100, 200)));
			Assert.Equal(down, pos.GetDirection(new Position(100, 0)));
			Assert.Equal(left, pos.GetDirection(new Position(0, 100)));
			Assert.Equal(right, pos.GetDirection(new Position(200, 100)));
		}
	}
}
