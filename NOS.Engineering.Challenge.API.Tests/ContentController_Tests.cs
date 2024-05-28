using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NOS.Engineering.Challenge.API.Controllers;
using NOS.Engineering.Challenge.API.Models;
using NOS.Engineering.Challenge.Cache;
using NOS.Engineering.Challenge.Managers;
using NOS.Engineering.Challenge.Models;
using Xunit;

namespace NOS.Engineering.Challenge.API.Tests
{
    public class ContentControllerTests
    {
        private readonly Mock<IContentsManager> _mockManager;
        private readonly Mock<ICacheService<Content>> _mockCacheService;
        private readonly ILogger<ContentController> _logger;
        private readonly ContentController _controller;

        public ContentControllerTests()
        {
            _mockManager = new Mock<IContentsManager>();
            _mockCacheService = new Mock<ICacheService<Content>>();
            _logger = new NullLogger<ContentController>();
            _controller = new ContentController(_mockManager.Object, _mockCacheService.Object, _logger);
        }

        private Content CreateContent(Guid id)
        {
            return new Content(
                id,
                "Title",
                "SubTitle",
                "Description",
                "ImageUrl",
                120,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(2),
                new List<string> { "Genre1", "Genre2" }
            );
        }

        [Fact]
        public async Task GetManyContents_ReturnsOkResult_WithContents()
        {
            // Arrange
            var contents = new List<Content> { CreateContent(Guid.NewGuid()) };
            _mockManager.Setup(m => m.GetManyContents()).ReturnsAsync(contents);

            // Act
            var result = await _controller.GetManyContents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContents = Assert.IsAssignableFrom<IEnumerable<Content>>(okResult.Value);
            Assert.Single(returnContents);
        }

