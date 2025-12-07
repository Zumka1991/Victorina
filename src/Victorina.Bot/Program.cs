using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Victorina.Application;
using Victorina.Bot;
using Victorina.Bot.Handlers;
using Victorina.Bot.Services;
using Victorina.Infrastructure;
using Victorina.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<BotConfiguration>(
    builder.Configuration.GetSection("Bot"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

// Infrastructure
builder.Services.AddInfrastructure(connectionString);

// Application
builder.Services.AddApplication();

// Bot services
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var config = builder.Configuration.GetSection("Bot").Get<BotConfiguration>()!;
    return new TelegramBotClient(config.BotToken);
});

builder.Services.AddSingleton<KeyboardService>();
builder.Services.AddSingleton<UserStateService>();
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddHostedService<BotService>();
builder.Services.AddHostedService<QuestionTimeoutService>();
builder.Services.AddHostedService<BotMatchmakingNotificationService>();
builder.Services.AddHostedService<BotGameProgressService>();

var host = builder.Build();

// Apply migrations on startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VictorinaDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
