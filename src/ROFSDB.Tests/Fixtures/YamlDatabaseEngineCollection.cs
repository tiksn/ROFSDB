using Xunit;

namespace TIKSN.ROFSDB.Tests.Fixtures
{
    [CollectionDefinition("Yaml Database Engine collection")]
    public class YamlDatabaseEngineCollection : ICollectionFixture<YamlDatabaseEngineFixture>
    {
    }
}