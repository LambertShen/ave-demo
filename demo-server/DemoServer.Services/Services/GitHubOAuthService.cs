using Microsoft.Extensions.Options;
using Octokit;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Options;

namespace DemoServer.Services.Services;

public class GitHubOAuthService : IGitHubOAuthService
{
    private readonly GitHubOptions _options;
    private readonly GitHubClient _publicClient;

    public GitHubOAuthService(IOptions<GitHubOptions> options)
    {
        _options = options.Value;
        
        // 创建公共客户端用于 OAuth 流程 - 不需要预设凭据
        _publicClient = new GitHubClient(new ProductHeaderValue(_options.AppName));
    }

    public string GetAuthorizationUrl(string state, params string[] scopes)
    {
        // 验证配置
        if (string.IsNullOrEmpty(_options.ClientId))
        {
            throw new InvalidOperationException("GitHub ClientId is not configured");
        }
        
        if (string.IsNullOrEmpty(_options.RedirectUri))
        {
            throw new InvalidOperationException("GitHub RedirectUri is not configured");
        }
        
        var scopesList = scopes?.Length > 0 ? scopes : new[] { "user" };
        
        var request = new OauthLoginRequest(_options.ClientId)
        {
            RedirectUri = new Uri(_options.RedirectUri),
            State = state,
        };
        
        foreach (var scope in scopesList)
        {
            request.Scopes.Add(scope);
        }

        return _publicClient.Oauth.GetGitHubLoginUrl(request).ToString();
    }

    public async Task<string> GetAccessTokenAsync(string code, string state)
    {
        // 验证配置
        if (string.IsNullOrEmpty(_options.ClientId))
        {
            throw new InvalidOperationException("GitHub ClientId is not configured");
        }
        
        if (string.IsNullOrEmpty(_options.ClientSecret))
        {
            throw new InvalidOperationException("GitHub ClientSecret is not configured");
        }
        
        var request = new OauthTokenRequest(_options.ClientId, _options.ClientSecret, code)
        {
            RedirectUri = new Uri(_options.RedirectUri)
        };

        var token = await _publicClient.Oauth.CreateAccessToken(request);
        return token.AccessToken;
    }

    public GitHubClient CreateAuthenticatedClient(string accessToken)
    {
        return new GitHubClient(new ProductHeaderValue(_options.AppName))
        {
            Credentials = new Credentials(accessToken)
        };
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var client = CreateAuthenticatedClient(accessToken);
            await client.User.Current();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while validating token: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return false;
        }
    }

    public async Task<User> GetCurrentUserAsync(string accessToken)
    {
        var client = CreateAuthenticatedClient(accessToken);
        return await client.User.Current();
    }

    public Task<string> GetAppAccessTokenAsync()
    {
        // 验证配置
        if (string.IsNullOrEmpty(_options.ClientId))
        {
            throw new InvalidOperationException("GitHub ClientId is not configured");
        }
        
        if (string.IsNullOrEmpty(_options.ClientSecret))
        {
            throw new InvalidOperationException("GitHub ClientSecret is not configured");
        }

        try
        {
            // 注意：这个方法主要用于 GitHub Apps，对于 OAuth Apps 可能有限制
            // 如果你使用的是 GitHub App，你需要生成 JWT 并使用它来获取安装访问令牌
            
            // 对于 OAuth Apps，我们可以使用 Client Credentials 进行基本认证
            // 但这种方式的 API 访问权限有限
            var appClient = CreateAppAuthenticatedClient();
            
            // 这里返回一个标识，表示使用了应用程序级别的认证
            // 实际的访问是通过 CreateAppAuthenticatedClient() 实现的
            var token = "app_authenticated_" + _options.ClientId;
            return Task.FromResult(token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to authenticate as application: {ex.Message}", ex);
        }
    }

    public GitHubClient CreateAppAuthenticatedClient()
    {
        // 验证配置
        if (string.IsNullOrEmpty(_options.ClientId))
        {
            throw new InvalidOperationException("GitHub ClientId is not configured");
        }
        
        if (string.IsNullOrEmpty(_options.ClientSecret))
        {
            throw new InvalidOperationException("GitHub ClientSecret is not configured");
        }

        // 使用 ClientId 和 ClientSecret 进行 Basic Authentication
        // 这种方式可以访问公共 API，但权限有限
        return new GitHubClient(new ProductHeaderValue(_options.AppName))
        {
            Credentials = new Credentials(_options.ClientId, _options.ClientSecret)
        };
    }

    /// <summary>
    /// 使用配置中的 AccessToken 创建已认证的 GitHub 客户端
    /// </summary>
    /// <returns>已认证的 GitHubClient</returns>
    public GitHubClient CreateClientFromAccessTokenAsync()
    {
        // 验证配置
        if (string.IsNullOrEmpty(_options.AccessToken))
        {
            throw new InvalidOperationException("GitHub AccessToken is not configured");
        }
        
        return CreateAuthenticatedClient(_options.AccessToken);
    }

    /// <summary>
    /// 获取配置中的 AccessToken
    /// </summary>
    /// <returns>AccessToken</returns>
    public string GetConfiguredAccessToken()
    {
        if (string.IsNullOrEmpty(_options.AccessToken))
        {
            throw new InvalidOperationException("GitHub AccessToken is not configured");
        }
        
        return _options.AccessToken;
    }

    /// <summary>
    /// 使用配置中的 AccessToken 获取用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    public async Task<User> GetUserFromAccessTokenAsync()
    {
        var client = CreateClientFromAccessTokenAsync();
        return await client.User.Current();
    }

    /// <summary>
    /// 验证配置中的 AccessToken 是否有效
    /// </summary>
    /// <returns>令牌是否有效</returns>
    public async Task<bool> ValidateConfiguredTokenAsync()
    {
        try
        {
            var client = CreateClientFromAccessTokenAsync();
            await client.User.Current();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
