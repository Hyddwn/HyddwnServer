// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Mabi.Const;

namespace Aura.Channel.World
{
    public class EgoInfo
    {
        /// <summary>
        ///     Creates and initializes new EgoInfo.
        /// </summary>
        public EgoInfo()
        {
            StrLevel = 1;
            IntLevel = 1;
            DexLevel = 1;
            WillLevel = 1;
            LuckLevel = 1;
            SocialLevel = 1;
            LastFeeding = DateTime.Now;
        }

        /// <summary>
        ///     Ego's race, displayed in stat window.
        /// </summary>
        public EgoRace Race { get; set; }

        /// <summary>
        ///     Ego's name, displayed in stat window.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Ego's strength level
        /// </summary>
        public byte StrLevel { get; set; }

        /// <summary>
        ///     Ego's strength exp
        /// </summary>
        public int StrExp { get; set; }

        /// <summary>
        ///     Ego's intelligence level
        /// </summary>
        public byte IntLevel { get; set; }

        /// <summary>
        ///     Ego's intelligence exp
        /// </summary>
        public int IntExp { get; set; }

        /// <summary>
        ///     Ego's dexterity level
        /// </summary>
        public byte DexLevel { get; set; }

        /// <summary>
        ///     Ego's dexterity exp
        /// </summary>
        public int DexExp { get; set; }

        /// <summary>
        ///     Ego's will level
        /// </summary>
        public byte WillLevel { get; set; }

        /// <summary>
        ///     Ego's will exp
        /// </summary>
        public int WillExp { get; set; }

        /// <summary>
        ///     Ego's luck level
        /// </summary>
        public byte LuckLevel { get; set; }

        /// <summary>
        ///     Ego's luck exp
        /// </summary>
        public int LuckExp { get; set; }

        /// <summary>
        ///     Ego's social level
        /// </summary>
        public byte SocialLevel { get; set; }

        /// <summary>
        ///     Ego's social exp
        /// </summary>
        public int SocialExp { get; set; }

        /// <summary>
        ///     Awakening energy counter
        /// </summary>
        public byte AwakeningEnergy { get; set; }

        /// <summary>
        ///     Awakening exp
        /// </summary>
        public int AwakeningExp { get; set; }

        /// <summary>
        ///     Time the ego was fed last
        /// </summary>
        public DateTime LastFeeding { get; set; }

        /// <summary>
        ///     Creates a copy of this object.
        /// </summary>
        /// <returns></returns>
        public EgoInfo Copy()
        {
            var result = new EgoInfo();

            result.Name = Name;
            result.Race = Race;
            result.StrLevel = StrLevel;
            result.StrExp = StrExp;
            result.IntLevel = IntLevel;
            result.IntExp = IntExp;
            result.DexLevel = DexLevel;
            result.DexExp = DexExp;
            result.WillLevel = WillLevel;
            result.WillExp = WillExp;
            result.LuckLevel = LuckLevel;
            result.LuckExp = LuckExp;
            result.SocialLevel = SocialLevel;
            result.SocialExp = SocialExp;
            result.AwakeningEnergy = AwakeningEnergy;
            result.AwakeningExp = AwakeningExp;
            result.LastFeeding = LastFeeding;

            return result;
        }
    }
}