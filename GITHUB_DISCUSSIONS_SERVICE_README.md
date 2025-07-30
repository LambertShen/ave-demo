# GitHub Discussions Service

GitHubDiscussionsService 是一个使用 GraphQL API 实现对 GitHub Discussions 和 Categories 进行 CRUD 操作的服务。

## 功能特性

### Discussions 功能
- **获取讨论列表**: 支持分页、排序、分类筛选
- **获取讨论详情**: 支持按 ID 或按仓库+编号获取
- **创建讨论**: 创建新的讨论主题
- **更新讨论**: 修改讨论标题、内容、分类
- **删除讨论**: 删除指定讨论
- **锁定/解锁讨论**: 管理讨论状态
- **点赞操作**: 为讨论点赞或取消点赞

### Discussion Comments 功能
- **获取评论列表**: 支持分页获取讨论评论
- **创建评论**: 创建评论或回复评论
- **更新评论**: 修改评论内容
- **删除评论**: 删除指定评论
- **评论点赞**: 为评论点赞或取消点赞
- **标记答案**: 将评论标记为最佳答案或取消标记

### Discussion Categories 功能
- **获取分类列表**: 获取仓库的所有讨论分类
- **获取分类详情**: 根据 ID 获取分类信息
- **创建分类**: 创建新的讨论分类
- **更新分类**: 修改分类信息
- **删除分类**: 删除指定分类

## 数据模型

### 核心模型

#### GitHubDiscussion
```csharp
public class GitHubDiscussion
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string Url { get; set; }
    public int Number { get; set; }
    public string State { get; set; }
    public bool Locked { get; set; }
    public string CategoryId { get; set; }
    public DiscussionCategory? Category { get; set; }
    public DiscussionAuthor? Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int UpvoteCount { get; set; }
    public bool ViewerHasUpvoted { get; set; }
    public int CommentsTotalCount { get; set; }
    public List<DiscussionComment> Comments { get; set; }
    public List<DiscussionLabel> Labels { get; set; }
    public bool AnswerChosenAt { get; set; }
    public DiscussionComment? Answer { get; set; }
}
```

#### DiscussionCategory
```csharp
public class DiscussionCategory
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Emoji { get; set; }
    public string EmojiHTML { get; set; }
    public string Slug { get; set; }
    public bool IsAnswerable { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

#### DiscussionComment
```csharp
public class DiscussionComment
{
    public string Id { get; set; }
    public string Body { get; set; }
    public string BodyHTML { get; set; }
    public string Url { get; set; }
    public DiscussionAuthor? Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int UpvoteCount { get; set; }
    public bool ViewerHasUpvoted { get; set; }
    public bool IsAnswer { get; set; }
    public bool IsMinimized { get; set; }
    public string? MinimizedReason { get; set; }
    public List<DiscussionComment> Replies { get; set; }
}
```

## API 接口

### Discussions 接口

#### 获取讨论列表
```http
GET /api/GitHubDiscussions/{owner}/{name}?first=20&after=cursor&categoryId=xxx&orderBy=UPDATED_AT&direction=DESC
```

#### 获取讨论详情
```http
GET /api/GitHubDiscussions/by-id/{discussionId}
GET /api/GitHubDiscussions/{owner}/{name}/{number}
```

#### 创建讨论
```http
POST /api/GitHubDiscussions
Content-Type: application/json

{
  "repositoryId": "repo_id",
  "title": "Discussion Title",
  "body": "Discussion content",
  "categoryId": "category_id"
}
```

#### 更新讨论
```http
PUT /api/GitHubDiscussions/{discussionId}
Content-Type: application/json

