using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIKSN.ROFSDB.FileStorageAdapters;
using Xunit;
using Xunit.Abstractions;
using Zio;
using Zio.FileSystems;

namespace TIKSN.ROFSDB.Tests.Fixtures;

public class DatabaseEngineFixture : IAsyncLifetime
{
    private static readonly string[] SerializationFormats = ["YAML", "TOML", "HCL", "PSD1"];
    private FrozenDictionary<string, ServiceProvider> formatServiceProviders;
    private MemoryFileSystem memoryFileSystem;
    public FrozenDictionary<string, IDatabaseEngine> DatabaseEngines { get; private set; }

    public Task DisposeAsync()
    {
        memoryFileSystem?.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        this.memoryFileSystem = new();
        WriteFiles(memoryFileSystem);
        this.formatServiceProviders = SerializationFormats.ToFrozenDictionary(
            keySelector: x => x,
            elementSelector: x =>
            {
                var subFileSystem = memoryFileSystem.GetOrCreateSubFileSystem($"/{x}");
                var services = new ServiceCollection();
                services.AddSingleton<IFileSystem>(subFileSystem);
                services.AddSingleton<IFileStorage>(new ZioFileSystemToFileStorageAdapter(subFileSystem));
                services.AddROFSDB();
                return services.BuildServiceProvider();
            });
        DatabaseEngines = formatServiceProviders.ToFrozenDictionary(x => x.Key, y => y.Value.GetRequiredService<IDatabaseEngine>());
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

    private static void WriteEuropeanCountries(IFileSystem fileSystem, StringBuilder stringBuilder)
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
    }

    private static void WriteFiles(IFileSystem fileSystem)
    {
        WriteFolders(fileSystem);

        var stringBuilder = new StringBuilder();
        WriteNorthAmericanCountries(fileSystem, stringBuilder);
        WriteEuropeanCountries(fileSystem, stringBuilder);
        WriteMegacities(fileSystem, stringBuilder);
        WriteNonMegacities(fileSystem, stringBuilder);
    }

    private static void WriteFolders(IFileSystem fileSystem)
    {
        foreach (var format in SerializationFormats)
        {
            fileSystem.CreateDirectory($"/{format}/Countries/");
            fileSystem.CreateDirectory($"/{format}/Cities/");
        }
    }

    private static void WriteMegacities(IFileSystem fileSystem, StringBuilder stringBuilder)
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

        #region PSD1

        stringBuilder.Clear();
        stringBuilder.AppendLine("@{");
        stringBuilder.AppendLine("    ID = 918909193");
        stringBuilder.AppendLine("    Name = 'New York City'");
        stringBuilder.AppendLine("    CountryID = 1100746772");
        stringBuilder.AppendLine("}");
        fileSystem.WriteAllText("/PSD1/Cities/Megacities-NewYorkCity.psd1", stringBuilder.ToString(), Encoding.UTF8);

        #endregion PSD1
    }

    private static void WriteNonMegacities(IFileSystem fileSystem, StringBuilder stringBuilder)
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
    }

    private static void WriteNorthAmericanCountries(IFileSystem fileSystem, StringBuilder stringBuilder)
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
    }
}