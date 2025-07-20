# DemoServer GitHub OAuth Integration

这个项目演示了如何在 ASP.NET Core Web API 中集成 GitHub OAuth 身份认证。

## 项目结构

```
demo-server/
├── DemoServer.sln                                # 解决方案文件
├── DemoServer.Api/                               # Web API 项目
│   ├── Controllers/GitHubOAuthController.cs      # GitHub OAuth API 控制器
│   ├── Program.cs                                # 应用入口点
│   ├── appsettings.json                          # 应用配置
│   └── ...
└── DemoServer.Services/                          # 服务层项目
    ├── Services/GitHubOAuthService.cs            # GitHub OAuth 服务实现
    ├── Interfaces/IGitHubOAuthService.cs         # GitHub OAuth 服务接口
    ├── Options/GitHubOptions.cs                  # GitHub 配置选项
    └── Extensions/ServiceCollectionExtensions.cs
```

## 功能特性

- ✅ GitHub OAuth 2.0 认证流程
- ✅ 获取授权 URL
- ✅ 处理授权回调
- ✅ 访问令牌验证
- ✅ 获取认证用户基本信息
- ✅ 应用程序级别认证（Client Credentials）🆕
- ✅ 公共 API 访问（无需用户授权）🆕
- ✅ 完整的 Swagger API 文档
- ✅ 依赖注入配置
- ✅ 配置选项模式

## API 端点

### GitHub OAuth 身份认证

1. **获取授权 URL**
   ```
   GET /api/github/oauth/authorize?state=mystate&scopes=user
   ```
   
2. **处理回调**（GitHub 会自动调用）
   ```
   GET /api/github/oauth/callback?code=...&state=mystate
   ```

3. **验证访问令牌**
   ```
   GET /api/github/oauth/validate
   Authorization: Bearer YOUR_ACCESS_TOKEN
   ```

4. **获取认证用户信息**
   ```
   GET /api/github/oauth/user
   Authorization: Bearer YOUR_ACCESS_TOKEN
   ```

### 应用程序级别认证（无需用户授权）

5. **获取应用程序访问令牌**
   ```
   POST /api/github/oauth/app-token
   ```

6. **测试应用程序级别访问**
   ```
   GET /api/github/oauth/app-test
   ```

## 设置步骤

### 1. 创建 GitHub OAuth App

1. 访问 GitHub Settings > Developer settings > OAuth Apps
2. 点击 "New OAuth App"
3. 填写应用信息：
   - Application name: `DemoServer`
   - Homepage URL: `https://localhost:7090`
   - Authorization callback URL: `https://localhost:7090/api/github/oauth/callback`
4. 获取 Client ID 和 Client Secret

### 2. 配置应用

在 `appsettings.Development.json` 中更新 GitHub 配置：

```json
{
  "GitHub": {
    "ClientId": "YOUR_GITHUB_CLIENT_ID",
    "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET",
    "RedirectUri": "https://localhost:7090/api/github/oauth/callback",
    "AppName": "DemoServer-Dev"
  }
}
```

### 3. 运行应用

```bash
cd demo-server
dotnet run --project DemoServer.Api
```

应用将在以下地址启动：
- API: `https://localhost:7090`
- Swagger UI: `https://localhost:7090/swagger`

## API 端点

### GitHub OAuth 流程

1. **获取授权 URL**
   ```
   GET /api/github/auth/url?state=mystate&scopes=user,repo
   ```
   
2. **处理回调**（GitHub 会自动调用）
   ```
   GET /api/github/auth/callback?code=...&state=mystate
   ```

### GitHub API 调用

使用获取到的 access token 调用以下端点：

3. **验证访问令牌**
   ```
   GET /api/github/oauth/validate
   Authorization: Bearer YOUR_ACCESS_TOKEN
   ```

4. **获取认证用户信息**
   ```
   GET /api/github/oauth/user
   Authorization: Bearer YOUR_ACCESS_TOKEN
   ```

## 使用示例

### 完整的 OAuth 身份认证流程

1. 客户端调用 `/api/github/oauth/authorize` 获取授权 URL
2. 重定向用户到该 URL 进行 GitHub 授权
3. GitHub 重定向回 `/api/github/oauth/callback` 并带上授权码
4. 应用使用授权码获取 access token
5. 使用 access token 验证身份和获取用户信息

### 测试流程

1. 打开 Swagger UI: `https://localhost:7090/swagger`
2. 调用 `GET /api/github/oauth/authorize` 获取授权链接
3. 在浏览器中访问返回的授权链接
4. 授权后会重定向到回调地址并获得 access token
5. 复制 access token，在 Swagger 中设置 Authorization 头
6. 调用其他认证相关的 API 端点

## 依赖包

- **Octokit.NET**: GitHub API 的 .NET 客户端库
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI 支持
- **Microsoft.Extensions.Options.ConfigurationExtensions**: 配置选项支持

## 安全注意事项

⚠️ **重要**: 
- 永远不要将 Client Secret 提交到版本控制
- 在生产环境中使用环境变量或安全的配置管理
- 考虑实现 token 的安全存储和刷新机制
- 验证 OAuth state 参数以防止 CSRF 攻击

## 扩展功能

可以进一步扩展的功能：
- Token 刷新机制
- 用户会话管理
- JWT 令牌集成
- 角色和权限管理
- 多提供商 OAuth 支持（Google, Microsoft 等）
- 令牌加密存储
