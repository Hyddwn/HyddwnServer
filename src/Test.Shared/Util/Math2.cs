// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Xunit;

namespace Aura.Tests.Shared
{
	public class Math2Tests
	{
		[Fact]
		public void MultiplyCheckedShort()
		{
			// Positive
			Assert.Equal(10000, Math2.MultiplyChecked((short)5000, 2));
			Assert.Equal(short.MaxValue, Math2.MultiplyChecked((short)20000, 2));

			// Negative
			Assert.Equal(-10000, Math2.MultiplyChecked((short)-5000, 2));
			Assert.Equal(short.MinValue, Math2.MultiplyChecked((short)-20000, 2));
		}

		[Fact]
		public void MultiplyCheckedInt()
		{
			// Positive
			Assert.Equal(100000, Math2.MultiplyChecked(50000, 2));
			Assert.Equal(2000000000, Math2.MultiplyChecked(1000000000, 2));
			Assert.Equal(int.MaxValue, Math2.MultiplyChecked(2000000000, 2));

			// Negative
			Assert.Equal(-100000, Math2.MultiplyChecked(-50000, 2));
			Assert.Equal(-2000000000, Math2.MultiplyChecked(-1000000000, 2));
			Assert.Equal(int.MinValue, Math2.MultiplyChecked(-2000000000, 2));
		}

		[Fact]
		public void MultiplyCheckedLong()
		{
			// Positive
			Assert.Equal(100000L, Math2.MultiplyChecked(50000L, 2));
			Assert.Equal(2000000000L, Math2.MultiplyChecked(1000000000L, 2));
			Assert.Equal(4000000000L, Math2.MultiplyChecked(2000000000L, 2));
			Assert.Equal(long.MaxValue, Math2.MultiplyChecked(5000000000000000000, 2));

			// Negative
			Assert.Equal(-100000L, Math2.MultiplyChecked(-50000, 2));
			Assert.Equal(-2000000000L, Math2.MultiplyChecked(-1000000000L, 2));
			Assert.Equal(-4000000000L, Math2.MultiplyChecked(-2000000000L, 2));
			Assert.Equal(long.MinValue, Math2.MultiplyChecked(-5000000000000000000, 2));
		}
	}
}