{
  "title": "Updated Title",
  "body": "Updated content",
  "categoryId": "new_category_id"
}
```

#### 删除讨论
```http
DELETE /api/GitHubDiscussions/{discussionId}
```

#### 锁定/解锁讨论
```http
POST /api/GitHubDiscussions/{discussionId}/lock
POST /api/GitHubDiscussions/{discussionId}/unlock
```

#### 点赞操作
```http
POST /api/GitHubDiscussions/{discussionId}/upvote
DELETE /api/GitHubDiscussions/{discussionId}/upvote
```

### Comments 接口

#### 获取评论列表
```http
GET /api/GitHubDiscussions/{discussionId}/comments?first=20&after=cursor
```

#### 创建评论
```http
POST /api/GitHubDiscussions/comments
Content-Type: application/json

{
  "discussionId": "discussion_id",
  "body": "Comment content",
  "replyToId": "parent_comment_id" // 可选，用于回复
}
```

#### 更新评论
```http
PUT /api/GitHubDiscussions/comments/{commentId}
Content-Type: application/json

{
  "body": "Updated comment content"
}
```

#### 删除评论
```http
DELETE /api/GitHubDiscussions/comments/{commentId}
```

#### 评论点赞
```http
POST /api/GitHubDiscussions/comments/{commentId}/upvote
DELETE /api/GitHubDiscussions/comments/{commentId}/upvote
```

#### 标记答案
```http
POST /api/GitHubDiscussions/comments/{commentId}/mark-as-answer
POST /api/GitHubDiscussions/comments/{commentId}/unmark-as-answer
```

### Categories 接口

#### 获取分类列表
```http
GET /api/GitHubDiscussions/{owner}/{name}/categories
```

#### 获取分类详情
```http
GET /api/GitHubDiscussions/categories/{categoryId}
```

#### 创建分类
```http
POST /api/GitHubDiscussions/categories
Content-Type: application/json

{
  "repositoryId": "repo_id",
  "name": "Category Name",
  "description": "Category description",
  "emoji": "🎯",
  "isAnswerable": true
}
```

#### 更新分类
```http
PUT /api/GitHubDiscussions/categories/{categoryId}
Content-Type: application/json

{
  "name": "Updated Name",
  "description": "Updated description",
  "emoji": "🔧",
  "isAnswerable": false
}
```

#### 删除分类
```http
DELETE /api/GitHubDiscussions/categories/{categoryId}
```

## 使用示例

### 1. 配置服务

在 `Program.cs` 中确保已注册服务：

```csharp
builder.Services.AddGitHubOAuthServices(builder.Configuration);
```

### 2. 配置 GitHub Token

在 `appsettings.json` 中配置：

```json
{
  "GitHub": {
    "AccessToken": "your_github_token"
  }
}
```

### 3. 使用服务

```csharp
public class ExampleController : ControllerBase
{
    private readonly IGitHubDiscussionsService _discussionsService;

    public ExampleController(IGitHubDiscussionsService discussionsService)
    {
        _discussionsService = discussionsService;
    }

    public async Task<IActionResult> GetDiscussions()
    {
        var discussions = await _discussionsService.GetDiscussionsAsync(
            "microsoft", "vscode", first: 10);
        return Ok(discussions);
    }
}
```

## 权限要求

使用此服务需要 GitHub Token 具有以下权限：
- `public_repo` 或 `repo` (取决于仓库类型)
- `read:discussion` - 读取讨论
- `write:discussion` - 创建、更新、删除讨论和评论

## 注意事项

1. **GraphQL API 限制**: GitHub GraphQL API 有速率限制，请合理使用
2. **权限控制**: 某些操作需要相应的仓库权限
3. **错误处理**: 所有方法都会抛出 `InvalidOperationException` 当 GraphQL 返回错误时
4. **分页**: 大多数列表方法支持基于游标的分页
5. **异步操作**: 所有方法都是异步的，建议使用 `await` 调用

## 测试

项目包含了完整的 HTTP 测试文件 `GitHubDiscussions.http`，可以直接在 VS Code 中使用 REST Client 扩展进行测试。

记得在测试前：
1. 设置正确的 GitHub Token
2. 替换测试文件中的仓库信息和 ID
3. 确保 Token 有足够的权限
