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
        catch
        {
            return false;
        }
    }

    public async Task<User> GetCurrentUserAsync(string accessToken)
    {
        var client = CreateAuthenticatedClient(accessToken);
        return await client.User.Current();
    }
}
