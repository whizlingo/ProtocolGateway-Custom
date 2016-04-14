// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.ProtocolGateway.Providers.CloudStorage
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.ProtocolGateway.Mqtt.Persistence;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    public class BlobSessionStatePersistenceProvider : ISessionStatePersistenceProvider
    {
        readonly CloudBlobContainer container;

        internal BlobSessionStatePersistenceProvider(string connectionString, string containerName)
        {
            CloudStorageAccount cloudStorageAccount;
            if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Could not parse CloudStorageAccount having value: {0}",
                    connectionString));
            }

            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            this.container = blobClient.GetContainerReference(containerName);
        }

        public static async Task<BlobSessionStatePersistenceProvider> CreateAsync(string connectionString, string containerName)
        {
            var manager = new BlobSessionStatePersistenceProvider(connectionString, containerName);
            await manager.InitializeAsync();
            return manager;
        }

        async Task InitializeAsync()
        {
            try
            {
                await this.container.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize Blob Storage Manager.", ex); // todo: custom exception type
            }
        }

        public ISessionState Create(bool transient)
        {
            return new BlobSessionState(transient);
        }

        public async Task<ISessionState> GetAsync(string id)
        {
            // todo: handle server busy (throttle?)

            CloudBlockBlob blob = this.container.GetBlockBlobReference(id);
            JsonSerializer serializer = JsonSerializer.Create();

            try
            {
                using (Stream stream = await blob.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream(new byte[blob.Properties.Length])) // we don't expect it to be big (i.e. bigger than 85KB leading to LOH alloc)
                    {
                        await stream.CopyToAsync(memoryStream);

                        memoryStream.Position = 0;
                        using (var streamReader = new StreamReader(memoryStream))
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            var sessionState = serializer.Deserialize<BlobSessionState>(jsonReader);
                            sessionState.ETag = blob.Properties.ETag;

                            return sessionState;
                        }
                    }
                }
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task SetAsync(string id, ISessionState sessionState)
        {
            var state = sessionState as BlobSessionState;

            if (state == null)
            {
                throw new ArgumentException("Cannot set Session State object that hasn't been acquired from provider.", "sessionState");
            }

            if (state.IsTransient)
            {
                throw new ArgumentException("Cannot persist transient Session State object.", "sessionState");
            }

            CloudBlockBlob blob = this.container.GetBlockBlobReference(id);
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                JsonSerializer serializer = JsonSerializer.Create();
                serializer.Serialize(streamWriter, state);
                streamWriter.Flush();

                memoryStream.Position = 0;
                AccessCondition accessCondition = state.ETag == null
                    ? AccessCondition.GenerateIfNoneMatchCondition("*") // create
                    : AccessCondition.GenerateIfMatchCondition(state.ETag); // update
                await blob.UploadFromStreamAsync(memoryStream, accessCondition, null, null);
                state.ETag = blob.Properties.ETag;
            }
        }

        public async Task DeleteAsync(string id, ISessionState sessionState)
        {
            var state = sessionState as BlobSessionState;

            if (state == null)
            {
                throw new ArgumentException("Cannot set Session State object that hasn't been acquired from provider.", "sessionState");
            }

            CloudBlockBlob blob = this.container.GetBlockBlobReference(id);
            await blob.DeleteAsync(
                DeleteSnapshotsOption.None,
                new AccessCondition
                {
                    IfMatchETag = state.ETag
                },
                null,
                null);
        }
    }
}
