using Microsoft.Extensions.DependencyInjection;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIKSN.ROFSDB.FileStorageAdapters;
using TIKSN.ROFSDB.Tests.Models;
using Xunit;
using Xunit.Abstractions;
using Zio;
using Zio.FileSystems;

namespace TIKSN.ROFSDB.Tests.Fixtures;

public class DatabaseContextFixture : IAsyncLifetime
{
    private static readonly string[] SerializationFormats = ["YAML", "TOML", "HCL", "JSON", "PSD1", "PARQUET", "KDL"];
    private FrozenDictionary<string, ServiceProvider> formatServiceProviders;
    private MemoryFileSystem memoryFileSystem;
    public FrozenDictionary<string, IDatabaseContext> DatabaseContexts { get; private set; }

    public Task DisposeAsync()
    {
        memoryFileSystem?.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        this.memoryFileSystem = new();
        await WriteFiles(memoryFileSystem);
        this.formatServiceProviders = SerializationFormats.ToFrozenDictionary(
            keySelector: x => x,
            elementSelector: x =>
            {
                var subFileSystem = memoryFileSystem.GetOrCreateSubFileSystem($"/{x}");
                var services = new ServiceCollection();
                services.AddSingleton<IFileSystem>(subFileSystem);
                services.AddSingleton<IFileStorage>(new ZioFileSystemToFileStorageAdapter(subFileSystem));
                services.AddROFSDB();
                services.AddSingleton<IDatabaseContext, DatabaseContext>();
                return services.BuildServiceProvider();
            });
        DatabaseContexts = formatServiceProviders.ToFrozenDictionary(x => x.Key, y => y.Value.GetRequiredService<IDatabaseContext>());
    }

    public void WriteFilesAndFoldersToTestOutput(string fileFormat, ITestOutputHelper testOutputHelper)
    {
        var items = formatServiceProviders[fileFormat].GetRequiredService<IFileSystem>()
            .EnumerateItems(UPath.Root, System.IO.SearchOption.AllDirectories)
            .OrderBy(x => x.FullName)
            .ToArray();

        foreach (var item in items)
        {
            var itemKind = item.IsDirectory ? "Folder" : "File";
            testOutputHelper.WriteLine($"{itemKind}: {item.FullName}");
        }
    }

