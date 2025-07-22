namespace DemoServer.Services.Options;

public class GitHubOptions
{
    public const string SectionName = "GitHub";
    
    /// <summary>
    /// GitHub OAuth App 的 Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// GitHub OAuth App 的 Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// OAuth 重定向 URI
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// GitHub 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// GitHub API 的基础 URL (默认为 GitHub.com)
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.github.com";
    
    /// <summary>
    /// 应用名称，用于 User-Agent
    /// </summary>
    public string AppName { get; set; } = "DemoServer";
}
