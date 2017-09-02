// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Aura.Mabi.Util;

namespace Aura.Login.Database
{
    public class Item
    {
        public ItemInfo Info;

        public Item()
        {
        }

        public Item(int itemId, Pocket pocket, uint color1, uint color2, uint color3)
        {
            Info.Id = itemId;
            Info.Pocket = pocket;
            Info.Color1 = color1;
            Info.Color2 = color2;
            Info.Color3 = color3;
        }

        public long Id { get; set; }

        /// <summary>
        ///     Returns whether the item is in an equipment pocket (Head, Equip, Style).
        /// </summary>
        public bool IsVisible
        {
            get
            {
                // Head
                if (Info.Pocket >= Pocket.Face && Info.Pocket <= Pocket.Hair)
                    return true;

                // Equipment
                if (Info.Pocket >= Pocket.Armor && Info.Pocket <= Pocket.Magazine2)
                    return true;

                // Style
                if (Info.Pocket >= Pocket.ArmorStyle && Info.Pocket <= Pocket.RobeStyle)
                    return true;

                if (Info.Pocket == Pocket.TailStyle)
                    return true;

                return false;
            }
        }

        /// <summary>
        ///     Returns list of items, based on CharCardSetInfo list.
        /// </summary>
        /// <param name="cardItems"></param>
        /// <returns></returns>
        public static List<Item> CardItemsToItems(IEnumerable<CharCardSetData> cardItems)
        {
            return cardItems.Select(cardItem => new Item(cardItem.Class, (Pocket) cardItem.Pocket, cardItem.Color1,
                cardItem.Color2, cardItem.Color3)).ToList();
        }

        /// <summary>
        ///     Changes item colors, using MTRandom and hash.
        /// </summary>
        /// <remarks>
        ///     The hash is converted to an int, which is used as the seed for
        ///     MTRandom, the RNG Mabi is using. That RNG is used to get specific
        ///     "random" colors from the color map db.
        ///     Used to generate "random" colors on the equipment of
        ///     new characters and partners.
        /// </remarks>
        public static void GenerateItemColors(ref List<Item> items, string hash)
        {
            var ihash = hash.Aggregate(5381, (current, ch) => current * 33 + (int) ch);

            var rnd = new MTRandom(ihash);
            foreach (var item in items.Where(a => a.Info.Pocket != Pocket.Face && a.Info.Pocket != Pocket.Hair))
            {
                var dataInfo = AuraData.ItemDb.Find(item.Info.Id);
                if (dataInfo == null)
                    continue;

                item.Info.Color1 = item.Info.Color1 != 0
                    ? item.Info.Color1
                    : AuraData.ColorMapDb.GetRandom(dataInfo.ColorMap1, rnd);
                item.Info.Color2 = item.Info.Color2 != 0
                    ? item.Info.Color2
                    : AuraData.ColorMapDb.GetRandom(dataInfo.ColorMap2, rnd);
                item.Info.Color3 = item.Info.Color3 != 0
                    ? item.Info.Color3
                    : AuraData.ColorMapDb.GetRandom(dataInfo.ColorMap3, rnd);
            }
        }
    }
}