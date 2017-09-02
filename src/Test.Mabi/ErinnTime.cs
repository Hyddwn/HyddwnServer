// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Aura.Tests.Mabi
{
	public class ErinnTimeTests
	{
		[Fact]
		public void ErinnTimeCalculation()
		{
			var time1 = new ErinnTime(DateTime.Parse("2015-01-01 00:00"));
			Assert.Equal(550, time1.Year);
			Assert.Equal(5, time1.Month);
			Assert.Equal(1, time1.Day);
			Assert.Equal(0, time1.Hour);
			Assert.Equal(0, time1.Minute);

			var time2 = new ErinnTime(DateTime.Parse("2015-01-01 23:59"));
			Assert.Equal(550, time2.Year);
			Assert.Equal(5, time2.Month);
			Assert.Equal(40, time2.Day);
			Assert.Equal(23, time2.Hour);
			Assert.Equal(20, time2.Minute);

			var time3 = new ErinnTime(ErinnTime.BeginOfTime);
			Assert.Equal(1, time3.Year);
			Assert.Equal(1, time3.Month);
			Assert.Equal(1, time3.Day);
			Assert.Equal(0, time3.Hour);
			Assert.Equal(0, time3.Minute);

			var dt4 = DateTime.Parse("2016-06-26 00:00");
			for (int i = 0; i < 10; ++i)
			{
				var time = new ErinnTime(dt4.AddDays(i * 7));
				Assert.Equal(628 + i * 1, time.Year);
				Assert.Equal(1, time.Month);
				Assert.Equal(1, time.Day);
				Assert.Equal(0, time.Hour);
				Assert.Equal(0, time.Minute);
			}

			var dt5 = DateTime.Parse("2016-06-26 00:00");
			for (int i = 0; i < 7; ++i)
			{
				var time = new ErinnTime(dt5.AddDays(i));
				Assert.Equal(628, time.Year);
				Assert.Equal(i + 1, time.Month);
				Assert.Equal(1, time.Day);
				Assert.Equal(0, time.Hour);
				Assert.Equal(0, time.Minute);
			}

			var dt6 = DateTime.Parse("2016-06-26 00:00");
			for (int i = 0; i < 24; ++i)
			{
				var time = new ErinnTime(dt6.AddMilliseconds(i * (24 * 60 * 1500)));
				Assert.Equal(628, time.Year);
				Assert.Equal(1, time.Month);
				Assert.Equal(i + 1, time.Day);
				Assert.Equal(0, time.Hour);
				Assert.Equal(0, time.Minute);
			}

			var dt7 = DateTime.Parse("2016-06-26 00:00");
			for (int i = 0; i < 24; ++i)
			{
				for (int j = 0; j < 60; ++j)
				{
					var time = new ErinnTime(dt6.AddMilliseconds((i * (60 * 1500)) + (j * 1500)));
					Assert.Equal(628, time.Year);
					Assert.Equal(1, time.Month);
					Assert.Equal(1, time.Day);
					Assert.Equal(i, time.Hour);
					Assert.Equal(j, time.Minute);
				}
			}
		}

		[Fact]
		public void ErinnTimeToString()
		{
			var time1 = new ErinnTime(DateTime.Parse("2015-01-01 00:00"));

			Assert.Equal("550-5-01 00:00", time1.ToString());
			Assert.Equal("0550-05-01 0:0", time1.ToString("yyyy-MM-dd H:m"));
			Assert.Equal("00:00 AM", time1.ToString("HH:mm tt"));

			Assert.Equal("01 Lughnasadh 550", time1.ToString("dd MMMM yyy"));
			ErinnTime.SetMonthNames("0", "1", "2", "3", "Foobar", "5", "6");
			Assert.Equal("01 Foobar 550", time1.ToString("dd MMMM yyy"));

			var time2 = new ErinnTime(DateTime.Parse("2015-01-01 23:59"));

			Assert.Equal("23:20 PM", time2.ToString("HH:mm tt"));

			var dt3 = DateTime.Parse("2016-06-26 00:00");
			for (int year = 0; year < 2; ++year)
			{
				for (int month = 0; month < 7; ++month)
				{
					for (int day = 0; day < 40; ++day)
					{
						for (int hour = 0; hour < 24; ++hour)
						{
							for (int minute = 0; minute < 60; ++minute)
							{
								var time = new ErinnTime(dt3.AddMilliseconds((year * (7 * 40 * 24 * 60 * 1500)) + (month * (40 * 24 * 60 * 1500)) + (day * (24 * 60 * 1500)) + (hour * (60 * 1500)) + (minute * 1500)));
								Assert.Equal(string.Format("{0}-{1}-{2:00} {3:00}:{4:00}", 628 + year, month + 1, day + 1, hour, minute), time.ToString());
							}
						}
					}
				}
			}
		}

		[Fact]
		public void BeginOfTimeCap()
		{
			var time1 = new ErinnTime(ErinnTime.BeginOfTime);
			Assert.Equal("1-1-01 00:00", time1.ToString());

			var time2 = new ErinnTime(DateTime.MinValue);
			Assert.Equal("1-1-01 00:00", time2.ToString());

			var time3 = new ErinnTime(new DateTime(2000, 1, 1));
			Assert.Equal("1-1-01 00:00", time3.ToString());

			var time4 = new ErinnTime(DateTime.MaxValue);
			Assert.Equal("417187-6-40 23:59", time4.ToString());
		}

		[Fact]
		public void GetNextTime()
		{
			// 12 hours
			{
				var now = DateTime.Parse("2016-01-01 00:00:00");
				var then = DateTime.Parse("2016-01-01 00:18:00");
				var time = ErinnTime.GetNextTime(now, 12, 0);
				Assert.Equal(then, time.DateTime);
			}

			// 24 hours
			{
				var now = DateTime.Parse("2016-01-01 00:00:00");
				var then = DateTime.Parse("2016-01-01 00:36:00");
				var time = ErinnTime.GetNextTime(now, 0, 0);
				Assert.Equal(then, time.DateTime);
			}

			// Hours
			for (int i = 1; i < 24; ++i)
			{
				var now = DateTime.Parse("2016-01-01 00:00:00");
				var then = now.AddSeconds(i * 90);
				var time = ErinnTime.GetNextTime(now, i, 0);
				Assert.Equal(then, time.DateTime);
			}

			// Minutes
			for (int i = 0; i < 60; ++i)
			{
				var now = DateTime.Parse("2016-01-01 00:00:00");
				var then = now.AddSeconds(90).AddMilliseconds(i * 1500);
				var time = ErinnTime.GetNextTime(now, 1, i);
				Assert.Equal(then, time.DateTime);
			}

			// 23 hours, rollover
			{
				var now = DateTime.Parse("2016-01-01 00:04:30");
				var then = now.AddSeconds(23 * 90);
				var time = ErinnTime.GetNextTime(now, 2, 0);
				Assert.Equal(then, time.DateTime);
			}

			// Real world 1
			{
				var now = DateTime.Parse("2016-12-02 16:32");
				var then = DateTime.Parse("2016-12-02 16:40:30");
				var time = ErinnTime.GetNextTime(now, 19, 0);
				Assert.Equal(then, time.DateTime);
			}

			// Real world 2
			{
				var now = DateTime.Parse("2016-12-02 11:17");
				var then = DateTime.Parse("2016-12-02 11:52:30");
				var time = ErinnTime.GetNextTime(now, 19, 0);
				Assert.Equal(then, time.DateTime);
			}

			// 2 hours based on now (one random test, just in case)
			{
				var erinnNow = ErinnTime.Now;
				var now = erinnNow.DateTime;
				var then = now.AddSeconds(2 * 90);
				var time = ErinnTime.GetNextTime(now, (erinnNow.Hour + 2) % 24, erinnNow.Minute);
				Assert.Equal(then, time.DateTime);
			}
		}
	}
}
