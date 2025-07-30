# GitHub Repository Service

## 概述

GitHubRepositoryService 是一个使用 Octokit 实现的 GitHub 仓库管理服务，提供了完整的 CRUD 操作功能。

## 功能特性

### 仓库查询
- 获取当前用户的仓库列表
- 获取指定用户的仓库列表
- 获取组织的仓库列表
- 根据仓库 ID 或 所有者/仓库名 获取仓库详情
- 搜索仓库

### 仓库管理
- 创建新仓库
- 在组织下创建仓库
- 更新仓库信息
- 删除仓库
- Fork 仓库

### 仓库内容
- 获取分支列表
- 获取标签列表

### 协作者管理
- 获取协作者列表
- 添加协作者
- 移除协作者

## API 端点

所有 API 端点都位于 `/api/GitHubRepository` 路径下：

### 仓库操作
- `GET /` - 获取仓库列表
- `GET /organization/{organizationName}` - 获取组织仓库
- `GET /{repositoryId}` - 根据 ID 获取仓库
- `GET /{owner}/{repositoryName}` - 获取指定仓库
- `POST /` - 创建仓库
- `POST /organization/{organizationName}` - 在组织下创建仓库
- `PUT /{owner}/{repositoryName}` - 更新仓库
- `DELETE /{owner}/{repositoryName}` - 删除仓库
- `POST /{owner}/{repositoryName}/fork` - Fork 仓库

### 仓库内容
- `GET /{owner}/{repositoryName}/branches` - 获取分支
- `GET /{owner}/{repositoryName}/tags` - 获取标签

### 协作者管理
- `GET /{owner}/{repositoryName}/collaborators` - 获取协作者
- `POST /{owner}/{repositoryName}/collaborators/{username}` - 添加协作者
- `DELETE /{owner}/{repositoryName}/collaborators/{username}` - 移除协作者

### 搜索
- `GET /search?query={query}` - 搜索仓库

## 使用示例

### 创建仓库
```json
{
    "name": "my-new-repo",
    "description": "My new repository",
    "private": false,
    "hasIssues": true,
    "hasProjects": true,
    "hasWiki": true,
    "autoInit": true,
    "gitignoreTemplate": "VisualStudio",
    "licenseTemplate": "mit"
}
```

### 更新仓库
```json
{
    "name": "updated-repo-name",
    "description": "Updated description",
    "private": false,
    "hasIssues": true,
    "hasProjects": true,
    "hasWiki": true
}
```

### Fork 仓库
```json
{
    "organization": "my-org",
    "defaultBranchOnly": false
}
```

## 权限

协作者权限类型包括：
- `Read` - 只读权限
- `Write` - 读写权限
- `Admin` - 管理员权限

## 测试

使用 `GitHubRepository.http` 文件进行 API 测试。确保设置正确的 GitHub 访问令牌。

## 依赖注入

服务已在 `ServiceCollectionExtensions.cs` 中注册：

```csharp
services.AddScoped<IGitHubRepositoryService, GitHubRepositoryService>();
```

## 注意事项

1. 所有操作都需要有效的 GitHub 访问令牌
2. 删除仓库是不可逆操作，请谨慎使用
3. 创建组织仓库需要相应的组织权限
4. API 调用受到 GitHub Rate Limiting 限制
