// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Mabi.Const;

namespace Aura.Login.Database
{
    public class Character
    {
        public Character()
        {
            Height = 1;
            Weight = 1;
            Upper = 1;
            Lower = 1;

            Life = 10;
            Mana = 10;
            Stamina = 100;
            Str = 10;
            Int = 10;
            Dex = 10;
            Will = 10;
            Luck = 10;
        }

        public long EntityId { get; set; }
        public long CreatureId { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }

        public int Race { get; set; }
        public int Face { get; set; }
        public byte SkinColor { get; set; }
        public int Hair { get; set; }
        public int HairColor { get; set; }
        public short EyeType { get; set; }
        public byte EyeColor { get; set; }
        public byte MouthType { get; set; }
        public CreatureStates State { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public float Upper { get; set; }
        public float Lower { get; set; }
        public uint Color1 { get; set; }
        public uint Color2 { get; set; }
        public uint Color3 { get; set; }
        public byte Age { get; set; }

        public int Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public short AP { get; set; }
        public float Life { get; set; }
        public float Mana { get; set; }
        public float Stamina { get; set; }
        public float Str { get; set; }
        public float Int { get; set; }
        public float Dex { get; set; }
        public float Will { get; set; }
        public float Luck { get; set; }
        public short Defense { get; set; }
        public float Protection { get; set; }

        /// <summary>
        ///     Time at which the character may be deleted for good.
        /// </summary>
        /// <remarks>
        ///     If MinValue, the character is normal.
        ///     If MaxValue, it's "gone".
        ///     If it's above Now the character can be recovered.
        ///     If it's below Now, the character can be deleted.
        /// </remarks>
        public DateTime DeletionTime { get; set; }

        /// <summary>
        ///     Deletion state of the character, based on DeletionTime.
        ///     0: Normal, 1: Recoverable, 2: DeleteReady, 3: ToBeDeleted
        /// </summary>
        public DeletionFlag DeletionFlag
        {
            get
            {
                if (DeletionTime == DateTime.MaxValue)
                    return DeletionFlag.Delete;
                if (DeletionTime <= DateTime.MinValue)
                    return DeletionFlag.Normal;
                if (DeletionTime >= DateTime.Now)
                    return DeletionFlag.Recover;
                return DeletionFlag.Ready;
            }
        }
    }
}