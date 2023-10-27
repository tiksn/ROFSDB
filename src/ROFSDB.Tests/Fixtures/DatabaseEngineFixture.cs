using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIKSN.ROFSDB.FileStorageAdapters;
using TIKSN.ROFSDB.Serialization;
using Xunit;
using Xunit.Abstractions;
using Zio;
using Zio.FileSystems;

namespace TIKSN.ROFSDB.Tests.Fixtures
{
    public class DatabaseEngineFixture : IAsyncLifetime
    {
        private MemoryFileSystem memoryFileSystem;

        public IReadOnlyDictionary<string, IDatabaseEngine> DatabaseEngines { get; private set; }

        public Task DisposeAsync()
        {
            memoryFileSystem?.Dispose();
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            this.memoryFileSystem = new();
            WriteFiles(this.memoryFileSystem);
            var yamlSerialization = new YamlSerialization();
            var tomlSerialization = new TomlSerialization();
            var hclSerialization = new HclSerialization();
            var databaseEngines = new Dictionary<string, IDatabaseEngine>();
            databaseEngines["YAML"] = new DatabaseEngine(new ZioFileSystemToFileStorageAdapter(this.memoryFileSystem), yamlSerialization);
            databaseEngines["TOML"] = new DatabaseEngine(new ZioFileSystemToFileStorageAdapter(this.memoryFileSystem), tomlSerialization);
            databaseEngines["HCL"] = new DatabaseEngine(new ZioFileSystemToFileStorageAdapter(this.memoryFileSystem), hclSerialization);
            DatabaseEngines = databaseEngines;
        }

        public void WriteFilesAndFoldersToTestOutput(ITestOutputHelper testOutputHelper)
        {
            var items = memoryFileSystem
                .EnumerateItems(UPath.Root, System.IO.SearchOption.AllDirectories)
                .OrderBy(x => x.FullName)
                .ToArray();

            foreach (var item in items)
            {
                var itemKind = item.IsDirectory ? "Folder" : "File";
                testOutputHelper.WriteLine($"{itemKind}: {item.FullName}");
            }
        }

