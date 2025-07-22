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
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
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
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Get(owner, name, issueNumber);
    }

    public async Task<IReadOnlyList<Issue>> GetUserIssuesAsync(
        ItemStateFilter state = ItemStateFilter.Open,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var request = new IssueRequest
        {
            State = state,
            Since = since,
            Filter = IssueFilter.Created // Only get issues created by the user
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
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
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
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
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
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
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
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Comment.Create(owner, name, issueNumber, comment);
    }
    
    /// <summary>
    /// 关闭 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="comment">关闭时添加的评论（可选）</param>
    /// <returns>关闭后的 Issue</returns>
    public async Task<Issue> CloseIssueAsync(string owner, string name, int issueNumber, string? comment = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        // 如果有评论，先添加评论
        if (!string.IsNullOrEmpty(comment))
        {
            await client.Issue.Comment.Create(owner, name, issueNumber, comment);
        }
        
        // 关闭 Issue
        var issueUpdate = new IssueUpdate
        {
            State = ItemState.Closed
        };
        
        return await client.Issue.Update(owner, name, issueNumber, issueUpdate);
    }
    
    /// <summary>
    /// 重新打开 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="comment">重新打开时添加的评论（可选）</param>
    /// <returns>重新打开后的 Issue</returns>
    public async Task<Issue> ReopenIssueAsync(string owner, string name, int issueNumber, string? comment = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        // 如果有评论，先添加评论
        if (!string.IsNullOrEmpty(comment))
        {
            await client.Issue.Comment.Create(owner, name, issueNumber, comment);
        }
        
        // 重新打开 Issue
        var issueUpdate = new IssueUpdate
        {
            State = ItemState.Open
        };
        
        return await client.Issue.Update(owner, name, issueNumber, issueUpdate);
    }
    
    /// <summary>
    /// 批量操作 - 为多个 Issue 添加标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumbers">Issue 编号列表</param>
    /// <param name="labels">要添加的标签</param>
    /// <returns>Task</returns>
    public async Task AddLabelsToBatchIssuesAsync(string owner, string name, int[] issueNumbers, string[] labels)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var tasks = issueNumbers.Select(async issueNumber =>
        {
            await client.Issue.Labels.AddToIssue(owner, name, issueNumber, labels);
        });
        
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// 批量关闭 Issues
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumbers">要关闭的 Issue 编号列表</param>
    /// <param name="comment">关闭时添加的评论（可选）</param>
    /// <returns>Task</returns>
    public async Task CloseBatchIssuesAsync(string owner, string name, int[] issueNumbers, string? comment = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var tasks = issueNumbers.Select(async issueNumber =>
        {
            await CloseIssueAsync(owner, name, issueNumber, comment);
        });
        
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// 更新 Issue 评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="commentId">评论ID</param>
    /// <param name="body">新的评论内容</param>
    /// <returns>更新后的评论</returns>
    public async Task<IssueComment> UpdateIssueCommentAsync(string owner, string name, long commentId, string body)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Comment.Update(owner, name, commentId, body);
    }
    
    /// <summary>
    /// 删除 Issue 评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="commentId">评论ID</param>
    /// <returns>Task</returns>
    public async Task DeleteIssueCommentAsync(string owner, string name, long commentId)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.Issue.Comment.Delete(owner, name, commentId);
    }
    
    /// <summary>
    /// 添加标签到 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="labels">要添加的标签</param>
    /// <returns>更新后的标签列表</returns>
    public async Task<IReadOnlyList<Label>> AddLabelsToIssueAsync(string owner, string name, int issueNumber, string[] labels)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Labels.AddToIssue(owner, name, issueNumber, labels);
    }
    
    /// <summary>
    /// 从 Issue 移除标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="labelName">要移除的标签名称</param>
    /// <returns>剩余的标签列表</returns>
    public async Task<IReadOnlyList<Label>> RemoveLabelFromIssueAsync(string owner, string name, int issueNumber, string labelName)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Issue.Labels.RemoveFromIssue(owner, name, issueNumber, labelName);
    }
    
    /// <summary>
    /// 清除 Issue 的所有标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <returns>Task</returns>
    public async Task ClearLabelsFromIssueAsync(string owner, string name, int issueNumber)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.Issue.Labels.RemoveAllFromIssue(owner, name, issueNumber);
    }
}
