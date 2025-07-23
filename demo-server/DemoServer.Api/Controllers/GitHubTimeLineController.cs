using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using DemoServer.Api.Models;
using Octokit;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/github/timeline")]
[Tags("GitHub Timeline")]
public class GitHubTimeLineController : ControllerBase
{
    private readonly IGitHubTimeLineService _timelineService;

    public GitHubTimeLineController(IGitHubTimeLineService timelineService)
    {
        _timelineService = timelineService;
    }

    #region Issue Timeline

    /// <summary>
    /// 获取 Issue 的时间线事件
    /// </summary>
    [HttpGet("issues/{owner}/{repo}/{issueNumber}")]
    public async Task<IActionResult> GetIssueTimeline(
        string owner,
        string repo,
        int issueNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var timeline = await _timelineService.GetIssueTimelineAsync(owner, repo, issueNumber, pageSize, page);

            var result = timeline.Select(item => new
            {
                id = item.Id,
                eventName = GetSafeEventName(item.Event),
                createdAt = item.CreatedAt,
                actor = item.Actor != null ? new
                {
                    login = item.Actor.Login,
                    avatarUrl = item.Actor.AvatarUrl,
                    htmlUrl = item.Actor.HtmlUrl
                } : null,
                commitId = item.CommitId
            });

            return Ok(new
            {
                timeline = result,
                totalCount = timeline.Count,
                page,
                pageSize
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Issue 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Issue 时间线失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 为 Issue 添加评论
    /// </summary>
    [HttpPost("issues/{owner}/{repo}/{issueNumber}/comments")]
    public async Task<IActionResult> CreateIssueComment(
        string owner,
        string repo,
        int issueNumber,
        [FromBody] CreateCommentRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Body))
            {
                return BadRequest(new { error = "评论内容不能为空" });
            }

            var comment = await _timelineService.CreateIssueCommentAsync(owner, repo, issueNumber, request.Body);

            return Ok(new
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
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Issue 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"创建评论失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 关闭 Issue
    /// </summary>
    [HttpPost("issues/{owner}/{repo}/{issueNumber}/close")]
    public async Task<IActionResult> CloseIssue(string owner, string repo, int issueNumber)
    {
        try
        {
            var issue = await _timelineService.CloseIssueAsync(owner, repo, issueNumber);

            return Ok(new
            {
                number = issue.Number,
                state = issue.State.ToString().ToLower(),
                title = issue.Title,
                closedAt = issue.ClosedAt,
                htmlUrl = issue.HtmlUrl
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Issue 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"关闭 Issue 失败: {ex.Message}" });
        }
    }

    #endregion

    #region Pull Request Timeline

    /// <summary>
    /// 获取 Pull Request 的时间线事件
    /// </summary>
    [HttpGet("pulls/{owner}/{repo}/{pullNumber}")]
    public async Task<IActionResult> GetPullRequestTimeline(
        string owner,
        string repo,
        int pullNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var timeline = await _timelineService.GetPullRequestTimelineAsync(owner, repo, pullNumber, pageSize, page);

            var result = timeline.Select(item => new
            {
                id = item.Id,
                eventName = GetSafeEventName(item.Event),
                createdAt = item.CreatedAt,
                actor = item.Actor != null ? new
                {
                    login = item.Actor.Login,
                    avatarUrl = item.Actor.AvatarUrl,
                    htmlUrl = item.Actor.HtmlUrl
                } : null,
                commitId = item.CommitId
            });

            return Ok(new
            {
                timeline = result,
                totalCount = timeline.Count,
                page,
                pageSize
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Pull Request 时间线失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 合并 Pull Request
    /// </summary>
    [HttpPost("pulls/{owner}/{repo}/{pullNumber}/merge")]
    public async Task<IActionResult> MergePullRequest(
        string owner,
        string repo,
        int pullNumber,
        [FromBody] MergePullRequestRequest request)
    {
        try
        {
            var mergeMethod = ParseMergeMethod(request.MergeMethod);
            
            var result = await _timelineService.MergePullRequestAsync(
                owner, repo, pullNumber, request.CommitTitle, request.CommitMessage, mergeMethod);

            return Ok(new
            {
                merged = result.Merged,
                sha = result.Sha,
                message = result.Message
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"合并 Pull Request 失败: {ex.Message}" });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 安全地获取事件名称，避免未知枚举值导致的异常
    /// </summary>
    private static string GetSafeEventName(StringEnum<EventInfoState> eventEnum)
    {
        try
        {
            return eventEnum.ToString();
        }
        catch (ArgumentException)
        {
            // 当遇到未知的枚举值时，返回原始字符串值
            return eventEnum.StringValue ?? "unknown";
        }
    }

    private PullRequestMergeMethod ParseMergeMethod(string? method)
    {
        return method?.ToLower() switch
        {
            "squash" => PullRequestMergeMethod.Squash,
            "rebase" => PullRequestMergeMethod.Rebase,
            _ => PullRequestMergeMethod.Merge
        };
    }

    #endregion
}
