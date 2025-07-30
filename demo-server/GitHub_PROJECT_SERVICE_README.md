# GitHub Project Service API

GitHubProjectService 是一个使用 GitHub GraphQL API 实现对 GitHub Projects v2 进行 CRUD 操作的服务。

## 功能特性

- ✅ 获取用户/组织的项目列表
- ✅ 根据ID获取项目详情
- ✅ 创建新项目
- ✅ 更新项目信息
- ✅ 删除项目
- ✅ 获取项目字段
- ✅ 管理项目条目（获取、添加、删除）
- ✅ 分页支持
- ✅ 错误处理

## 技术架构

### 依赖项
- `GraphQL.Client` - GraphQL 客户端库
- `GraphQL.Client.Serializer.Newtonsoft` - JSON 序列化支持
- `Octokit` - GitHub API 库（用于其他 GitHub 服务）

### 主要组件

1. **Models** (`ProjectModels.cs`)
   - `GitHubProject` - 项目实体
   - `ProjectItem` - 项目条目实体
   - `ProjectField` - 项目字段实体
   - GraphQL 响应模型

2. **Service Interface** (`IGitHubProjectService.cs`)
   - 定义了所有项目相关的操作接口

3. **Service Implementation** (`GitHubProjectService.cs`)
   - 实现 GraphQL 查询和变更操作
   - 处理分页和错误

4. **Controller** (`GitHubProjectController.cs`)
   - REST API 端点
   - 请求验证和错误处理

## API 端点

### 项目管理

| 方法 | 端点 | 描述 |
|------|------|------|
| GET | `/api/githubproject` | 获取当前用户的项目列表 |
| GET | `/api/githubproject/organization/{orgLogin}` | 获取组织的项目列表 |
| GET | `/api/githubproject/user/{userLogin}` | 获取指定用户的项目列表 |
| GET | `/api/githubproject/{projectId}` | 获取项目详情 |
| POST | `/api/githubproject` | 创建新项目 |
| PUT | `/api/githubproject/{projectId}` | 更新项目 |
| DELETE | `/api/githubproject/{projectId}` | 删除项目 |

### 项目条目管理

| 方法 | 端点 | 描述 |
|------|------|------|
| GET | `/api/githubproject/{projectId}/items` | 获取项目条目列表 |
| POST | `/api/githubproject/{projectId}/items` | 添加项目条目 |
| DELETE | `/api/githubproject/{projectId}/items/{itemId}` | 删除项目条目 |

### 辅助端点

| 方法 | 端点 | 描述 |
|------|------|------|
| GET | `/api/githubproject/owner/{login}` | 获取所有者信息 |
| GET | `/api/githubproject/{projectId}/fields` | 获取项目字段 |

## 使用方法

### 1. 配置依赖注入

在 `Program.cs` 中确保已经注册服务：

```csharp
builder.Services.AddGitHubOAuthServices(builder.Configuration);
```

### 2. 配置访问令牌

在 `appsettings.json` 或 `appsettings.Development.json` 中配置 GitHub 访问令牌：

```json
{
  "GitHub": {
    "AccessToken": "ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AppName": "DemoServer"
  }
}
```

### 3. 基本使用示例

#### 获取项目列表
```http
GET /api/githubproject
```

#### 创建项目
```http
POST /api/githubproject
Content-Type: application/json

{
  "ownerId": "MDQ6VXNlcjEyMzQ1Njc4",
  "title": "我的新项目",
  "description": "项目描述",
  "public": false
}
```

#### 更新项目
```http
PUT /api/githubproject/{projectId}
Content-Type: application/json

{
  "title": "更新后的标题",
  "description": "更新后的描述"
}
```

## 权限要求

使用此 API 需要在配置文件中设置有效的 GitHub 访问令牌，该令牌需要具有以下权限：

- `project` - 读取和写入项目
- `repo` - 访问仓库信息（如果需要添加 Issues/PRs 到项目）

## 配置示例

在 `appsettings.json` 中添加：

```json
{
  "GitHub": {
    "AccessToken": "ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AppName": "DemoServer",
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret",
    "RedirectUri": "http://localhost:5000/oauth/callback"
  }
}

## 分页支持

大部分列表 API 都支持分页：

- `first` - 每页返回的项目数量（默认：20）
- `after` - 分页游标，用于获取下一页

响应包含分页信息：
```json
{
  "projects": [...],
  "hasNextPage": true,
  "endCursor": "Y3Vyc29yOnYyOpHOABcd"
}
```

## 错误处理

API 会返回适当的 HTTP 状态码：

- `200` - 成功
- `400` - 请求参数错误
- `404` - 资源未找到
- `500` - 服务器内部错误

错误响应格式：
```json
{
  "error": "错误描述信息"
}
```

## GraphQL 查询示例

服务内部使用的 GraphQL 查询示例：

### 获取项目列表
```graphql
query GetProjects($first: Int!, $after: String) {
  viewer {
    projectsV2(first: $first, after: $after) {
      nodes {
        id
        title
        description
        url
        state
        createdAt
        updatedAt
        public
        number
      }
      pageInfo {
        hasNextPage
        endCursor
      }
    }
  }
}
```

### 创建项目
```graphql
mutation CreateProject($input: CreateProjectV2Input!) {
  createProjectV2(input: $input) {
    projectV2 {
      id
      title
      description
      url
      state
      createdAt
      updatedAt
      public
      number
    }
  }
}
```

## 注意事项

1. **访问令牌配置**：请在 `appsettings.json` 中配置有效的 GitHub 访问令牌
2. **API 限制**：GitHub GraphQL API 有速率限制，请合理使用
3. **项目条目更新**：`UpdateProjectItemAsync` 方法需要根据具体的字段类型进行实现
4. **权限检查**：确保访问令牌有足够的权限访问目标项目
5. **错误处理**：如果访问令牌未配置或无效，API 将返回 500 错误

## 测试

使用提供的 `GitHubProject.http` 文件进行 API 测试。无需手动传递访问令牌，服务会自动从配置中获取。
