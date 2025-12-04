using Microsoft.EntityFrameworkCore;
using Victorina.Application;
using Victorina.Infrastructure;
using Victorina.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

// Infrastructure & Application
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

// API
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VictorinaDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// API Endpoints
app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Time = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("System");

// Categories
app.MapGet("/api/categories", async (VictorinaDbContext db) =>
{
    var categories = await db.Categories
        .Where(c => c.IsActive)
        .OrderBy(c => c.Name)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Emoji,
            QuestionsCount = c.Questions.Count(q => q.IsActive)
        })
        .ToListAsync();
    return Results.Ok(categories);
}).WithTags("Categories");

app.MapPost("/api/categories", async (VictorinaDbContext db, CategoryDto dto) =>
{
    var category = new Victorina.Domain.Entities.Category
    {
        Name = dto.Name,
        Description = dto.Description,
        Emoji = dto.Emoji,
        IsActive = true
    };
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/api/categories/{category.Id}", category);
}).WithTags("Categories");

app.MapPut("/api/categories/{id}", async (VictorinaDbContext db, int id, CategoryDto dto) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category == null) return Results.NotFound();

    category.Name = dto.Name;
    category.Description = dto.Description;
    category.Emoji = dto.Emoji;
    await db.SaveChangesAsync();
    return Results.Ok(category);
}).WithTags("Categories");

app.MapDelete("/api/categories/{id}", async (VictorinaDbContext db, int id) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category == null) return Results.NotFound();

    category.IsActive = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Categories");

// Questions
app.MapGet("/api/questions", async (VictorinaDbContext db, int? categoryId, int page = 1, int pageSize = 20) =>
{
    var query = db.Questions
        .Include(q => q.Category)
        .Where(q => q.IsActive);

    if (categoryId.HasValue)
        query = query.Where(q => q.CategoryId == categoryId.Value);

    var total = await query.CountAsync();
    var questions = await query
        .OrderByDescending(q => q.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(q => new
        {
            q.Id,
            q.Text,
            q.CorrectAnswer,
            q.WrongAnswer1,
            q.WrongAnswer2,
            q.WrongAnswer3,
            q.Explanation,
            Category = q.Category.Name,
            q.CategoryId,
            q.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(new { Items = questions, Total = total, Page = page, PageSize = pageSize });
}).WithTags("Questions");

app.MapPost("/api/questions", async (VictorinaDbContext db, QuestionDto dto) =>
{
    var question = new Victorina.Domain.Entities.Question
    {
        CategoryId = dto.CategoryId,
        Text = dto.Text,
        CorrectAnswer = dto.CorrectAnswer,
        WrongAnswer1 = dto.WrongAnswer1,
        WrongAnswer2 = dto.WrongAnswer2,
        WrongAnswer3 = dto.WrongAnswer3,
        Explanation = dto.Explanation,
        IsActive = true
    };
    db.Questions.Add(question);
    await db.SaveChangesAsync();
    return Results.Created($"/api/questions/{question.Id}", question);
}).WithTags("Questions");

app.MapPut("/api/questions/{id}", async (VictorinaDbContext db, int id, QuestionDto dto) =>
{
    var question = await db.Questions.FindAsync(id);
    if (question == null) return Results.NotFound();

    question.CategoryId = dto.CategoryId;
    question.Text = dto.Text;
    question.CorrectAnswer = dto.CorrectAnswer;
    question.WrongAnswer1 = dto.WrongAnswer1;
    question.WrongAnswer2 = dto.WrongAnswer2;
    question.WrongAnswer3 = dto.WrongAnswer3;
    question.Explanation = dto.Explanation;
    await db.SaveChangesAsync();
    return Results.Ok(question);
}).WithTags("Questions");

app.MapDelete("/api/questions/{id}", async (VictorinaDbContext db, int id) =>
{
    var question = await db.Questions.FindAsync(id);
    if (question == null) return Results.NotFound();

    question.IsActive = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Questions");

// Statistics
app.MapGet("/api/stats", async (VictorinaDbContext db) =>
{
    var stats = new
    {
        TotalUsers = await db.Users.CountAsync(),
        TotalGames = await db.Games.CountAsync(),
        TotalQuestions = await db.Questions.CountAsync(q => q.IsActive),
        TotalCategories = await db.Categories.CountAsync(c => c.IsActive),
        GamesToday = await db.Games.CountAsync(g => g.CreatedAt.Date == DateTime.UtcNow.Date)
    };
    return Results.Ok(stats);
}).WithTags("Statistics");

app.MapGet("/api/stats/top-players", async (VictorinaDbContext db, int count = 10) =>
{
    var topPlayers = await db.Users
        .OrderByDescending(u => u.GamesWon)
        .Take(count)
        .Select(u => new
        {
            u.Username,
            u.FirstName,
            u.GamesPlayed,
            u.GamesWon,
            WinRate = u.GamesPlayed > 0 ? (double)u.GamesWon / u.GamesPlayed * 100 : 0
        })
        .ToListAsync();
    return Results.Ok(topPlayers);
}).WithTags("Statistics");

// Settings
app.MapGet("/api/settings", async (VictorinaDbContext db) =>
{
    var settings = await db.GameSettings.ToListAsync();
    return Results.Ok(settings);
}).WithTags("Settings");

app.MapPut("/api/settings/{key}", async (VictorinaDbContext db, string key, SettingDto dto) =>
{
    var setting = await db.GameSettings.FirstOrDefaultAsync(s => s.Key == key);
    if (setting == null) return Results.NotFound();

    setting.Value = dto.Value;
    await db.SaveChangesAsync();
    return Results.Ok(setting);
}).WithTags("Settings");

// Seed Data
app.MapPost("/api/seed", async (VictorinaDbContext db) =>
{
    var hasData = await db.Categories.AnyAsync();
    if (hasData)
    {
        return Results.Ok(new { Message = "Данные уже существуют. Используйте /api/seed/reset для пересоздания.", CategoriesCount = await db.Categories.CountAsync(), QuestionsCount = await db.Questions.CountAsync() });
    }

    await SeedData.SeedTestDataAsync(db);
    return Results.Ok(new { Message = "Тестовые данные добавлены!", CategoriesCount = await db.Categories.CountAsync(), QuestionsCount = await db.Questions.CountAsync() });
}).WithTags("Seed");

app.MapPost("/api/seed/reset", async (VictorinaDbContext db) =>
{
    await SeedData.ClearAndReseedAsync(db);
    return Results.Ok(new { Message = "Данные пересозданы!", CategoriesCount = await db.Categories.CountAsync(), QuestionsCount = await db.Questions.CountAsync() });
}).WithTags("Seed");

app.Run();

// DTOs
record CategoryDto(string Name, string? Description, string? Emoji);
record QuestionDto(int CategoryId, string Text, string CorrectAnswer, string WrongAnswer1, string WrongAnswer2, string WrongAnswer3, string? Explanation);
record SettingDto(string Value);
