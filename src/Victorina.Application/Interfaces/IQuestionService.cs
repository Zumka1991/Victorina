using Victorina.Domain.Entities;

namespace Victorina.Application.Interfaces;

public interface IQuestionService
{
    Task<IList<Question>> GetRandomQuestionsAsync(int count, int? categoryId = null);
    Task<IList<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryAsync(int id);
}
