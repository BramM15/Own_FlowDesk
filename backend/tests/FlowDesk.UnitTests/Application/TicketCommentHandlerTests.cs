using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using Moq;
using Xunit;

namespace FlowDesk.UnitTests.Application;

public class TicketCommentHandlerTests
{
    private readonly Mock<ITicketCommentRepository> _mockRepo;
    private readonly TicketCommentHandler _handler;

    public TicketCommentHandlerTests()
    {
        _mockRepo = new Mock<ITicketCommentRepository>();
        _handler = new TicketCommentHandler(_mockRepo.Object);
    }

    // --- GET METHODS ---

    [Fact]
    public async Task GetByTicketIdAsync_ShouldReturnListOfComments()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var comments = new List<TicketComment>
        {
            new TicketComment("Comment 1", ticketId, Guid.NewGuid()),
            new TicketComment("Comment 2", ticketId, Guid.NewGuid())
        };

        _mockRepo.Setup(r => r.GetByTicketIdAsync(ticketId)).ReturnsAsync(comments);

        // Act
        var result = await _handler.GetByTicketIdAsync(ticketId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(ticketId, c.TicketId));
    }

    // --- CREATE METHODS ---

    [Fact]
    public async Task CreateAsync_ShouldCreateAndSaveComment()
    {
        // Arrange
        var content = "Heb je al geprobeerd de pc opnieuw op te starten?";
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Laat de repository de gemaakte entiteit netjes teruggeven
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<TicketComment>()))
                 .ReturnsAsync((TicketComment c) => c);

        // Act
        var result = await _handler.CreateAsync(content, ticketId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(content, result.Content);
        Assert.Equal(ticketId, result.TicketId);
        Assert.Equal(userId, result.UserId);

        // Verifieer of de opslagmethode daadwerkelijk één keer is aangeroepen
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<TicketComment>()), Times.Once);
    }

    // --- DELETE METHODS ---

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenCommentDoesNotExist()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetAsync(commentId)).ReturnsAsync((TicketComment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.DeleteAsync(commentId));

        Assert.Equal("Comment not found", exception.Message);
        
        // Zorg ervoor dat Delete NOOIT wordt aangeroepen als hij niet bestaat
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteComment_WhenCommentExists()
    {
        // Arrange
        var comment = new TicketComment("Verkeerde comment", Guid.NewGuid(), Guid.NewGuid());
        _mockRepo.Setup(r => r.GetAsync(comment.Id)).ReturnsAsync(comment);

        // Act
        await _handler.DeleteAsync(comment.Id);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(comment.Id), Times.Once);
    }
}