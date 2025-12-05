using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Victorina.Application;
using Victorina.Infrastructure;
using Victorina.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Ensure uploads directory exists
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");

// Infrastructure & Application
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

// API
builder.Services.AddControllers(); // Add controller support
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
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

// Static files for uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// API Endpoints
app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy", Time = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("System");

// File Upload
app.MapPost("/api/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest(new { Error = "Expected multipart/form-data" });

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file == null || file.Length == 0)
        return Results.BadRequest(new { Error = "No file uploaded" });

    // Validate file type
    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
    if (!allowedTypes.Contains(file.ContentType.ToLower()))
        return Results.BadRequest(new { Error = "Only images are allowed (jpeg, png, gif, webp)" });

    // Max 5MB
    if (file.Length > 5 * 1024 * 1024)
        return Results.BadRequest(new { Error = "File size must be less than 5MB" });

    // Generate unique filename
    var extension = Path.GetExtension(file.FileName).ToLower();
    var fileName = $"{Guid.NewGuid()}{extension}";
    var filePath = Path.Combine(uploadsPath, fileName);

    await using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    var url = $"/uploads/{fileName}";
    return Results.Json(new { url, fileName });
}).WithTags("Upload").DisableAntiforgery();

// Categories
app.MapGet("/api/categories", async (VictorinaDbContext db, string? languageCode, string? categoryGroup) =>
{
    var query = db.Categories.Where(c => c.IsActive);

    if (!string.IsNullOrEmpty(languageCode))
        query = query.Where(c => c.LanguageCode == languageCode);

    if (!string.IsNullOrEmpty(categoryGroup))
        query = query.Where(c => c.CategoryGroup == categoryGroup);

    var categories = await query
        .OrderBy(c => c.TranslationGroupId)
        .ThenBy(c => c.LanguageCode)
        .ThenBy(c => c.Name)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Emoji,
            c.LanguageCode,
            c.TranslationGroupId,
            c.CategoryGroup,
            QuestionsCount = c.Questions.Count(q => q.IsActive)
        })
        .ToListAsync();
    return Results.Ok(categories);
}).WithTags("Categories");

app.MapPost("/api/categories", async (VictorinaDbContext db, CategoryDto dto) =>
{
    // If TranslationGroupId is provided, use it; otherwise generate a new one
    var translationGroupId = dto.TranslationGroupId ?? Guid.NewGuid();

    var category = new Victorina.Domain.Entities.Category
    {
        Name = dto.Name,
        Description = dto.Description,
        Emoji = dto.Emoji,
        LanguageCode = dto.LanguageCode ?? "ru",
        TranslationGroupId = translationGroupId,
        CategoryGroup = dto.CategoryGroup ?? "general",
        IsActive = true
    };
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/api/categories/{category.Id}", new
    {
        category.Id,
        category.Name,
        category.Description,
        category.Emoji,
        category.LanguageCode,
        category.TranslationGroupId,
        category.CategoryGroup
    });
}).WithTags("Categories");

app.MapPut("/api/categories/{id}", async (VictorinaDbContext db, int id, CategoryDto dto) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category == null) return Results.NotFound();

    category.Name = dto.Name;
    category.Description = dto.Description;
    category.Emoji = dto.Emoji;
    category.LanguageCode = dto.LanguageCode ?? category.LanguageCode;
    category.CategoryGroup = dto.CategoryGroup ?? category.CategoryGroup;
    if (dto.TranslationGroupId.HasValue)
        category.TranslationGroupId = dto.TranslationGroupId;
    await db.SaveChangesAsync();
    return Results.Ok(new
    {
        category.Id,
        category.Name,
        category.Description,
        category.Emoji,
        category.LanguageCode,
        category.TranslationGroupId,
        category.CategoryGroup
    });
}).WithTags("Categories");

app.MapDelete("/api/categories/{id}", async (VictorinaDbContext db, int id) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category == null) return Results.NotFound();

    category.IsActive = false;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Categories");

