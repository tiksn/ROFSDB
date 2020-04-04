using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using TIKSN.ROFSDB.Tests.Fixtures;
using TIKSN.ROFSDB.Tests.Models;
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
        public async Task CityCountTest()
        {
            var count = await yamlDatabaseEngineFixture.DatabaseEngine.GetDocumentsAsync<City>("Cities", default).CountAsync(default);

            count.Should().Be(6);
        }

        [Fact]
        public async Task CollectionNameTest()
        {
            var actual = await yamlDatabaseEngineFixture.DatabaseEngine.GetCollectionsAsync(default).ToArrayAsync(default);
            var expected = new[] { "Countries", "Cities" };

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ContryCityRelationsTest()
        {
            await foreach (var city in yamlDatabaseEngineFixture.DatabaseEngine.GetDocumentsAsync<City>("Cities", default))
            {
                var countryFound = await yamlDatabaseEngineFixture.DatabaseEngine.GetDocumentsAsync<Country>("Countries", default).AnyAsync(x => x.ID == city.CountryID);

                countryFound.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ContryCountTest()
        {
            var count = await yamlDatabaseEngineFixture.DatabaseEngine.GetDocumentsAsync<Country>("Countries", default).CountAsync(default);

            count.Should().Be(5);
        }
    }
}