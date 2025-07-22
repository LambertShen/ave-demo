using Octokit;
using DemoServer.Services.Interfaces;

namespace DemoServer.Services.Services;

public class GitHubIssuesService : IGitHubIssuesService
{
    private readonly IGitHubOAuthService _oauthService;

    public GitHubIssuesService(IGitHubOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    public async Task<IReadOnlyList<Issue>> GetRepositoryIssuesAsync(
        string owner,
        string name,
        ItemStateFilter state = ItemStateFilter.Open,
        string[]? labels = null,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        
        var request = new RepositoryIssueRequest
        {
            State = state,
            Since = since
        };

        if (labels != null && labels.Length > 0)
        {
            foreach (var label in labels)
            {
                request.Labels.Add(label);
            }
        }

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100), // GitHub limits maximum to 100
            PageCount = 1,
            StartPage = page
        };

        return await client.Issue.GetAllForRepository(owner, name, request, options);
    }

    public async Task<Issue> GetIssueAsync(string owner, string name, int issueNumber)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        return await client.Issue.Get(owner, name, issueNumber);
    }

    public async Task<IReadOnlyList<Issue>> GetUserIssuesAsync(
        ItemStateFilter state = ItemStateFilter.Open,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        
        var request = new IssueRequest
        {
            State = state,
            Since = since,
            Filter = IssueFilter.Created // 只获取用户创建的 Issues
        };

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Issue.GetAllForCurrent(request, options);
    }

    public async Task<Issue> CreateIssueAsync(
        string owner,
        string name,
        string title,
        string? body = null,
        string[]? labels = null,
        string[]? assignees = null,
        int? milestone = null)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        
        var newIssue = new NewIssue(title)
        {
            Body = body,
            Milestone = milestone
        };

        if (labels != null)
        {
            foreach (var label in labels)
            {
                newIssue.Labels.Add(label);
            }
        }

        if (assignees != null)
        {
            foreach (var assignee in assignees)
            {
                newIssue.Assignees.Add(assignee);
            }
        }

        return await client.Issue.Create(owner, name, newIssue);
    }

    public async Task<Issue> UpdateIssueAsync(
        string owner,
        string name,
        int issueNumber,
        string? title = null,
        string? body = null,
        ItemState? state = null,
        string[]? labels = null,
        string[]? assignees = null)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        
        var issueUpdate = new IssueUpdate();

        if (!string.IsNullOrEmpty(title))
            issueUpdate.Title = title;

        if (body != null)
            issueUpdate.Body = body;

        if (state.HasValue)
            issueUpdate.State = state.Value;

        if (labels != null)
        {
            issueUpdate.ClearLabels();
            foreach (var label in labels)
            {
                issueUpdate.AddLabel(label);
            }
        }

        if (assignees != null)
        {
            issueUpdate.ClearAssignees();
            foreach (var assignee in assignees)
            {
                issueUpdate.AddAssignee(assignee);
            }
        }

        return await client.Issue.Update(owner, name, issueNumber, issueUpdate);
    }

    public async Task<IReadOnlyList<IssueComment>> GetIssueCommentsAsync(
        string owner,
        string name,
        int issueNumber,
        DateTimeOffset? since = null)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        
        var request = new IssueCommentRequest
        {
            Since = since
        };

        return await client.Issue.Comment.GetAllForIssue(owner, name, issueNumber, request);
    }

    public async Task<IssueComment> CreateIssueCommentAsync(
        string owner,
        string name,
        int issueNumber,
        string comment)
    {
        var client = _oauthService.CreateAppAuthenticatedClient();
        return await client.Issue.Comment.Create(owner, name, issueNumber, comment);
    }
}
