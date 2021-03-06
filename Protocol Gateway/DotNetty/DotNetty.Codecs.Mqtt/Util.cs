﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.Mqtt
{
    static class Util
    {
        public const string ProtocolName = "MQIsdp";
        public const int ProtocolLevel = 3;

        static readonly char[] TopicWildcards = { '#', '+' };

        public static void ValidateTopicName(string topicName)
        {
            if (topicName.Length == 0)
            {
                throw new DecoderException("[MQTT-4.7.3-1]");
            }

            if (topicName.IndexOfAny(TopicWildcards) > 0)
            {
                throw new DecoderException(string.Format("Invalid PUBLISH topic name: {0}", topicName));
            }
        }

        public static void ValidatePacketId(int packetId)
        {
            if (packetId < 1)
            {
                throw new DecoderException("Invalid packet identifier: " + packetId);
            }
        }

        public static void ValidateClientId(string clientId)
        {
            if (clientId == null)
            {
                throw new DecoderException("Client identifier is required.");
            }
        }
    }
}