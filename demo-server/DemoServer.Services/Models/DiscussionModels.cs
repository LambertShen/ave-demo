using Newtonsoft.Json;

namespace DemoServer.Services.Models;

public class GitHubDiscussion
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Number { get; set; }
    public bool Locked { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public DiscussionCategory? Category { get; set; }
    public DiscussionAuthor? Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int UpvoteCount { get; set; }
    public bool ViewerHasUpvoted { get; set; }
    public int CommentsTotalCount { get; set; }
    public List<DiscussionComment> Comments { get; set; } = new();
    public List<DiscussionLabel> Labels { get; set; } = new();
    public bool AnswerChosenAt { get; set; }
    public DiscussionComment? Answer { get; set; }
    
    /// <summary>
    /// 讨论状态：根据是否被锁定判断
    /// </summary>
    public string State => Locked ? "LOCKED" : "OPEN";
}

public class DiscussionCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public string EmojiHTML { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsAnswerable { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class DiscussionAuthor
{
    public string Login { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class DiscussionComment
{
    public string Id { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyHTML { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DiscussionAuthor? Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int UpvoteCount { get; set; }
    public bool ViewerHasUpvoted { get; set; }
    public bool IsAnswer { get; set; }
    public bool IsMinimized { get; set; }
    public string? MinimizedReason { get; set; }
    public List<DiscussionComment> Replies { get; set; } = new();
}

public class DiscussionLabel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

// Request Models for Creating/Updating Discussions
public class CreateDiscussionRequest
{
    public string RepositoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
}

public class UpdateDiscussionRequest
{
    public string DiscussionId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? CategoryId { get; set; }
}

public class CreateDiscussionCommentRequest
{
    public string DiscussionId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ReplyToId { get; set; }
}

public class UpdateDiscussionCommentRequest
{
    public string CommentId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

// Request Models for Creating/Updating Categories
public class CreateDiscussionCategoryRequest
{
    public string RepositoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public bool IsAnswerable { get; set; } = false;
}

public class UpdateDiscussionCategoryRequest
{
    public string CategoryId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public bool? IsAnswerable { get; set; }
}

// Response Models
public class DiscussionsResponse
{
    public List<GitHubDiscussion> Discussions { get; set; } = new();
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
    public int TotalCount { get; set; }
}

public class DiscussionCategoriesResponse
{
    public List<DiscussionCategory> Categories { get; set; } = new();
    public int TotalCount { get; set; }
}

public class DiscussionCommentsResponse
{
    public List<DiscussionComment> Comments { get; set; } = new();
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
    public int TotalCount { get; set; }
}
