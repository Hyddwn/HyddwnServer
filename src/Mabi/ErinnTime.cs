// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Mabi
{
	/// <summary>
	/// Wrapper around DateTime, to calculate the current time in Erinn.
	/// </summary>
	[Serializable]
	public class ErinnTime
	{
		/// <summary>
		/// 1,500ms (1.5 seconds)
		/// </summary>
		public const long TicksPerMinute = 15000000;
		/// <summary>
		/// 90,000ms (1.5 minutes)
		/// </summary>
		public const long TicksPerHour = TicksPerMinute * 60;

		/// <summary>
		/// Erinn months, starting on Imbolic (Sunday).
		/// </summary>
		protected static string[] _months = new string[] { "Imbolic", "Alban Eiler", "Baltane", "Alban Heruin", "Lughnasadh", "Alban Elved", "Samhain" };

		/// <summary>
		/// Release of KR.
		/// </summary>
		public static readonly DateTime BeginOfTime = DateTime.Parse("2004-06-20");

		/// <summary>
		/// Erinn hour of this instance.
		/// </summary>
		public int Hour { get; protected set; }
		/// <summary>
		/// Erinn minute of this instance.
		/// </summary>
		public int Minute { get; protected set; }
		/// <summary>
		/// Erinn year of this instance.
		/// </summary>
		public int Year { get; protected set; }
		/// <summary>
		/// Erinn month of this instance.
		/// </summary>
		public int Month { get; protected set; }
		/// <summary>
		/// Erinn day of this instance.
		/// </summary>
		public int Day { get; protected set; }

		/// <summary>
		/// DateTime object used by this instance.
		/// </summary>
		public DateTime DateTime { get; protected set; }

		/// <summary>
		/// Time stamp for this Erinn date (Format: yyyymdd).
		/// </summary>
		public int DateTimeStamp { get { return (this.Year * 1000 + this.Month * 100 + this.Day); } }

		/// <summary>
		/// Returns a new MabiTime instance based on the current time.
		/// </summary>
		public static ErinnTime Now { get { return new ErinnTime(); } }

		/// <summary>
		/// Returns true if the Erinn hour of this instance is between 6:00pm and 5:59am.
		/// </summary>
		public bool IsNight { get { return (this.Hour >= 18 || this.Hour < 6); } }

		/// <summary>
		/// Returns true if it's not night, duh.
		/// </summary>
		public bool IsDay { get { return !this.IsNight; ; } }

		/// <summary>
		/// Returns true if time of this instance is 0:00am.
		/// </summary>
		public bool IsMidnight { get { return (this.Hour == 0 && this.Minute == 0); } }

		/// <summary>
		/// Returns true if time of this instance is 6:00am.
		/// </summary>
		public bool IsDawn { get { return (this.Hour == 6 && this.Minute == 0); } }

		/// <summary>
		/// Returns true if time of this instance is 6:00pm.
		/// </summary>
		public bool IsDusk { get { return (this.Hour == 18 && this.Minute == 0); } }

		/// <summary>
		/// Creates new instance, based on DateTime.Now.
		/// </summary>
		public ErinnTime() : this(DateTime.Now) { }

		/// <summary>
		/// Creates new instance, based on given DateTime.
		/// </summary>
		/// <param name="dt"></param>
		public ErinnTime(DateTime dt)
		{
			this.DateTime = (dt < BeginOfTime ? BeginOfTime : dt);
			this.Hour = (int)((this.DateTime.Ticks / TicksPerHour) % 24);
			this.Minute = (int)((this.DateTime.Ticks / TicksPerMinute) % 60);

			// Based on the theory that 1 year (1 week realtime) consists of
			// 7 months (7 days) with 40 days (1440 / 36 min) each.
			this.Year = (int)Math.Floor((this.DateTime.Ticks - BeginOfTime.Ticks) / TicksPerMinute / 60 / 24 / 280f) + 1;
			this.Month = (int)this.DateTime.DayOfWeek + 1;
			this.Day = (int)Math.Floor((this.DateTime.Hour * 60 + this.DateTime.Minute) / 36f) + 1;
		}

		/// <summary>
		/// Returns the DateTime for last Saturday at 12:00.
		/// </summary>
		/// <returns></returns>
		public DateTime GetLastSaturday()
		{
			var lastSaturday = DateTime.MinValue;

			if (this.DateTime.DayOfWeek == DayOfWeek.Saturday)
				lastSaturday = (this.DateTime.Hour < 12) ? this.DateTime.AddDays(-7) : this.DateTime;
			else
				lastSaturday = this.DateTime.AddDays(-(int)this.DateTime.DayOfWeek - 1);

			lastSaturday = lastSaturday.Date.AddHours(12);

			return lastSaturday;
		}

		/// <summary>
		/// Returns a string with the Erinn time of this instance in AM/PM.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.ToString("y-M-dd HH:mm");
		}

		/// <summary>
		/// Returns a string with the Erinn time of this instance.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format)
		{
			var h12 = this.Hour % 12;
			if (this.Hour == 12)
				h12 = 12;

			format = format.Replace("ampm", (h12.ToString("00") + ":" + this.Minute.ToString("00") + (this.Hour < 12 ? " A.M." : " P.M.")));

			format = format.Replace("hh", h12.ToString("00"));
			format = format.Replace("h", h12.ToString());

			format = format.Replace("HH", this.Hour.ToString("00"));
			format = format.Replace("H", this.Hour.ToString());

			format = format.Replace("mm", this.Minute.ToString("00"));
			format = format.Replace("m", this.Minute.ToString());

			format = format.Replace("yyyy", this.Year.ToString("0000"));
			format = format.Replace("yyy", this.Year.ToString("000"));
			format = format.Replace("yy", this.Year.ToString("00"));
			format = format.Replace("y", this.Year.ToString("0"));

			format = format.Replace("dd", this.Day.ToString("00"));
			format = format.Replace("d", this.Day.ToString());

			format = format.Replace("MMMM", _months[this.Month - 1]);
			format = format.Replace("MM", this.Month.ToString("00"));
			format = format.Replace("M", this.Month.ToString());

			format = format.Replace("tt", (this.Hour < 12 ? "AM" : "PM"));
			format = format.Replace("t", (this.Hour < 12 ? "A" : "P"));

			return format;
		}

		/// <summary>
		/// Sets month names used in formatting for all instances of ErinnTime.
		/// </summary>
		/// <param name="imbolic">Sunday</param>
		/// <param name="albanEiler">Monday</param>
		/// <param name="baltane">Tuesday</param>
		/// <param name="albanHeruin">Wednesday</param>
		/// <param name="lughnasadh">Thursday</param>
		/// <param name="albanElved">Friday</param>
		/// <param name="samhain">Saturday</param>
		public static void SetMonthNames(string imbolic, string albanEiler, string baltane, string albanHeruin, string lughnasadh, string albanElved, string samhain)
		{
			_months[0] = imbolic;
			_months[1] = albanEiler;
			_months[2] = baltane;
			_months[3] = albanHeruin;
			_months[4] = lughnasadh;
			_months[5] = albanElved;
			_months[6] = samhain;
		}
	}

	public static class ErinnMonth
	{
		/// <summary>
		/// Sunday
		/// </summary>
		public const int Imbolic = 1;

		/// <summary>
		/// Monday
		/// </summary>
		public const int AlbanEiler = 2;

		/// <summary>
		/// Tuesday
		/// </summary>
		public const int Baltane = 3;

		/// <summary>
		/// Wednesday
		/// </summary>
		public const int AlbanHeruin = 4;

		/// <summary>
		/// Thursday
		/// </summary>
		public const int Lughnasadh = 5;

		/// <summary>
		/// Friday
		/// </summary>
		public const int AlbanElved = 6;

		/// <summary>
		/// Saturday
		/// </summary>
		public const int Samhain = 7;
	}
}
