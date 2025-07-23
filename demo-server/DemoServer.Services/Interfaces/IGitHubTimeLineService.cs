using Octokit;

namespace DemoServer.Services.Interfaces;

public interface IGitHubTimeLineService
{
    #region Issue Timeline

    /// <summary>
    /// 获取 Issue 的时间线事件
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 号码</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="page">页码</param>
    /// <returns>时间线事件列表</returns>
    Task<IReadOnlyList<TimelineEventInfo>> GetIssueTimelineAsync(
        string owner,
        string repo,
        int issueNumber,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 为 Issue 添加评论事件
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 号码</param>
    /// <param name="body">评论内容</param>
    /// <returns>创建的评论</returns>
    Task<IssueComment> CreateIssueCommentAsync(
        string owner,
        string repo,
        int issueNumber,
        string body);

    /// <summary>
    /// 更新 Issue 评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <param name="body">新的评论内容</param>
    /// <returns>更新的评论</returns>
    Task<IssueComment> UpdateIssueCommentAsync(
        string owner,
        string repo,
        long commentId,
        string body);

    /// <summary>
    /// 删除 Issue 评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    Task DeleteIssueCommentAsync(
        string owner,
        string repo,
        long commentId);

    /// <summary>
    /// 为 Issue 添加标签事件
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 号码</param>
    /// <param name="labels">标签名称列表</param>
    /// <returns>更新后的 Issue 标签</returns>
    Task<IReadOnlyList<Label>> AddLabelsToIssueAsync(
        string owner,
        string repo,
        int issueNumber,
        string[] labels);

    /// <summary>
    /// 从 Issue 移除标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 号码</param>
    /// <param name="labelName">标签名称</param>
    Task RemoveLabelFromIssueAsync(
        string owner,
        string repo,
        int issueNumber,
        string labelName);

    /// <summary>
    /// 关闭 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 号码</param>
    /// <returns>更新后的 Issue</returns>
    Task<Issue> CloseIssueAsync(
        string owner,
        string repo,
        int issueNumber);

    /// <summary>
    /// 重新打开 Issue
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="issueNumber">Issue 号码</param>
    /// <returns>更新后的 Issue</returns>
    Task<Issue> ReopenIssueAsync(
        string owner,
        string repo,
        int issueNumber);

    #endregion

    #region Pull Request Timeline

    /// <summary>
    /// 获取 Pull Request 的时间线事件
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="page">页码</param>
    /// <returns>时间线事件列表</returns>
    Task<IReadOnlyList<TimelineEventInfo>> GetPullRequestTimelineAsync(
        string owner,
        string repo,
        int pullNumber,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 为 Pull Request 添加评论事件
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <param name="body">评论内容</param>
    /// <returns>创建的评论</returns>
    Task<IssueComment> CreatePullRequestCommentAsync(
        string owner,
        string repo,
        int pullNumber,
        string body);

    /// <summary>
    /// 为 Pull Request 添加代码评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <param name="body">评论内容</param>
    /// <param name="commitSha">提交 SHA</param>
    /// <param name="path">文件路径</param>
    /// <param name="line">行号</param>
    /// <returns>创建的代码评论</returns>
    Task<PullRequestReviewComment> CreatePullRequestReviewCommentAsync(
        string owner,
        string repo,
        int pullNumber,
        string body,
        string commitSha,
        string path,
        int line);

    /// <summary>
    /// 更新 Pull Request 代码评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <param name="body">新的评论内容</param>
    /// <returns>更新的评论</returns>
    Task<PullRequestReviewComment> UpdatePullRequestReviewCommentAsync(
        string owner,
        string repo,
        long commentId,
        string body);

    /// <summary>
    /// 删除 Pull Request 代码评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    Task DeletePullRequestReviewCommentAsync(
        string owner,
        string repo,
        long commentId);

    /// <summary>
    /// 为 Pull Request 添加标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <param name="labels">标签名称列表</param>
    /// <returns>更新后的标签</returns>
    Task<IReadOnlyList<Label>> AddLabelsToPullRequestAsync(
        string owner,
        string repo,
        int pullNumber,
        string[] labels);

    /// <summary>
    /// 从 Pull Request 移除标签
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <param name="labelName">标签名称</param>
    Task RemoveLabelFromPullRequestAsync(
        string owner,
        string repo,
        int pullNumber,
        string labelName);

    /// <summary>
    /// 关闭 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <returns>更新后的 Pull Request</returns>
    Task<PullRequest> ClosePullRequestAsync(
        string owner,
        string repo,
        int pullNumber);

    /// <summary>
    /// 重新打开 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <returns>更新后的 Pull Request</returns>
    Task<PullRequest> ReopenPullRequestAsync(
        string owner,
        string repo,
        int pullNumber);

    /// <summary>
    /// 合并 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="pullNumber">Pull Request 号码</param>
    /// <param name="commitTitle">合并提交标题</param>
    /// <param name="commitMessage">合并提交消息</param>
    /// <param name="mergeMethod">合并方式</param>
    /// <returns>合并结果</returns>
    Task<PullRequestMerge> MergePullRequestAsync(
        string owner,
        string repo,
        int pullNumber,
        string? commitTitle = null,
        string? commitMessage = null,
        PullRequestMergeMethod mergeMethod = PullRequestMergeMethod.Merge);

    #endregion

    #region Timeline Event Details

    /// <summary>
    /// 根据时间线事件类型获取详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="eventInfo">时间线事件信息</param>
    /// <returns>事件详细信息</returns>
    Task<object?> GetTimelineEventDetailsAsync(
        string owner,
        string repo,
        TimelineEventInfo eventInfo);

    /// <summary>
    /// 获取提交事件详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commitSha">提交 SHA</param>
    /// <returns>提交详细信息</returns>
    Task<GitHubCommit?> GetCommitDetailsAsync(
        string owner,
        string repo,
        string commitSha);

    /// <summary>
    /// 获取评论事件详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <returns>评论详细信息</returns>
    Task<IssueComment?> GetCommentDetailsAsync(
        string owner,
        string repo,
        long commentId);

    /// <summary>
    /// 获取标签事件详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="labelName">标签名称</param>
    /// <returns>标签详细信息</returns>
    Task<Label?> GetLabelDetailsAsync(
        string owner,
        string repo,
        string labelName);

    /// <summary>
    /// 获取里程碑事件详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="milestoneNumber">里程碑编号</param>
    /// <returns>里程碑详细信息</returns>
    Task<Milestone?> GetMilestoneDetailsAsync(
        string owner,
        string repo,
        int milestoneNumber);

    /// <summary>
    /// 获取用户详细信息
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>用户详细信息</returns>
    Task<User?> GetUserDetailsAsync(string username);

    #endregion
}
