// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Aura.Mabi.Const;

namespace Aura.Data.Database
{
    public class RegionInfoData
    {
        public RegionInfoData()
        {
            Areas = new List<AreaData>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public List<AreaData> Areas { get; set; }

        /// <summary>
        ///     Returns area with given name or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AreaData GetArea(string name)
        {
            return Areas.FirstOrDefault(a => a.Name == name);
        }

        /// <summary>
        ///     Returns event by id or null if it doesn't exist.
        /// </summary>
        /// <returns></returns>
        public EventData GetEvent(long eventId)
        {
            foreach (var area in Areas)
                if (area.Events.ContainsKey(eventId))
                    return area.Events[eventId];

            return null;
        }

        /// <summary>
        ///     Returns index of the area in the list.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public int GetAreaIndex(int areaId)
        {
            var id = 1;
            foreach (var area in Areas)
            {
                if (area.Id == areaId)
                    return id;

                id++;
            }

            return -1;
        }

        /// <summary>
        ///     Returns random coordinates inside the actual region.
        /// </summary>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public Point RandomCoord(Random rnd)
        {
            var result = new Point();
            result.X = rnd.Next(X1, X2);
            result.Y = rnd.Next(Y1, Y2);

            return result;
        }

        /// <summary>
        ///     Creates copy of this region data.
        /// </summary>
        /// <returns></returns>
        public RegionInfoData Copy()
        {
            var result = new RegionInfoData();
            result.Id = Id;
            result.Name = Name;
            result.GroupId = GroupId;
            result.X1 = X1;
            result.Y1 = Y1;
            result.X2 = X2;
            result.Y2 = Y2;

            foreach (var area in Areas)
                result.Areas.Add(area.Copy(true, true));

            return result;
        }
    }

