using Guess43.Application.Abstractions;
using Guess43.Infrastructure.Persistence;
using Guess43.Infrastructure.Persistence.Repositories;
using Guess43.Infrastructure.Security;
using Guess43.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guess43.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public const string ConnectionStringName = "Postgres";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not configured.");

        connectionString = NormalizeConnectionString(connectionString);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ISecretNumberGenerator, CryptoSecretNumberGenerator>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "Jwt:SigningKey must be configured.")
            .Validate(o => o.AccessTokenMinutes > 0, "Jwt:AccessTokenMinutes must be positive.")
            .ValidateOnStart();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .Validate(o => o.Days > 0, "RefreshToken:Days must be positive.")
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Accepts either an Npgsql keyword connection string or a postgres:// URI
    /// (as provided by managed hosts such as Render) and returns Npgsql keyword form.
    /// </summary>
    internal static string NormalizeConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var database = uri.AbsolutePath.TrimStart('/');
        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = database,
            SslMode = Npgsql.SslMode.Require,
        };
        return builder.ConnectionString;
    }
}
