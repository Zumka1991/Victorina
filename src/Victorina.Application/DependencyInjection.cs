using Microsoft.Extensions.DependencyInjection;
using Victorina.Application.Interfaces;
using Victorina.Application.Services;

namespace Victorina.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<GameSessionStore>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IBotService, BotService>();

        // Background services for bot functionality
        services.AddHostedService<MatchmakingTimeoutService>();
        services.AddHostedService<BotAnswerService>();
        services.AddHostedService<BotReadyService>();

        return services;
    }
}
