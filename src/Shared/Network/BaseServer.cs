// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Aura.Shared.Util;

namespace Aura.Shared.Network
{
    /// <summary>
    ///     Base server, for specialized servers to inherit from.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class BaseServer<TClient> where TClient : BaseClient, new()
    {
        public delegate void ClientConnectionEventHandler(TClient client);

        private readonly Socket _socket;

        protected BaseServer()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            Clients = new List<TClient>();
        }

        public List<TClient> Clients { get; set; }

        public PacketHandlerManager<TClient> Handlers { get; set; }

        /// <summary>
        ///     Raised when client successfully connected.
        /// </summary>
        public event ClientConnectionEventHandler ClientConnected;

        /// <summary>
        ///     Raised when client disconnected for any reason.
        /// </summary>
        public event ClientConnectionEventHandler ClientDisconnected;

        /// <summary>
        ///     Starts listener.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Start(int port)
        {
            if (Handlers == null)
            {
                Log.Error("No packet handler manager set, start canceled.");
                return;
            }

            Start(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        ///     Starts listener.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Start(string host, int port)
        {
            if (Handlers == null)
            {
                Log.Error("No packet handler manager set, start canceled.");
                return;
            }

            Start(new IPEndPoint(IPAddress.Parse(host), port));
        }

        /// <summary>
        ///     Starts listener.
        /// </summary>
        /// <param name="endPoint"></param>
        private void Start(IPEndPoint endPoint)
        {
            try
            {
                _socket.Bind(endPoint);
                _socket.Listen(10);

                _socket.BeginAccept(OnAccept, _socket);

                Log.Status("Server ready, listening on {0}.", _socket.LocalEndPoint);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Unable to set up socket; perhaps you're already running a server?");
                CliUtil.Exit(1);
            }
        }

        /// <summary>
        ///     Stops listener.
        /// </summary>
        public void Stop()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Handles incoming connections.
        /// </summary>
        /// <param name="result"></param>
        private void OnAccept(IAsyncResult result)
        {
            var client = new TClient();

            try
            {
                client.Socket = (result.AsyncState as Socket).EndAccept(result);

                // We don't need this here, since it's inherited from the parent
                // client.Socket.NoDelay = true;

                client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, OnReceive, client);

                AddClient(client);
                Log.Info("Connection established from '{0}.", client.Address);

                OnClientConnected(client);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "While accepting connection.");
            }
            finally
            {
                _socket.BeginAccept(OnAccept, _socket);
            }
        }

        /// <summary>
        ///     Starts receiving for client.
        /// </summary>
        /// <param name="client"></param>
        public void AddReceivingClient(TClient client)
        {
            client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, OnReceive, client);
        }

        /// <summary>
        ///     Handles sending packets, obviously.
        /// </summary>
        /// <param name="result"></param>
        protected void OnReceive(IAsyncResult result)
        {
            var client = result.AsyncState as TClient;

            try
            {
                var bytesReceived = client.Socket.EndReceive(result);
                var ptr = 0;

                if (bytesReceived == 0)
                {
                    Log.Info("Connection closed from '{0}.", client.Address);
                    KillAndRemoveClient(client);
                    OnClientDisconnected(client);
                    return;
                }

                // Handle all received bytes
                while (bytesReceived > 0)
                {
                    // Length of new packet
                    var length = GetPacketLength(client.Buffer, ptr);

                    // Shouldn't actually happen...
                    if (length > client.Buffer.Length)
                        throw new Exception(string.Format("Buffer too small to receive full packet ({0}).", length));

                    // Read whole packet and ...
                    var buffer = new byte[length];
                    Buffer.BlockCopy(client.Buffer, ptr, buffer, 0, length);
                    bytesReceived -= length;
                    ptr += length;

                    // Handle it
                    HandleBuffer(client, buffer);
                }

                // Stop if client was killed while handling.
                if (client.State == ClientState.Dead)
                {
                    Log.Info("Killed connection from '{0}'.", client.Address);
                    RemoveClient(client);
                    OnClientDisconnected(client);
                    return;
                }

                // Round $round+1, receive!
                client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, OnReceive, client);
            }
            catch (SocketException)
            {
                Log.Info("Connection lost from '{0}'.", client.Address);
                KillAndRemoveClient(client);
                OnClientDisconnected(client);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "While receiving data from '{0}'.", client.Address);
                KillAndRemoveClient(client);
                OnClientDisconnected(client);
            }
        }

        /// <summary>
        ///     Kills and removes client from server.
        /// </summary>
        /// <param name="client"></param>
        protected void KillAndRemoveClient(TClient client)
        {
            client.Kill();
            RemoveClient(client);
        }

        /// <summary>
        ///     Adds client to list.
        /// </summary>
        /// <param name="client"></param>
        protected void AddClient(TClient client)
        {
            lock (Clients)
            {
                Clients.Add(client);
                //Log.Status("Connected clients: {0}", _clients.Count);
            }
        }

        /// <summary>
        ///     Removes client from list.
        /// </summary>
        /// <param name="client"></param>
        protected void RemoveClient(TClient client)
        {
            lock (Clients)
            {
                Clients.Remove(client);
                //Log.Status("Connected clients: {0}", _clients.Count);
            }
        }

        /// <summary>
        ///     Returns length of the new incoming packet, so it can be received.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        protected abstract int GetPacketLength(byte[] buffer, int ptr);

        protected abstract void HandleBuffer(TClient client, byte[] buffer);

        protected virtual void OnClientConnected(TClient client)
        {
            if (ClientConnected != null)
                ClientConnected(client);
        }

        protected virtual void OnClientDisconnected(TClient client)
        {
            if (ClientDisconnected != null)
                ClientDisconnected(client);
        }
    }
}