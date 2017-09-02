// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Drawing;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
    /// <summary>
    ///     A client in an area of a region.
    /// </summary>
    /// <remarks>
    ///     Is this considered to be an entity by the client?
    /// </remarks>
    public class ClientEvent : IShapedEntity
    {
        /// <summary>
        ///     Creates new client event
        /// </summary>
        /// <param name="eventData"></param>
        public ClientEvent(EventData eventData, string regionName, string areaName)
        {
            Shapes = new List<Point[]>();

            EntityId = eventData.Id;
            Data = eventData;
            GlobalName = string.Format("{0}/{1}/{2}", regionName, areaName, eventData.Name);

            Handlers = new Collection<SignalType, Action<Creature, EventData>>();

            foreach (var shape in Data.Shapes)
                Shapes.Add(shape.GetPoints(0, 0, 0));
        }

        /// <summary>
        ///     Handler that is called if this event is triggered
        /// </summary>
        public Collection<SignalType, Action<Creature, EventData>> Handlers { get; }

        /// <summary>
        ///     Data for this event
        /// </summary>
        public EventData Data { get; }

        /// <summary>
        ///     Global name, consisting of region, area, and event name.
        /// </summary>
        /// <remarks>
        ///     Not necessarily unique, the last part, the event name might be empty.
        /// </remarks>
        /// <example>
        ///     Tin_Beginner_Tutorial/_Tin_Beginner_Tutorial_01/tuto_start
        /// </example>
        public string GlobalName { get; }

        /// <summary>
        ///     Event's id
        /// </summary>
        public long EntityId { get; }

        /// <summary>
        ///     Shapes of this event.
        /// </summary>
        public List<Point[]> Shapes { get; }

        /// <summary>
        ///     Specifies whether other entities collide with this one's shape.
        /// </summary>
        public bool IsCollision => Data.Type == EventType.Collision;

        /// <summary>
        ///     Returns true if the given position is inside the event rectangle.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsInside(int x, int y)
        {
            if (Shapes.Count == 0)
                return false;

            var result = false;
            var point = new Point(x, y);

            foreach (var points in Shapes)
            {
                for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                    if (points[i].Y > point.Y != points[j].Y > point.Y && point.X <
                        (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) +
                        points[i].X)
                        result = !result;

                if (result)
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns true if the given position is inside the event rectangle.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsInside(Position pos)
        {
            return IsInside(pos.X, pos.Y);
        }
    }
}