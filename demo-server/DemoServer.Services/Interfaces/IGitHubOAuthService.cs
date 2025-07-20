using Octokit;

namespace DemoServer.Services.Interfaces;

public interface IGitHubOAuthService
{
    /// <summary>
    /// 获取 OAuth 授权 URL
    /// </summary>
    /// <param name="state">状态参数，用于防止 CSRF 攻击</param>
    /// <param name="scopes">请求的权限范围</param>
    /// <returns>GitHub OAuth 授权 URL</returns>
    string GetAuthorizationUrl(string state, params string[] scopes);
    
    /// <summary>
    /// 使用授权码获取访问令牌
    /// </summary>
    /// <param name="code">GitHub 返回的授权码</param>
    /// <param name="state">状态参数</param>
    /// <returns>访问令牌</returns>
    Task<string> GetAccessTokenAsync(string code, string state);
    
    /// <summary>
    /// 使用访问令牌创建已认证的 GitHub 客户端
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <returns>已认证的 GitHubClient 实例</returns>
    GitHubClient CreateAuthenticatedClient(string accessToken);
    
    /// <summary>
    /// 验证访问令牌是否有效
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <returns>令牌是否有效</returns>
    Task<bool> ValidateTokenAsync(string accessToken);
    
    /// <summary>
    /// 获取当前用户信息（用于验证身份）
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <returns>用户信息</returns>
    Task<User> GetCurrentUserAsync(string accessToken);
    
    /// <summary>
    /// 使用 Client Credentials 获取应用程序访问令牌（仅限 GitHub Apps）
    /// 注意：这种方式获取的 token 用于应用程序级别的 API 访问，不代表任何用户
    /// </summary>
    /// <returns>应用程序访问令牌</returns>
    Task<string> GetAppAccessTokenAsync();
    
    /// <summary>
    /// 使用 ClientId 和 ClientSecret 创建已认证的 GitHub 客户端
    /// 用于不需要用户授权的 GitHub API 调用
    /// </summary>
    /// <returns>使用 Basic Auth 的 GitHubClient 实例</returns>
    GitHubClient CreateAppAuthenticatedClient();
}
