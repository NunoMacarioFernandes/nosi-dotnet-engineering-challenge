using NOS.Engineering.Challenge.Database;
using NOS.Engineering.Challenge.Models;

namespace NOS.Engineering.Challenge.Managers;

public class ContentsManager : IContentsManager
{
    private readonly IDatabase<Content?, ContentDto> _database;

    public ContentsManager(IDatabase<Content?, ContentDto> database)
    {
        _database = database;
    }

    public Task<IEnumerable<Content?>> GetManyContents()
    {
        return _database.ReadAll();
    }

    public Task<Content?> CreateContent(ContentDto content)
    {
        return _database.Create(content);
    }

    public Task<Content?> GetContent(Guid id)
    {
        return _database.Read(id);
    }

    public Task<Content?> UpdateContent(Guid id, ContentDto content)
    {
        return _database.Update(id, content);
    }

    public Task<Guid> DeleteContent(Guid id)
    {
        return _database.Delete(id);
    }

    public async Task<IEnumerable<Content?>> GetFiltered(string? title, string? genre)
    {
        var res = await _database.ReadAll().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(title))
        {
            res = res.Where(c => c?.Title?.Contains(title, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            res = res.Where(c => c?.GenreList?.Any(g => string.Equals(g, genre, StringComparison.OrdinalIgnoreCase)) ?? false);
        }

        return res;
    }
}