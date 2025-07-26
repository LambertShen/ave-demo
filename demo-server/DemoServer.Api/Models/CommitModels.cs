namespace DemoServer.Api.Models;

/// <summary>
/// 创建提交请求模型
/// </summary>
public class CreateCommitRequest
{
    /// <summary>
    /// 提交消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Git 树 SHA
    /// </summary>
    public string Tree { get; set; } = string.Empty;
    
    /// <summary>
    /// 父提交列表
    /// </summary>
    public IEnumerable<string>? Parents { get; set; }
    
    /// <summary>
    /// 作者信息
    /// </summary>
    public SignatureRequest? Author { get; set; }
    
    /// <summary>
    /// 提交者信息
    /// </summary>
    public SignatureRequest? Committer { get; set; }
}

/// <summary>
/// 签名信息请求模型
/// </summary>
public class SignatureRequest
{
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 日期
    /// </summary>
    public DateTimeOffset? Date { get; set; }
}

/// <summary>
/// 创建提交状态请求模型
/// </summary>
public class CreateCommitStatusRequest
{
    /// <summary>
    /// 状态 (pending, success, error, failure)
    /// </summary>
    public string State { get; set; } = "pending";
    
    /// <summary>
    /// 目标 URL
    /// </summary>
    public string? TargetUrl { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 上下文
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// 创建提交评论请求模型
/// </summary>
public class CreateCommitCommentRequest
{
    /// <summary>
    /// 评论内容
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// 文件路径
    /// </summary>
    public string? Path { get; set; }
    
    /// <summary>
    /// 行号
    /// </summary>
    public int? Line { get; set; }
}

/// <summary>
/// 更新提交评论请求模型
/// </summary>
public class UpdateCommitCommentRequest
{
    /// <summary>
    /// 新的评论内容
    /// </summary>
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// 提交文件请求模型
/// </summary>
public class CommitFilesRequest
{
    /// <summary>
    /// 文件更改映射 (文件路径 -> 文件内容)
    /// </summary>
    public Dictionary<string, string> FileChanges { get; set; } = new();
    
    /// <summary>
    /// 要删除的文件列表
    /// </summary>
    public List<string> FilesToDelete { get; set; } = new();
    
    /// <summary>
    /// 提交消息
    /// </summary>
    public string CommitMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// 分支名称
    /// </summary>
    public string Branch { get; set; } = "main";
    
    /// <summary>
    /// 作者信息
    /// </summary>
    public CommitAuthorRequest? Author { get; set; }
    
    /// <summary>
    /// 提交者信息
    /// </summary>
    public CommitAuthorRequest? Committer { get; set; }
}

/// <summary>
/// 提交单个文件请求模型
/// </summary>
public class CommitSingleFileRequest
{
    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 文件内容
    /// </summary>
    public string FileContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 提交消息
    /// </summary>
    public string CommitMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// 分支名称
    /// </summary>
    public string? Branch { get; set; } = "main";
    
    /// <summary>
    /// 作者信息
    /// </summary>
    public CommitAuthorRequest? Author { get; set; }
    
    /// <summary>
    /// 提交者信息
    /// </summary>
    public CommitAuthorRequest? Committer { get; set; }
}

/// <summary>
/// 删除文件请求模型
/// </summary>
public class DeleteFilesRequest
{
    /// <summary>
    /// 要删除的文件路径列表
    /// </summary>
    public List<string> FilePaths { get; set; } = new();
    
    /// <summary>
    /// 提交消息
    /// </summary>
    public string CommitMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// 分支名称
    /// </summary>
    public string? Branch { get; set; } = "main";
    
    /// <summary>
    /// 作者信息
    /// </summary>
    public CommitAuthorRequest? Author { get; set; }
    
    /// <summary>
    /// 提交者信息
    /// </summary>
    public CommitAuthorRequest? Committer { get; set; }
}

/// <summary>
/// 提交作者请求模型
/// </summary>
public class CommitAuthorRequest
{
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 日期
    /// </summary>
    public DateTimeOffset? Date { get; set; }
}
