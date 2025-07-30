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

    #region Timeline Event Details

    /// <summary>
    /// 获取时间线事件的详细信息
    /// </summary>
    [HttpGet("events/{owner}/{repo}/details")]
    public async Task<IActionResult> GetTimelineEventDetails(
        string owner,
        string repo,
        [FromQuery] string eventType,
        [FromQuery] string? commitId = null,
        [FromQuery] long? commentId = null,
        [FromQuery] string? labelName = null,
        [FromQuery] int? milestoneNumber = null,
        [FromQuery] string? username = null)
    {
        try
        {
            object? details = eventType.ToLower() switch
            {
                "committed" when !string.IsNullOrEmpty(commitId) => 
                    await _timelineService.GetCommitDetailsAsync(owner, repo, commitId),
                
                "commented" when commentId.HasValue => 
                    await _timelineService.GetCommentDetailsAsync(owner, repo, commentId.Value),
                
                "labeled" or "unlabeled" when !string.IsNullOrEmpty(labelName) => 
                    await _timelineService.GetLabelDetailsAsync(owner, repo, labelName),
                
                "milestoned" or "demilestoned" when milestoneNumber.HasValue => 
                    await _timelineService.GetMilestoneDetailsAsync(owner, repo, milestoneNumber.Value),
                
                "assigned" or "unassigned" or "reviewed" or "review_requested" when !string.IsNullOrEmpty(username) => 
                    await _timelineService.GetUserDetailsAsync(username),
                
                _ => null
            };

            if (details == null)
            {
                return NotFound(new { error = $"无法获取事件类型 '{eventType}' 的详细信息" });
            }

            return Ok(new
            {
                eventType,
                details
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取事件详细信息失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取提交详细信息
    /// </summary>
    [HttpGet("commit/{owner}/{repo}/{commitSha}")]
    public async Task<IActionResult> GetCommitDetails(string owner, string repo, string commitSha)
    {
        try
        {
            var commit = await _timelineService.GetCommitDetailsAsync(owner, repo, commitSha);
            
            if (commit == null)
            {
                return NotFound(new { error = "提交不存在" });
            }

            return Ok(new
            {
                sha = commit.Sha,
                message = commit.Commit.Message,
                author = new
                {
                    name = commit.Commit.Author.Name,
                    email = commit.Commit.Author.Email,
                    date = commit.Commit.Author.Date
                },
                committer = new
                {
                    name = commit.Commit.Committer.Name,
                    email = commit.Commit.Committer.Email,
                    date = commit.Commit.Committer.Date
                },
                stats = commit.Stats != null ? new
                {
                    total = commit.Stats.Total,
                    additions = commit.Stats.Additions,
                    deletions = commit.Stats.Deletions
                } : null,
                url = commit.HtmlUrl,
                filesChanged = commit.Files?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取提交详细信息失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取评论详细信息
    /// </summary>
    [HttpGet("comment/{owner}/{repo}/{commentId}")]
    public async Task<IActionResult> GetCommentDetails(string owner, string repo, long commentId)
    {
        try
        {
            var comment = await _timelineService.GetCommentDetailsAsync(owner, repo, commentId);
            
            if (comment == null)
            {
                return NotFound(new { error = "评论不存在" });
            }

            return Ok(new
            {
                id = comment.Id,
                body = comment.Body,
                user = new
                {
                    login = comment.User.Login,
                    avatarUrl = comment.User.AvatarUrl,
                    htmlUrl = comment.User.HtmlUrl
                },
                createdAt = comment.CreatedAt,
                updatedAt = comment.UpdatedAt,
                htmlUrl = comment.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取评论详细信息失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取标签详细信息
    /// </summary>
    [HttpGet("label/{owner}/{repo}/{labelName}")]
    public async Task<IActionResult> GetLabelDetails(string owner, string repo, string labelName)
    {
        try
        {
            var label = await _timelineService.GetLabelDetailsAsync(owner, repo, labelName);
            
            if (label == null)
            {
                return NotFound(new { error = "标签不存在" });
            }

            return Ok(new
            {
                name = label.Name,
                color = label.Color,
                description = label.Description,
                @default = label.Default
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取标签详细信息失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取里程碑详细信息
    /// </summary>
    [HttpGet("milestone/{owner}/{repo}/{milestoneNumber}")]
    public async Task<IActionResult> GetMilestoneDetails(string owner, string repo, int milestoneNumber)
    {
        try
        {
            var milestone = await _timelineService.GetMilestoneDetailsAsync(owner, repo, milestoneNumber);
            
            if (milestone == null)
            {
                return NotFound(new { error = "里程碑不存在" });
            }

            return Ok(new
            {
                number = milestone.Number,
                title = milestone.Title,
                description = milestone.Description,
                state = milestone.State.ToString().ToLower(),
                openIssues = milestone.OpenIssues,
                closedIssues = milestone.ClosedIssues,
                createdAt = milestone.CreatedAt,
                updatedAt = milestone.UpdatedAt,
                dueOn = milestone.DueOn,
                htmlUrl = milestone.HtmlUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取里程碑详细信息失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取用户详细信息
    /// </summary>
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserDetails(string username)
    {
        try
        {
            var user = await _timelineService.GetUserDetailsAsync(username);
            
            if (user == null)
            {
                return NotFound(new { error = "用户不存在" });
            }

            return Ok(new
            {
                login = user.Login,
                name = user.Name,
                email = user.Email,
                avatarUrl = user.AvatarUrl,
                htmlUrl = user.HtmlUrl,
                bio = user.Bio,
                company = user.Company,
                location = user.Location,
                blog = user.Blog,
                publicRepos = user.PublicRepos,
                publicGists = user.PublicGists,
                followers = user.Followers,
                following = user.Following,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取用户详细信息失败: {ex.Message}" });
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