    private static async Task WriteEuropeanCountries(IFileSystem fileSystem, StringBuilder stringBuilder)
    {
        #region YAML

        stringBuilder.Clear();
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1419150635");
        stringBuilder.AppendLine("Name: Austria");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1552721979");
        stringBuilder.AppendLine("Name: France");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1501801186");
        stringBuilder.AppendLine("Name: Italy");
        stringBuilder.AppendLine("...");
        fileSystem.WriteAllText("/YAML/Countries/Europe.yaml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion YAML

        #region TOML

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1419150635");
        stringBuilder.AppendLine("name = 'Austria'");
        fileSystem.WriteAllText("/TOML/Countries/Europe-Austria.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1552721979");
        stringBuilder.AppendLine("name = 'France'");
        fileSystem.WriteAllText("/TOML/Countries/Europe-France.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1501801186");
        stringBuilder.AppendLine("name = 'Italy'");
        fileSystem.WriteAllText("/TOML/Countries/Europe-Italy.toml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion TOML

        #region HCL

        stringBuilder.Clear();
        stringBuilder.AppendLine("\"id\" = 1419150635");
        stringBuilder.AppendLine("\"name\" = \"Austria\"");
        fileSystem.WriteAllText("/HCL/Countries/Europe-Austria.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("\"id\" = 1552721979");
        stringBuilder.AppendLine("\"name\" = \"France\"");
        fileSystem.WriteAllText("/HCL/Countries/Europe-France.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("\"id\" = 1501801186");
        stringBuilder.AppendLine("\"name\" = \"Italy\"");
        fileSystem.WriteAllText("/HCL/Countries/Europe-Italy.hcl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion HCL

        #region JSON

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1419150635,");
        stringBuilder.AppendLine("  \"Name\": \"Austria\"");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Countries/Europe-Austria.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1552721979,");
        stringBuilder.AppendLine("  \"Name\": \"France\"");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Countries/Europe-France.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1501801186,");
        stringBuilder.AppendLine("  \"Name\": \"Italy\"");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Countries/Europe-Italy.json", stringBuilder.ToString(), Encoding.UTF8);

        #endregion JSON

        #region PSD1

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1419150635");
        stringBuilder.AppendLine("    Name = 'Austria'");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Countries/Europe-Austria.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1552721979");
        stringBuilder.AppendLine("    Name = 'France'");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Countries/Europe-France.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1501801186");
        stringBuilder.AppendLine("    Name = 'Italy'");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Countries/Europe-Italy.psd1", stringBuilder.ToString(), Encoding.UTF8);

        #endregion PSD1

        #region KDL

        stringBuilder.Clear();
        stringBuilder.AppendLine("country {");
        stringBuilder.AppendLine("  id 1419150635");
        stringBuilder.AppendLine("  name \"Austria\"");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("country {");
        stringBuilder.AppendLine("  id 1552721979");
        stringBuilder.AppendLine("  name \"France\"");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("country {");
        stringBuilder.AppendLine("  id 1501801186");
        stringBuilder.AppendLine("  name \"Italy\"");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/KDL/Countries/Europe.kdl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion KDL

        #region PARQUET

        await WriteParquetCountries(fileSystem, [
            new Country { ID = 1419150635, Name = "Austria" },
            new Country { ID = 1552721979, Name = "France" },
            new Country { ID = 1501801186, Name = "Italy" }
        ], "/PARQUET/Countries/Europe.parquet");

        #endregion PARQUET
    }

    private static async Task WriteFiles(IFileSystem fileSystem)
    {
        WriteFolders(fileSystem);

        var stringBuilder = new StringBuilder();
        await WriteNorthAmericanCountries(fileSystem, stringBuilder);
        await WriteEuropeanCountries(fileSystem, stringBuilder);
        await WriteMegacities(fileSystem, stringBuilder);
        await WriteNonMegacities(fileSystem, stringBuilder);
    }

    private static void WriteFolders(IFileSystem fileSystem)
    {
        foreach (var format in SerializationFormats)
        {
            fileSystem.CreateDirectory($"/{format}/Countries/");
            fileSystem.CreateDirectory($"/{format}/Cities/");
        }
    }

    private static async Task WriteMegacities(IFileSystem fileSystem, StringBuilder stringBuilder)
    {
        #region YAML

        stringBuilder.Clear();
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 918909193");
        stringBuilder.AppendLine("Name: New York City");
        stringBuilder.AppendLine("CountryID: 1100746772");
        stringBuilder.AppendLine("...");
        fileSystem.WriteAllText("/YAML/Cities/Megacities.yaml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion YAML

        #region TOML

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 918909193");
        stringBuilder.AppendLine("name = 'New York City'");
        stringBuilder.AppendLine("country_id = 1100746772");
        fileSystem.WriteAllText("/TOML/Cities/Megacities-NewYorkCity.toml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion TOML

        #region HCL

        stringBuilder.Clear();
        stringBuilder.AppendLine("ID = 918909193");
        stringBuilder.AppendLine("Name = \"New York City\"");
        stringBuilder.AppendLine("CountryID = 1100746772");
        fileSystem.WriteAllText("/HCL/Cities/Megacities-NewYorkCity.hcl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion HCL

        #region JSON

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 918909193,");
        stringBuilder.AppendLine("  \"Name\": \"New York City\",");
        stringBuilder.AppendLine("  \"CountryID\": 1100746772");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Cities/Megacities-NewYorkCity.json", stringBuilder.ToString(), Encoding.UTF8);

        #endregion JSON

        #region PSD1

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 918909193");
        stringBuilder.AppendLine("    Name = 'New York City'");
        stringBuilder.AppendLine("    CountryID = 1100746772");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Megacities-NewYorkCity.psd1", stringBuilder.ToString(), Encoding.UTF8);

        #endregion PSD1

        #region KDL

        stringBuilder.Clear();
        stringBuilder.AppendLine("city id=918909193 name=\"New York City\" country-id=1100746772");
        fileSystem.WriteAllText("/KDL/Cities/Megacities.kdl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion KDL

        #region PARQUET

        await WriteParquetCities(fileSystem, [
            new City { ID = 918909193, Name = "New York City", CountryID = 1100746772 }
        ], "/PARQUET/Cities/Megacities.parquet");

        #endregion PARQUET
    }

    private static async Task WriteNonMegacities(IFileSystem fileSystem, StringBuilder stringBuilder)
    {
        #region YAML

        stringBuilder.Clear();
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 356389956");
        stringBuilder.AppendLine("Name: Austin");
        stringBuilder.AppendLine("CountryID: 1100746772");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1572248850");
        stringBuilder.AppendLine("Name: Toronto");
        stringBuilder.AppendLine("CountryID: 965475701");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1859443008");
        stringBuilder.AppendLine("Name: Vienna");
        stringBuilder.AppendLine("CountryID: 1419150635");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1948404451");
        stringBuilder.AppendLine("Name: Paris");
        stringBuilder.AppendLine("CountryID: 1552721979");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1062005753");
        stringBuilder.AppendLine("Name: Rome");
        stringBuilder.AppendLine("CountryID: 1501801186");
        stringBuilder.AppendLine("...");
        fileSystem.WriteAllText("/YAML/Cities/Non-Megacities.yaml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion YAML

        #region TOML

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 356389956");
        stringBuilder.AppendLine("name = 'Austin'");
        stringBuilder.AppendLine("country_id = 1100746772");
        fileSystem.WriteAllText("/TOML/Cities/Non-Megacities-Austin.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1572248850");
        stringBuilder.AppendLine("name = 'Toronto'");
        stringBuilder.AppendLine("country_id = 965475701");
        fileSystem.WriteAllText("/TOML/Cities/Non-Megacities-Toronto.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1859443008");
        stringBuilder.AppendLine("name = 'Vienna'");
        stringBuilder.AppendLine("country_id = 1419150635");
        fileSystem.WriteAllText("/TOML/Cities/Non-Megacities-Vienna.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1948404451");
        stringBuilder.AppendLine("name = 'Paris'");
        stringBuilder.AppendLine("country_id = 1552721979");
        fileSystem.WriteAllText("/TOML/Cities/Non-Megacities-Paris.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1062005753");
        stringBuilder.AppendLine("name = 'Rome'");
        stringBuilder.AppendLine("country_id = 1501801186");
        fileSystem.WriteAllText("/TOML/Cities/Non-Megacities-Rome.toml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion TOML

        #region HCL

        stringBuilder.Clear();
        stringBuilder.AppendLine("Id = 356389956");
        stringBuilder.AppendLine("Name = \"Austin\"");
        stringBuilder.AppendLine("CountryID = 1100746772");
        fileSystem.WriteAllText("/HCL/Cities/Non-Megacities-Austin.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("Id = 1572248850");
        stringBuilder.AppendLine("Name = \"Toronto\"");
        stringBuilder.AppendLine("CountryID = 965475701");
        fileSystem.WriteAllText("/HCL/Cities/Non-Megacities-Toronto.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("Id = 1859443008");
        stringBuilder.AppendLine("Name = \"Vienna\"");
        stringBuilder.AppendLine("CountryID = 1419150635");
        fileSystem.WriteAllText("/HCL/Cities/Non-Megacities-Vienna.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("Id = 1948404451");
        stringBuilder.AppendLine("Name = \"Paris\"");
        stringBuilder.AppendLine("CountryID = 1552721979");
        fileSystem.WriteAllText("/HCL/Cities/Non-Megacities-Paris.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("Id = 1062005753");
        stringBuilder.AppendLine("Name = \"Rome\"");
        stringBuilder.AppendLine("CountryID = 1501801186");
        fileSystem.WriteAllText("/HCL/Cities/Non-Megacities-Rome.hcl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion HCL

        #region JSON

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 356389956,");
        stringBuilder.AppendLine("  \"Name\": \"Austin\",");
        stringBuilder.AppendLine("  \"CountryID\": 1100746772");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Cities/Non-Megacities-Austin.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1572248850,");
        stringBuilder.AppendLine("  \"Name\": \"Toronto\",");
        stringBuilder.AppendLine("  \"CountryID\": 965475701");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Cities/Non-Megacities-Toronto.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1859443008,");
        stringBuilder.AppendLine("  \"Name\": \"Vienna\",");
        stringBuilder.AppendLine("  \"CountryID\": 1419150635");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Cities/Non-Megacities-Vienna.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1948404451,");
        stringBuilder.AppendLine("  \"Name\": \"Paris\",");
        stringBuilder.AppendLine("  \"CountryID\": 1552721979");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Cities/Non-Megacities-Paris.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1062005753,");
        stringBuilder.AppendLine("  \"Name\": \"Rome\",");
        stringBuilder.AppendLine("  \"CountryID\": 1501801186");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Cities/Non-Megacities-Rome.json", stringBuilder.ToString(), Encoding.UTF8);

        #endregion JSON

        #region PSD1

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 356389956");
        stringBuilder.AppendLine("    Name = 'Austin'");
        stringBuilder.AppendLine("    CountryID = 1100746772");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Non-Megacities-Austin.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1572248850");
        stringBuilder.AppendLine("    Name = 'Toronto'");
        stringBuilder.AppendLine("    CountryID = 965475701");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Non-Megacities-Toronto.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1859443008");
        stringBuilder.AppendLine("    Name = 'Vienna'");
        stringBuilder.AppendLine("    CountryID = 1419150635");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Non-Megacities-Vienna.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1948404451");
        stringBuilder.AppendLine("    Name = 'Paris'");
        stringBuilder.AppendLine("    CountryID = 1552721979");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Non-Megacities-Paris.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1062005753");
        stringBuilder.AppendLine("    Name = 'Rome'");
        stringBuilder.AppendLine("    CountryID = 1501801186");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Non-Megacities-Rome.psd1", stringBuilder.ToString(), Encoding.UTF8);

        #endregion PSD1

        #region KDL

        stringBuilder.Clear();
        stringBuilder.AppendLine("city id=356389956 name=\"Austin\" country-id=1100746772");
        stringBuilder.AppendLine("city id=1572248850 name=\"Toronto\" country-id=965475701");
        stringBuilder.AppendLine("city id=1859443008 name=\"Vienna\" country-id=1419150635");
        stringBuilder.AppendLine("city id=1948404451 name=\"Paris\" country-id=1552721979");
        stringBuilder.AppendLine("city id=1062005753 name=\"Rome\" country-id=1501801186");
        fileSystem.WriteAllText("/KDL/Cities/Non-Megacities.kdl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion KDL

        #region PARQUET

        await WriteParquetCities(fileSystem, [
            new City { ID = 356389956, Name = "Austin", CountryID = 1100746772 },
            new City { ID = 1572248850, Name = "Toronto", CountryID = 965475701 },
            new City { ID = 1859443008, Name = "Vienna", CountryID = 1419150635 },
            new City { ID = 1948404451, Name = "Paris", CountryID = 1552721979 },
            new City { ID = 1062005753, Name = "Rome", CountryID = 1501801186 }
        ], "/PARQUET/Cities/Non-Megacities.parquet");

        #endregion PARQUET
    }

    private static async Task WriteNorthAmericanCountries(IFileSystem fileSystem, StringBuilder stringBuilder)
    {
        #region YAML

        stringBuilder.Clear();
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 1100746772");
        stringBuilder.AppendLine("Name: United States");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("ID: 965475701");
        stringBuilder.AppendLine("Name: Canada");
        stringBuilder.AppendLine("...");
        fileSystem.WriteAllText("/YAML/Countries/NorthAmerica.yaml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion YAML

        #region TOML

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 1100746772");
        stringBuilder.AppendLine("name = 'United States'");
        fileSystem.WriteAllText("/TOML/Countries/NorthAmerica-UnitedStates.toml", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("id = 965475701");
        stringBuilder.AppendLine("name = 'Canada'");
        fileSystem.WriteAllText("/TOML/Countries/NorthAmerica-Canada.toml", stringBuilder.ToString(), Encoding.UTF8);

        #endregion TOML

        #region HCL

        stringBuilder.Clear();
        stringBuilder.AppendLine("\"id\" = 1100746772");
        stringBuilder.AppendLine("\"name\" = \"United States\"");
        fileSystem.WriteAllText("/HCL/Countries/NorthAmerica-UnitedStates.hcl", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("\"id\" = 965475701");
        stringBuilder.AppendLine("\"name\" = \"Canada\"");
        fileSystem.WriteAllText("/HCL/Countries/NorthAmerica-Canada.hcl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion HCL

        #region JSON

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 1100746772,");
        stringBuilder.AppendLine("  \"Name\": \"United States\"");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Countries/NorthAmerica-UnitedStates.json", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("  \"ID\": 965475701,");
        stringBuilder.AppendLine("  \"Name\": \"Canada\"");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/JSON/Countries/NorthAmerica-Canada.json", stringBuilder.ToString(), Encoding.UTF8);

        #endregion JSON

        #region PSD1

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 1100746772");
        stringBuilder.AppendLine("    Name = 'United States'");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Countries/NorthAmerica-UnitedStates.psd1", stringBuilder.ToString(), Encoding.UTF8);

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 965475701");
        stringBuilder.AppendLine("    Name = 'Canada'");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Countries/NorthAmerica-Canada.psd1", stringBuilder.ToString(), Encoding.UTF8);

        #endregion PSD1

        #region KDL

        stringBuilder.Clear();
        stringBuilder.AppendLine("country id=1100746772 name=\"United States\"");
        stringBuilder.AppendLine("country id=965475701 name=\"Canada\"");
        fileSystem.WriteAllText("/KDL/Countries/NorthAmerica.kdl", stringBuilder.ToString(), Encoding.UTF8);

        #endregion KDL

        #region PARQUET

        await WriteParquetCountries(fileSystem, [
            new Country { ID = 1100746772, Name = "United States" },
            new Country { ID = 965475701, Name = "Canada" }
        ], "/PARQUET/Countries/NorthAmerica.parquet");

        #endregion PARQUET
    }

    private static async Task WriteParquetCities(IFileSystem fileSystem, City[] cities, string filePath)
    {
        var schema = new ParquetSchema(
            new DataField<int>("ID"),
            new DataField<string>("Name"),
            new DataField<int>("CountryID")
        );

        await using var stream = fileSystem.OpenFile(filePath, FileMode.Create, FileAccess.Write);
        await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);

        using var rowGroup = parquetWriter.CreateRowGroup();
        await rowGroup.WriteColumnAsync(new DataColumn(schema.DataFields[0], cities.Select(c => c.ID).ToArray()));
        await rowGroup.WriteColumnAsync(new DataColumn(schema.DataFields[1], cities.Select(c => c.Name).ToArray()));
        await rowGroup.WriteColumnAsync(new DataColumn(schema.DataFields[2], cities.Select(c => c.CountryID).ToArray()));
    }

    private static async Task WriteParquetCountries(IFileSystem fileSystem, Country[] countries, string filePath)
    {
        var schema = new ParquetSchema(
            new DataField<int>("ID"),
            new DataField<string>("Name")
        );

        await using var stream = fileSystem.OpenFile(filePath, FileMode.Create, FileAccess.Write);
        await using var parquetWriter = await ParquetWriter.CreateAsync(schema, stream);

        using var rowGroup = parquetWriter.CreateRowGroup();
        await rowGroup.WriteColumnAsync(new DataColumn(schema.DataFields[0], countries.Select(c => c.ID).ToArray()));
        await rowGroup.WriteColumnAsync(new DataColumn(schema.DataFields[1], countries.Select(c => c.Name).ToArray()));
    }
}