    public class AreaData
    {
        public AreaData()
        {
            Props = new Dictionary<long, PropData>();
            Events = new Dictionary<long, EventData>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public Dictionary<long, PropData> Props { get; set; }
        public Dictionary<long, EventData> Events { get; set; }

        /// <summary>
        ///     Creates a copy of the area data.
        /// </summary>
        /// <param name="copyProps"></param>
        /// <param name="copyEvents"></param>
        /// <returns></returns>
        public AreaData Copy(bool copyProps, bool copyEvents)
        {
            var result = new AreaData();
            result.Id = Id;
            result.Name = Name;
            result.X1 = X1;
            result.Y1 = Y1;
            result.X2 = X2;
            result.Y2 = Y2;
            result.Props = new Dictionary<long, PropData>();
            result.Events = new Dictionary<long, EventData>();

            if (copyProps)
                foreach (var original in Props.Values)
                {
                    var item = original.Copy();
                    result.Props.Add(item.EntityId, item);
                }

            if (copyEvents)
                foreach (var original in Events.Values)
                {
                    var item = original.Copy();
                    result.Events.Add(item.Id, item);
                }

            return result;
        }

        /// <summary>
        ///     Returns prop with given name or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PropData GetProp(string name)
        {
            return Props.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        ///     Returns event with given name or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EventData GetEvent(string name)
        {
            return Events.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
        }
    }

    public class PropData
    {
        public PropData()
        {
            Shapes = new List<ShapeData>();
            Parameters = new List<RegionElementData>();
        }

        public long EntityId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Direction { get; set; }
        public float Scale { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public List<ShapeData> Shapes { get; set; }
        public List<RegionElementData> Parameters { get; set; }

        /// <summary>
        ///     Returns drop type, if one exists, or -1.
        /// </summary>
        /// <returns></returns>
        public int GetDropType()
        {
            foreach (var param in Parameters)
            {
                // TODO: Event or SignalType can probably be checked as
                //   well for finding drop props.
                if (param.XML == null || param.XML.Attribute("droptype") == null)
                    continue;

                return int.Parse(param.XML.Attribute("droptype").Value);
            }

            return -1;
        }

        public PropData Copy()
        {
            var result = new PropData();
            result.EntityId = EntityId;
            result.Id = Id;
            result.Name = Name;
            result.X = X;
            result.Y = Y;
            result.Direction = Direction;
            result.Scale = Scale;

            result.Shapes = new List<ShapeData>(Shapes.Count);
            foreach (var item in Shapes)
                result.Shapes.Add(item.Copy());

            result.Parameters = new List<RegionElementData>(Parameters.Count);
            foreach (var item in Parameters)
                result.Parameters.Add(item.Copy());

            return result;
        }
    }

    public class ShapeData
    {
        public float DirX1 { get; set; }
        public float DirX2 { get; set; }
        public float DirY1 { get; set; }
        public float DirY2 { get; set; }
        public float LenX { get; set; }
        public float LenY { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }

        public Point[] GetPoints(float radianAngle, int pivotX, int pivotY)
        {
            var points = new Point[4];

            double a00 = DirX1 * LenX;
            double a01 = DirX2 * LenX;
            double a02 = DirY1 * LenY;
            double a03 = DirY2 * LenY;

            var sx1 = PosX - a00 - a02;
            if (sx1 < PosX) sx1 = Math.Ceiling(sx1);
            var sy1 = PosY - a01 - a03;
            if (sy1 < PosY) sy1 = Math.Ceiling(sy1);
            var sx2 = PosX + a00 - a02;
            if (sx2 < PosX) sx2 = Math.Ceiling(sx2);
            var sy2 = PosY + a01 - a03;
            if (sy2 < PosY) sy2 = Math.Ceiling(sy2);
            var sx3 = PosX + a00 + a02;
            if (sx3 < PosX) sx3 = Math.Ceiling(sx3);
            var sy3 = PosY + a01 + a03;
            if (sy3 < PosY) sy3 = Math.Ceiling(sy3);
            var sx4 = PosX - a00 + a02;
            if (sx4 < PosX) sx4 = Math.Ceiling(sx4);
            var sy4 = PosY - a01 + a03;
            if (sy4 < PosY) sy4 = Math.Ceiling(sy4);

            if (a02 * a01 > a03 * a00)
            {
                points[0] = new Point((int) sx1, (int) sy1);
                points[1] = new Point((int) sx2, (int) sy2);
                points[2] = new Point((int) sx3, (int) sy3);
                points[3] = new Point((int) sx4, (int) sy4);
            }
            else
            {
                points[0] = new Point((int) sx1, (int) sy1);
                points[3] = new Point((int) sx2, (int) sy2);
                points[2] = new Point((int) sx3, (int) sy3);
                points[1] = new Point((int) sx4, (int) sy4);
            }

            var cosTheta = Math.Cos(radianAngle);
            var sinTheta = Math.Sin(radianAngle);

            for (var i = 0; i < points.Length; ++i)
            {
                var x = (int) (cosTheta * points[i].X - sinTheta * points[i].Y + pivotX);
                var y = (int) (sinTheta * points[i].X + cosTheta * points[i].Y + pivotY);
                points[i].X = x;
                points[i].Y = y;
            }

            return points;
        }

        public ShapeData Copy()
        {
            var result = new ShapeData();
            result.DirX1 = DirX1;
            result.DirX2 = DirX2;
            result.DirY1 = DirY1;
            result.DirY2 = DirY2;
            result.LenX = LenX;
            result.LenY = LenY;
            result.PosX = PosX;
            result.PosY = PosY;

            return result;
        }
    }

    public class EventData
    {
        public EventData()
        {
            Shapes = new List<ShapeData>();
            Parameters = new List<RegionElementData>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public EventType Type { get; set; }
        public int RegionId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool IsAltar { get; set; }
        public List<ShapeData> Shapes { get; set; }
        public List<RegionElementData> Parameters { get; set; }

        public EventData Copy()
        {
            var result = new EventData();
            result.Id = Id;
            result.Name = Name;
            result.Type = Type;
            result.RegionId = RegionId;
            result.X = X;
            result.Y = Y;
            result.IsAltar = IsAltar;

            result.Shapes = new List<ShapeData>(Shapes.Count);
            foreach (var item in Shapes)
                result.Shapes.Add(item.Copy());

            result.Parameters = new List<RegionElementData>(Parameters.Count);
            foreach (var item in Parameters)
                result.Parameters.Add(item.Copy());

            return result;
        }
    }

    public class RegionElementData
    {
        public EventType EventType { get; set; }
        public SignalType SignalType { get; set; }
        public string Name { get; set; }
        public XElement XML { get; set; }

        public RegionElementData Copy()
        {
            var result = new RegionElementData();
            result.EventType = EventType;
            result.SignalType = SignalType;
            result.Name = Name;
            result.XML = XML != null ? new XElement(XML) : null;

            return result;
        }
    }

    public class RegionInfoDb : DatabaseDatIndexed<int, RegionInfoData>
    {
        private readonly Random _rnd = new Random(Environment.TickCount);

        /// <summary>
        ///     Returns region with given name or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RegionInfoData GetRegion(string name)
        {
            return Entries.Values.FirstOrDefault(a => a.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        ///     Returns random coordinates inside the actual region.
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public Point RandomCoord(int region)
        {
            var ri = Find(region);
            if (ri == null)
                return new Point();

            lock (_rnd)
            {
                return ri.RandomCoord(_rnd);
            }
        }

        /// <summary>
        ///     Returns group id for the given region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public int GetGroupId(int regionId)
        {
            var data = Find(regionId);
            if (data == null)
                return -1;

            return data.GroupId;
        }

        /// <summary>
        ///     Loads data.
        /// </summary>
        /// <param name="br"></param>
        protected override void Read(BinaryReader br)
        {
            var cRegions = br.ReadInt32();
            for (var l = 0; l < cRegions; ++l)
            {
                var ri = new RegionInfoData();

                ri.Id = br.ReadInt32();
                ri.Name = br.ReadString();
                ri.GroupId = br.ReadInt32();
                ri.X1 = br.ReadInt32();
                ri.Y1 = br.ReadInt32();
                ri.X2 = br.ReadInt32();
                ri.Y2 = br.ReadInt32();

                var cAreas = br.ReadInt32();
                for (var i = 0; i < cAreas; ++i)
                {
                    var ai = new AreaData();

                    ai.Id = br.ReadInt32();
                    ai.Name = br.ReadString();
                    ai.X1 = br.ReadInt32();
                    ai.Y1 = br.ReadInt32();
                    ai.X2 = br.ReadInt32();
                    ai.Y2 = br.ReadInt32();

                    var cProps = br.ReadInt32();
                    for (var j = 0; j < cProps; ++j)
                    {
                        var pi = new PropData();
                        pi.EntityId = br.ReadInt64();
                        pi.Id = br.ReadInt32();
                        pi.Name = br.ReadString();
                        pi.X = br.ReadSingle();
                        pi.Y = br.ReadSingle();
                        pi.Direction = br.ReadSingle();
                        pi.Scale = br.ReadSingle();
                        pi.Title = br.ReadString();
                        pi.State = br.ReadString();

                        var cShapes = br.ReadInt32();
                        for (var k = 0; k < cShapes; ++k)
                        {
                            var si = new ShapeData();
                            si.DirX1 = br.ReadSingle();
                            si.DirX2 = br.ReadSingle();
                            si.DirY1 = br.ReadSingle();
                            si.DirY2 = br.ReadSingle();
                            si.LenX = br.ReadSingle();
                            si.LenY = br.ReadSingle();
                            si.PosX = br.ReadSingle();
                            si.PosY = br.ReadSingle();

                            pi.Shapes.Add(si);
                        }

                        var cElements = br.ReadInt32();
                        for (var k = 0; k < cElements; ++k)
                        {
                            var red = new RegionElementData();
                            red.EventType = (EventType) br.ReadInt32();
                            red.SignalType = (SignalType) br.ReadInt32();
                            red.Name = br.ReadString();

                            var xml = br.ReadString();
                            red.XML = !string.IsNullOrWhiteSpace(xml) ? XElement.Parse(xml) : null;

                            pi.Parameters.Add(red);
                        }

                        ai.Props.Add(pi.EntityId, pi);
                    }

                    var cEvents = br.ReadInt32();
                    for (var j = 0; j < cEvents; ++j)
                    {
                        var ei = new EventData();
                        ei.Id = br.ReadInt64();
                        ei.Name = br.ReadString();
                        ei.Path = string.Format("{0}/{1}/{2}", ri.Name, ai.Name, ei.Name);
                        ei.RegionId = ri.Id;
                        ei.X = br.ReadSingle();
                        ei.Y = br.ReadSingle();
                        ei.Type = (EventType) br.ReadInt32();

                        var cShapes = br.ReadInt32();
                        for (var k = 0; k < cShapes; ++k)
                        {
                            var si = new ShapeData();
                            si.DirX1 = br.ReadSingle();
                            si.DirX2 = br.ReadSingle();
                            si.DirY1 = br.ReadSingle();
                            si.DirY2 = br.ReadSingle();
                            si.LenX = br.ReadSingle();
                            si.LenY = br.ReadSingle();
                            si.PosX = br.ReadSingle();
                            si.PosY = br.ReadSingle();

                            ei.Shapes.Add(si);
                        }

                        var cElements = br.ReadInt32();
                        for (var k = 0; k < cElements; ++k)
                        {
                            var red = new RegionElementData();
                            red.EventType = (EventType) br.ReadInt32();
                            red.SignalType = (SignalType) br.ReadInt32();
                            red.Name = br.ReadString();

                            var xml = br.ReadString();
                            red.XML = !string.IsNullOrWhiteSpace(xml) ? XElement.Parse(xml) : null;

                            if (!ei.IsAltar && red.EventType == EventType.Altar && red.SignalType == SignalType.StepOn)
                                ei.IsAltar = true;

                            ei.Parameters.Add(red);
                        }

                        if (ei.Name == "nekojima_altar_eventbox")
                            ei.IsAltar = true;

                        ai.Events.Add(ei.Id, ei);
                    }

                    ri.Areas.Add(ai);
                }

                Entries.Add(ri.Id, ri);
            }
        }
    }
}