        private static void WriteEuropeanCountries(MemoryFileSystem memoryFileSystem, StringBuilder stringBuilder)
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
            memoryFileSystem.WriteAllText("/Countries/Europe.yaml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion YAML

            #region TOML

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1419150635");
            stringBuilder.AppendLine("name = 'Austria'");
            memoryFileSystem.WriteAllText("/Countries/Europe-Austria.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1552721979");
            stringBuilder.AppendLine("name = 'France'");
            memoryFileSystem.WriteAllText("/Countries/Europe-France.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1501801186");
            stringBuilder.AppendLine("name = 'Italy'");
            memoryFileSystem.WriteAllText("/Countries/Europe-Italy.toml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion TOML

            #region HCL

            stringBuilder.Clear();
            stringBuilder.AppendLine("\"id\" = 1419150635");
            stringBuilder.AppendLine("\"name\" = \"Austria\"");
            memoryFileSystem.WriteAllText("/Countries/Europe-Austria.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("\"id\" = 1552721979");
            stringBuilder.AppendLine("\"name\" = \"France\"");
            memoryFileSystem.WriteAllText("/Countries/Europe-France.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("\"id\" = 1501801186");
            stringBuilder.AppendLine("\"name\" = \"Italy\"");
            memoryFileSystem.WriteAllText("/Countries/Europe-Italy.hcl", stringBuilder.ToString(), Encoding.UTF8);

            #endregion HCL
        }

        private static void WriteFiles(MemoryFileSystem memoryFileSystem)
        {
            WriteFolders(memoryFileSystem);

            var stringBuilder = new StringBuilder();
            WriteNorthAmericanCountries(memoryFileSystem, stringBuilder);
            WriteEuropeanCountries(memoryFileSystem, stringBuilder);
            WriteMegacities(memoryFileSystem, stringBuilder);
            WriteNonMegacities(memoryFileSystem, stringBuilder);
        }

        private static void WriteFolders(MemoryFileSystem memoryFileSystem)
        {
            memoryFileSystem.CreateDirectory("/Countries/");
            memoryFileSystem.CreateDirectory("/Cities/");
        }

        private static void WriteMegacities(MemoryFileSystem memoryFileSystem, StringBuilder stringBuilder)
        {
            #region YAML

            stringBuilder.Clear();
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("ID: 918909193");
            stringBuilder.AppendLine("Name: New York City");
            stringBuilder.AppendLine("CountryID: 1100746772");
            stringBuilder.AppendLine("...");
            memoryFileSystem.WriteAllText("/Cities/Megacities.yaml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion YAML

            #region TOML

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 918909193");
            stringBuilder.AppendLine("name = 'New York City'");
            stringBuilder.AppendLine("country_id = 1100746772");
            memoryFileSystem.WriteAllText("/Cities/Megacities-NewYorkCity.toml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion TOML

            #region HCL

            stringBuilder.Clear();
            stringBuilder.AppendLine("ID = 918909193");
            stringBuilder.AppendLine("Name = \"New York City\"");
            stringBuilder.AppendLine("CountryID = 1100746772");
            memoryFileSystem.WriteAllText("/Cities/Megacities-NewYorkCity.hcl", stringBuilder.ToString(), Encoding.UTF8);

            #endregion HCL
        }

        private static void WriteNonMegacities(MemoryFileSystem memoryFileSystem, StringBuilder stringBuilder)
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
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities.yaml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion YAML

            #region TOML

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 356389956");
            stringBuilder.AppendLine("name = 'Austin'");
            stringBuilder.AppendLine("country_id = 1100746772");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Austin.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1572248850");
            stringBuilder.AppendLine("name = 'Toronto'");
            stringBuilder.AppendLine("country_id = 965475701");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Toronto.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1859443008");
            stringBuilder.AppendLine("name = 'Vienna'");
            stringBuilder.AppendLine("country_id = 1419150635");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Vienna.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1948404451");
            stringBuilder.AppendLine("name = 'Paris'");
            stringBuilder.AppendLine("country_id = 1552721979");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Paris.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1062005753");
            stringBuilder.AppendLine("name = 'Rome'");
            stringBuilder.AppendLine("country_id = 1501801186");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Rome.toml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion TOML

            #region HCL

            stringBuilder.Clear();
            stringBuilder.AppendLine("Id = 356389956");
            stringBuilder.AppendLine("Name = \"Austin\"");
            stringBuilder.AppendLine("CountryID = 1100746772");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Austin.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("Id = 1572248850");
            stringBuilder.AppendLine("Name = \"Toronto\"");
            stringBuilder.AppendLine("CountryID = 965475701");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Toronto.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("Id = 1859443008");
            stringBuilder.AppendLine("Name = \"Vienna\"");
            stringBuilder.AppendLine("CountryID = 1419150635");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Vienna.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("Id = 1948404451");
            stringBuilder.AppendLine("Name = \"Paris\"");
            stringBuilder.AppendLine("CountryID = 1552721979");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Paris.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("Id = 1062005753");
            stringBuilder.AppendLine("Name = \"Rome\"");
            stringBuilder.AppendLine("CountryID = 1501801186");
            memoryFileSystem.WriteAllText("/Cities/Non-Megacities-Rome.hcl", stringBuilder.ToString(), Encoding.UTF8);

            #endregion HCL
        }

        private static void WriteNorthAmericanCountries(MemoryFileSystem memoryFileSystem, StringBuilder stringBuilder)
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
            memoryFileSystem.WriteAllText("/Countries/NorthAmerica.yaml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion YAML

            #region TOML

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 1100746772");
            stringBuilder.AppendLine("name = 'United States'");
            memoryFileSystem.WriteAllText("/Countries/NorthAmerica-UnitedStates.toml", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("id = 965475701");
            stringBuilder.AppendLine("name = 'Canada'");
            memoryFileSystem.WriteAllText("/Countries/NorthAmerica-Canada.toml", stringBuilder.ToString(), Encoding.UTF8);

            #endregion TOML

            #region HCL

            stringBuilder.Clear();
            stringBuilder.AppendLine("\"id\" = 1100746772");
            stringBuilder.AppendLine("\"name\" = \"United States\"");
            memoryFileSystem.WriteAllText("/Countries/NorthAmerica-UnitedStates.hcl", stringBuilder.ToString(), Encoding.UTF8);

            stringBuilder.Clear();
            stringBuilder.AppendLine("\"id\" = 965475701");
            stringBuilder.AppendLine("\"name\" = \"Canada\"");
            memoryFileSystem.WriteAllText("/Countries/NorthAmerica-Canada.hcl", stringBuilder.ToString(), Encoding.UTF8);

            #endregion HCL
        }
    }
}