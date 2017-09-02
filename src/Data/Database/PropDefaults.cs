// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.IO;

namespace Aura.Data.Database
{
    public class PropDefaultData
    {
        public PropDefaultData()
        {
            Shapes = new List<ShapeData>();
        }

        public int Id { get; set; }
        public int Order { get; set; }
        public float Direction { get; set; }
        public float Scale { get; set; }
        public string State { get; set; }
        public List<ShapeData> Shapes { get; set; }
    }

    public class PropDefaultsDb : DatabaseDatIndexed<int, List<PropDefaultData>>
    {
        protected override void Read(BinaryReader br)
        {
            var count = br.ReadInt32();
            for (var i = 0; i < count; ++i)
            {
                var prop = new PropDefaultData();

                prop.Id = br.ReadInt32();
                prop.Order = br.ReadInt32();
                var shapeCount = br.ReadInt32();
                for (var j = 0; j < shapeCount; ++j)
                {
                    var shape = new ShapeData();
                    shape.DirX1 = br.ReadSingle();
                    shape.DirX2 = br.ReadSingle();
                    shape.DirY1 = br.ReadSingle();
                    shape.DirY2 = br.ReadSingle();
                    shape.LenX = br.ReadSingle();
                    shape.LenY = br.ReadSingle();
                    shape.PosX = br.ReadSingle();
                    shape.PosY = br.ReadSingle();

                    prop.Shapes.Add(shape);
                }

                var isCollision = br.ReadByte();
                var isFixedAltitude = br.ReadByte();
                prop.Scale = br.ReadSingle();
                prop.Direction = br.ReadSingle();
                prop.State = br.ReadString();

                if (!Entries.ContainsKey(prop.Id))
                    Entries[prop.Id] = new List<PropDefaultData>();

                Entries[prop.Id].Add(prop);
            }
        }
    }
}