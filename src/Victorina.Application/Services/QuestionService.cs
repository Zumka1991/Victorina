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

    public async Task<IList<Question>> GetRandomQuestionsAsync(int count, int? categoryId = null)
    {
        var query = _context.Questions.Where(q => q.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(q => q.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();
    }

    public async Task<IList<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }
}
