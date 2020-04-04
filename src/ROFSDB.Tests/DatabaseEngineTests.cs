using System;
using System.Threading.Tasks;
using TIKSN.ROFSDB.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace TIKSN.ROFSDB.Tests
{
    [Collection("Yaml Database Engine collection")]
    public class DatabaseEngineTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly YamlDatabaseEngineFixture yamlDatabaseEngineFixture;

        public DatabaseEngineTests(ITestOutputHelper testOutputHelper, YamlDatabaseEngineFixture yamlDatabaseEngineFixture)
        {
            this.testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
            this.yamlDatabaseEngineFixture = yamlDatabaseEngineFixture ?? throw new ArgumentNullException(nameof(yamlDatabaseEngineFixture));
        }

        [Fact]
        public async Task Test1()
        {
        }

        [Fact]
        public async Task Test2()
        {
        }
    }
}