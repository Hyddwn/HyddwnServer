// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using Aura.Channel.World.Entities.Props;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
    /// <remarks>
    ///     Not all options are used in all props. Things like ExtraData, State,
    ///     etc. are all very prop specific.
    /// </remarks>
    public class Prop : Entity, IShapedEntity
    {
        public float _resource;

        private XElement _xml;

        /// <summary>
        ///     Data about the prop from the db.
        /// </summary>
        public PropsDbData Data;

        /// <summary>
        ///     Marshable prop information used for packets.
        /// </summary>
        public PropInfo Info;

        /// <summary>
        ///     Creates new prop with no specific entity id.
        /// </summary>
        /// <remarks>
        ///     The entity id is assigned automatically when the prop is added
        ///     to a region, as the id depends on the prop's location and the
        ///     region it's eventually added to.
        ///     If the entity id is not supposed to be set upon adding the prop
        ///     to a region, it should be set to anything but 0.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="scale"></param>
        /// <param name="altitude"></param>
        /// <param name="ident"></param>
        /// <param name="title"></param>
        public Prop(int id, int regionId, int x, int y, float direction, float scale = 1f, float altitude = 0,
            string state = "", string ident = "", string title = "")
            : this(0, id, regionId, x, y, direction, scale, altitude, state, ident, title)
        {
        }

        /// <summary>
        ///     Creates new prop, based on prop data.
        /// </summary>
        /// <param name="propData"></param>
        /// <param name="regionId"></param>
        /// <param name="regionName"></param>
        /// <param name="areaName"></param>
        public Prop(PropData propData, int regionId, string regionName, string areaName)
            : this(propData.EntityId, propData.Id, regionId, (int) propData.X, (int) propData.Y, propData.Direction,
                propData.Scale, 0, "", "", "")
        {
            // Set full name
            GlobalName = string.Format("{0}/{1}/{2}", regionName, areaName, propData.Name);

            // Save parameters for use by dungeons
            Parameters = propData.Parameters.ToList();

            // Add drop behaviour if drop type exists
            var dropType = propData.GetDropType();
            if (dropType != -1)
                Behavior = GetDropBehavior(dropType);

            // Replace default shapes with the ones loaded from region.
            // (Is this really necessary?)
            State = propData.State;
            Shapes.Clear();
            Shapes.AddRange(propData.Shapes.Select(a => a.GetPoints(0, 0, 0)));
        }

        /// <summary>
        ///     Creates new prop with given entity id.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="id"></param>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="scale"></param>
        /// <param name="altitude"></param>
        /// <param name="state"></param>
        /// <param name="ident"></param>
        /// <param name="title"></param>
        public Prop(long entityId, int id, int regionId, int x, int y, float direction, float scale, float altitude,
            string state, string ident, string title)
        {
            Shapes = new List<Point[]>();
            Temp = new PropTemp();
            Vars = new ScriptVariables();
            Extensions = new PropExtensionManager(this);

            _resource = 100;

            EntityId = entityId;
            Ident = ident;
            Title = title;
            Info.Id = id;
            Info.Region = regionId;
            Info.X = x;
            Info.Y = y;
            Info.Altitude = altitude;
            Info.Direction = direction;
            Info.Scale = scale;
            LastCollect = DateTime.Now;

            Info.Color1 =
                Info.Color2 =
                    Info.Color3 =
                        Info.Color4 =
                            Info.Color5 =
                                Info.Color6 =
                                    Info.Color7 =
                                        Info.Color8 =
                                            Info.Color9 = 0xFF808080;

            State = state;
            UpdateShapes();

            // Load prop data
            if ((Data = AuraData.PropsDb.Find(Info.Id)) == null)
                Log.Warning("Prop: No data found for '{0}'.", Info.Id);
        }

        /// <summary>
        ///     Returns entity data type "Prop".
        /// </summary>
        public override DataType DataType => DataType.Prop;

        /// <summary>
        ///     Temporary variables for this prop
        /// </summary>
        public PropTemp Temp { get; }

        /// <summary>
        ///     Scripting variables. (TODO: Remove Temp?)
        /// </summary>
        public ScriptVariables Vars { get; }

        /// <summary>
        ///     True if this prop was spawned by the server.
        /// </summary>
        /// <remarks>
        ///     *sigh* Yes, we're checking the id, happy now, devCAT? .__.
        /// </remarks>
        public bool ServerSide => EntityId >= MabiId.ServerProps;

        /// <summary>
        ///     Returns true if prop is not server sided and has a state or extra data.
        /// </summary>
        public bool ModifiedClientSide
        {
            get
            {
                if (ServerSide)
                    return false;

                // Props that only have one default state appear to be "single",
                // while others have default states like "off" or "closed".
                // Sending everything that has *some* state made EntitiesAppear
                // explode, so we'll limit it to meaningful states.
                // See also: Region.GetVisibleEntities
                var hasStates = !string.IsNullOrWhiteSpace(State) && State != "single";

                return hasStates || HasXml;
            }
        }

        /// <summary>
        ///     Called when a player interacts with the prop (touch, attack).
        /// </summary>
        public PropFunc Behavior { get; set; }

        /// <summary>
        ///     Prop's ident? Name?
        /// </summary>
        /// <remarks>
        ///     It's not clear what this is used for, dungeon portals specify
        ///     "_upstairs" and "_downstairs", but what's displayed above them
        ///     is the title, same with Guild Stones.
        /// </remarks>
        public string Ident { get; set; }

        /// <summary>
        ///     Prop's title (only supported by specific props)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Global name, consisting of region, area, and prop name.
        /// </summary>
        /// <remarks>
        ///     Not necessarily unique, the last part, the prop name might be empty.
        /// </remarks>
        /// <example>
        ///     Uladh_main/town_TirChonaill/windmill_tircho
        /// </example>
        public string GlobalName { get; }

        /// <summary>
        ///     Remaining resource amount
        /// </summary>
        public float Resource
        {
            get => _resource;
            set => _resource = Math2.Clamp(0, 100, value);
        }

        /// <summary>
        ///     Time at which something was collected from the prop last.
        /// </summary>
        public DateTime LastCollect { get; set; }

        /// <summary>
        ///     Prop's state (only supported by specific props)
        /// </summary>
        /// <remarks>
        ///     Some known states: single, closed, open, state1-3
        /// </remarks>
        public string State { get; set; }

        /// <summary>
        ///     Additional options as XML.
        /// </summary>
        public XElement Xml => _xml ?? (_xml = new XElement("xml"));

        /// <summary>
        ///     True if prop has an XML element.
        /// </summary>
        public bool HasXml => _xml != null;

        /// <summary>
        ///     Gets or sets the prop's region, forwarding to Info.Region.
        /// </summary>
        public override int RegionId
        {
            get => Info.Region;
            set => Info.Region = value;
        }

        /// <summary>
        ///     List of extensions (TODO: needs research).
        /// </summary>
        public PropExtensionManager Extensions { get; }

        /// <summary>
        ///     List of parameters for client side props.
        /// </summary>
        /// <remarks>
        ///     While the parameters are very similar to the extensions, they can't
        ///     be sent to the client, that messes with the prop's functionality.
        ///     For example, sending the parameters as extensions to the client
        ///     will override the original functionality of production props.
        ///     As a result you won't start a skill anymore, but send a touch
        ///     prop packet instead. Since we still need the parameters though,
        ///     we just dump them here for now, until we know more.
        ///     Don't change this list during run-time.
        /// </remarks>
        public List<RegionElementData> Parameters { get; }

        /// <summary>
        ///     List of shapes for the prop (collision).
        /// </summary>
        public List<Point[]> Shapes { get; protected set; }

        /// <summary>
        ///     Specifies whether other entities collide with this one's shape.
        /// </summary>
        /// <remarks>
        ///     Apparently the fireworks prop blocks movement...
        ///     TODO: Figure out why.
        /// </remarks>
        public bool IsCollision
        {
            get
            {
                if (Info.Id == 208) // Fireworks
                    return false;
                return true;
            }
        }

        /// <summary>
        ///     Returns prop's static position (Info.X|Y).
        /// </summary>
        /// <returns></returns>
        public override Position GetPosition()
        {
            return new Position((int) Info.X, (int) Info.Y);
        }

        /// <summary>
        ///     Returns information about the prop as string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Prop: 0x{0:X16}, Region: {1}, X: {2}, Y: {3}", EntityId, Info.Region, Info.X, Info.Y);
        }

        /// <summary>
        ///     Returns prop behavior for dropping.
        /// </summary>
        /// <param name="dropType"></param>
        /// <returns></returns>
        public static PropFunc GetDropBehavior(int dropType)
        {
            return (creature, prop) =>
            {
                if (RandomProvider.Get().NextDouble() > ChannelServer.Instance.Conf.World.PropDropChance)
                    return;

                var dropInfo = AuraData.PropDropDb.Find(dropType);
                if (dropInfo == null)
                {
                    Log.Warning("GetDropBehavior: Unknown prop drop type '{0}'.", dropType);
                    return;
                }

                var rnd = RandomProvider.Get();

                // Get random item from potential drops
                var dropItemInfo = dropInfo.GetRndItem(rnd);
                var rndAmount = dropItemInfo.Amount > 1 ? (ushort) rnd.Next(1, dropItemInfo.Amount) : (ushort) 1;

                var item = new Item(dropItemInfo.ItemClass);
                item.Info.Amount = rndAmount;
                item.Drop(prop.Region, creature.GetPosition(), Item.DropRadius, creature, false);
            };
        }

        /// <summary>
        ///     Returns prop behavior for warping.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static PropFunc GetWarpBehavior(int region, int x, int y)
        {
            return (creature, prop) => { creature.Warp(region, x, y); };
        }

        /// <summary>
        ///     Returns a prop behavior that doesn't do anything.
        /// </summary>
        /// <remarks>
        ///     Use to prevent unimplemented messages.
        /// </remarks>
        /// <returns></returns>
        public static PropFunc GetEmptyBehavior()
        {
            return (creature, prop) => { };
        }

        /// <summary>
        ///     Returns true if prop's data has the tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public override bool HasTag(string tag)
        {
            if (Data == null)
                return false;

            return Data.HasTag(tag);
        }

        /// <summary>
        ///     Removes prop from its current region.
        /// </summary>
        public override void Disappear()
        {
            if (Region != Region.Limbo)
                Region.RemoveProp(this);

            base.Disappear();
        }

        /// <summary>
        ///     Sets prop's state and broadcasts update.
        /// </summary>
        /// <param name="state"></param>
        public void SetState(string state)
        {
            State = state;
            UpdateCollisions();
            if (Region != Region.Limbo)
                Send.PropUpdate(this);
        }

        /// <summary>
        ///     Updates shapes for current state from defaults db.
        /// </summary>
        public void UpdateShapes()
        {
            Shapes.Clear();

            // Get list of defaults for prop
            var defaultsList = AuraData.PropDefaultsDb.Find(Info.Id);
            if (defaultsList != null && defaultsList.Count != 0)
            {
                // Get first default if state is empty, or the first one that
                // matches the current state.
                var def = string.IsNullOrWhiteSpace(State)
                    ? defaultsList.First()
                    : defaultsList.FirstOrDefault(a => a.State == State);

                if (def == null)
                {
                    Log.Warning("Prop.UpdateShapes: No defaults found for state '{0}' and prop '{1}'.", State, Info.Id);
                }
                else
                {
                    State = def.State;

                    foreach (var shape in def.Shapes)
                        Shapes.Add(shape.GetPoints(Info.Direction, (int) Info.X, (int) Info.Y));
                }
            }
        }

        /// <summary>
        ///     Updates prop's shapes and collisions in current region.
        /// </summary>
        private void UpdateCollisions()
        {
            UpdateShapes();

            if (Region != Region.Limbo)
            {
                Region.Collisions.Remove(EntityId);
                Region.Collisions.Add(this);
            }
        }

        /// <summary>
        ///     Returns targetable creatures in given range of prop.
        ///     Owner of the prop and creatures he can't target are excluded.
        /// </summary>
        /// <param name="owner">Owner of the prop, who is excluded and used as reference for CanTarget (null to ignore).</param>
        /// <param name="range"></param>
        /// <returns></returns>
        public ICollection<Creature> GetTargetableCreaturesInRange(Creature owner, int range)
        {
            var pos = GetPosition();
            var targetable = Region.GetCreatures(target =>
            {
                var targetPos = target.GetPosition();

                return target != owner // Exclude owner
                       && (owner == null || owner.CanTarget(target)) // Check targetability
                       && !target.IsDead // Check if target's alive (in case owner is null)
                       && !target.Has(CreatureStates.NamedNpc) // Don't hit NamedNpcs (in case owner is null)
                       && targetPos.InRange(pos, range) // Check range
                       && !Region.Collisions.Any(pos, targetPos) // Check collisions between entities
                       && !target.Conditions.Has(ConditionsA.Invisible); // Check visiblility (GM)
            });

            return targetable;
        }

        /// <summary>
        ///     Returns true if the given position is inside the prop.
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
    }

    public delegate void PropFunc(Creature creature, Prop prop);

    /// <summary>
    ///     Temporary prop variables
    /// </summary>
    public class PropTemp
    {
        public ItemData CampfireFirewood;
        public SkillRankData CampfireSkillRank;
    }
}