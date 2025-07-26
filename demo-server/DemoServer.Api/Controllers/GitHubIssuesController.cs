using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using DemoServer.Api.Models;
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
    /// Retrieves the list of issues for the specified repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="authorization">Authorization header containing the access token.</param>
    /// <param name="state">The state of the issues: open, closed, or all.</param>
    /// <param name="labels">Filter by labels, separated by commas.</param>
    /// <param name="sort">The sorting method: created, updated, or comments.</param>
    /// <param name="direction">The sorting direction: asc or desc.</param>
    /// <param name="page">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of items per page (maximum 100).</param>
    /// <returns>A list of issues.</returns>
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
                htmlUrl = issue.HtmlUrl,
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

    /// <summary>
    /// 更新 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的 Issue</returns>
    [HttpPatch("{owner}/{repo}/{issueNumber}")]
    public async Task<IActionResult> UpdateIssue(
        string owner,
        string repo,
        int issueNumber,
        [FromBody] UpdateIssueRequest request)
    {
        try
        {
            var state = ParseItemState(request.State);
            var issue = await _issuesService.UpdateIssueAsync(
                owner, repo, issueNumber, request.Title, request.Body, 
                state, request.Labels, request.Assignees);

            return Ok(new
            {
                id = issue.Id,
                number = issue.Number,
                title = issue.Title,
                body = issue.Body,
                state = issue.State.ToString().ToLower(),
                labels = issue.Labels.Select(l => new { name = l.Name, color = l.Color }),
                assignees = issue.Assignees.Select(a => new { login = a.Login, avatarUrl = a.AvatarUrl }),
                updatedAt = issue.UpdatedAt,
                htmlUrl = issue.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to update issue: {ex.Message}");
        }
    }

    /// <summary>
    /// 关闭 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="request">关闭请求</param>
    /// <returns>关闭后的 Issue</returns>
    [HttpPost("{owner}/{repo}/{issueNumber}/close")]
    public async Task<IActionResult> CloseIssue(
        string owner,
        string repo,
        int issueNumber,
        [FromBody] CloseIssueRequest? request = null)
    {
        try
        {
            var issue = await _issuesService.CloseIssueAsync(owner, repo, issueNumber, request?.Comment);

            return Ok(new
            {
                id = issue.Id,
                number = issue.Number,
                title = issue.Title,
                state = issue.State.ToString().ToLower(),
                closedAt = issue.ClosedAt,
                htmlUrl = issue.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to close issue: {ex.Message}");
        }
    }

    /// <summary>
    /// 重新打开 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="request">重新打开请求</param>
    /// <returns>重新打开后的 Issue</returns>
    [HttpPost("{owner}/{repo}/{issueNumber}/reopen")]
    public async Task<IActionResult> ReopenIssue(
        string owner,
        string repo,
        int issueNumber,
        [FromBody] ReopenIssueRequest? request = null)
    {
        try
        {
            var issue = await _issuesService.ReopenIssueAsync(owner, repo, issueNumber, request?.Comment);

            return Ok(new
            {
                id = issue.Id,
                number = issue.Number,
                title = issue.Title,
                state = issue.State.ToString().ToLower(),
                reopenedAt = issue.UpdatedAt,
                htmlUrl = issue.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to reopen issue: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新 Issue 评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的评论</returns>
    [HttpPatch("{owner}/{repo}/comments/{commentId}")]
    public async Task<IActionResult> UpdateIssueComment(
        string owner,
        string repo,
        long commentId,
        [FromBody] UpdateCommentRequest request)
    {
        if (string.IsNullOrEmpty(request.Body))
        {
            return BadRequest("Comment body is required");
        }

        try
        {
            var comment = await _issuesService.UpdateIssueCommentAsync(owner, repo, commentId, request.Body);

            return Ok(new
            {
                id = comment.Id,
                body = comment.Body,
                user = new
                {
                    login = comment.User.Login,
                    avatarUrl = comment.User.AvatarUrl
                },
                updatedAt = comment.UpdatedAt,
                htmlUrl = comment.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to update comment: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除 Issue 评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{owner}/{repo}/comments/{commentId}")]
    public async Task<IActionResult> DeleteIssueComment(
        string owner,
        string repo,
        long commentId)
    {
        try
        {
            await _issuesService.DeleteIssueCommentAsync(owner, repo, commentId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to delete comment: {ex.Message}");
        }
    }

    /// <summary>
    /// 为 Issue 添加标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="request">标签请求</param>
    /// <returns>更新后的标签列表</returns>
    [HttpPost("{owner}/{repo}/{issueNumber}/labels")]
    public async Task<IActionResult> AddLabelsToIssue(
        string owner,
        string repo,
        int issueNumber,
        [FromBody] LabelsRequest request)
    {
        if (request.Labels == null || request.Labels.Length == 0)
        {
            return BadRequest("Labels are required");
        }

        try
        {
            var labels = await _issuesService.AddLabelsToIssueAsync(owner, repo, issueNumber, request.Labels);

            return Ok(labels.Select(l => new { name = l.Name, color = l.Color, description = l.Description }));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to add labels: {ex.Message}");
        }
    }

    /// <summary>
    /// 从 Issue 移除标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="labelName">标签名称</param>
    /// <returns>剩余标签列表</returns>
    [HttpDelete("{owner}/{repo}/{issueNumber}/labels/{labelName}")]
    public async Task<IActionResult> RemoveLabelFromIssue(
        string owner,
        string repo,
        int issueNumber,
        string labelName)
    {
        try
        {
            var labels = await _issuesService.RemoveLabelFromIssueAsync(owner, repo, issueNumber, labelName);

            return Ok(labels.Select(l => new { name = l.Name, color = l.Color, description = l.Description }));
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to remove label: {ex.Message}");
        }
    }

    /// <summary>
    /// 清除 Issue 的所有标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{owner}/{repo}/{issueNumber}/labels")]
    public async Task<IActionResult> ClearLabelsFromIssue(
        string owner,
        string repo,
        int issueNumber)
    {
        try
        {
            await _issuesService.ClearLabelsFromIssueAsync(owner, repo, issueNumber);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to clear labels: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量为多个 Issue 添加标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="request">批量标签请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("{owner}/{repo}/batch/labels")]
    public async Task<IActionResult> AddLabelsToBatchIssues(
        string owner,
        string repo,
        [FromBody] BatchLabelsRequest request)
    {
        if (request.IssueNumbers == null || request.IssueNumbers.Length == 0)
        {
            return BadRequest("Issue numbers are required");
        }

        if (request.Labels == null || request.Labels.Length == 0)
        {
            return BadRequest("Labels are required");
        }

        try
        {
            await _issuesService.AddLabelsToBatchIssuesAsync(owner, repo, request.IssueNumbers, request.Labels);

            return Ok(new { 
                message = $"Successfully added labels to {request.IssueNumbers.Length} issues",
                processedIssues = request.IssueNumbers.Length
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to add labels to batch issues: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量关闭多个 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="request">批量关闭请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("{owner}/{repo}/batch/close")]
    public async Task<IActionResult> CloseBatchIssues(
        string owner,
        string repo,
        [FromBody] BatchCloseRequest request)
    {
        if (request.IssueNumbers == null || request.IssueNumbers.Length == 0)
        {
            return BadRequest("Issue numbers are required");
        }

        try
        {
            await _issuesService.CloseBatchIssuesAsync(owner, repo, request.IssueNumbers, request.Comment);

            return Ok(new { 
                message = $"Successfully closed {request.IssueNumbers.Length} issues",
                closedIssues = request.IssueNumbers.Length
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to close batch issues: {ex.Message}");
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

    private static ItemState? ParseItemState(string? state)
    {
        if (string.IsNullOrEmpty(state))
            return null;
            
        return state.ToLower() switch
        {
            "open" => ItemState.Open,
            "closed" => ItemState.Closed,
            _ => null
        };
    }
}
