// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.ProtocolGateway.Providers.CloudStorage
{
    using System;
    using Microsoft.Azure.Devices.ProtocolGateway.Mqtt.Persistence;
    using Microsoft.WindowsAzure.Storage.Table;

    class TableMessageDeliveryState : TableEntity, IQos2MessageDeliveryState
    {
        public TableMessageDeliveryState()
        {
        }

        public TableMessageDeliveryState(string messageId)
        {
            this.MessageId = messageId;
            this.LastModified = DateTime.UtcNow;
        }

        public DateTime LastModified
        {
            get { return this.Timestamp.UtcDateTime; }
            set { this.Timestamp = value; }
        }

        public string MessageId { get; set; }
    }
}