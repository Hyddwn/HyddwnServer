// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.Network
{
    /// <summary>
    ///     A special ChannelClient, with the encryption reversed
    /// </summary>
    public class InternalClient : ChannelClient
    {
        protected override void EncodeBuffer(byte[] buffer)
        {
            Crypto.FromClient(buffer, 6, buffer.Length - 4);
        }

        public override void DecodeBuffer(byte[] buffer)
        {
            Crypto.FromServer(buffer);
        }
    }
}