// Get all translations for a category group
app.MapGet("/api/categories/translations/{translationGroupId}", async (VictorinaDbContext db, Guid translationGroupId) =>
{
    var translations = await db.Categories
        .Where(c => c.IsActive && c.TranslationGroupId == translationGroupId)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Emoji,
            c.LanguageCode,
            c.TranslationGroupId,
            c.CategoryGroup,
            QuestionsCount = c.Questions.Count(q => q.IsActive)
        })
        .ToListAsync();
    return Results.Ok(translations);
}).WithTags("Categories");

// Get user's categories (categories they've played)
app.MapGet("/api/categories/user/{telegramId}", async (VictorinaDbContext db, long telegramId, string? languageCode) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    if (user == null) return Results.Ok(new List<object>());

    // Get categories from games user participated in
    var categoriesQuery = db.Games
        .Where(g => g.Players.Any(p => p.UserId == user.Id))
        .Where(g => g.CategoryId.HasValue)
        .Select(g => g.Category!)
        .Distinct();

    if (!string.IsNullOrEmpty(languageCode))
        categoriesQuery = categoriesQuery.Where(c => c.LanguageCode == languageCode);

    var categories = await categoriesQuery
        .Where(c => c.IsActive)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Emoji,
            c.LanguageCode,
            c.TranslationGroupId,
            c.CategoryGroup,
            QuestionsCount = c.Questions.Count(q => q.IsActive)
        })
        .ToListAsync();

    return Results.Ok(categories);
}).WithTags("Categories");

// Questions
app.MapGet("/api/questions", async (VictorinaDbContext db, int? categoryId, string? languageCode, string? search, int page = 1, int pageSize = 20) =>
{
    var query = db.Questions
        .Include(q => q.Category)
        .Where(q => q.IsActive);

    if (categoryId.HasValue)
        query = query.Where(q => q.CategoryId == categoryId.Value);

    if (!string.IsNullOrEmpty(languageCode))
        query = query.Where(q => q.LanguageCode == languageCode);

    if (!string.IsNullOrEmpty(search))
        query = query.Where(q => q.Text.Contains(search) ||
                                 q.CorrectAnswer.Contains(search) ||
                                 q.WrongAnswer1.Contains(search) ||
                                 q.WrongAnswer2.Contains(search) ||
                                 q.WrongAnswer3.Contains(search));

    var total = await query.CountAsync();
    var questions = await query
        .OrderBy(q => q.TranslationGroupId)
        .ThenBy(q => q.LanguageCode)
        .ThenByDescending(q => q.CreatedAt)
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
            q.ImageUrl,
            q.LanguageCode,
            q.TranslationGroupId,
            Category = q.Category.Name,
            q.CategoryId,
            q.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(new { Items = questions, Total = total, Page = page, PageSize = pageSize });
}).WithTags("Questions");

