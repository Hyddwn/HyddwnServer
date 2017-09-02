// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Shared.Network
{
    /// <summary>
    ///     Packet handler manager base class.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class PacketHandlerManager<TClient> where TClient : BaseClient
    {
        public delegate void PacketHandlerFunc(TClient client, Packet packet);

        private readonly Dictionary<int, PacketHandlerFunc> _handlers;

        protected PacketHandlerManager()
        {
            _handlers = new Dictionary<int, PacketHandlerFunc>();
        }

        /// <summary>
        ///     Adds and/or overwrites handler.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="handler"></param>
        public void Add(int op, PacketHandlerFunc handler)
        {
            _handlers[op] = handler;
        }

        /// <summary>
        ///     Adds all methods with a Handler attribute.
        /// </summary>
        public void AutoLoad()
        {
            foreach (var method in GetType().GetMethods())
            foreach (PacketHandlerAttribute attr in method.GetCustomAttributes(typeof(PacketHandlerAttribute), false))
            {
                var del = (PacketHandlerFunc) Delegate.CreateDelegate(typeof(PacketHandlerFunc), this, method);
                foreach (var op in attr.Ops)
                    Add(op, del);
            }
        }

        /// <summary>
        ///     Runs handler for packet's op, or logs it as unimplemented.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        public virtual void Handle(TClient client, Packet packet)
        {
            // Don't log internal packets
            //if (packet.Op < Op.Internal.ServerIdentify)
            //    Log.Debug("R: " + packet);

            PacketHandlerFunc handler;
            if (!_handlers.TryGetValue(packet.Op, out handler))
            {
                UnknownPacket(client, packet);
                return;
            }

            try
            {
                handler(client, packet);
            }
            catch (PacketElementTypeException ex)
            {
                Log.Error(
                    "PacketElementTypeException: " + ex.Message + Environment.NewLine +
                    ex.StackTrace + Environment.NewLine +
                    "Packet: " + Environment.NewLine +
                    packet
                );
            }
        }

        public virtual void UnknownPacket(TClient client, Packet packet)
        {
            Log.Unimplemented("PacketHandlerManager: Handler for '{0:X4}', '{1}'.", packet.Op, Op.GetName(packet.Op));
            Log.Debug(packet);
        }
    }

    /// <summary>
    ///     Methods having this attribute are registered as packet handlers,
    ///     for the ops.
    /// </summary>
    public class PacketHandlerAttribute : Attribute
    {
        public PacketHandlerAttribute(params int[] ops)
        {
            Ops = ops;
        }

        public int[] Ops { get; protected set; }
    }
}