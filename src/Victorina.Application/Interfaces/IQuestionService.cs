using Victorina.Domain.Entities;

namespace Victorina.Application.Interfaces;

public interface IQuestionService
{
    Task<IList<Question>> GetRandomQuestionsAsync(int count, int? categoryId = null, string languageCode = "ru");
    Task<Dictionary<string, List<Question>>> GetQuestionsWithTranslationsAsync(int count, IEnumerable<string> languageCodes, int? categoryId = null);
    Task<Dictionary<string, List<Question>>> GetQuestionsForMultipleLanguagesAsync(int count, IEnumerable<string> languageCodes, int? categoryId = null);
    Task<IList<Category>> GetCategoriesAsync(string languageCode = "ru");
    Task<Category?> GetCategoryAsync(int id);
    Task<string?> GetCategoryNameAsync(Guid? translationGroupId, int? categoryId, string languageCode);
    Task<List<Question>> GetQuestionsByTranslationGroupIdsAsync(IEnumerable<Guid> translationGroupIds, string languageCode);
}
