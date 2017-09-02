// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aura.Tests.Channel.World.Entities
{
	public class ItemTests
	{
		static ItemTests()
		{
			AuraData.ItemDb.Load("../../../../system/db/items.txt", true);
		}

		[Fact]
		public void GetCollectionList()
		{
			var item = new Item(60100); // Tir Chonaill Merchant License
			Assert.Throws<InvalidOperationException>(() => item.GetCollectionList());

			item = new Item(1213); // Nao's Gift Collection
			Assert.Equal("".PadLeft(24, '0'), new string(item.GetCollectionList()));
		}

		[Fact]
		public void SetCollectionList()
		{
			var item = new Item(60100); // Tir Chonaill Merchant License
			Assert.Throws<InvalidOperationException>(() => item.SetCollectionList("00000000".ToCharArray()));

			item = new Item(1213); // Nao's Gift Collection
			Assert.Throws<ArgumentException>(() => item.SetCollectionList("0000".ToCharArray()));
			Assert.Throws<ArgumentNullException>(() => item.SetCollectionList(null));

			item.SetCollectionList("10000000".ToCharArray());
			Assert.Equal(new byte[] { 0x80 }, item.MetaData1.GetBin("COLLIST"));
			Assert.Equal("10000000", new string(item.GetCollectionList()));

			item.SetCollectionList("10101010".ToCharArray());
			Assert.Equal(new byte[] { 0xAA }, item.MetaData1.GetBin("COLLIST"));
			Assert.Equal("10101010", new string(item.GetCollectionList()));

			item.SetCollectionList("1111111101000000".ToCharArray());
			Assert.Equal(new byte[] { 0xFF, 0x40 }, item.MetaData1.GetBin("COLLIST"));
			Assert.Equal("1111111101000000", new string(item.GetCollectionList()));
		}

		[Fact]
		public void ModifyCollectionList()
		{
			var item = new Item(1213); // Nao's Gift Collection
			item.SetCollectionList("00000000".ToCharArray());
			Assert.Equal(new byte[] { 0x00 }, item.MetaData1.GetBin("COLLIST"));
			Assert.Equal("00000000", new string(item.GetCollectionList()));

			var list = item.GetCollectionList();
			list[0] = '1';
			list[2] = '1';
			item.SetCollectionList(list);
			Assert.Equal(new byte[] { 0xA0 }, item.MetaData1.GetBin("COLLIST"));
			Assert.Equal("10100000", new string(item.GetCollectionList()));
		}
	}
}
