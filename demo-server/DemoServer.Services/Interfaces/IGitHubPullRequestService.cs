using Octokit;

namespace DemoServer.Services.Interfaces;

public interface IGitHubPullRequestService
{
    /// <summary>
    /// 获取指定仓库的 Pull Request 列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="state">Pull Request 状态</param>
    /// <param name="head">头部分支</param>
    /// <param name="base">基础分支</param>
    /// <param name="sort">排序方式</param>
    /// <param name="direction">排序方向</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="page">页码</param>
    /// <returns>Pull Request 列表</returns>
    Task<IReadOnlyList<PullRequest>> GetRepositoryPullRequestsAsync(
        string owner,
        string name,
        ItemStateFilter state = ItemStateFilter.Open,
        string? head = null,
        string? @base = null,
        PullRequestSort sort = PullRequestSort.Created,
        SortDirection direction = SortDirection.Descending,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 获取指定 Pull Request 的详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>Pull Request 详细信息</returns>
    Task<PullRequest> GetPullRequestAsync(string owner, string name, int number);

    /// <summary>
    /// 创建新的 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="title">标题</param>
    /// <param name="head">头部分支</param>
    /// <param name="@base">基础分支</param>
    /// <param name="body">描述内容</param>
    /// <param name="draft">是否为草稿</param>
    /// <param name="maintainerCanModify">维护者是否可以修改</param>
    /// <returns>创建的 Pull Request</returns>
    Task<PullRequest> CreatePullRequestAsync(
        string owner,
        string name,
        string title,
        string head,
        string @base,
        string? body = null,
        bool draft = false,
        bool maintainerCanModify = true);

    /// <summary>
    /// 更新 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="title">新标题</param>
    /// <param name="body">新描述内容</param>
    /// <param name="state">新状态</param>
    /// <param name="@base">新基础分支</param>
    /// <param name="maintainerCanModify">维护者是否可以修改</param>
    /// <returns>更新后的 Pull Request</returns>
    Task<PullRequest> UpdatePullRequestAsync(
        string owner,
        string name,
        int number,
        string? title = null,
        string? body = null,
        ItemState? state = null,
        string? @base = null,
        bool? maintainerCanModify = null);

    /// <summary>
    /// 合并 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="commitMessage">合并提交消息</param>
    /// <param name="commitTitle">合并提交标题</param>
    /// <param name="mergeMethod">合并方法</param>
    /// <returns>合并结果</returns>
    Task<PullRequestMerge> MergePullRequestAsync(
        string owner,
        string name,
        int number,
        string? commitMessage = null,
        string? commitTitle = null,
        PullRequestMergeMethod mergeMethod = PullRequestMergeMethod.Merge);

    /// <summary>
    /// 关闭 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>关闭后的 Pull Request</returns>
    Task<PullRequest> ClosePullRequestAsync(string owner, string name, int number);

    /// <summary>
    /// 重新打开 Pull Request
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>重新打开的 Pull Request</returns>
    Task<PullRequest> ReopenPullRequestAsync(string owner, string name, int number);

    /// <summary>
    /// 获取 Pull Request 的文件变更
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>文件变更列表</returns>
    Task<IReadOnlyList<PullRequestFile>> GetPullRequestFilesAsync(string owner, string name, int number);

    /// <summary>
    /// 获取 Pull Request 的提交记录
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>提交记录列表</returns>
    Task<IReadOnlyList<PullRequestCommit>> GetPullRequestCommitsAsync(string owner, string name, int number);

    /// <summary>
    /// 获取 Pull Request 的评审
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <returns>评审列表</returns>
    Task<IReadOnlyList<PullRequestReview>> GetPullRequestReviewsAsync(string owner, string name, int number);

    /// <summary>
    /// 创建 Pull Request 评审
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="commitId">提交 ID</param>
    /// <param name="body">评审内容</param>
    /// <param name="event">评审事件</param>
    /// <returns>创建的评审</returns>
    Task<PullRequestReview> CreatePullRequestReviewAsync(
        string owner,
        string name,
        int number,
        string? commitId = null,
        string? body = null,
        PullRequestReviewEvent @event = PullRequestReviewEvent.Comment);

    /// <summary>
    /// 请求 Pull Request 评审者
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">Pull Request 编号</param>
    /// <param name="reviewers">评审者用户名列表</param>
    /// <param name="teamReviewers">团队评审者列表</param>
    /// <returns>Pull Request 信息</returns>
    Task<PullRequest> RequestPullRequestReviewAsync(
        string owner,
        string name,
        int number,
        IReadOnlyList<string>? reviewers = null,
        IReadOnlyList<string>? teamReviewers = null);
}
