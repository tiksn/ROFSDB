using System.Collections.Generic;
using System.Threading;
using TIKSN.ROFSDB.Tests.Models;

namespace TIKSN.ROFSDB.Tests;

public class DatabaseContext(IDatabaseEngine databaseEngine) : IDatabaseContext
{
    private readonly IDatabaseEngine databaseEngine = databaseEngine ?? throw new System.ArgumentNullException(nameof(databaseEngine));

    public IAsyncEnumerable<City> GetCitiesAsync(CancellationToken cancellationToken)
    {
        return this.databaseEngine.GetDocumentsAsync<City>("Cities", cancellationToken);
    }

    public IAsyncEnumerable<Country> GetCountriesAsync(CancellationToken cancellationToken)
    {
        return this.databaseEngine.GetDocumentsAsync<Country>("Countries", cancellationToken);
    }
}