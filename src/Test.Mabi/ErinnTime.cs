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
			Assert.Equal(4, time1.Month);
			Assert.Equal(1, time1.Day);
			Assert.Equal(0, time1.Hour);
			Assert.Equal(0, time1.Minute);

			var time2 = new ErinnTime(DateTime.Parse("2015-01-01 23:59"));
			Assert.Equal(550, time2.Year);
			Assert.Equal(4, time2.Month);
			Assert.Equal(40, time2.Day);
			Assert.Equal(23, time2.Hour);
			Assert.Equal(20, time2.Minute);

			var time3 = new ErinnTime(ErinnTime.BeginOfTime);
			Assert.Equal(1, time3.Year);
			Assert.Equal(2, time3.Month);
			Assert.Equal(1, time3.Day);
			Assert.Equal(0, time3.Hour);
			Assert.Equal(0, time3.Minute);
		}

		[Fact]
		public void ErinnTimeToString()
		{
			var time1 = new ErinnTime(DateTime.Parse("2015-01-01 00:00"));
			
			Assert.Equal("550-4-01 00:00", time1.ToString());
			Assert.Equal("0550-04-01 0:0", time1.ToString("yyyy-MM-dd H:m"));
			Assert.Equal("00:00 AM", time1.ToString("HH:mm tt"));
			
			var time2 = new ErinnTime(DateTime.Parse("2015-01-01 23:59"));

			Assert.Equal("23:20 PM", time2.ToString("HH:mm tt"));
		}
	}
}
