using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Victorina.Infrastructure.Data;
using Victorina.Domain.Entities;

namespace Victorina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly VictorinaDbContext _context;
    private readonly ILogger<TranslationController> _logger;
    private static readonly HttpClient _httpClient = new();

    private static readonly string[] SupportedLanguages = { "ru", "hi", "pt", "fa", "de", "uz", "en" };

    public TranslationController(VictorinaDbContext context, ILogger<TranslationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("auto-translate")]
    public async Task AutoTranslate()
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            // Find all Russian questions
            var russianQuestions = await _context.Questions
                .Where(q => q.LanguageCode == "ru" && q.TranslationGroupId != null)
                .ToListAsync();

            await SendEvent("progress", new {
                message = $"Found {russianQuestions.Count} Russian questions",
                total = russianQuestions.Count,
                current = 0
            });

            int translated = 0;
            int skipped = 0;
            int failed = 0;

            for (int i = 0; i < russianQuestions.Count; i++)
            {
                var ruQuestion = russianQuestions[i];

                // Check which translations are missing
                var existingTranslations = await _context.Questions
                    .Where(q => q.TranslationGroupId == ruQuestion.TranslationGroupId)
                    .Select(q => q.LanguageCode)
                    .ToListAsync();

                var missingLanguages = SupportedLanguages
                    .Where(lang => lang != "ru" && !existingTranslations.Contains(lang))
                    .ToList();

                if (missingLanguages.Count == 0)
                {
                    skipped++;
                    await SendEvent("log", new {
                        message = $"Skipped: \"{ruQuestion.Text}\" (already has all translations)",
                        type = "skip"
                    });
                    continue;
                }

                await SendEvent("log", new {
                    message = $"Translating ({i+1}/{russianQuestions.Count}): \"{ruQuestion.Text}\" to {string.Join(", ", missingLanguages)}",
                    type = "translating"
                });

                // Get category for this question
                var category = await _context.Categories.FindAsync(ruQuestion.CategoryId);

                if (category?.TranslationGroupId == null)
                {
                    _logger.LogWarning("Question {QuestionId} has no category or translation group", ruQuestion.Id);
                    skipped++;
                    continue;
                }

                var categoryTranslationGroupId = category.TranslationGroupId;

                foreach (var targetLang in missingLanguages)
                {
                    try
                    {
                        // Find target category with same translation group
                        var targetCategory = await _context.Categories
                            .FirstOrDefaultAsync(c =>
                                c.LanguageCode == targetLang &&
                                c.TranslationGroupId == categoryTranslationGroupId);

                        if (targetCategory == null)
                        {
                            _logger.LogWarning("No category found for language {Lang}", targetLang);
                            continue;
                        }

                        // Translate all fields
                        var translatedText = await TranslateText(ruQuestion.Text, "ru", targetLang);
                        var translatedCorrect = await TranslateText(ruQuestion.CorrectAnswer, "ru", targetLang);
                        var translatedWrong1 = await TranslateText(ruQuestion.WrongAnswer1, "ru", targetLang);
                        var translatedWrong2 = await TranslateText(ruQuestion.WrongAnswer2, "ru", targetLang);
                        var translatedWrong3 = await TranslateText(ruQuestion.WrongAnswer3, "ru", targetLang);
                        var translatedExplanation = string.IsNullOrEmpty(ruQuestion.Explanation)
                            ? null
                            : await TranslateText(ruQuestion.Explanation, "ru", targetLang);

                        // Create translated question
                        var newQuestion = new Question
                        {
                            CategoryId = targetCategory.Id,
                            LanguageCode = targetLang,
                            TranslationGroupId = ruQuestion.TranslationGroupId,
                            Text = translatedText,
                            CorrectAnswer = translatedCorrect,
                            WrongAnswer1 = translatedWrong1,
                            WrongAnswer2 = translatedWrong2,
                            WrongAnswer3 = translatedWrong3,
                            Explanation = translatedExplanation,
                            ImageUrl = ruQuestion.ImageUrl
                        };

                        _context.Questions.Add(newQuestion);
                        await _context.SaveChangesAsync();

                        translated++;

                        // Small delay to avoid rate limiting
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to translate question to {Lang}", targetLang);
                        failed++;
                        await SendEvent("log", new {
                            message = $"Failed to translate to {targetLang}: {ex.Message}",
                            type = "error"
                        });
                    }
                }

                await SendEvent("progress", new {
                    message = $"Processed {i + 1}/{russianQuestions.Count}",
                    total = russianQuestions.Count,
                    current = i + 1
                });
            }

            await SendEvent("complete", new {
                translated,
                skipped,
                failed,
                message = $"Translation complete! Translated: {translated}, Skipped: {skipped}, Failed: {failed}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto-translate");
            await SendEvent("error", new { error = ex.Message });
        }
    }

    private async Task<string> TranslateText(string text, string sourceLang, string targetLang)
    {
        var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={sourceLang}|{targetLang}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MyMemoryResponse>(content);

        return result?.responseData?.translatedText?.Trim() ?? text;
    }

    private async Task SendEvent(string eventType, object data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(data, options);
        await Response.WriteAsync($"event: {eventType}\n");
        await Response.WriteAsync($"data: {json}\n\n");
        await Response.Body.FlushAsync();
    }

    private class MyMemoryResponse
    {
        public ResponseData? responseData { get; set; }
    }

    private class ResponseData
    {
        public string? translatedText { get; set; }
    }
}
