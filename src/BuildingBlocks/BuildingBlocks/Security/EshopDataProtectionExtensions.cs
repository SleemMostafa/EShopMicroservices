using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Security;

public static class EshopDataProtectionExtensions
{
    private const string SectionName = "DataProtection";
    private const string ContainerKeysPath = "/home/app/.aspnet/DataProtection-Keys";

    public static IServiceCollection AddEshopDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName)
    {
        var keysPath = configuration[$"{SectionName}:KeysPath"];

        if (string.IsNullOrWhiteSpace(keysPath))
        {
            keysPath = GetDefaultKeysPath();
        }

        Directory.CreateDirectory(keysPath);

        services
            .AddDataProtection()
            .SetApplicationName(applicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        return services;
    }

    private static string GetDefaultKeysPath()
    {
        if (string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return ContainerKeysPath;
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(localAppData, "EShopMicroservices", "DataProtection-Keys");
    }
}
