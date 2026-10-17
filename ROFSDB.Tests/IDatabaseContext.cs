using System.Collections.Generic;
using System.Threading;
using TIKSN.ROFSDB.Tests.Models;

namespace TIKSN.ROFSDB.Tests;

public interface IDatabaseContext
{
    IAsyncEnumerable<City> GetCitiesAsync(CancellationToken cancellationToken);

    IAsyncEnumerable<Country> GetCountriesAsync(CancellationToken cancellationToken);
}