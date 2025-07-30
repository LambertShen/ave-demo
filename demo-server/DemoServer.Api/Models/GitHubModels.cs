namespace DemoServer.Api.Models;

/// <summary>
/// 创建评论请求模型
/// </summary>
public class CreateCommentRequest
{
    /// <summary>
    /// 评论内容
    /// </summary>
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// 合并 Pull Request 请求模型
/// </summary>
public class MergePullRequestRequest
{
    /// <summary>
    /// 提交标题
    /// </summary>
    public string? CommitTitle { get; set; }
    
    /// <summary>
    /// 提交消息
    /// </summary>
    public string? CommitMessage { get; set; }
    
    /// <summary>
    /// 合并方法 (merge, squash, rebase)
    /// </summary>
    public string? MergeMethod { get; set; } = "merge";
}

/// <summary>
/// 创建 Issue 请求模型
/// </summary>
public class CreateIssueRequest
{
    /// <summary>
    /// Issue 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Issue 内容
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// 分配给的用户列表
    /// </summary>
    public string[]? Assignees { get; set; }
    
    /// <summary>
    /// 里程碑编号
    /// </summary>
    public int? Milestone { get; set; }
    
    /// <summary>
    /// 标签列表
    /// </summary>
    public string[]? Labels { get; set; }
}

/// <summary>
/// 更新 Issue 请求模型
/// </summary>
public class UpdateIssueRequest
{
    /// <summary>
    /// Issue 标题
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Issue 内容
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// Issue 状态 (open, closed)
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// 分配给的用户列表
    /// </summary>
    public string[]? Assignees { get; set; }
    
    /// <summary>
    /// 里程碑编号
    /// </summary>
    public int? Milestone { get; set; }
    
    /// <summary>
    /// 标签列表
    /// </summary>
    public string[]? Labels { get; set; }
}

/// <summary>
/// 创建 Pull Request 请求模型
/// </summary>
public class CreatePullRequestRequest
{
    /// <summary>
    /// Pull Request 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 源分支（头分支）
    /// </summary>
    public string Head { get; set; } = string.Empty;
    
    /// <summary>
    /// 目标分支（基分支）
    /// </summary>
    public string Base { get; set; } = string.Empty;
    
    /// <summary>
    /// Pull Request 描述
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// 是否为草稿
    /// </summary>
    public bool Draft { get; set; } = false;
    
    /// <summary>
    /// 维护者是否可以修改
    /// </summary>
    public bool MaintainerCanModify { get; set; } = true;
}

/// <summary>
/// 更新 Pull Request 请求模型
/// </summary>
public class UpdatePullRequestRequest
{
    /// <summary>
    /// Pull Request 标题
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Pull Request 描述
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// Pull Request 状态 (open, closed)
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// 目标分支（基分支）
    /// </summary>
    public string? Base { get; set; }
    
    /// <summary>
    /// 维护者是否可以修改
    /// </summary>
    public bool? MaintainerCanModify { get; set; }
}

/// <summary>
/// 关闭 Issue 请求模型
/// </summary>
public class CloseIssueRequest
{
    /// <summary>
    /// 关闭时添加的评论
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// 重新打开 Issue 请求模型
/// </summary>
public class ReopenIssueRequest
{
    /// <summary>
    /// 重新打开时添加的评论
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// 更新评论请求模型
/// </summary>
public class UpdateCommentRequest
{
    /// <summary>
    /// 新的评论内容
    /// </summary>
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// 标签操作请求模型
/// </summary>
public class LabelsRequest
{
    /// <summary>
    /// 标签列表
    /// </summary>
    public string[] Labels { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 批量标签操作请求模型
/// </summary>
public class BatchLabelsRequest
{
    /// <summary>
    /// Issue 编号列表
    /// </summary>
    public int[] IssueNumbers { get; set; } = Array.Empty<int>();
    
    /// <summary>
    /// 标签列表
    /// </summary>
    public string[] Labels { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 批量关闭 Issue 请求模型
/// </summary>
public class BatchCloseRequest
{
    /// <summary>
    /// Issue 编号列表
    /// </summary>
    public int[] IssueNumbers { get; set; } = Array.Empty<int>();
    
    /// <summary>
    /// 关闭时添加的评论
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// 创建 Pull Request 评审请求模型
/// </summary>
public class CreatePullRequestReviewRequest
{
    /// <summary>
    /// 提交 ID
    /// </summary>
    public string? CommitId { get; set; }
    
    /// <summary>
    /// 评审内容
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// 评审事件 (comment, approve, request_changes)
    /// </summary>
    public string Event { get; set; } = "comment";
}

/// <summary>
/// 请求 Pull Request 评审请求模型
/// </summary>
public class RequestPullRequestReviewRequest
{
    /// <summary>
    /// 评审人员列表
    /// </summary>
    public string[]? Reviewers { get; set; }
    
    /// <summary>
    /// 评审团队列表
    /// </summary>
    public string[]? TeamReviewers { get; set; }
}
