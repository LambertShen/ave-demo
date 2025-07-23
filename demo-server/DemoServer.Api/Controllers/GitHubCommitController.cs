using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using DemoServer.Api.Models;
using Octokit;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/github/commits")]
[Tags("GitHub Commits")]
public class GitHubCommitController : ControllerBase
{
    private readonly IGitHubCommitService _commitService;

    public GitHubCommitController(IGitHubCommitService commitService)
    {
        _commitService = commitService;
    }

    /// <summary>
    /// 获取指定仓库的提交列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="sha">分支或提交的 SHA（可选）</param>
    /// <param name="path">文件路径过滤（可选）</param>
    /// <param name="author">作者过滤（可选）</param>
    /// <param name="since">起始时间</param>
    /// <param name="until">结束时间</param>
    /// <param name="page">页码（从1开始）</param>
    /// <param name="pageSize">每页数量（最大100）</param>
    /// <returns>提交列表</returns>
    [HttpGet("{owner}/{repo}")]
    public async Task<IActionResult> GetRepositoryCommits(
        string owner,
        string repo,
        [FromQuery] string? sha = null,
        [FromQuery] string? path = null,
        [FromQuery] string? author = null,
        [FromQuery] DateTimeOffset? since = null,
        [FromQuery] DateTimeOffset? until = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var commits = await _commitService.GetRepositoryCommitsAsync(
                owner, repo, sha, path, author, since, until, pageSize, page);

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
                    },
                    committer = new
                    {
                        name = commit.Commit.Committer.Name,
                        email = commit.Commit.Committer.Email,
                        date = commit.Commit.Committer.Date
                    },
                    tree = new
                    {
                        sha = commit.Commit.Tree.Sha
                    },
                    comment_count = commit.Commit.CommentCount
                },
                author = commit.Author != null ? new
                {
                    login = commit.Author.Login,
                    avatar_url = commit.Author.AvatarUrl,
                    html_url = commit.Author.HtmlUrl
                } : null,
                committer = commit.Committer != null ? new
                {
                    login = commit.Committer.Login,
                    avatar_url = commit.Committer.AvatarUrl,
                    html_url = commit.Committer.HtmlUrl
                } : null,
                html_url = commit.HtmlUrl,
                parents = commit.Parents.Select(p => new { sha = p.Sha })
            });

            return Ok(new
            {
                commits = result,
                count = commits.Count,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取提交列表失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取指定提交的详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <returns>提交详细信息</returns>
    [HttpGet("{owner}/{repo}/{sha}")]
    public async Task<IActionResult> GetCommit(string owner, string repo, string sha)
    {
        try
        {
            var commit = await _commitService.GetCommitAsync(owner, repo, sha);

            var result = new
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
                    },
                    committer = new
                    {
                        name = commit.Commit.Committer.Name,
                        email = commit.Commit.Committer.Email,
                        date = commit.Commit.Committer.Date
                    },
                    tree = new
                    {
                        sha = commit.Commit.Tree.Sha
                    },
                    comment_count = commit.Commit.CommentCount
                },
                author = commit.Author != null ? new
                {
                    login = commit.Author.Login,
                    avatar_url = commit.Author.AvatarUrl,
                    html_url = commit.Author.HtmlUrl
                } : null,
                committer = commit.Committer != null ? new
                {
                    login = commit.Committer.Login,
                    avatar_url = commit.Committer.AvatarUrl,
                    html_url = commit.Committer.HtmlUrl
                } : null,
                html_url = commit.HtmlUrl,
                parents = commit.Parents.Select(p => new { sha = p.Sha }),
                stats = new
                {
                    additions = commit.Stats.Additions,
                    deletions = commit.Stats.Deletions,
                    total = commit.Stats.Total
                },
                files = commit.Files.Select(file => new
                {
                    filename = file.Filename,
                    status = file.Status,
                    additions = file.Additions,
                    deletions = file.Deletions,
                    changes = file.Changes,
                    blob_url = file.BlobUrl,
                    patch = file.Patch
                })
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "提交不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取提交信息失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 创建新的提交
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="request">创建请求</param>
    /// <returns>创建的提交</returns>
    [HttpPost("{owner}/{repo}")]
    public async Task<IActionResult> CreateCommit(string owner, string repo, [FromBody] CreateCommitRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Message) || string.IsNullOrEmpty(request.Tree))
            {
                return BadRequest(new { error = "Message 和 Tree 为必填字段" });
            }

            // Signature 在 Octokit 中可能有不同的构造器，先设为 null
            Signature? author = null;
            if (request.Author != null)
            {
                // 使用简单的 string, string 构造器或者直接 null
                author = null;
            }

            Signature? committer = null;
            if (request.Committer != null)
            {
                committer = null;
            }

            var commit = await _commitService.CreateCommitAsync(
                owner, repo, request.Message, request.Tree, request.Parents ?? new List<string>(), author, committer);

            var result = new
            {
                sha = commit.Sha,
                message = commit.Message,
                author = new
                {
                    name = commit.Author.Name,
                    email = commit.Author.Email,
                    date = commit.Author.Date
                },
                committer = new
                {
                    name = commit.Committer.Name,
                    email = commit.Committer.Email,
                    date = commit.Committer.Date
                },
                tree = new
                {
                    sha = commit.Tree.Sha
                },
                parents = commit.Parents.Select(p => new { sha = p.Sha })
            };

            return CreatedAtAction(
                nameof(GetCommit),
                new { owner, repo, sha = commit.Sha },
                result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"创建提交失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 比较两个提交之间的差异
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="base">基础提交的 SHA</param>
    /// <param name="head">头部提交的 SHA</param>
    /// <returns>提交比较结果</returns>
    [HttpGet("{owner}/{repo}/compare/{base}...{head}")]
    public async Task<IActionResult> CompareCommits(string owner, string repo, string @base, string head)
    {
        try
        {
            var compareResult = await _commitService.CompareCommitsAsync(owner, repo, @base, head);

            var result = new
            {
                base_commit = new
                {
                    sha = compareResult.BaseCommit.Sha,
                    commit = new
                    {
                        message = compareResult.BaseCommit.Commit.Message,
                        author = new
                        {
                            name = compareResult.BaseCommit.Commit.Author.Name,
                            email = compareResult.BaseCommit.Commit.Author.Email,
                            date = compareResult.BaseCommit.Commit.Author.Date
                        }
                    }
                },
                merge_base_commit = new
                {
                    sha = compareResult.MergeBaseCommit.Sha,
                    commit = new
                    {
                        message = compareResult.MergeBaseCommit.Commit.Message
                    }
                },
                status = compareResult.Status,
                ahead_by = compareResult.AheadBy,
                behind_by = compareResult.BehindBy,
                total_commits = compareResult.TotalCommits,
                commits = compareResult.Commits.Select(commit => new
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
                    } : null
                }),
                files = compareResult.Files.Select(file => new
                {
                    filename = file.Filename,
                    status = file.Status,
                    additions = file.Additions,
                    deletions = file.Deletions,
                    changes = file.Changes,
                    patch = file.Patch
                })
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "提交不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"比较提交失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取提交的状态检查
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <returns>状态检查列表</returns>
    [HttpGet("{owner}/{repo}/{sha}/statuses")]
    public async Task<IActionResult> GetCommitStatuses(string owner, string repo, string sha)
    {
        try
        {
            var statuses = await _commitService.GetCommitStatusesAsync(owner, repo, sha);

            var result = statuses.Select(status => new
            {
                id = status.Id,
                state = status.State.StringValue,
                description = status.Description,
                target_url = status.TargetUrl,
                context = status.Context,
                created_at = status.CreatedAt,
                updated_at = status.UpdatedAt,
                creator = status.Creator != null ? new
                {
                    login = status.Creator.Login,
                    avatar_url = status.Creator.AvatarUrl
                } : null
            });

            return Ok(new
            {
                statuses = result,
                count = statuses.Count
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "提交不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取提交状态失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 创建提交状态
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <param name="request">状态创建请求</param>
    /// <returns>创建的状态</returns>
    [HttpPost("{owner}/{repo}/{sha}/statuses")]
    public async Task<IActionResult> CreateCommitStatus(string owner, string repo, string sha, [FromBody] CreateCommitStatusRequest request)
    {
        try
        {
            var state = ParseCommitState(request.State ?? "pending");

            var status = await _commitService.CreateCommitStatusAsync(
                owner, repo, sha, state, request.TargetUrl, request.Description, request.Context);

            var result = new
            {
                id = status.Id,
                state = status.State.StringValue,
                description = status.Description,
                target_url = status.TargetUrl,
                context = status.Context,
                created_at = status.CreatedAt,
                updated_at = status.UpdatedAt
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "提交不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"创建提交状态失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取提交的评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <returns>评论列表</returns>
    [HttpGet("{owner}/{repo}/{sha}/comments")]
    public async Task<IActionResult> GetCommitComments(string owner, string repo, string sha)
    {
        try
        {
            var comments = await _commitService.GetCommitCommentsAsync(owner, repo, sha);

            var result = comments.Select(comment => new
            {
                id = comment.Id,
                body = comment.Body,
                path = comment.Path,
                line = comment.Line,
                position = comment.Position,
                user = new
                {
                    login = comment.User.Login,
                    avatar_url = comment.User.AvatarUrl
                },
                created_at = comment.CreatedAt,
                updated_at = comment.UpdatedAt,
                html_url = comment.HtmlUrl
            });

            return Ok(new
            {
                comments = result,
                count = comments.Count
            });
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "提交不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取提交评论失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 创建提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <param name="request">评论创建请求</param>
    /// <returns>创建的评论</returns>
    [HttpPost("{owner}/{repo}/{sha}/comments")]
    public async Task<IActionResult> CreateCommitComment(string owner, string repo, string sha, [FromBody] CreateCommitCommentRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Body))
            {
                return BadRequest(new { error = "Body 为必填字段" });
            }

            var comment = await _commitService.CreateCommitCommentAsync(
                owner, repo, sha, request.Body, request.Path, request.Line);

            var result = new
            {
                id = comment.Id,
                body = comment.Body,
                path = comment.Path,
                line = comment.Line,
                position = comment.Position,
                user = new
                {
                    login = comment.User.Login,
                    avatar_url = comment.User.AvatarUrl
                },
                created_at = comment.CreatedAt,
                updated_at = comment.UpdatedAt,
                html_url = comment.HtmlUrl
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "提交不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"创建提交评论失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 更新提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的评论</returns>
    [HttpPut("{owner}/{repo}/comments/{commentId}")]
    public async Task<IActionResult> UpdateCommitComment(string owner, string repo, long commentId, [FromBody] UpdateCommitCommentRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Body))
            {
                return BadRequest(new { error = "Body 为必填字段" });
            }

            var comment = await _commitService.UpdateCommitCommentAsync(owner, repo, commentId, request.Body);

            var result = new
            {
                id = comment.Id,
                body = comment.Body,
                path = comment.Path,
                line = comment.Line,
                position = comment.Position,
                user = new
                {
                    login = comment.User.Login,
                    avatar_url = comment.User.AvatarUrl
                },
                created_at = comment.CreatedAt,
                updated_at = comment.UpdatedAt,
                html_url = comment.HtmlUrl
            };

            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "评论不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"更新提交评论失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 删除提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{owner}/{repo}/comments/{commentId}")]
    public async Task<IActionResult> DeleteCommitComment(string owner, string repo, long commentId)
    {
        try
        {
            await _commitService.DeleteCommitCommentAsync(owner, repo, commentId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "评论不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"删除提交评论失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取仓库的所有提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="since">起始时间</param>
    /// <param name="page">页码（从1开始）</param>
    /// <param name="pageSize">每页数量（最大100）</param>
    /// <returns>评论列表</returns>
    [HttpGet("{owner}/{repo}/comments")]
    public async Task<IActionResult> GetRepositoryCommitComments(
        string owner,
        string repo,
        [FromQuery] DateTimeOffset? since = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var comments = await _commitService.GetRepositoryCommitCommentsAsync(owner, repo, since, pageSize, page);

            var result = comments.Select(comment => new
            {
                id = comment.Id,
                body = comment.Body,
                path = comment.Path,
                line = comment.Line,
                position = comment.Position,
                commit_id = comment.CommitId,
                user = new
                {
                    login = comment.User.Login,
                    avatar_url = comment.User.AvatarUrl
                },
                created_at = comment.CreatedAt,
                updated_at = comment.UpdatedAt,
                html_url = comment.HtmlUrl
            });

            return Ok(new
            {
                comments = result,
                count = comments.Count,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"获取仓库提交评论失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 提交文件并创建 commit 记录
    /// </summary>
    [HttpPost("{owner}/{repo}/commit-files")]
    public async Task<IActionResult> CommitFiles(string owner, string repo, [FromBody] CommitFilesRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.CommitMessage))
            {
                return BadRequest(new { error = "提交消息不能为空" });
            }

            if (request.FileChanges.Count == 0 && request.FilesToDelete.Count == 0)
            {
                return BadRequest(new { error = "必须至少有一个文件变更或删除" });
            }

            // 转换作者信息
            DemoServer.Services.Models.CommitAuthor? author = null;
            if (request.Author != null)
            {
                author = new DemoServer.Services.Models.CommitAuthor
                {
                    Name = request.Author.Name,
                    Email = request.Author.Email,
                    Date = request.Author.Date
                };
            }

            // 转换提交者信息
            DemoServer.Services.Models.CommitAuthor? committer = null;
            if (request.Committer != null)
            {
                committer = new DemoServer.Services.Models.CommitAuthor
                {
                    Name = request.Committer.Name,
                    Email = request.Committer.Email,
                    Date = request.Committer.Date
                };
            }

            GitHubCommit commit;

            // 处理文件变更
            if (request.FileChanges.Count > 0)
            {
                commit = await _commitService.CommitFilesAsync(
                    owner, 
                    repo, 
                    request.FileChanges, 
                    request.CommitMessage, 
                    request.Branch,
                    author,
                    committer);
            }
            // 处理文件删除
            else if (request.FilesToDelete.Count > 0)
            {
                commit = await _commitService.DeleteFilesAsync(
                    owner, 
                    repo, 
                    request.FilesToDelete, 
                    request.CommitMessage, 
                    request.Branch,
                    author,
                    committer);
            }
            else
            {
                return BadRequest(new { error = "必须至少有一个文件变更或删除" });
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
                url = commit.HtmlUrl,
                tree = new { sha = commit.Commit.Tree.Sha },
                filesChanged = request.FileChanges.Count,
                filesDeleted = request.FilesToDelete.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"提交文件失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 提交单个文件
    /// </summary>
    [HttpPost("{owner}/{repo}/commit-file")]
    public async Task<IActionResult> CommitSingleFile(string owner, string repo, [FromBody] CommitSingleFileRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.CommitMessage) || 
                string.IsNullOrEmpty(request.FilePath) || 
                string.IsNullOrEmpty(request.FileContent))
            {
                return BadRequest(new { error = "提交消息、文件路径和文件内容都不能为空" });
            }

            // 转换作者信息
            DemoServer.Services.Models.CommitAuthor? author = null;
            if (request.Author != null)
            {
                author = new DemoServer.Services.Models.CommitAuthor
                {
                    Name = request.Author.Name,
                    Email = request.Author.Email,
                    Date = request.Author.Date
                };
            }

            // 转换提交者信息
            DemoServer.Services.Models.CommitAuthor? committer = null;
            if (request.Committer != null)
            {
                committer = new DemoServer.Services.Models.CommitAuthor
                {
                    Name = request.Committer.Name,
                    Email = request.Committer.Email,
                    Date = request.Committer.Date
                };
            }

            var commit = await _commitService.CommitSingleFileAsync(
                owner, 
                repo, 
                request.FilePath, 
                request.FileContent, 
                request.CommitMessage, 
                request.Branch ?? "main",
                author,
                committer);

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
                url = commit.HtmlUrl,
                filePath = request.FilePath,
                tree = new { sha = commit.Commit.Tree.Sha }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"提交单个文件失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 删除文件并提交
    /// </summary>
    [HttpPost("{owner}/{repo}/delete-files")]
    public async Task<IActionResult> DeleteFiles(string owner, string repo, [FromBody] DeleteFilesRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.CommitMessage) || 
                request.FilePaths == null || 
                request.FilePaths.Count == 0)
            {
                return BadRequest(new { error = "提交消息和要删除的文件路径不能为空" });
            }

            // 转换作者信息
            DemoServer.Services.Models.CommitAuthor? author = null;
            if (request.Author != null)
            {
                author = new DemoServer.Services.Models.CommitAuthor
                {
                    Name = request.Author.Name,
                    Email = request.Author.Email,
                    Date = request.Author.Date
                };
            }

            // 转换提交者信息
            DemoServer.Services.Models.CommitAuthor? committer = null;
            if (request.Committer != null)
            {
                committer = new DemoServer.Services.Models.CommitAuthor
                {
                    Name = request.Committer.Name,
                    Email = request.Committer.Email,
                    Date = request.Committer.Date
                };
            }

            var commit = await _commitService.DeleteFilesAsync(
                owner, 
                repo, 
                request.FilePaths, 
                request.CommitMessage, 
                request.Branch ?? "main",
                author,
                committer);

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
                url = commit.HtmlUrl,
                filesDeleted = request.FilePaths,
                tree = new { sha = commit.Commit.Tree.Sha }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"删除文件失败: {ex.Message}" });
        }
    }

    #region Helper Methods

    private CommitState ParseCommitState(string state)
    {
        return state.ToLower() switch
        {
            "pending" => CommitState.Pending,
            "success" => CommitState.Success,
            "error" => CommitState.Error,
            "failure" => CommitState.Failure,
            _ => CommitState.Pending
        };
    }

    #endregion
}
