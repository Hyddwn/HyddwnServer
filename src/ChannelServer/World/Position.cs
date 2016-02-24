// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Drawing;

namespace Aura.Channel.World
{
	/// <summary>
	/// Describes the current position of an entity.
	/// </summary>
	public struct Position
	{
		public readonly int X;
		public readonly int Y;

		/// <summary>
		/// Returns position with X and Y being 0.
		/// </summary>
		public static Position Zero { get { return new Position(0, 0); } }

		public Position(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Position(Position pos)
		{
			this.X = pos.X;
			this.Y = pos.Y;
		}

		/// <summary>
		/// New Position based on location id (i.e. 0x3000RRRRXXXXYYYY).
		/// </summary>
		/// <param name="locationId"></param>
		public Position(long locationId)
		{
			this.X = (short)(locationId >> 16) * 20;
			this.Y = (short)(locationId >> 00) * 20;
		}

		/// <summary>
		/// Returns distance between this and another position.
		/// </summary>
		/// <param name="otherPos"></param>
		/// <returns></returns>
		public int GetDistance(Position otherPos)
		{
			return (int)Math.Sqrt(Math.Pow(X - otherPos.X, 2) + Math.Pow(Y - otherPos.Y, 2));
		}

		/// <summary>
		/// Returns true if the other position is within range.
		/// </summary>
		/// <param name="otherPos"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public bool InRange(Position otherPos, int range)
		{
			return this.InRange(otherPos.X, otherPos.Y, range);
		}

		/// <summary>
		/// Returns true if the other position is within range.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="range"></param>
		/// <returns></returns>
		public bool InRange(int x, int y, int range)
		{
			return (Math.Pow(X - x, 2) + Math.Pow(Y - y, 2) <= Math.Pow(range, 2));
		}

		/// <summary>
		/// Returns true if this position is in a cone, based on the parameters.
		/// </summary>
		/// <param name="tip">Tip of the cone.</param>
		/// <param name="direction">Cone's direction as radian.</param>
		/// <param name="radius">Cone's radius.</param>
		/// <param name="angle">Cone's angle as radian.</param>
		/// <returns></returns>
		public bool InCone(Position tip, double direction, int radius, double angle)
		{
			var halfAngle = angle / 2;

			var tx1 = tip.X + (Math.Cos(-halfAngle + direction) * radius);
			var ty1 = tip.Y + (Math.Sin(-halfAngle + direction) * radius);
			var tx2 = tip.X + (Math.Cos(halfAngle + direction) * radius);
			var ty2 = tip.Y + (Math.Sin(halfAngle + direction) * radius);
			var tx3 = tip.X;
			var ty3 = tip.Y;
			var px = X;
			var py = Y;

			// http://stackoverflow.com/questions/2049582/how-to-determine-a-point-in-a-2d-triangle
			var A = 1.0 / 2.0 * (-ty2 * tx3 + ty1 * (-tx2 + tx3) + tx1 * (ty2 - ty3) + tx2 * ty3);
			var sign = (A < 0 ? -1 : 1);
			var s = (ty1 * tx3 - tx1 * ty3 + (ty3 - ty1) * px + (tx1 - tx3) * py) * sign;
			var t = (tx1 * ty2 - ty1 * tx2 + (ty1 - ty2) * px + (tx2 - tx1) * py) * sign;

			return (s > 0 && t > 0 && (s + t) < 2 * A * sign);
		}

		/// <summary>
		/// Returns random position around this position,
		/// not nearer than min, and not further than max.
		/// </summary>
		/// <param name="distanceMax"></param>
		/// <param name="rnd"></param>
		/// <param name="distanceMin"></param>
		/// <returns></returns>
		public Position GetRandomInRange(int distanceMin, int distanceMax, Random rnd)
		{
			return this.GetRandom(rnd.Next(distanceMin, distanceMax + 1), rnd);
		}

		/// <summary>
		/// Returns random position in radius around this position.
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Position GetRandomInRange(int radius, Random rnd)
		{
			return this.GetRandom(rnd.Next(radius + 1), rnd);
		}

		/// <summary>
		/// Returns random position in radius around this position.
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="rnd"></param>
		/// <returns></returns>
		private Position GetRandom(int distance, Random rnd)
		{
			var angle = rnd.NextDouble() * Math.PI * 2;
			var x = this.X + distance * Math.Cos(angle);
			var y = this.Y + distance * Math.Sin(angle);

			return new Position((int)x, (int)y);
		}

		/// <summary>
		/// Returns random position in rect, centered on this position.
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Position GetRandomInRect(int width, int height, Random rnd)
		{
			var x = rnd.Next(this.X - width / 2, this.X + width / 2 + 1);
			var y = rnd.Next(this.Y - height / 2, this.Y + height / 2 + 1);

			return new Position((int)x, (int)y);
		}

		/// <summary>
		/// Returns position on the line between position and other.
		/// </summary>
		/// <remarks>
		/// When you knock someone back, he gets pushed in the opposite
		/// direction. The other position would be the enemy, the distance
		/// the amount how far to push him away. A negative distance will
		/// return a position between you two.
		/// </remarks>
		public Position GetRelative(Position other, int distance)
		{
			if (this == other)
				return this;

			var deltaX = (double)other.X - this.X;
			var deltaY = (double)other.Y - this.Y;

			var deltaXY = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

			var newX = other.X + (distance / deltaXY) * (deltaX);
			var newY = other.Y + (distance / deltaXY) * (deltaY);

			return new Position((int)newX, (int)newY);
		}

		/// <summary>
		/// Returns position in direction and distance.
		/// </summary>
		/// <param name="radian"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public Position GetRelative(double radian, int distance)
		{
			var deltaX = Math.Cos(radian);
			var deltaY = Math.Sin(radian);

			var deltaXY = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

			var newX = this.X + (distance / deltaXY) * (deltaX);
			var newY = this.Y + (distance / deltaXY) * (deltaY);

			return new Position((int)newX, (int)newY);
		}

		/// <summary>
		/// Returns whether the position is inside the given points.
		/// </summary>
		public bool InPolygon(params Point[] points)
		{
			var result = false;

			for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
			{
				if (((points[i].Y > this.Y) != (points[j].Y > this.Y)) && (this.X < (points[j].X - points[i].X) * (this.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
					result = !result;
			}

			return result;
		}

		public static bool operator ==(Position pos1, Position pos2)
		{
			return (pos1.X == pos2.X && pos1.Y == pos2.Y);
		}

		public static bool operator !=(Position pos1, Position pos2)
		{
			return !(pos1 == pos2);
		}

		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is Position && this == (Position)obj;
		}

		public override string ToString()
		{
			return "(Position: " + this.X + ", " + this.Y + ")";
		}
	}
}
