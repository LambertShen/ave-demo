# GitHub Project Views CRUD API 文档

本文档描述了新添加的 GitHub Project Views 的 CRUD 操作功能。

## 概述

GitHub Project Views 允许用户以不同的方式查看和组织项目数据。支持的视图类型包括：
- **TABLE**: 表格视图
- **BOARD**: 看板视图  
- **ROADMAP**: 路线图视图

## 新增的数据模型

### ProjectView
项目视图的主要数据模型：
```csharp
public class ProjectView
{
    public string Id { get; set; }              // 视图ID
    public string Name { get; set; }            // 视图名称
    public string? Description { get; set; }    // 视图描述（只读，GitHub API 不支持设置）
    public string ProjectId { get; set; }       // 所属项目ID
    public int Number { get; set; }             // 视图编号
    public DateTimeOffset CreatedAt { get; set; }   // 创建时间
    public DateTimeOffset UpdatedAt { get; set; }   // 更新时间
    public string Layout { get; set; }          // 布局类型 (TABLE, BOARD, ROADMAP)
    public List<ProjectViewField> Fields { get; set; }      // 字段配置（高级功能）
    public ProjectViewFilter? Filter { get; set; }          // 过滤器（基础支持）
    public ProjectViewGroupBy? GroupBy { get; set; }        // 分组配置（高级功能）
    public List<ProjectViewSortBy> SortBy { get; set; }     // 排序配置（高级功能）
}
```

### 请求模型

#### CreateProjectViewRequest
```csharp
public class CreateProjectViewRequest
{
    public string ProjectId { get; set; }       // 项目ID
    public string Name { get; set; }            // 视图名称
    public string Layout { get; set; } = "TABLE";  // 布局类型
    // 注意: GitHub API 不支持在创建时设置 description
}
```

#### UpdateProjectViewRequest
```csharp
public class UpdateProjectViewRequest
{
    public string ViewId { get; set; }          // 视图ID
    public string? Name { get; set; }           // 新的视图名称
    public string? Layout { get; set; }         // 新的布局类型
    // 注意: GitHub API 不支持更新 description
    // 高级配置（如字段、过滤器、排序）需要单独的 API 调用
}
```

## 新增的服务方法

### IGitHubProjectService 接口新增方法

1. **GetProjectViewsAsync** - 获取项目的视图列表
2. **GetProjectViewByIdAsync** - 根据ID获取项目视图详情
3. **CreateProjectViewAsync** - 创建项目视图
4. **UpdateProjectViewAsync** - 更新项目视图
5. **DeleteProjectViewAsync** - 删除项目视图
6. **CopyProjectViewAsync** - 复制项目视图

### 方法详细说明

#### 获取项目视图列表
```csharp
Task<(List<ProjectView> Views, bool HasNextPage, string? EndCursor)> GetProjectViewsAsync(
    string projectId, 
    int first = 20, 
    string? after = null);
```

#### 创建项目视图
```csharp
Task<ProjectView?> CreateProjectViewAsync(CreateProjectViewRequest request);
```

#### 更新项目视图
```csharp
Task<ProjectView?> UpdateProjectViewAsync(UpdateProjectViewRequest request);
```

## REST API 端点

### 基础URL
```
/api/github/projects/{projectId}/views
```

### 支持的操作

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/views` | 获取项目的视图列表 |
| GET | `/views/{viewId}` | 获取特定视图详情 |
| POST | `/views` | 创建新视图 |
| PUT | `/views/{viewId}` | 更新视图 |
| DELETE | `/views/{viewId}` | 删除视图 |
| POST | `/views/{viewId}/copy` | 复制视图 |

## 使用示例

### 1. 创建表格视图
```http
POST /api/github/projects/{projectId}/views
Content-Type: application/json

{
    "name": "Tasks Overview",
    "layout": "TABLE"
}
```

### 2. 创建看板视图
```http
POST /api/github/projects/{projectId}/views
Content-Type: application/json

{
    "name": "Development Board",
    "layout": "BOARD"
}
```

### 3. 更新视图
```http
PUT /api/github/projects/{projectId}/views/{viewId}
Content-Type: application/json

{
    "name": "Updated View Name"
}
```

### 4. 复制视图
```http
POST /api/github/projects/{projectId}/views/{viewId}/copy
Content-Type: application/json

{
    "newName": "Copy of Original View"
}
```

## 错误处理

所有 API 端点都包含适当的错误处理：
- **400 Bad Request**: 请求参数无效或 GraphQL 错误
- **404 Not Found**: 视图或项目不存在
- **500 Internal Server Error**: 服务器内部错误

## 权限要求

使用这些 API 需要：
1. 有效的 GitHub Access Token
2. 对指定项目的适当权限
3. Token 必须具有项目管理权限

## 测试

使用提供的 `ProjectViews.http` 文件进行 API 测试。确保：
1. 替换 `YOUR_PROJECT_ID_HERE` 为实际的项目ID
2. 替换 `YOUR_VIEW_ID_HERE` 为实际的视图ID
3. 确保 GitHub Access Token 正确配置

## GraphQL 查询

底层使用的 GitHub GraphQL API 查询包括：
- `ProjectV2.views` - 获取视图列表
- `createProjectV2View` - 创建视图
- `updateProjectV2View` - 更新视图
- `deleteProjectV2View` - 删除视图
- `copyProjectV2View` - 复制视图

## 注意事项

1. **GitHub Projects V2 API 限制**: GitHub API 不支持在创建或更新视图时设置 `description` 字段
2. **高级配置**: 视图的字段配置、过滤器、排序等高级功能需要额外的 GraphQL mutation
3. **API 稳定性**: GitHub Projects V2 API 目前还在 beta 阶段，某些功能可能会发生变化
4. **权限要求**: 需要适当的项目管理权限才能创建、更新或删除视图
5. **视图类型限制**: 某些视图类型可能需要特定的字段配置才能正常工作
6. **删除不可逆**: 删除视图是不可逆的操作，请谨慎使用

## 当前 API 支持的功能

✅ **基础功能**:
- 获取视图列表
- 获取视图详情
- 创建视图（名称 + 布局类型）
- 更新视图（名称 + 布局类型）
- 删除视图
- 复制视图
- 基础过滤器查询字符串

⚠️ **部分支持**:
- 视图描述（只读）
- 过滤器（基础查询字符串）

❌ **暂不支持**:
- 视图描述的创建和更新
- 详细的字段配置
- 复杂的过滤器条件
- 分组和排序配置

这些限制主要是由于 GitHub GraphQL API 的当前实现。未来可能会有更多功能支持。
