using Octokit;
using DemoServer.Services.Interfaces;

namespace DemoServer.Services.Services;

public class GitHubPullRequestService : IGitHubPullRequestService
{
    private readonly IGitHubOAuthService _oauthService;

    public GitHubPullRequestService(IGitHubOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    public async Task<IReadOnlyList<PullRequest>> GetRepositoryPullRequestsAsync(
        string owner,
        string name,
        ItemStateFilter state = ItemStateFilter.Open,
        string? head = null,
        string? @base = null,
        PullRequestSort sort = PullRequestSort.Created,
        SortDirection direction = SortDirection.Descending,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var request = new PullRequestRequest
        {
            State = state,
            Head = head,
            Base = @base,
            SortProperty = sort,
            SortDirection = direction
        };

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.PullRequest.GetAllForRepository(owner, name, request, options);
    }

    public async Task<PullRequest> GetPullRequestAsync(string owner, string name, int number)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.PullRequest.Get(owner, name, number);
    }

    public async Task<PullRequest> CreatePullRequestAsync(
        string owner,
        string name,
        string title,
        string head,
        string @base,
        string? body = null,
        bool draft = false,
        bool maintainerCanModify = true)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var newPullRequest = new NewPullRequest(title, head, @base)
        {
            Body = body,
            Draft = draft,
            MaintainerCanModify = maintainerCanModify
        };

        return await client.PullRequest.Create(owner, name, newPullRequest);
    }

    public async Task<PullRequest> UpdatePullRequestAsync(
        string owner,
        string name,
        int number,
        string? title = null,
        string? body = null,
        ItemState? state = null,
        string? @base = null,
        bool? maintainerCanModify = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var pullRequestUpdate = new PullRequestUpdate();

        if (!string.IsNullOrEmpty(title))
            pullRequestUpdate.Title = title;

        if (body != null)
            pullRequestUpdate.Body = body;

        if (state.HasValue)
            pullRequestUpdate.State = state.Value;

        if (!string.IsNullOrEmpty(@base))
            pullRequestUpdate.Base = @base;

        if (maintainerCanModify.HasValue)
            pullRequestUpdate.MaintainerCanModify = maintainerCanModify.Value;

        return await client.PullRequest.Update(owner, name, number, pullRequestUpdate);
    }

    public async Task<PullRequestMerge> MergePullRequestAsync(
        string owner,
        string name,
        int number,
        string? commitMessage = null,
        string? commitTitle = null,
        PullRequestMergeMethod mergeMethod = PullRequestMergeMethod.Merge)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var mergePullRequest = new MergePullRequest
        {
            CommitMessage = commitMessage,
            CommitTitle = commitTitle,
            MergeMethod = mergeMethod
        };

        return await client.PullRequest.Merge(owner, name, number, mergePullRequest);
    }

    public async Task<PullRequest> ClosePullRequestAsync(string owner, string name, int number)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var pullRequestUpdate = new PullRequestUpdate
        {
            State = ItemState.Closed
        };

        return await client.PullRequest.Update(owner, name, number, pullRequestUpdate);
    }

    public async Task<PullRequest> ReopenPullRequestAsync(string owner, string name, int number)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var pullRequestUpdate = new PullRequestUpdate
        {
            State = ItemState.Open
        };

        return await client.PullRequest.Update(owner, name, number, pullRequestUpdate);
    }

    public async Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(string owner, string name, int number)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.PullRequest.Files(owner, name, number);
    }

    public async Task<IReadOnlyList<PullRequestCommit>> GetPullRequestCommitsAsync(string owner, string name, int number)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.PullRequest.Commits(owner, name, number);
    }

    public async Task<IReadOnlyList<PullRequestReview>> GetPullRequestReviewsAsync(string owner, string name, int number)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.PullRequest.Review.GetAll(owner, name, number);
    }

    public async Task<PullRequestReview> CreatePullRequestReviewAsync(
        string owner,
        string name,
        int number,
        string? commitId = null,
        string? body = null,
        PullRequestReviewEvent @event = PullRequestReviewEvent.Comment)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var pullRequestReviewCreate = new PullRequestReviewCreate
        {
            CommitId = commitId,
            Body = body,
            Event = @event
        };

        return await client.PullRequest.Review.Create(owner, name, number, pullRequestReviewCreate);
    }

    public async Task<PullRequest> RequestPullRequestReviewAsync(
        string owner,
        string name,
        int number,
        IReadOnlyList<string>? reviewers = null,
        IReadOnlyList<string>? teamReviewers = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var reviewersList = reviewers?.ToList() ?? new List<string>();
        var teamReviewersList = teamReviewers?.ToList() ?? new List<string>();
        
        var pullRequestReviewRequest = new PullRequestReviewRequest(reviewersList, teamReviewersList);

        return await client.PullRequest.ReviewRequest.Create(owner, name, number, pullRequestReviewRequest);
    }
}
