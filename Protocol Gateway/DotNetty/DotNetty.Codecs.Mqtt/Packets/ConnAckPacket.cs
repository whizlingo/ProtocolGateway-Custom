﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Mqtt.Packets
{
    public sealed class ConnAckPacket : Packet
    {
        public override PacketType PacketType
        {
            get { return PacketType.CONNACK; }
        }

        public ConnectReturnCode ReturnCode { get; set; }
    }
}