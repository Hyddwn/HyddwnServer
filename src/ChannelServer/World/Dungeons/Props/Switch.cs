// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;

namespace Aura.Channel.World.Dungeons.Props
{
    /// <summary>
    ///     Switch prop, as found in dungeons.
    /// </summary>
    public class Switch : DungeonProp
    {
        /// <summary>
        ///     Creates new switch prop with a default prop id.
        /// </summary>
        /// <param name="name">Name of the prop.</param>
        /// <param name="color">Color of the orb.</param>
        public Switch(string name, uint color)
            : this(10202, name, color)
        {
        }

        /// <summary>
        ///     Creates new switch prop.
        /// </summary>
        /// <param name="propId">Id of the switch prop.</param>
        /// <param name="name">Name of the prop.</param>
        /// <param name="color">Color of the orb.</param>
        public Switch(int propId, string name, uint color)
            : base(propId, name)
        {
            Name = name;
            State = "off";
            Info.Color2 = color;

            Behavior = DefaultBehavior;
        }

        /// <summary>
        ///     Returns true if the switch is currently on.
        /// </summary>
        public bool IsTurnedOn => State == "on";

        /// <summary>
        ///     Default behavior of the switch, turning it on.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="prop"></param>
        private void DefaultBehavior(Creature creature, Prop prop)
        {
            if (!IsTurnedOn)
                TurnOn();
        }

        /// <summary>
        ///     Turns switch on.
        /// </summary>
        public void TurnOn()
        {
            SetState("on");
        }

        /// <summary>
        ///     Turns switch off.
        /// </summary>
        public void TurnOff()
        {
            SetState("off");
        }
    }
}