        [Fact]
        public async Task GetManyContents_ReturnsNotFound_WhenNoContents()
        {
            // Arrange
            _mockManager.Setup(m => m.GetManyContents()).ReturnsAsync(new List<Content>());

            // Act
            var result = await _controller.GetManyContents();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetFilteredContents_ReturnsOkResult_WithFilteredContents()
        {
            // Arrange
            var contents = new List<Content> { CreateContent(Guid.NewGuid()) };
            _mockManager.Setup(m => m.GetFiltered(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(contents);

            // Act
            var result = await _controller.GetFilteredContents("title", "genre");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContents = Assert.IsAssignableFrom<IEnumerable<Content>>(okResult.Value);
            Assert.Single(returnContents);
        }

        [Fact]
        public async Task GetFilteredContents_ReturnsNotFound_WhenNoFilteredContents()
        {
            // Arrange
            _mockManager.Setup(m => m.GetFiltered(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<Content>());

            // Act
            var result = await _controller.GetFilteredContents("title", "genre");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetContent_ReturnsOkResult_WithContentFromCache()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);
            _mockCacheService.Setup(c => c.GetAsync(contentId)).ReturnsAsync(content);

            // Act
            var result = await _controller.GetContent(contentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContent = Assert.IsType<Content>(okResult.Value);
            Assert.Equal(contentId, returnContent.Id);
        }

        [Fact]
        public async Task GetContent_ReturnsOkResult_WithContentFromManager()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);
            _mockCacheService.Setup(c => c.GetAsync(contentId)).ReturnsAsync((Content)null);
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync(content);

            // Act
            var result = await _controller.GetContent(contentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContent = Assert.IsType<Content>(okResult.Value);
            Assert.Equal(contentId, returnContent.Id);
        }

        [Fact]
        public async Task GetContent_ReturnsNotFound_WhenContentNotExists()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            _mockCacheService.Setup(c => c.GetAsync(contentId)).ReturnsAsync((Content)null);
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync((Content)null);

            // Act
            var result = await _controller.GetContent(contentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateContent_ReturnsOkResult_WithCreatedContent()
        {
            // Arrange
            var contentInput = new ContentInput();
            var createdContent = CreateContent(Guid.NewGuid());
            _mockManager.Setup(m => m.CreateContent(It.IsAny<ContentDto>())).ReturnsAsync(createdContent);

            // Act
            var result = await _controller.CreateContent(contentInput);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContent = Assert.IsType<Content>(okResult.Value);
            Assert.Equal(createdContent.Id, returnContent.Id);
        }

        [Fact]
        public async Task CreateContent_ReturnsProblem_WhenCreationFails()
        {
            // Arrange
            var contentInput = new ContentInput();
            _mockManager.Setup(m => m.CreateContent(It.IsAny<ContentDto>())).ReturnsAsync((Content)null);

            // Act
            var result = await _controller.CreateContent(contentInput);

            // Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task UpdateContent_ReturnsOkResult_WithUpdatedContent()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var contentInput = new ContentInput();
            var updatedContent = CreateContent(contentId);
            _mockManager.Setup(m => m.UpdateContent(contentId, It.IsAny<ContentDto>())).ReturnsAsync(updatedContent);

            // Act
            var result = await _controller.UpdateContent(contentId, contentInput);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContent = Assert.IsType<Content>(okResult.Value);
            Assert.Equal(contentId, returnContent.Id);
        }

        [Fact]
        public async Task UpdateContent_ReturnsNotFound_WhenContentNotExists()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var contentInput = new ContentInput();
            _mockManager.Setup(m => m.UpdateContent(contentId, It.IsAny<ContentDto>())).ReturnsAsync((Content)null);

            // Act
            var result = await _controller.UpdateContent(contentId, contentInput);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteContent_ReturnsOkResult_WithDeletedContentId()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            _mockManager.Setup(m => m.DeleteContent(contentId));

            // Act
            var result = await _controller.DeleteContent(contentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContentId = Assert.IsType<Guid>(okResult.Value);
            Assert.Equal(contentId, returnContentId);
        }

        [Fact]
        public async Task DeleteContent_ReturnsNotFound_WhenContentNotExists()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            _mockManager.Setup(m => m.DeleteContent(contentId));

            // Act
            var result = await _controller.DeleteContent(contentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddGenres_ReturnsOkResult_WithUpdatedContent()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync(content);
            _mockManager.Setup(m => m.UpdateContent(contentId, It.IsAny<ContentDto>())).ReturnsAsync(content);

            // Act
            var result = await _controller.AddGenres(contentId, new List<string> { "Drama" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContent = Assert.IsType<Content>(okResult.Value);
            Assert.Equal(contentId, returnContent.Id);
        }

        [Fact]
        public async Task AddGenres_ReturnsNotFound_WhenContentNotExists()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync((Content)null);

            // Act
            var result = await _controller.AddGenres(contentId, new List<string> { "Drama" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddGenres_ReturnsBadRequest_WhenGenreAlreadyExists()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync(content);

            // Act
            var result = await _controller.AddGenres(contentId, new List<string> { "Action" });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorMessage = Assert.IsType<ErrorMessage>(badRequestResult.Value);
            Assert.Equal("Genre already exists", errorMessage.Error);
        }

        [Fact]
        public async Task RemoveGenres_ReturnsOkResult_WithUpdatedContent()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId);
            var genreList = content.GenreList.ToList();
            genreList.RemoveAll(genreList.Contains);
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync(content);
            _mockManager.Setup(m => m.UpdateContent(contentId, It.IsAny<ContentDto>())).ReturnsAsync(content);

            // Act
            var result = await _controller.RemoveGenres(contentId, new List<string> { "Drama" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnContent = Assert.IsType<Content>(okResult.Value);
            Assert.Equal(contentId, returnContent.Id);
            Assert.DoesNotContain("Drama", returnContent.GenreList);
        }

        [Fact]
        public async Task RemoveGenres_ReturnsNotFound_WhenContentNotExists()
        {
            // Arrange
            var contentId = Guid.NewGuid();
            _mockManager.Setup(m => m.GetContent(contentId)).ReturnsAsync((Content)null);

            // Act
            var result = await _controller.RemoveGenres(contentId, new List<string> { "Drama" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
