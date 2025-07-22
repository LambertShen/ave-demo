using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/github/oauth")]
[Tags("GitHub OAuth")]
public class GitHubOAuthController : ControllerBase
{
    private readonly IGitHubOAuthService _gitHubOAuthService;

    public GitHubOAuthController(IGitHubOAuthService gitHubOAuthService)
    {
        _gitHubOAuthService = gitHubOAuthService;
    }

    /// <summary>
    /// 获取 GitHub OAuth 授权 URL
    /// </summary>
    /// <param name="state">状态参数，用于防止 CSRF 攻击</param>
    /// <param name="scopes">请求的权限范围，用逗号分隔 (默认: user)</param>
    /// <returns>GitHub OAuth 授权 URL</returns>
    [HttpGet("authorize")]
    public IActionResult GetAuthorizationUrl([FromQuery] string state = "default", [FromQuery] string scopes = "user")
    {
        try
        {
            var scopeArray = scopes.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var authUrl = _gitHubOAuthService.GetAuthorizationUrl(state, scopeArray);
            
            return Ok(new { authorizationUrl = authUrl, state, scopes = scopeArray });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to generate authorization URL: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查 GitHub OAuth 配置状态
    /// </summary>
    /// <returns>配置状态信息</returns>
    [HttpGet("config/status")]
    public IActionResult GetConfigStatus()
    {
        try
        {
            // 尝试生成一个测试 URL 来验证配置
            var testUrl = _gitHubOAuthService.GetAuthorizationUrl("test", "user");
            
            return Ok(new 
            { 
                isConfigured = true,
                message = "GitHub OAuth configuration is valid",
                hasValidUrl = !string.IsNullOrEmpty(testUrl)
            });
        }
        catch (Exception ex)
        {
            return Ok(new 
            { 
                isConfigured = false,
                message = $"Configuration error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// OAuth 回调端点 - 处理 GitHub 返回的授权码
    /// </summary>
    /// <param name="code">GitHub 返回的授权码</param>
    /// <param name="state">状态参数</param>
    /// <returns>访问令牌和用户基本信息</returns>
    [HttpGet("callback")]
    public async Task<IActionResult> HandleCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization code is required");
        }

        try
        {
            var accessToken = await _gitHubOAuthService.GetAccessTokenAsync(code, state);
            
            // 获取用户基本信息
            var user = await _gitHubOAuthService.GetCurrentUserAsync(accessToken);
            
            // 在实际应用中，你应该将令牌安全地存储（如数据库、缓存等）
            return Ok(new 
            { 
                accessToken,
                user = new
                {
                    id = user.Id,
                    login = user.Login,
                    name = user.Name,
                    avatarUrl = user.AvatarUrl
                },
                message = "OAuth authentication successful" 
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get access token: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证访问令牌
    /// </summary>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <returns>令牌验证结果</returns>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken([FromHeader(Name = "Authorization")] string authorization)
    {
        var accessToken = ExtractTokenFromAuthHeader(authorization);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("Access token is required");
        }

        try
        {
            var isValid = await _gitHubOAuthService.ValidateTokenAsync(accessToken);
            return Ok(new { isValid, message = isValid ? "Token is valid" : "Token is invalid" });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to validate token: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前认证用户的详细信息
    /// </summary>
    /// <param name="authorization">包含访问令牌的 Authorization 头</param>
    /// <returns>认证用户的详细信息</returns>
    [HttpGet("user")]
    public async Task<IActionResult> GetAuthenticatedUser([FromHeader(Name = "Authorization")] string authorization)
    {
        var accessToken = ExtractTokenFromAuthHeader(authorization);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("Access token is required");
        }

        try
        {
            var user = await _gitHubOAuthService.GetCurrentUserAsync(accessToken);
            return Ok(new
            {
                id = user.Id,
                login = user.Login,
                name = user.Name,
                email = user.Email,
                avatarUrl = user.AvatarUrl,
                type = user.Type.ToString(),
                createdAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get user info: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取应用程序级别的访问令牌（使用 Client Credentials）
    /// </summary>
    /// <returns>应用程序访问令牌</returns>
    [HttpPost("app-token")]
    public async Task<IActionResult> GetAppToken()
    {
        try
        {
            var appToken = await _gitHubOAuthService.GetAppAccessTokenAsync();
            
            return Ok(new 
            { 
                appToken,
                tokenType = "application",
                message = "Application token generated successfully",
                note = "This token is for application-level API access with limited permissions"
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get application token: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试应用程序级别的 GitHub API 访问
    /// </summary>
    /// <returns>GitHub API 速率限制信息</returns>
    [HttpGet("app-test")]
    public async Task<IActionResult> TestAppAccess()
    {
        try
        {
            var appClient = _gitHubOAuthService.CreateAppAuthenticatedClient();
            
            // 获取 API 速率限制信息作为测试
            var rateLimit = await appClient.RateLimit.GetRateLimits();
            
            return Ok(new 
            { 
                message = "Application-level API access successful",
                rateLimit = new
                {
                    core = new
                    {
                        limit = rateLimit.Resources.Core.Limit,
                        remaining = rateLimit.Resources.Core.Remaining,
                        reset = rateLimit.Resources.Core.Reset
                    },
                    search = new
                    {
                        limit = rateLimit.Resources.Search.Limit,
                        remaining = rateLimit.Resources.Search.Remaining,
                        reset = rateLimit.Resources.Search.Reset
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to test application access: {ex.Message}");
        }
    }

    /// <summary>
    /// 使用配置的 AppCode 获取 AccessToken
    /// </summary>
    /// <param name="state">状态参数（可选）</param>
    /// <returns>AccessToken 和用户信息</returns>
    [HttpPost("appcode/token")]
    public async Task<IActionResult> GetTokenFromAppCode([FromQuery] string? state = null)
    {
        try
        {
            var accessToken = _gitHubOAuthService.GetConfiguredAccessToken();
            var user = await _gitHubOAuthService.GetUserFromAccessTokenAsync();

            return Ok(new
            {
                accessToken = accessToken,
                user = new
                {
                    id = user.Id,
                    login = user.Login,
                    name = user.Name,
                    email = user.Email,
                    avatarUrl = user.AvatarUrl,
                    company = user.Company,
                    location = user.Location,
                    bio = user.Bio,
                    publicRepos = user.PublicRepos,
                    followers = user.Followers,
                    following = user.Following,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt,
                    htmlUrl = user.HtmlUrl
                },
                message = "Successfully authenticated using AppCode"
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to authenticate with AppCode: {ex.Message}");
        }
    }

    /// <summary>
    /// 使用配置的 AppCode 验证并获取用户信息
    /// </summary>
    /// <param name="state">状态参数（可选）</param>
    /// <returns>用户信息</returns>
    [HttpGet("appcode/user")]
    public async Task<IActionResult> GetUserFromAppCode([FromQuery] string? state = null)
    {
        try
        {
            var client = _gitHubOAuthService.CreateClientFromAccessTokenAsync();
            var user = await client.User.Current();

            return Ok(new
            {
                id = user.Id,
                login = user.Login,
                name = user.Name,
                email = user.Email,
                avatarUrl = user.AvatarUrl,
                company = user.Company,
                location = user.Location,
                bio = user.Bio,
                publicRepos = user.PublicRepos,
                followers = user.Followers,
                following = user.Following,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt,
                htmlUrl = user.HtmlUrl,
                type = user.Type.ToString(),
                siteAdmin = user.SiteAdmin
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to get user info from AppCode: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试 AppCode 认证状态
    /// </summary>
    /// <returns>认证状态</returns>
    [HttpGet("appcode/test")]
    public async Task<IActionResult> TestAppCodeAuth()
    {
        try
        {
            var accessToken = _gitHubOAuthService.GetConfiguredAccessToken();
            var isValid = await _gitHubOAuthService.ValidateConfiguredTokenAsync();

            return Ok(new
            {
                hasAppCode = !string.IsNullOrEmpty(accessToken),
                isValid = isValid,
                message = isValid ? "AppCode authentication is working" : "AppCode authentication failed",
                tokenLength = accessToken?.Length ?? 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"AppCode authentication test failed: {ex.Message}");
        }
    }

    private string? ExtractTokenFromAuthHeader(string authorization)
    {
        if (string.IsNullOrEmpty(authorization))
            return null;

        // 支持 "Bearer token" 或 "token" 格式
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization.Substring("Bearer ".Length).Trim();
        }
        
        return authorization.Trim();
    }
}
