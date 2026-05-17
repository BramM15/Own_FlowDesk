namespace FlowDesk.API.DTOs;

public record CreateCommentRequest(string Content, Guid UserId);