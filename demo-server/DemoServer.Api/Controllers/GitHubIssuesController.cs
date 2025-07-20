using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using Octokit;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/github/issues")]
[Tags("GitHub Issues")]
public class GitHubIssuesController : ControllerBase
{
    private readonly IGitHubIssuesService _issuesService;

    public GitHubIssuesController(IGitHubIssuesService issuesService)
    {
        _issuesService = issuesService;
    }

    /// <summary>
    /// 获取指定仓库的 Issues 列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <param name="state">Issue 状态：open, closed, all</param>
    /// <param name="labels">标签过滤，用逗号分隔</param>
    /// <param name="sort">排序方式：created, updated, comments</param>
    /// <param name="direction">排序方向：asc, desc</param>
    /// <param name="page">页码（从1开始）</param>
    /// <param name="pageSize">每页数量（最大100）</param>
    /// <returns>Issues 列表</returns>
    [HttpGet("{owner}/{repo}")]
    public async Task<IActionResult> GetRepositoryIssues(
        string owner,
        string repo,
        [FromQuery] string state = "open",
        [FromQuery] string? labels = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var stateFilter = ParseStateFilter(state);
            var labelArray = string.IsNullOrEmpty(labels) ? null : labels.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var issues = await _issuesService.GetRepositoryIssuesAsync(
                owner, repo, stateFilter, labelArray, null, pageSize, page);

            var result = issues.Select(issue => new
            {
                id = issue.Id,
                number = issue.Number,
                title = issue.Title,
                body = issue.Body,
                state = issue.State.ToString().ToLower(),
                user = new
                {
                    login = issue.User.Login,
                    avatarUrl = issue.User.AvatarUrl
                },
                labels = issue.Labels.Select(l => new { name = l.Name, color = l.Color }),
                assignees = issue.Assignees.Select(a => new { login = a.Login, avatarUrl = a.AvatarUrl }),
                milestone = issue.Milestone != null ? new
                {
                    title = issue.Milestone.Title,
                    number = issue.Milestone.Number,
                    state = issue.Milestone.State.ToString().ToLower()
                } : null,
                commentsCount = issue.Comments,
                createdAt = issue.CreatedAt,
                updatedAt = issue.UpdatedAt,
                closedAt = issue.ClosedAt,
                htmlUrl = issue.HtmlUrl
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get repository issues: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取指定 Issue 的详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <returns>Issue 详细信息</returns>
    [HttpGet("{owner}/{repo}/{issueNumber}")]
    public async Task<IActionResult> GetIssue(
        string owner,
        string repo,
        int issueNumber)
    {
        try
        {
            var issue = await _issuesService.GetIssueAsync(owner, repo, issueNumber);

            var result = new
            {
                id = issue.Id,
                number = issue.Number,
                title = issue.Title,
                body = issue.Body,
                state = issue.State.ToString().ToLower(),
                user = new
                {
                    login = issue.User.Login,
                    avatarUrl = issue.User.AvatarUrl
                },
                labels = issue.Labels.Select(l => new { name = l.Name, color = l.Color }),
                assignees = issue.Assignees.Select(a => new { login = a.Login, avatarUrl = a.AvatarUrl }),
                milestone = issue.Milestone != null ? new
                {
                    title = issue.Milestone.Title,
                    number = issue.Milestone.Number,
                    state = issue.Milestone.State.ToString().ToLower()
                } : null,
                commentsCount = issue.Comments,
                createdAt = issue.CreatedAt,
                updatedAt = issue.UpdatedAt,
                closedAt = issue.ClosedAt,
                htmlUrl = issue.HtmlUrl
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get issue: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前用户创建的所有 Issues
    /// </summary>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <param name="state">Issue 状态</param>
    /// <param name="sort">排序方式</param>
    /// <param name="direction">排序方向</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>用户创建的 Issues 列表</returns>
    [HttpGet("user")]
    public async Task<IActionResult> GetUserIssues(
        [FromQuery] string state = "open",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var stateFilter = ParseStateFilter(state);

            var issues = await _issuesService.GetUserIssuesAsync(
                stateFilter, null, pageSize, page);

            var result = issues.Select(issue => new
            {
                id = issue.Id,
                number = issue.Number,
                title = issue.Title,
                repository = new
                {
                    name = issue.Repository?.Name,
                    fullName = issue.Repository?.FullName,
                    owner = issue.Repository?.Owner?.Login
                },
                state = issue.State.ToString().ToLower(),
                labels = issue.Labels.Select(l => new { name = l.Name, color = l.Color }),
                commentsCount = issue.Comments,
                createdAt = issue.CreatedAt,
                updatedAt = issue.UpdatedAt,
                htmlUrl = issue.HtmlUrl
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get user issues: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新的 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <param name="request">Issue 创建请求</param>
    /// <returns>创建的 Issue</returns>
    [HttpPost("{owner}/{repo}")]
    public async Task<IActionResult> CreateIssue(
        string owner,
        string repo,
        [FromBody] CreateIssueRequest request)
    {
        if (string.IsNullOrEmpty(request.Title))
        {
            return BadRequest("Issue title is required");
        }

        try
        {
            var issue = await _issuesService.CreateIssueAsync(
                owner, repo, request.Title, request.Body, request.Labels, request.Assignees, request.Milestone);

            return CreatedAtAction(
                nameof(GetIssue),
                new { owner, repo, issueNumber = issue.Number },
                new
                {
                    id = issue.Id,
                    number = issue.Number,
                    title = issue.Title,
                    body = issue.Body,
                    state = issue.State.ToString().ToLower(),
                    htmlUrl = issue.HtmlUrl,
                    createdAt = issue.CreatedAt
                });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create issue: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取 Issue 的评论列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <returns>评论列表</returns>
    [HttpGet("{owner}/{repo}/{issueNumber}/comments")]
    public async Task<IActionResult> GetIssueComments(
        string owner,
        string repo,
        int issueNumber)
    {
        try
        {
            var comments = await _issuesService.GetIssueCommentsAsync(owner, repo, issueNumber);

            var result = comments.Select(comment => new
            {
                id = comment.Id,
                body = comment.Body,
                user = new
                {
                    login = comment.User.Login,
                    avatarUrl = comment.User.AvatarUrl
                },
                createdAt = comment.CreatedAt,
                updatedAt = comment.UpdatedAt,
                htmlUrl = comment.HtmlUrl
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get issue comments: {ex.Message}");
        }
    }

    /// <summary>
    /// 为 Issue 添加评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <param name="request">评论请求</param>
    /// <returns>创建的评论</returns>
    [HttpPost("{owner}/{repo}/{issueNumber}/comments")]
    public async Task<IActionResult> CreateIssueComment(
        string owner,
        string repo,
        int issueNumber,
        [FromBody] CreateCommentRequest request)
    {
        if (string.IsNullOrEmpty(request.Body))
        {
            return BadRequest("Comment body is required");
        }

        try
        {
            var comment = await _issuesService.CreateIssueCommentAsync(owner, repo, issueNumber, request.Body);

            return CreatedAtAction(
                nameof(GetIssueComments),
                new { owner, repo, issueNumber },
                new
                {
                    id = comment.Id,
                    body = comment.Body,
                    user = new
                    {
                        login = comment.User.Login,
                        avatarUrl = comment.User.AvatarUrl
                    },
                    createdAt = comment.CreatedAt,
                    htmlUrl = comment.HtmlUrl
                });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create comment: {ex.Message}");
        }
    }

    private static ItemStateFilter ParseStateFilter(string state)
    {
        return state.ToLower() switch
        {
            "open" => ItemStateFilter.Open,
            "closed" => ItemStateFilter.Closed,
            "all" => ItemStateFilter.All,
            _ => ItemStateFilter.Open
        };
    }
}

/// <summary>
/// 创建 Issue 的请求模型
/// </summary>
public class CreateIssueRequest
{
    /// <summary>
    /// Issue 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Issue 内容
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// 标签列表
    /// </summary>
    public string[]? Labels { get; set; }
    
    /// <summary>
    /// 指派人员列表
    /// </summary>
    public string[]? Assignees { get; set; }
    
    /// <summary>
    /// 里程碑编号
    /// </summary>
    public int? Milestone { get; set; }
}

/// <summary>
/// 创建评论的请求模型
/// </summary>
public class CreateCommentRequest
{
    /// <summary>
    /// 评论内容
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
