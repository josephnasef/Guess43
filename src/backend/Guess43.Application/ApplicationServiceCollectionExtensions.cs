using FluentValidation;
using Guess43.Application.Authentication;
using Guess43.Application.Games;
using Guess43.Application.Statistics;
using Guess43.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Guess43.Application;

/// <summary>Registers Application use-case handlers and validators.</summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Authentication
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();

        // Users
        services.AddScoped<GetProfileHandler>();
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<DeleteAccountHandler>();

        // Games
        services.AddScoped<StartGameHandler>();
        services.AddScoped<SubmitGuessHandler>();
        services.AddScoped<GetActiveGameHandler>();
        services.AddScoped<GetGameHistoryHandler>();
        services.AddScoped<GetGameByIdHandler>();
        services.AddScoped<DeleteGameHandler>();

        // Statistics (bonus)
        services.AddScoped<GetPerformanceHandler>();

        services.AddValidatorsFromAssembly(
            typeof(ApplicationServiceCollectionExtensions).Assembly,
            ServiceLifetime.Singleton);

        return services;
    }
}
