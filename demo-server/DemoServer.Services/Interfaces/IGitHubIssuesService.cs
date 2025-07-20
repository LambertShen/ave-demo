using Octokit;

namespace DemoServer.Services.Interfaces;

public interface IGitHubIssuesService
{
    /// <summary>
    /// 获取指定仓库的 Issues 列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="state">Issue 状态（open, closed, all）</param>
    /// <param name="labels">标签过滤（可选）</param>
    /// <param name="since">获取此时间之后更新的 Issues</param>
    /// <param name="pageSize">每页数量（默认30，最大100）</param>
    /// <param name="page">页码（从1开始）</param>
    /// <returns>Issues 列表</returns>
    Task<IReadOnlyList<Issue>> GetRepositoryIssuesAsync(
        string owner,
        string name,
        ItemStateFilter state = ItemStateFilter.Open,
        string[]? labels = null,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 获取指定 Issue 的详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <returns>Issue 详细信息</returns>
    Task<Issue> GetIssueAsync(string owner, string name, int issueNumber);

    /// <summary>
    /// 获取当前用户创建的所有 Issues
    /// </summary>
    /// <param name="state">Issue 状态</param>
    /// <param name="since">获取此时间之后更新的 Issues</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="page">页码</param>
    /// <returns>用户的 Issues 列表</returns>
    Task<IReadOnlyList<Issue>> GetUserIssuesAsync(
        ItemStateFilter state = ItemStateFilter.Open,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 创建新的 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="title">Issue 标题</param>
    /// <param name="body">Issue 内容</param>
    /// <param name="labels">标签</param>
    /// <param name="assignees">指派人员</param>
    /// <param name="milestone">里程碑编号</param>
    /// <returns>创建的 Issue</returns>
    Task<Issue> CreateIssueAsync(
        string owner,
        string name,
        string title,
        string? body = null,
        string[]? labels = null,
        string[]? assignees = null,
        int? milestone = null);

    /// <summary>
    /// 更新 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="title">新标题</param>
    /// <param name="body">新内容</param>
    /// <param name="state">新状态</param>
    /// <param name="labels">新标签</param>
    /// <param name="assignees">新指派人员</param>
    /// <returns>更新后的 Issue</returns>
    Task<Issue> UpdateIssueAsync(
        string owner,
        string name,
        int issueNumber,
        string? title = null,
        string? body = null,
        ItemState? state = null,
        string[]? labels = null,
        string[]? assignees = null);

    /// <summary>
    /// 获取 Issue 的评论列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="since">获取此时间之后的评论</param>
    /// <returns>评论列表</returns>
    Task<IReadOnlyList<IssueComment>> GetIssueCommentsAsync(
        string owner,
        string name,
        int issueNumber,
        DateTimeOffset? since = null);

    /// <summary>
    /// 为 Issue 添加评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="issueNumber">Issue 编号</param>
    /// <param name="comment">评论内容</param>
    /// <returns>创建的评论</returns>
    Task<IssueComment> CreateIssueCommentAsync(
        string owner,
        string name,
        int issueNumber,
        string comment);
}
