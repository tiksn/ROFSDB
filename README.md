# ROFSDB

[![Version](https://img.shields.io/nuget/v/ROFSDB.svg)](https://www.nuget.org/packages/ROFSDB)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/ROFSDB.svg)](https://www.nuget.org/packages/ROFSDB)
[![GitHub Super-Linter](https://github.com/tiksn/ROFSDB/workflows/Lint/badge.svg)](https://github.com/marketplace/actions/super-linter)

**Read-Only FileSystem Database** - A lightweight .NET library for reading structured data from the filesystem as if it were a database.

## Overview

ROFSDB provides a simple, efficient way to read and query data stored in files organized in a filesystem structure. It treats directories as collections and files as documents, supporting multiple serialization formats. Perfect for configuration data, reference data, or any read-only data that needs to be version-controlled and easily accessible.

## Features

- 🗂️ **Collection-based organization** - Directories represent collections, files represent documents
- 📄 **Multiple format support** - JSON, YAML, TOML, KDL, HCL, PowerShell Data files (PSD1), and Parquet
- ⚡ **Async streaming** - Efficient `IAsyncEnumerable<T>` API for memory-efficient data access
- 🔌 **Dependency Injection** - Built-in support for Microsoft.Extensions.DependencyInjection
- 🔄 **Flexible storage** - Works with physical filesystems, embedded resources, or virtual filesystems (via Zio)
- 🎯 **Type-safe** - Strongly-typed document models with automatic deserialization
- 🚀 **High performance** - Optimized for read-only scenarios with minimal overhead

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package ROFSDB
```

Or via Package Manager Console:

```powershell
Install-Package ROFSDB
```

## Quick Start

### 1. Define Your Models

```csharp
public class City
{
    public string Name { get; set; }
    public int Population { get; set; }
    public int CountryID { get; set; }
}

public class Country
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}
```

### 2. Organize Your Data

Create a directory structure like this:

```
data/
├── Cities/
│   ├── city1.json
│   ├── city2.yaml
│   ├── city3.toml
│   └── city4.kdl
└── Countries/
    ├── country1.json
    ├── country2.yaml
    └── country3.kdl
```

### 3. Configure Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TIKSN.ROFSDB;

var services = new ServiceCollection();

// Add ROFSDB with physical file system
services.AddROFSDB();
services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(@"C:\path\to\data"));

// Or use embedded resources
services.AddSingleton<IFileProvider>(
    new EmbeddedFileProvider(typeof(Program).Assembly));

var serviceProvider = services.BuildServiceProvider();
```

### 4. Define a Typed Database Context

For a better-structured data access, you can define a typed context:

```csharp
public interface ICityContext
{
    IAsyncEnumerable<City> GetCitiesAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<Country> GetCountriesAsync(CancellationToken cancellationToken);
}

public class CityContext(IDatabaseEngine databaseEngine) : ICityContext
{
    public IAsyncEnumerable<City> GetCitiesAsync(CancellationToken cancellationToken)
        => databaseEngine.GetDocumentsAsync<City>("Cities", cancellationToken);

    public IAsyncEnumerable<Country> GetCountriesAsync(CancellationToken cancellationToken)
        => databaseEngine.GetDocumentsAsync<Country>("Countries", cancellationToken);
}

// Then register it in DI
services.AddSingleton<ICityContext, CityContext>();
```

### 5. Use the Typed Context

```csharp
var cityContext = serviceProvider.GetRequiredService<ICityContext>();

// Get all documents from a collection
await foreach (var city in cityContext.GetCitiesAsync(cancellationToken))
{
    Console.WriteLine($"City: {city.Name}, Population: {city.Population}");
}

// Query with LINQ
var largeCities = await cityContext
    .GetCitiesAsync(cancellationToken)
    .Where(c => c.Population > 1000000)
    .ToListAsync(cancellationToken);
```

## Supported Formats

ROFSDB supports the following file formats:

| Format | Extension | Description |
|--------|-----------|-------------|
| **JSON** | `.json` | JavaScript Object Notation |
| **YAML** | `.yaml`, `.yml` | YAML Ain't Markup Language |
| **TOML** | `.toml` | Tom's Obvious, Minimal Language |
| **KDL** | `.kdl` | KDL Document Language |
| **HCL** | `.hcl` | HashiCorp Configuration Language |
| **PowerShell Data** | `.psd1` | PowerShell Data files |
| **Parquet** | `.parquet` | Apache Parquet columnar storage format |

### Format Examples

**JSON** (`city.json`):
```json
{
  "name": "New York",
  "population": 8336817,
  "countryID": 1100746772
}
```

**YAML** (`city.yaml`):
```yaml
name: New York
population: 8336817
countryID: 1100746772
```

**TOML** (`city.toml`):
```toml
name = "New York"
population = 8336817
countryID = 1100746772
```

**KDL** (`city.kdl`):
```kdl
city name="New York" population=8336817 country-id=1100746772
```

KDL files target KDL v2 and use top-level nodes named after the target C# model type in acronym-aware kebab case:

- `Country` maps to `country`
- `City` maps to `city`
- `IPAddressRule` maps to `ip-address-rule`
- `ID` maps to `id`
- `CountryID` maps to `country-id`

KDL members can be written as node properties or as scalar child nodes with one argument:

```kdl
country id=1419150635 name="Austria"

country {
  id 1552721979
  name "France"
}
```

One `.kdl` file can contain multiple documents of the same type:

```kdl
country id=1419150635 name="Austria"
country id=1552721979 name="France"
country id=1501801186 name="Italy"
```

KDL support is strict: node and member names are case-sensitive, unknown members throw, unexpected top-level node names throw, and duplicate members throw. Missing members keep their normal CLR default values. The initial KDL mapper supports flat POCOs with public writable scalar properties. KDL v2 scalar literals such as `#true`, `#false`, and `#null` are supported.

Files in most text formats can contain either a single document or multiple documents, depending on the format. JSON arrays, YAML multi-document streams, and KDL repeated top-level nodes are automatically expanded into individual documents.

## Advanced Usage

### Using Zio Virtual File System

For virtual filesystems or in-memory storage:

```csharp
using Zio;
using TIKSN.ROFSDB;

services.AddROFSDB();
services.AddRofsDbZioFileStorage();
services.AddSingleton<IFileSystem>(new MemoryFileSystem());
```

### Custom File Storage

Implement `IFileStorage` to use custom storage backends:

```csharp
public class CustomFileStorage : IFileStorage
{
    public IAsyncEnumerable<FileStorageEntry> ListAsync(string fullPath, CancellationToken cancellationToken)
    {
        // Your implementation
    }

    public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken)
    {
        // Your implementation
    }
}

services.AddROFSDB<CustomFileStorage>();
```

### Custom Serialization

Implement `ISerialization` to add support for additional formats:

```csharp
public class CustomSerialization : ISerialization
{
    public IEnumerable<string> FileExtensions => new[] { ".xml" };

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        // Your deserialization logic
    }
}

services.AddKeyedSingleton<ISerialization, CustomSerialization>("XML");
```

## Requirements

- .NET 10.0 or later
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.FileProviders.Abstractions

## Project Structure

```
ROFSDB/
├── DatabaseEngine.cs          # Main database engine implementation
├── IDatabaseEngine.cs          # Database engine interface
├── DatabaseContext.cs         # Typed database context implementation
├── IDatabaseContext.cs         # Typed database context interface
├── IFileStorage.cs             # File storage abstraction
├── FileStorageAdapters/        # Storage adapter implementations
│   ├── FileProviderToFileStorageAdapter.cs
│   └── ZioFileSystemToFileStorageAdapter.cs
├── Serialization/              # Serialization format implementations
│   ├── JsonSerialization.cs
│   ├── KdlSerialization.cs
│   ├── YamlSerialization.cs
│   ├── TomlSerialization.cs
│   ├── HclSerialization.cs
│   ├── ParquetSerialization.cs
│   └── PowerShellDataFileSerialization.cs
└── ServiceCollectionExtensions.cs  # DI extension methods
```

## License

See [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Links

- [NuGet Package](https://www.nuget.org/packages/ROFSDB)
- [GitHub Repository](https://github.com/tiksn/ROFSDB)
- [KDL](https://kdl.dev/)
- [KdlSharp](https://github.com/AndreyAkinshin/KdlSharp)