app.MapPost("/api/questions", async (VictorinaDbContext db, QuestionDto dto) =>
{
    // If TranslationGroupId is provided, use it; otherwise generate a new one for this question
    var translationGroupId = dto.TranslationGroupId ?? Guid.NewGuid();

    var question = new Victorina.Domain.Entities.Question
    {
        CategoryId = dto.CategoryId,
        Text = dto.Text,
        CorrectAnswer = dto.CorrectAnswer,
        WrongAnswer1 = dto.WrongAnswer1,
        WrongAnswer2 = dto.WrongAnswer2,
        WrongAnswer3 = dto.WrongAnswer3,
        Explanation = dto.Explanation,
        ImageUrl = dto.ImageUrl,
        LanguageCode = dto.LanguageCode ?? "ru",
        TranslationGroupId = translationGroupId,
        IsActive = true
    };
    db.Questions.Add(question);
    await db.SaveChangesAsync();
    return Results.Created($"/api/questions/{question.Id}", new
    {
        question.Id,
        question.CategoryId,
        question.Text,
        question.CorrectAnswer,
        question.WrongAnswer1,
        question.WrongAnswer2,
        question.WrongAnswer3,
        question.Explanation,
        question.ImageUrl,
        question.LanguageCode,
        question.TranslationGroupId
    });
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
    question.ImageUrl = dto.ImageUrl;
    question.LanguageCode = dto.LanguageCode ?? question.LanguageCode;
    if (dto.TranslationGroupId.HasValue)
        question.TranslationGroupId = dto.TranslationGroupId;
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

// Get all translations for a question group
app.MapGet("/api/questions/translations/{translationGroupId}", async (VictorinaDbContext db, Guid translationGroupId) =>
{
    var translations = await db.Questions
        .Include(q => q.Category)
        .Where(q => q.IsActive && q.TranslationGroupId == translationGroupId)
        .Select(q => new
        {
            q.Id,
            q.Text,
            q.CorrectAnswer,
            q.WrongAnswer1,
            q.WrongAnswer2,
            q.WrongAnswer3,
            q.Explanation,
            q.ImageUrl,
            q.LanguageCode,
            q.TranslationGroupId,
            Category = q.Category.Name,
            q.CategoryId,
            q.CreatedAt
        })
        .ToListAsync();
    return Results.Ok(translations);
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

// Leaderboard
app.MapGet("/api/leaderboard", async (VictorinaDbContext db, string sort = "wins", int page = 1, int pageSize = 20) =>
{
    var query = db.Users.Where(u => u.GamesPlayed > 0);

    query = sort switch
    {
        "winrate" => query.OrderByDescending(u => u.GamesPlayed > 0 ? (double)u.GamesWon / u.GamesPlayed : 0)
                          .ThenByDescending(u => u.GamesWon),
        "games" => query.OrderByDescending(u => u.GamesPlayed),
        "correct" => query.OrderByDescending(u => u.TotalCorrectAnswers),
        _ => query.OrderByDescending(u => u.GamesWon)
    };

    var total = await query.CountAsync();
    var players = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new
        {
            u.Id,
            u.TelegramId,
            u.Username,
            u.FirstName,
            u.LastName,
            u.GamesPlayed,
            u.GamesWon,
            u.TotalCorrectAnswers,
            WinRate = u.GamesPlayed > 0 ? Math.Round((double)u.GamesWon / u.GamesPlayed * 100, 1) : 0
        })
        .ToListAsync();

    return Results.Ok(new { Items = players, Total = total, Page = page, PageSize = pageSize });
}).WithTags("Leaderboard");

app.MapGet("/api/leaderboard/user/{telegramId}", async (VictorinaDbContext db, long telegramId) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    if (user == null) return Results.NotFound();

    var rank = await db.Users
        .CountAsync(u => u.GamesWon > user.GamesWon ||
                        (u.GamesWon == user.GamesWon && u.GamesPlayed < user.GamesPlayed));

    return Results.Ok(new
    {
        user.Id,
        user.TelegramId,
        user.Username,
        user.FirstName,
        user.GamesPlayed,
        user.GamesWon,
        user.TotalCorrectAnswers,
        WinRate = user.GamesPlayed > 0 ? Math.Round((double)user.GamesWon / user.GamesPlayed * 100, 1) : 0,
        Rank = rank + 1
    });
}).WithTags("Leaderboard");

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

// Map controllers
app.MapControllers();

app.Run();

// DTOs
record CategoryDto(string Name, string? Description, string? Emoji, string? LanguageCode, Guid? TranslationGroupId, string? CategoryGroup);
record QuestionDto(int CategoryId, string Text, string CorrectAnswer, string WrongAnswer1, string WrongAnswer2, string WrongAnswer3, string? Explanation, string? ImageUrl, string? LanguageCode, Guid? TranslationGroupId);
record SettingDto(string Value);
