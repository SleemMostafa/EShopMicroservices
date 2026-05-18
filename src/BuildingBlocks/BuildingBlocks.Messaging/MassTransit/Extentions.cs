using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Security.Authentication;

namespace BuildingBlocks.Messaging.MassTransit;

public sealed class MessageBrokerOptions
{
    public string Host { get; init; } = "amqp://localhost:5672";

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public bool UseSsl { get; init; }

    public string? SslServerName { get; init; }

    public int RetryLimit { get; init; } = 3;

    public int RetryInitialIntervalSeconds { get; init; } = 1;

    public int RetryIntervalIncrementSeconds { get; init; } = 2;

    public int RedeliveryFirstIntervalSeconds { get; init; } = 10;

    public int RedeliverySecondIntervalSeconds { get; init; } = 30;

    public int RedeliveryThirdIntervalSeconds { get; init; } = 60;

    public double KillSwitchTripThreshold { get; init; } = 0.15;

    public int KillSwitchActivationThreshold { get; init; } = 10;

    public int KillSwitchRestartTimeoutSeconds { get; init; } = 60;
}

public static class Extentions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        var section = configuration.GetSection("MessageBroker");
        var options = new MessageBrokerOptions
        {
            Host = section["Host"] ?? "amqp://localhost:5672",
            UserName = section["UserName"] ?? "guest",
            Password = section["Password"] ?? "guest",
            UseSsl = GetBool(section, "UseSsl", false),
            SslServerName = section["SslServerName"],
            RetryLimit = GetInt(section, "RetryLimit", 3),
            RetryInitialIntervalSeconds = GetInt(section, "RetryInitialIntervalSeconds", 1),
            RetryIntervalIncrementSeconds = GetInt(section, "RetryIntervalIncrementSeconds", 2),
            RedeliveryFirstIntervalSeconds = GetInt(section, "RedeliveryFirstIntervalSeconds", 10),
            RedeliverySecondIntervalSeconds = GetInt(section, "RedeliverySecondIntervalSeconds", 30),
            RedeliveryThirdIntervalSeconds = GetInt(section, "RedeliveryThirdIntervalSeconds", 60),
            KillSwitchTripThreshold = GetDouble(section, "KillSwitchTripThreshold", 0.15),
            KillSwitchActivationThreshold = GetInt(section, "KillSwitchActivationThreshold", 10),
            KillSwitchRestartTimeoutSeconds = GetInt(section, "KillSwitchRestartTimeoutSeconds", 60)
        };

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
                config.AddConsumers(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(options.Host), host =>
                {
                    host.Username(options.UserName);
                    host.Password(options.Password);

                    if (options.UseSsl)
                    {
                        host.UseSsl(ssl =>
                        {
                            ssl.Protocol = SslProtocols.Tls12 | SslProtocols.Tls13;

                            if (!string.IsNullOrWhiteSpace(options.SslServerName))
                            {
                                ssl.ServerName = options.SslServerName;
                            }
                        });
                    }
                });

                configurator.UseMessageRetry(retry => retry.Incremental(
                    options.RetryLimit,
                    TimeSpan.FromSeconds(options.RetryInitialIntervalSeconds),
                    TimeSpan.FromSeconds(options.RetryIntervalIncrementSeconds)));

                configurator.UseDelayedRedelivery(redelivery => redelivery.Intervals(
                    TimeSpan.FromSeconds(options.RedeliveryFirstIntervalSeconds),
                    TimeSpan.FromSeconds(options.RedeliverySecondIntervalSeconds),
                    TimeSpan.FromSeconds(options.RedeliveryThirdIntervalSeconds)));

                configurator.UseInMemoryOutbox(context);

                configurator.UseKillSwitch(killSwitch => killSwitch
                    .SetActivationThreshold(options.KillSwitchActivationThreshold)
                    .SetTripThreshold(options.KillSwitchTripThreshold)
                    .SetRestartTimeout(TimeSpan.FromSeconds(options.KillSwitchRestartTimeoutSeconds)));

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static int GetInt(IConfiguration configuration, string key, int fallback)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallback;
    }

    private static double GetDouble(IConfiguration configuration, string key, double fallback)
    {
        return double.TryParse(configuration[key], out var value) ? value : fallback;
    }

    private static bool GetBool(IConfiguration configuration, string key, bool fallback)
    {
        return bool.TryParse(configuration[key], out var value) ? value : fallback;
    }
}
