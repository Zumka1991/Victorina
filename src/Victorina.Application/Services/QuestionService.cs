using Microsoft.EntityFrameworkCore;
using Victorina.Application.Interfaces;
using Victorina.Domain.Entities;
using Victorina.Infrastructure.Data;

namespace Victorina.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly VictorinaDbContext _context;

    public QuestionService(VictorinaDbContext context)
    {
        _context = context;
    }

    public async Task<IList<Question>> GetRandomQuestionsAsync(int count, int? categoryId = null, string languageCode = "ru")
    {
        var query = _context.Questions.Where(q => q.IsActive && q.LanguageCode == languageCode);

        if (categoryId.HasValue)
        {
            // Get category to check for TranslationGroupId
            var category = await _context.Categories.FindAsync(categoryId.Value);
            if (category?.TranslationGroupId != null)
            {
                // Find category with same TranslationGroupId in the requested language
                var targetCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.TranslationGroupId == category.TranslationGroupId && c.LanguageCode == languageCode);
                if (targetCategory != null)
                {
                    query = query.Where(q => q.CategoryId == targetCategory.Id);
                }
                else
                {
                    // Fallback to original category if no translation found
                    query = query.Where(q => q.CategoryId == categoryId.Value);
                }
            }
            else
            {
                query = query.Where(q => q.CategoryId == categoryId.Value);
            }
        }

        return await query
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Gets questions that have translations in ALL specified languages.
    /// Returns a dictionary mapping language code to list of questions for that language.
    /// Questions are matched by TranslationGroupId.
    /// </summary>
    public async Task<Dictionary<string, List<Question>>> GetQuestionsWithTranslationsAsync(
        int count,
        IEnumerable<string> languageCodes,
        int? categoryId = null)
    {
        var languages = languageCodes.ToList();
        if (languages.Count == 0) return new Dictionary<string, List<Question>>();

        // Find TranslationGroupIds that have translations in ALL requested languages
        var query = _context.Questions
            .Where(q => q.IsActive && q.TranslationGroupId != null && languages.Contains(q.LanguageCode));

        // Filter by category's TranslationGroupId for cross-language support
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            if (category?.TranslationGroupId != null)
            {
                // Get all category IDs in the same translation group
                var relatedCategoryIds = await _context.Categories
                    .Where(c => c.TranslationGroupId == category.TranslationGroupId)
                    .Select(c => c.Id)
                    .ToListAsync();
                query = query.Where(q => relatedCategoryIds.Contains(q.CategoryId));
            }
            else
            {
                query = query.Where(q => q.CategoryId == categoryId.Value);
            }
        }

        // Group by TranslationGroupId and filter only groups that have all languages
        var questionsByGroup = await query.ToListAsync();

        var completeGroups = questionsByGroup
            .GroupBy(q => q.TranslationGroupId!.Value)
            .Where(g => languages.All(lang => g.Any(q => q.LanguageCode == lang)))
            .OrderBy(_ => Random.Shared.Next()) // Random order
            .Take(count)
            .ToList();

        // Build result dictionary
        var result = languages.ToDictionary(
            lang => lang,
            lang => completeGroups
                .Select(g => g.First(q => q.LanguageCode == lang))
                .ToList()
        );

        return result;
    }

    /// <summary>
    /// Gets questions for multiple languages.
    /// If TranslationGroupId is available, matches questions across languages.
    /// Falls back to random questions per language if no translations exist.
    /// </summary>
    public async Task<Dictionary<string, List<Question>>> GetQuestionsForMultipleLanguagesAsync(
        int count,
        IEnumerable<string> languageCodes,
        int? categoryId = null)
    {
        var languages = languageCodes.Distinct().ToList();

        // First try to get questions with translations
        var withTranslations = await GetQuestionsWithTranslationsAsync(count, languages, categoryId);

        // Use translated questions if we have any (even if fewer than requested count)
        // This ensures both players get the SAME questions in their respective languages
        if (withTranslations.Count > 0 && withTranslations.Values.First().Count > 0)
        {
            return withTranslations;
        }

        // Fallback: get random questions per language (different questions for each player)
        // This only happens if NO translated questions exist at all
        var result = new Dictionary<string, List<Question>>();
        foreach (var lang in languages)
        {
            var questions = await GetRandomQuestionsAsync(count, categoryId, lang);
            result[lang] = questions.ToList();
        }
        return result;
    }

    public async Task<IList<Category>> GetCategoriesAsync(string languageCode = "ru")
    {
        return await _context.Categories
            .Where(c => c.IsActive && c.LanguageCode == languageCode)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    /// <summary>
    /// Gets category name in the specified language.
    /// Uses TranslationGroupId to find the category translation.
    /// Falls back to original category name if no translation found.
    /// </summary>
    public async Task<string?> GetCategoryNameAsync(Guid? translationGroupId, int? categoryId, string languageCode)
    {
        // First try to find by TranslationGroupId
        if (translationGroupId.HasValue)
        {
            var translatedCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.TranslationGroupId == translationGroupId && c.LanguageCode == languageCode);
            if (translatedCategory != null)
            {
                return translatedCategory.Name;
            }
        }

        // Fallback to original category
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            return category?.Name;
        }

        return null;
    }

    /// <summary>
    /// Gets questions by their TranslationGroupIds in the specified language.
    /// Returns questions in the same order as the input TranslationGroupIds.
    /// </summary>
    public async Task<List<Question>> GetQuestionsByTranslationGroupIdsAsync(IEnumerable<Guid> translationGroupIds, string languageCode)
    {
        var groupIds = translationGroupIds.ToList();
        if (groupIds.Count == 0) return new List<Question>();

        var questions = await _context.Questions
            .Where(q => q.IsActive && q.TranslationGroupId != null &&
                        groupIds.Contains(q.TranslationGroupId.Value) &&
                        q.LanguageCode == languageCode)
            .ToListAsync();

        // Return in the same order as input TranslationGroupIds
        var questionsByGroup = questions.ToDictionary(q => q.TranslationGroupId!.Value);
        return groupIds
            .Where(gid => questionsByGroup.ContainsKey(gid))
            .Select(gid => questionsByGroup[gid])
            .ToList();
    }
}
