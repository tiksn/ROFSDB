using Storage.Net;
using Storage.Net.Blobs;
using System;
using Xunit.Abstractions;

namespace TIKSN.ROFSDB.Tests
{
    public class DatabaseEngineTests : IDisposable
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly IDatabaseEngine databaseEngine;
        private readonly IBlobStorage blobStorage;

        public DatabaseEngineTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
            this.blobStorage = StorageFactory.Blobs.InMemory();
            this.databaseEngine = new DatabaseEngine();
        }

        public void Dispose()
        {
            blobStorage.Dispose();
        }
    }
}