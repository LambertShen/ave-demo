using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using Octokit;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/github/pull-requests")]
[Tags("GitHub Pull Requests")]
public class GitHubPullRequestController : ControllerBase
{
    private readonly IGitHubPullRequestService _pullRequestService;

    public GitHubPullRequestController(IGitHubPullRequestService pullRequestService)
    {
        _pullRequestService = pullRequestService;
    }

    /// <summary>
    /// 获取指定仓库的 Pull Request 列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="state">Pull Request 状态：open, closed, all</param>
    /// <param name="head">头部分支</param>
    /// <param name="base">基础分支</param>
    /// <param name="sort">排序方式：created, updated, popularity, long-running</param>
    /// <param name="direction">排序方向：asc, desc</param>
    /// <param name="page">页码（从1开始）</param>
    /// <param name="pageSize">每页数量（最大100）</param>
    /// <returns>Pull Request 列表</returns>
    [HttpGet("{owner}/{repo}")]
    public async Task<IActionResult> GetRepositoryPullRequests(
        string owner,
        string repo,
        [FromQuery] string state = "open",
        [FromQuery] string? head = null,
        [FromQuery] string? @base = null,
        [FromQuery] string sort = "created",
        [FromQuery] string direction = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var stateFilter = ParseStateFilter(state);
            var sortProperty = ParseSortProperty(sort);
            var sortDirection = ParseSortDirection(direction);

            var pullRequests = await _pullRequestService.GetRepositoryPullRequestsAsync(
                owner, repo, stateFilter, head, @base, sortProperty, sortDirection, pageSize, page);

            var result = pullRequests.Select(pr => new
            {
                number = pr.Number,
                title = pr.Title,
                body = pr.Body,
                state = pr.State.StringValue,
                head = new
                {
                    @ref = pr.Head.Ref,
                    sha = pr.Head.Sha,
                    repo = pr.Head.Repository != null ? new
                    {
                        name = pr.Head.Repository.Name,
                        full_name = pr.Head.Repository.FullName
                    } : null
                },
                @base = new
                {
                    @ref = pr.Base.Ref,
                    sha = pr.Base.Sha,
                    repo = pr.Base.Repository != null ? new
                    {
                        name = pr.Base.Repository.Name,
                        full_name = pr.Base.Repository.FullName
                    } : null
                },
                user = new
                {
                    login = pr.User.Login,
                    avatar_url = pr.User.AvatarUrl
                },
                created_at = pr.CreatedAt,
                updated_at = pr.UpdatedAt,
                merged_at = pr.MergedAt,
                closed_at = pr.ClosedAt,
                draft = pr.Draft,
                mergeable = pr.Mergeable,
                merged = pr.Merged,
                assignees = pr.Assignees.Select(a => new { login = a.Login, avatar_url = a.AvatarUrl }),
                requested_reviewers = pr.RequestedReviewers.Select(r => new { login = r.Login, avatar_url = r.AvatarUrl }),
                labels = pr.Labels.Select(l => new { name = l.Name, color = l.Color })
            });

            return Ok(new
            {
                pullRequests = result,
                count = pullRequests.Count,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Pull Request 列表失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取指定 Pull Request 的详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>Pull Request 详细信息</returns>
    [HttpGet("{owner}/{repo}/{number}")]
    public async Task<IActionResult> GetPullRequest(string owner, string repo, int number)
    {
        try
        {
            var pullRequest = await _pullRequestService.GetPullRequestAsync(owner, repo, number);

            var result = new
            {
                number = pullRequest.Number,
                title = pullRequest.Title,
                body = pullRequest.Body,
                state = pullRequest.State.StringValue,
                head = new
                {
                    @ref = pullRequest.Head.Ref,
                    sha = pullRequest.Head.Sha,
                    repo = pullRequest.Head.Repository != null ? new
                    {
                        name = pullRequest.Head.Repository.Name,
                        full_name = pullRequest.Head.Repository.FullName
                    } : null
                },
                @base = new
                {
                    @ref = pullRequest.Base.Ref,
                    sha = pullRequest.Base.Sha,
                    repo = pullRequest.Base.Repository != null ? new
                    {
                        name = pullRequest.Base.Repository.Name,
                        full_name = pullRequest.Base.Repository.FullName
                    } : null
                },
                user = new
                {
                    login = pullRequest.User.Login,
                    avatar_url = pullRequest.User.AvatarUrl
                },
                created_at = pullRequest.CreatedAt,
                updated_at = pullRequest.UpdatedAt,
                merged_at = pullRequest.MergedAt,
                closed_at = pullRequest.ClosedAt,
                draft = pullRequest.Draft,
                mergeable = pullRequest.Mergeable,
                merged = pullRequest.Merged,
                assignees = pullRequest.Assignees.Select(a => new { login = a.Login, avatar_url = a.AvatarUrl }),
                requested_reviewers = pullRequest.RequestedReviewers.Select(r => new { login = r.Login, avatar_url = r.AvatarUrl }),
                labels = pullRequest.Labels.Select(l => new { name = l.Name, color = l.Color }),
                additions = pullRequest.Additions,
                deletions = pullRequest.Deletions,
                changed_files = pullRequest.ChangedFiles,
                commits = pullRequest.Commits,
                comments = pullRequest.Comments
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Pull Request 失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 创建新的 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="request">创建请求</param>
    /// <returns>创建的 Pull Request</returns>
    [HttpPost("{owner}/{repo}")]
    public async Task<IActionResult> CreatePullRequest(string owner, string repo, [FromBody] CreatePullRequestRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Head) || string.IsNullOrEmpty(request.Base))
            {
                return BadRequest(new { error = "Title, Head, Base 为必填字段" });
            }

            var pullRequest = await _pullRequestService.CreatePullRequestAsync(
                owner, repo, request.Title, request.Head, request.Base, 
                request.Body, request.Draft, request.MaintainerCanModify);

            var result = new
            {
                number = pullRequest.Number,
                title = pullRequest.Title,
                body = pullRequest.Body,
                state = pullRequest.State.StringValue,
                head = new
                {
                    @ref = pullRequest.Head.Ref,
                    sha = pullRequest.Head.Sha
                },
                @base = new
                {
                    @ref = pullRequest.Base.Ref,
                    sha = pullRequest.Base.Sha
                },
                user = new
                {
                    login = pullRequest.User.Login,
                    avatar_url = pullRequest.User.AvatarUrl
                },
                created_at = pullRequest.CreatedAt,
                draft = pullRequest.Draft,
                html_url = pullRequest.HtmlUrl
            };

            return CreatedAtAction(
                nameof(GetPullRequest),
                new { owner, repo, number = pullRequest.Number },
                result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"创建 Pull Request 失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 更新 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的 Pull Request</returns>
    [HttpPut("{owner}/{repo}/{number}")]
    public async Task<IActionResult> UpdatePullRequest(string owner, string repo, int number, [FromBody] UpdatePullRequestRequest request)
    {
        try
        {
            ItemState? state = null;
            if (!string.IsNullOrEmpty(request.State))
            {
                state = ParseItemState(request.State);
            }

            var pullRequest = await _pullRequestService.UpdatePullRequestAsync(
                owner, repo, number, request.Title, request.Body, state, request.Base, request.MaintainerCanModify);

            var result = new
            {
                number = pullRequest.Number,
                title = pullRequest.Title,
                body = pullRequest.Body,
                state = pullRequest.State.StringValue,
                updated_at = pullRequest.UpdatedAt
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"更新 Pull Request 失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 合并 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="request">合并请求</param>
    /// <returns>合并结果</returns>
    [HttpPut("{owner}/{repo}/{number}/merge")]
    public async Task<IActionResult> MergePullRequest(string owner, string repo, int number, [FromBody] MergePullRequestRequest? request = null)
    {
        try
        {
            var mergeMethod = request?.MergeMethod ?? "merge";
            var parsedMergeMethod = ParseMergeMethod(mergeMethod);

            var mergeResult = await _pullRequestService.MergePullRequestAsync(
                owner, repo, number, request?.CommitMessage, request?.CommitTitle, parsedMergeMethod);

            return Ok(new
            {
                merged = mergeResult.Merged,
                sha = mergeResult.Sha,
                message = mergeResult.Message
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

    /// <summary>
    /// 关闭 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>关闭后的 Pull Request</returns>
    [HttpPut("{owner}/{repo}/{number}/close")]
    public async Task<IActionResult> ClosePullRequest(string owner, string repo, int number)
    {
        try
        {
            var pullRequest = await _pullRequestService.ClosePullRequestAsync(owner, repo, number);

            return Ok(new
            {
                number = pullRequest.Number,
                title = pullRequest.Title,
                state = pullRequest.State.StringValue,
                closed_at = pullRequest.ClosedAt
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"关闭 Pull Request 失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 重新打开 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>重新打开的 Pull Request</returns>
    [HttpPut("{owner}/{repo}/{number}/reopen")]
    public async Task<IActionResult> ReopenPullRequest(string owner, string repo, int number)
    {
        try
        {
            var pullRequest = await _pullRequestService.ReopenPullRequestAsync(owner, repo, number);

            return Ok(new
            {
                number = pullRequest.Number,
                title = pullRequest.Title,
                state = pullRequest.State.StringValue,
                updated_at = pullRequest.UpdatedAt
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"重新打开 Pull Request 失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取 Pull Request 的文件变更
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>文件变更列表</returns>
    [HttpGet("{owner}/{repo}/{number}/files")]
    public async Task<IActionResult> GetPullRequestFiles(string owner, string repo, int number)
    {
        try
        {
            var files = await _pullRequestService.GetPullRequestFilesAsync(owner, repo, number);

            var result = files.Select(file => new
            {
                filename = file.FileName,
                status = file.Status,
                additions = file.Additions,
                deletions = file.Deletions,
                changes = file.Changes,
                blob_url = file.BlobUrl,
                patch = file.Patch,
                contents_url = file.ContentsUrl
            });

            return Ok(new
            {
                files = result,
                count = files.Count
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Pull Request 文件变更失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取 Pull Request 的提交记录
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>提交记录列表</returns>
    [HttpGet("{owner}/{repo}/{number}/commits")]
    public async Task<IActionResult> GetPullRequestCommits(string owner, string repo, int number)
    {
        try
        {
            var commits = await _pullRequestService.GetPullRequestCommitsAsync(owner, repo, number);

            var result = commits.Select(commit => new
            {
                sha = commit.Sha,
                commit = new
                {
                    message = commit.Commit.Message,
                    author = new
                    {
                        name = commit.Commit.Author.Name,
                        email = commit.Commit.Author.Email,
                        date = commit.Commit.Author.Date
                    }
                },
                author = commit.Author != null ? new
                {
                    login = commit.Author.Login,
                    avatar_url = commit.Author.AvatarUrl
                } : null,
                html_url = commit.HtmlUrl
            });

            return Ok(new
            {
                commits = result,
                count = commits.Count
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Pull Request 提交记录失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取 Pull Request 的评审
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>评审列表</returns>
    [HttpGet("{owner}/{repo}/{number}/reviews")]
    public async Task<IActionResult> GetPullRequestReviews(string owner, string repo, int number)
    {
        try
        {
            var reviews = await _pullRequestService.GetPullRequestReviewsAsync(owner, repo, number);

            var result = reviews.Select(review => new
            {
                id = review.Id,
                body = review.Body,
                state = review.State,
                user = new
                {
                    login = review.User.Login,
                    avatar_url = review.User.AvatarUrl
                },
                submitted_at = review.SubmittedAt,
                commit_id = review.CommitId,
                html_url = review.HtmlUrl
            });

            return Ok(new
            {
                reviews = result,
                count = reviews.Count
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取 Pull Request 评审失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 创建 Pull Request 评审
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="request">评审请求</param>
    /// <returns>创建的评审</returns>
    [HttpPost("{owner}/{repo}/{number}/reviews")]
    public async Task<IActionResult> CreatePullRequestReview(string owner, string repo, int number, [FromBody] CreatePullRequestReviewRequest request)
    {
        try
        {
            var reviewEvent = ParseReviewEvent(request.Event ?? "comment");

            var review = await _pullRequestService.CreatePullRequestReviewAsync(
                owner, repo, number, request.CommitId, request.Body, reviewEvent);

            var result = new
            {
                id = review.Id,
                body = review.Body,
                state = review.State,
                user = new
                {
                    login = review.User.Login,
                    avatar_url = review.User.AvatarUrl
                },
                submitted_at = review.SubmittedAt,
                commit_id = review.CommitId,
                html_url = review.HtmlUrl
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"创建 Pull Request 评审失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 请求 Pull Request 评审者
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="request">评审请求</param>
    /// <returns>Pull Request 信息</returns>
    [HttpPost("{owner}/{repo}/{number}/requested_reviewers")]
    public async Task<IActionResult> RequestPullRequestReview(string owner, string repo, int number, [FromBody] RequestPullRequestReviewRequest request)
    {
        try
        {
            var pullRequest = await _pullRequestService.RequestPullRequestReviewAsync(
                owner, repo, number, request.Reviewers, request.TeamReviewers);

            var result = new
            {
                number = pullRequest.Number,
                title = pullRequest.Title,
                requested_reviewers = pullRequest.RequestedReviewers.Select(r => new { login = r.Login, avatar_url = r.AvatarUrl })
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Pull Request 不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"请求 Pull Request 评审失败: {ex.Message}" });
        }
    }

    #region Helper Methods

    private ItemStateFilter ParseStateFilter(string state)
    {
        return state.ToLower() switch
        {
            "open" => ItemStateFilter.Open,
            "closed" => ItemStateFilter.Closed,
            "all" => ItemStateFilter.All,
            _ => ItemStateFilter.Open
        };
    }

    private PullRequestSort ParseSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "created" => PullRequestSort.Created,
            "updated" => PullRequestSort.Updated,
            "popularity" => PullRequestSort.Popularity,
            "long-running" => PullRequestSort.LongRunning,
            _ => PullRequestSort.Created
        };
    }

    private SortDirection ParseSortDirection(string direction)
    {
        return direction.ToLower() switch
        {
            "asc" => SortDirection.Ascending,
            "desc" => SortDirection.Descending,
            _ => SortDirection.Descending
        };
    }

    private ItemState ParseItemState(string state)
    {
        return state.ToLower() switch
        {
            "open" => ItemState.Open,
            "closed" => ItemState.Closed,
            _ => ItemState.Open
        };
    }

    private PullRequestMergeMethod ParseMergeMethod(string method)
    {
        return method.ToLower() switch
        {
            "merge" => PullRequestMergeMethod.Merge,
            "squash" => PullRequestMergeMethod.Squash,
            "rebase" => PullRequestMergeMethod.Rebase,
            _ => PullRequestMergeMethod.Merge
        };
    }

    private PullRequestReviewEvent ParseReviewEvent(string eventType)
    {
        return eventType.ToLower() switch
        {
            "approve" => PullRequestReviewEvent.Approve,
            "request_changes" => PullRequestReviewEvent.RequestChanges,
            "comment" => PullRequestReviewEvent.Comment,
            _ => PullRequestReviewEvent.Comment
        };
    }

    #endregion

    #region Request Models

    public class CreatePullRequestRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Head { get; set; } = string.Empty;
        public string Base { get; set; } = string.Empty;
        public string? Body { get; set; }
        public bool Draft { get; set; } = false;
        public bool MaintainerCanModify { get; set; } = true;
    }

    public class UpdatePullRequestRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? State { get; set; }
        public string? Base { get; set; }
        public bool? MaintainerCanModify { get; set; }
    }

    public class MergePullRequestRequest
    {
        public string? CommitMessage { get; set; }
        public string? CommitTitle { get; set; }
        public string MergeMethod { get; set; } = "merge";
    }

    public class CreatePullRequestReviewRequest
    {
        public string? CommitId { get; set; }
        public string? Body { get; set; }
        public string Event { get; set; } = "comment";
    }

    public class RequestPullRequestReviewRequest
    {
        public IReadOnlyList<string>? Reviewers { get; set; }
        public IReadOnlyList<string>? TeamReviewers { get; set; }
    }

    #endregion
}
