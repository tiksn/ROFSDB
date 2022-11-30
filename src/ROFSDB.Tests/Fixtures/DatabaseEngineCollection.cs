using Xunit;

namespace TIKSN.ROFSDB.Tests.Fixtures
{
    [CollectionDefinition("Database Engine collection")]
    public class DatabaseEngineCollection : ICollectionFixture<DatabaseEngineFixture>
    {
    }
}