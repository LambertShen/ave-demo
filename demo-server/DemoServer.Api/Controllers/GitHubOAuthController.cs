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
