using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NOS.Engineering.Challenge.API.Models;
using NOS.Engineering.Challenge.Cache;
using NOS.Engineering.Challenge.Managers;
using NOS.Engineering.Challenge.Models;

namespace NOS.Engineering.Challenge.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ContentController : Controller
{
    private readonly IContentsManager _manager;
    private readonly ICacheService<Content> _cacheService;
    private readonly ILogger<ContentController> _logger;

    public ContentController(IContentsManager manager, ICacheService<Content> cacheService, ILogger<ContentController> logger)
    {
        _manager = manager;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetManyContents()
    {
        _logger.LogInformation("Attempting to retrieve contents...");

        var contents = await _manager.GetManyContents().ConfigureAwait(false);

        if (!contents.Any())
        {
            _logger.LogInformation("No contents found.");
            return NotFound();
        }

        _logger.LogInformation("Retrieved contents");
        return Ok(contents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContent(Guid id)
    {
        _logger.LogInformation("Attempting to retrieve content with ID '{Id}'...", id);
        
        var contentFromCache = await _cacheService.GetAsync(id).ConfigureAwait(false);
        if (contentFromCache != null)
        {
            _logger.LogInformation("Content with ID '{Id}' found in cache.", id);
            return Ok(contentFromCache);
        }

        var content = await _manager.GetContent(id).ConfigureAwait(false);
        if (content == null)
        {
            _logger.LogInformation("Content with ID '{Id}' not found.", id);
            return NotFound();
        }

        await _cacheService.SetAsync(content.Id, content).ConfigureAwait(false);
        _logger.LogInformation("Retrieved content with ID '{Id}'.", id);

        return Ok(content);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContent([FromBody] ContentInput content)
    {
        _logger.LogInformation("Attempting to create content...");
        var createdContent = await _manager.CreateContent(content.ToDto()).ConfigureAwait(false);

        if (createdContent == null)
        {
            _logger.LogInformation("Failed to create content.");
            return Problem();
        }

        _logger.LogInformation("Content created successfully, caching it.");
        await _cacheService.SetAsync(createdContent.Id, createdContent).ConfigureAwait(false);
        return Ok(createdContent);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateContent(Guid id, [FromBody] ContentInput content)
    {
        _logger.LogInformation("Attempting to update content with ID '{Id}'...", id);
        var updatedContent = await _manager.UpdateContent(id, content.ToDto()).ConfigureAwait(false);

        if (updatedContent == null)
        {
            _logger.LogInformation("Content with ID '{Id}' not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Content with ID '{Id}' updated successfully, updating cache...", id);
        await _cacheService.SetAsync(id, updatedContent).ConfigureAwait(false);
        return Ok(updatedContent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContent(Guid id)
    {
        _logger.LogInformation("Attempting to delete content with ID '{Id}'...", id);
        var deletedId = await _manager.DeleteContent(id).ConfigureAwait(false);

        if (deletedId == null)
        {
            _logger.LogInformation("Content with ID '{Id}' not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Content with ID '{Id}' deleted successfully, removing from cache...", id);
        await _cacheService.RemoveAsync(id).ConfigureAwait(false);

        return Ok(deletedId);
    }

    [HttpPost("{id}/genre")]
    public async Task<IActionResult> AddGenres(Guid id, [FromBody] IEnumerable<string> genre)
    {
        _logger.LogInformation("Attempting to add genres to content with ID '{Id}'...", id);

        var content = await _manager.GetContent(id).ConfigureAwait(false);
        if (content == null)
        {
            _logger.LogInformation("Content with ID '{Id}' not found.", id);
            return NotFound();
        }

        var genres = new List<string>();

        foreach (var newGenre in genre)
        {
            if (content.GenreList.Contains(newGenre))
            {
                return BadRequest(new ErrorMessage
                {
                    Error = "Genre already exists"
                });
            }
            else
            {
                genres.Add(newGenre);
            }
        }

        var dtoUpdated = new ContentDto(
            content.Title,
            content.SubTitle,
            content.Description,
            content.ImageUrl,
            content.Duration,
            content.StartTime,
            content.EndTime,
            content.GenreList.Concat(genres).ToList()
        );

        var updatedContent = await _manager.UpdateContent(id, dtoUpdated).ConfigureAwait(false);

        await _cacheService.SetAsync(id, updatedContent).ConfigureAwait(false);

        return Ok(updatedContent);
    }

    [HttpDelete("{id}/genre")]
    public async Task<IActionResult> RemoveGenres(Guid id, [FromBody] IEnumerable<string> genre)
    {
        _logger.LogInformation("Attempting to remove genres from content with ID '{Id}'...", id);

        var content = await _manager.GetContent(id).ConfigureAwait(false);

        if (content == null)
        {
            _logger.LogInformation("Content with ID '{Id}' not found.", id);
            return NotFound();
        }

        var genreList = content.GenreList.ToList();
        genreList.RemoveAll(genre.Contains);

        var contentUpdated = await _manager.UpdateContent(id, new ContentDto(
            content.Title,
            content.SubTitle,
            content.Description,
            content.ImageUrl,
            content.Duration,
            content.StartTime,
            content.EndTime,
            genreList
        )).ConfigureAwait(false);

        await _cacheService.SetAsync(id, contentUpdated).ConfigureAwait(false);

        return Ok(contentUpdated);
    }
}
