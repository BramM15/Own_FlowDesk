using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain;

public class TicketCommentTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesAndGenerateId()
    {
        // Arrange
        var content = "Dit is een test comment.";
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var comment = new TicketComment(content, ticketId, userId);

        // Assert
        Assert.NotEqual(Guid.Empty, comment.Id);
        Assert.Equal(content, comment.Content);
        Assert.Equal(ticketId, comment.TicketId);
        Assert.Equal(userId, comment.UserId);

        // Controleer of de CreatedAt datum recent is ingesteld (minder dan 1 seconde geleden)
        Assert.True((DateTime.UtcNow - comment.CreatedAt).TotalSeconds < 1);
    }
}