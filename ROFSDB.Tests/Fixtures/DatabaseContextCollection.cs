using Xunit;

namespace TIKSN.ROFSDB.Tests.Fixtures;

[CollectionDefinition("Database Context collection")]
public class DatabaseContextCollection : ICollectionFixture<DatabaseContextFixture>
{
}
