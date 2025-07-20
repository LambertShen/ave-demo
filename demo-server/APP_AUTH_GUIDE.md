# GitHub OAuth 应用程序级别认证使用指南

## 概述

我们为 `GitHubOAuthService` 添加了两个新方法，支持仅使用 `ClientId` 和 `ClientSecret` 进行应用程序级别的 GitHub API 访问。

## 新增方法

### 1. `GetAppAccessTokenAsync()`
```csharp
Task<string> GetAppAccessTokenAsync()
```
- **用途**: 获取应用程序级别的访问标识
- **认证方式**: 使用 ClientId 和 ClientSecret
- **返回**: 应用程序访问标识符

### 2. `CreateAppAuthenticatedClient()`
```csharp
GitHubClient CreateAppAuthenticatedClient()
```
- **用途**: 创建使用应用程序凭据的 GitHub 客户端
- **认证方式**: Basic Authentication (ClientId + ClientSecret)
- **返回**: 已认证的 GitHubClient 实例

## API 端点

### 获取应用程序令牌
```
POST /api/github/oauth/app-token
```
**响应示例**:
```json
{
  "appToken": "app_authenticated_Ov23liQDOJklqmXwN66n",
  "tokenType": "application",
  "message": "Application token generated successfully",
  "note": "This token is for application-level API access with limited permissions"
}
```

### 测试应用程序访问
```
GET /api/github/oauth/app-test
```
**响应示例**:
```json
{
  "message": "Application-level API access successful",
  "rateLimit": {
    "core": {
      "limit": 5000,
      "remaining": 4999,
      "reset": "2025-07-20T15:30:00Z"
    },
    "search": {
      "limit": 30,
      "remaining": 30,
      "reset": "2025-07-20T15:30:00Z"
    }
  }
}
```

## 使用场景

### 适用场景
- 获取公共仓库信息
- 搜索公共内容
- 获取 GitHub 状态和元数据
- 应用程序级别的监控和统计

### 不适用场景
- 访问私有仓库
- 代表用户执行操作
- 访问用户特定的数据
- 需要用户授权的操作

## 权限限制

使用 Client Credentials 认证的限制：
- ✅ 可以访问公共 API
- ✅ 更高的速率限制
- ❌ 无法访问私有资源
- ❌ 无法代表用户执行操作
- ❌ 无法访问用户特定数据

## 代码示例

### 在服务中使用
```csharp
public class MyService
{
    private readonly IGitHubOAuthService _gitHubService;
    
    public MyService(IGitHubOAuthService gitHubService)
    {
        _gitHubService = gitHubService;
    }
    
    public async Task<Repository> GetPublicRepositoryAsync(string owner, string repo)
    {
        var client = _gitHubService.CreateAppAuthenticatedClient();
        return await client.Repository.Get(owner, repo);
    }
    
    public async Task<string> GetAppTokenAsync()
    {
        return await _gitHubService.GetAppAccessTokenAsync();
    }
}
```

### 在控制器中使用
```csharp
[HttpGet("public-repo/{owner}/{name}")]
public async Task<IActionResult> GetPublicRepository(string owner, string name)
{
    try
    {
        var client = _gitHubOAuthService.CreateAppAuthenticatedClient();
        var repo = await client.Repository.Get(owner, name);
        
        return Ok(new 
        {
            name = repo.Name,
            description = repo.Description,
            stars = repo.StargazersCount,
            language = repo.Language
        });
    }
    catch (Exception ex)
    {
        return BadRequest($"Failed to get repository: {ex.Message}");
    }
}
```

## 安全注意事项

⚠️ **重要提醒**:
- Client Secret 应该安全存储，不要暴露在客户端代码中
- 这种认证方式适合服务器端应用程序
- 定期轮换 Client Secret
- 监控 API 使用情况和速率限制

## 与用户级别认证的区别

| 特性 | 用户级别 OAuth | 应用程序级别认证 |
|------|---------------|-----------------|
| 认证方式 | 授权码流程 | Client Credentials |
| 访问权限 | 用户授权的权限 | 公共 API 权限 |
| 速率限制 | 每用户 5000/小时 | 每应用 5000/小时 |
| 使用场景 | 代表用户操作 | 应用程序功能 |
| Token 生命周期 | 可刷新 | 无过期（基于凭据） |
