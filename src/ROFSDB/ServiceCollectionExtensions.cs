using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TIKSN.ROFSDB.FileStorageAdapters;
using TIKSN.ROFSDB.Serialization;

namespace TIKSN.ROFSDB;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddROFSDB(this IServiceCollection services)
    {
        return services.AddROFSDB<FileProviderToFileStorageAdapter>();
    }

    public static IServiceCollection AddROFSDB<TFileStorage>(this IServiceCollection services) where TFileStorage : class, IFileStorage
    {
        services.TryAddSingleton<IDatabaseEngine, DatabaseEngine>();
        services.TryAddSingleton<IFileStorage, TFileStorage>();

        services.TryAddKeyedSingleton<ISerialization, HclSerialization>("HCL");
        services.TryAddKeyedSingleton<ISerialization, TomlSerialization>("TOML");
        services.TryAddKeyedSingleton<ISerialization, YamlSerialization>("YAML");
        services.TryAddKeyedSingleton<ISerialization, PowerShellDataFileSerialization>("PSD1");

        return services;
    }

    public static IServiceCollection AddRofsDbZioFileStorage(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorage, ZioFileSystemToFileStorageAdapter>();

        return services;
    }
}