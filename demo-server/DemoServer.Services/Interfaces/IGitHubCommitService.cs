using Octokit;

namespace DemoServer.Services.Interfaces;

public interface IGitHubCommitService
{
    /// <summary>
    /// 获取指定仓库的提交列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="sha">分支或提交的 SHA（可选）</param>
    /// <param name="path">文件路径过滤（可选）</param>
    /// <param name="author">作者过滤（可选）</param>
    /// <param name="since">起始时间</param>
    /// <param name="until">结束时间</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="page">页码</param>
    /// <returns>提交列表</returns>
    Task<IReadOnlyList<GitHubCommit>> GetRepositoryCommitsAsync(
        string owner,
        string name,
        string? sha = null,
        string? path = null,
        string? author = null,
        DateTimeOffset? since = null,
        DateTimeOffset? until = null,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 获取指定提交的详细信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <returns>提交详细信息</returns>
    Task<GitHubCommit> GetCommitAsync(string owner, string name, string sha);

    /// <summary>
    /// 创建新的提交
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="message">提交消息</param>
    /// <param name="tree">树的 SHA</param>
    /// <param name="parents">父提交的 SHA 列表</param>
    /// <param name="author">作者信息（可选）</param>
    /// <param name="committer">提交者信息（可选）</param>
    /// <returns>创建的提交</returns>
    Task<Commit> CreateCommitAsync(
        string owner,
        string name,
        string message,
        string tree,
        IEnumerable<string> parents,
        Signature? author = null,
        Signature? committer = null);

    /// <summary>
    /// 比较两个提交之间的差异
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="@base">基础提交的 SHA</param>
    /// <param name="head">头部提交的 SHA</param>
    /// <returns>提交比较结果</returns>
    Task<CompareResult> CompareCommitsAsync(string owner, string name, string @base, string head);

    /// <summary>
    /// 获取提交的状态检查
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <returns>状态检查列表</returns>
    Task<IReadOnlyList<CommitStatus>> GetCommitStatusesAsync(string owner, string name, string sha);

    /// <summary>
    /// 创建提交状态
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <param name="state">状态</param>
    /// <param name="targetUrl">目标 URL（可选）</param>
    /// <param name="description">描述（可选）</param>
    /// <param name="context">上下文（可选）</param>
    /// <returns>创建的状态</returns>
    Task<CommitStatus> CreateCommitStatusAsync(
        string owner,
        string name,
        string sha,
        CommitState state,
        string? targetUrl = null,
        string? description = null,
        string? context = null);

    /// <summary>
    /// 获取提交的评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <returns>评论列表</returns>
    Task<IReadOnlyList<CommitComment>> GetCommitCommentsAsync(string owner, string name, string sha);

    /// <summary>
    /// 创建提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="sha">提交的 SHA</param>
    /// <param name="body">评论内容</param>
    /// <param name="path">文件路径（可选）</param>
    /// <param name="line">行号（可选）</param>
    /// <returns>创建的评论</returns>
    Task<CommitComment> CreateCommitCommentAsync(
        string owner,
        string name,
        string sha,
        string body,
        string? path = null,
        int? line = null);

    /// <summary>
    /// 更新提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <param name="body">新的评论内容</param>
    /// <returns>更新后的评论</returns>
    Task<CommitComment> UpdateCommitCommentAsync(string owner, string name, long commentId, string body);

    /// <summary>
    /// 删除提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="commentId">评论 ID</param>
    /// <returns>Task</returns>
    Task DeleteCommitCommentAsync(string owner, string name, long commentId);

    /// <summary>
    /// 获取所有仓库的提交评论
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="since">起始时间</param>
    /// <param name="pageSize">页面大小</param>
    /// <param name="page">页码</param>
    /// <returns>评论列表</returns>
    Task<IReadOnlyList<CommitComment>> GetRepositoryCommitCommentsAsync(
        string owner,
        string name,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1);

    /// <summary>
    /// 提交文件并创建 commit 记录
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="fileChanges">文件变更（文件路径 -> 文件内容）</param>
    /// <param name="commitMessage">提交消息</param>
    /// <param name="branch">分支名称，默认为 main</param>
    /// <param name="author">作者信息，可选</param>
    /// <param name="committer">提交者信息，可选</param>
    /// <returns>创建的 commit</returns>
    Task<GitHubCommit> CommitFilesAsync(
        string owner, 
        string repo, 
        Dictionary<string, string> fileChanges, 
        string commitMessage, 
        string branch = "main",
        DemoServer.Services.Models.CommitAuthor? author = null,
        DemoServer.Services.Models.CommitAuthor? committer = null);

    /// <summary>
    /// 提交单个文件并创建 commit 记录
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileContent">文件内容</param>
    /// <param name="commitMessage">提交消息</param>
    /// <param name="branch">分支名称，默认为 main</param>
    /// <param name="author">作者信息，可选</param>
    /// <param name="committer">提交者信息，可选</param>
    /// <returns>创建的 commit</returns>
    Task<GitHubCommit> CommitSingleFileAsync(
        string owner,
        string repo,
        string filePath,
        string fileContent,
        string commitMessage,
        string branch = "main",
        DemoServer.Services.Models.CommitAuthor? author = null,
        DemoServer.Services.Models.CommitAuthor? committer = null);

    /// <summary>
    /// 删除文件并创建 commit 记录
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repo">仓库名称</param>
    /// <param name="filePaths">要删除的文件路径列表</param>
    /// <param name="commitMessage">提交消息</param>
    /// <param name="branch">分支名称，默认为 main</param>
    /// <param name="author">作者信息，可选</param>
    /// <param name="committer">提交者信息，可选</param>
    /// <returns>创建的 commit</returns>
    Task<GitHubCommit> DeleteFilesAsync(
        string owner,
        string repo,
        List<string> filePaths,
        string commitMessage,
        string branch = "main",
        DemoServer.Services.Models.CommitAuthor? author = null,
        DemoServer.Services.Models.CommitAuthor? committer = null);
}
