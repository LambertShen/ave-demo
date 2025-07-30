using Octokit;
using DemoServer.Services.Interfaces;

namespace DemoServer.Services.Services;

public class GitHubTimeLineService : IGitHubTimeLineService
{
    private readonly IGitHubOAuthService _oauthService;

    public GitHubTimeLineService(IGitHubOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    #region Issue Timeline

    public async Task<IReadOnlyList<TimelineEventInfo>> GetIssueTimelineAsync(
        string owner,
        string repo,
        int issueNumber,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Issue.Timeline.GetAllForIssue(owner, repo, issueNumber, options);
    }

    public async Task<IssueComment> CreateIssueCommentAsync(
        string owner,
        string repo,
        int issueNumber,
        string body)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Comment.Create(owner, repo, issueNumber, body);
    }

    public async Task<IssueComment> UpdateIssueCommentAsync(
        string owner,
        string repo,
        long commentId,
        string body)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Comment.Update(owner, repo, commentId, body);
    }

    public async Task DeleteIssueCommentAsync(
        string owner,
        string repo,
        long commentId)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.Issue.Comment.Delete(owner, repo, commentId);
    }

    public async Task<IReadOnlyList<Label>> AddLabelsToIssueAsync(
        string owner,
        string repo,
        int issueNumber,
        string[] labels)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Labels.AddToIssue(owner, repo, issueNumber, labels);
    }

    public async Task RemoveLabelFromIssueAsync(
        string owner,
        string repo,
        int issueNumber,
        string labelName)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.Issue.Labels.RemoveFromIssue(owner, repo, issueNumber, labelName);
    }

    public async Task<Issue> CloseIssueAsync(
        string owner,
        string repo,
        int issueNumber)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var updateIssue = new IssueUpdate
        {
            State = ItemState.Closed
        };

        return await client.Issue.Update(owner, repo, issueNumber, updateIssue);
    }

    public async Task<Issue> ReopenIssueAsync(
        string owner,
        string repo,
        int issueNumber)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var updateIssue = new IssueUpdate
        {
            State = ItemState.Open
        };

        return await client.Issue.Update(owner, repo, issueNumber, updateIssue);
    }

    #endregion

    #region Pull Request Timeline

    public async Task<IReadOnlyList<TimelineEventInfo>> GetPullRequestTimelineAsync(
        string owner,
        string repo,
        int pullNumber,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Issue.Timeline.GetAllForIssue(owner, repo, pullNumber, options);
    }

    public async Task<IssueComment> CreatePullRequestCommentAsync(
        string owner,
        string repo,
        int pullNumber,
        string body)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Comment.Create(owner, repo, pullNumber, body);
    }

    public async Task<PullRequestReviewComment> CreatePullRequestReviewCommentAsync(
        string owner,
        string repo,
        int pullNumber,
        string body,
        string commitSha,
        string path,
        int line)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var comment = new PullRequestReviewCommentCreate(body, commitSha, path, line);
        return await client.PullRequest.ReviewComment.Create(owner, repo, pullNumber, comment);
    }

    public async Task<PullRequestReviewComment> UpdatePullRequestReviewCommentAsync(
        string owner,
        string repo,
        long commentId,
        string body)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var update = new PullRequestReviewCommentEdit(body);
        return await client.PullRequest.ReviewComment.Edit(owner, repo, commentId, update);
    }

    public async Task DeletePullRequestReviewCommentAsync(
        string owner,
        string repo,
        long commentId)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.PullRequest.ReviewComment.Delete(owner, repo, commentId);
    }

    public async Task<IReadOnlyList<Label>> AddLabelsToPullRequestAsync(
        string owner,
        string repo,
        int pullNumber,
        string[] labels)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Labels.AddToIssue(owner, repo, pullNumber, labels);
    }

    public async Task RemoveLabelFromPullRequestAsync(
        string owner,
        string repo,
        int pullNumber,
        string labelName)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.Issue.Labels.RemoveFromIssue(owner, repo, pullNumber, labelName);
    }

    public async Task<PullRequest> ClosePullRequestAsync(
        string owner,
        string repo,
        int pullNumber)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var updatePr = new PullRequestUpdate
        {
            State = ItemState.Closed
        };

        return await client.PullRequest.Update(owner, repo, pullNumber, updatePr);
    }

    public async Task<PullRequest> ReopenPullRequestAsync(
        string owner,
        string repo,
        int pullNumber)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var updatePr = new PullRequestUpdate
        {
            State = ItemState.Open
        };

        return await client.PullRequest.Update(owner, repo, pullNumber, updatePr);
    }

    public async Task<PullRequestMerge> MergePullRequestAsync(
        string owner,
        string repo,
        int pullNumber,
        string? commitTitle = null,
        string? commitMessage = null,
        PullRequestMergeMethod mergeMethod = PullRequestMergeMethod.Merge)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var merge = new MergePullRequest
        {
            CommitTitle = commitTitle,
            CommitMessage = commitMessage,
            MergeMethod = mergeMethod
        };

        return await client.PullRequest.Merge(owner, repo, pullNumber, merge);
    }

    #endregion

    #region Timeline Event Details

    public async Task<object?> GetTimelineEventDetailsAsync(
        string owner,
        string repo,
        TimelineEventInfo eventInfo)
    {
        try
        {
            var eventType = GetSafeEventName(eventInfo.Event);
            
            return eventType.ToLower() switch
            {
                "committed" when !string.IsNullOrEmpty(eventInfo.CommitId) => 
                    await GetCommitDetailsAsync(owner, repo, eventInfo.CommitId),
                
                "commented" when eventInfo.Id > 0 => 
                    await GetCommentDetailsAsync(owner, repo, eventInfo.Id),
                
                "labeled" or "unlabeled" when eventInfo.Label != null => 
                    await GetLabelDetailsAsync(owner, repo, eventInfo.Label.Name),
                
                "milestoned" or "demilestoned" when eventInfo.Milestone != null => 
                    await GetMilestoneDetailsAsync(owner, repo, eventInfo.Milestone.Number),
                
                "assigned" or "unassigned" when eventInfo.Assignee != null => 
                    await GetUserDetailsAsync(eventInfo.Assignee.Login),
                
                _ => new
                {
                    eventType,
                    actor = eventInfo.Actor,
                    createdAt = eventInfo.CreatedAt
                }
            };
        }
        catch (Exception)
        {
            return new
            {
                eventType = GetSafeEventName(eventInfo.Event),
                actor = eventInfo.Actor,
                createdAt = eventInfo.CreatedAt,
                error = "Failed to get detailed information"
            };
        }
    }

    public async Task<GitHubCommit?> GetCommitDetailsAsync(
        string owner,
        string repo,
        string commitSha)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            return await client.Repository.Commit.Get(owner, repo, commitSha);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IssueComment?> GetCommentDetailsAsync(
        string owner,
        string repo,
        long commentId)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            return await client.Issue.Comment.Get(owner, repo, commentId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Label?> GetLabelDetailsAsync(
        string owner,
        string repo,
        string labelName)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            return await client.Issue.Labels.Get(owner, repo, labelName);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Milestone?> GetMilestoneDetailsAsync(
        string owner,
        string repo,
        int milestoneNumber)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            return await client.Issue.Milestone.Get(owner, repo, milestoneNumber);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<User?> GetUserDetailsAsync(string username)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            return await client.User.Get(username);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string GetSafeEventName(StringEnum<EventInfoState> eventEnum)
    {
        try
        {
            return eventEnum.ToString();
        }
        catch (ArgumentException)
        {
            return eventEnum.StringValue ?? "unknown";
        }
    }

    #endregion
}
