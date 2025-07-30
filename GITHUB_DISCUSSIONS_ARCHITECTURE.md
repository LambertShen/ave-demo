# GitHub Discussions Service 架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                    GitHub Discussions Service                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐    ┌──────────────────┐    ┌─────────────┐  │
│  │   Controllers   │    │    Services      │    │   Models    │  │
│  │                 │    │                  │    │             │  │
│  │ GitHubDiscuss-  │◄──►│ IGitHubDiscuss-  │◄──►│ Discussion  │  │
│  │ ionsController  │    │ ionsService      │    │ Models      │  │
│  │                 │    │                  │    │             │  │
│  │ - Discussions   │    │ - Discussions    │    │ - GitHubD-  │  │
│  │ - Comments      │    │   CRUD           │    │   iscussion │  │
│  │ - Categories    │    │ - Comments CRUD  │    │ - Discussion│  │
│  │                 │    │ - Categories     │    │   Category  │  │
│  │                 │    │   CRUD           │    │ - Discussion│  │
│  │                 │    │                  │    │   Comment   │  │
│  └─────────────────┘    └──────────────────┘    └─────────────┘  │
│                                  │                               │
│                                  ▼                               │
│                        ┌─────────────────┐                      │
│                        │  GraphQL Client │                      │
│                        │                 │                      │
│                        │ - Authentication│                      │
│                        │ - Query Builder │                      │
│                        │ - Error Handling│                      │
│                        └─────────────────┘                      │
│                                  │                               │
└──────────────────────────────────┼───────────────────────────────┘
                                   │
                                   ▼
                      ┌─────────────────────────┐
                      │   GitHub GraphQL API   │
                      │                         │
                      │ - Discussions           │
                      │ - Discussion Comments   │
                      │ - Discussion Categories │
                      │ - Upvotes              │
                      │ - Locks                │
                      └─────────────────────────┘
```

## 主要组件说明

### 1. Controllers 层
- **GitHubDiscussionsController**: 提供 RESTful API 接口
  - 处理 HTTP 请求和响应
  - 参数验证和错误处理
  - 路由配置

### 2. Services 层
- **IGitHubDiscussionsService**: 服务接口定义
- **GitHubDiscussionsService**: 核心业务逻辑实现
  - GraphQL 查询构建
  - 数据转换和映射
  - 业务规则处理

### 3. Models 层
- **DiscussionModels**: 数据模型定义
  - 请求/响应模型
  - 实体模型
  - DTO 模型

### 4. GraphQL Client
- **连接管理**: 创建和管理 GraphQL 客户端
- **认证处理**: Bearer Token 认证
- **查询执行**: 执行 GraphQL 查询和变更

## API 端点总览

### Discussions
- `GET /api/GitHubDiscussions/{owner}/{name}` - 获取讨论列表
- `GET /api/GitHubDiscussions/by-id/{id}` - 根据ID获取讨论
- `GET /api/GitHubDiscussions/{owner}/{name}/{number}` - 根据编号获取讨论
- `POST /api/GitHubDiscussions` - 创建讨论
- `PUT /api/GitHubDiscussions/{id}` - 更新讨论
- `DELETE /api/GitHubDiscussions/{id}` - 删除讨论
- `POST /api/GitHubDiscussions/{id}/lock` - 锁定讨论
- `POST /api/GitHubDiscussions/{id}/unlock` - 解锁讨论
- `POST /api/GitHubDiscussions/{id}/upvote` - 点赞讨论
- `DELETE /api/GitHubDiscussions/{id}/upvote` - 取消点赞

### Comments
- `GET /api/GitHubDiscussions/{id}/comments` - 获取评论列表
- `POST /api/GitHubDiscussions/comments` - 创建评论
- `PUT /api/GitHubDiscussions/comments/{id}` - 更新评论
- `DELETE /api/GitHubDiscussions/comments/{id}` - 删除评论
- `POST /api/GitHubDiscussions/comments/{id}/upvote` - 评论点赞
- `DELETE /api/GitHubDiscussions/comments/{id}/upvote` - 取消评论点赞
- `POST /api/GitHubDiscussions/comments/{id}/mark-as-answer` - 标记为答案
- `POST /api/GitHubDiscussions/comments/{id}/unmark-as-answer` - 取消标记答案

### Categories
- `GET /api/GitHubDiscussions/{owner}/{name}/categories` - 获取分类列表
- `GET /api/GitHubDiscussions/categories/{id}` - 获取分类详情
- `POST /api/GitHubDiscussions/categories` - 创建分类
- `PUT /api/GitHubDiscussions/categories/{id}` - 更新分类
- `DELETE /api/GitHubDiscussions/categories/{id}` - 删除分类

## 技术特性

### ✅ 完整的 CRUD 操作
- Discussions 的创建、读取、更新、删除
- Comments 的完整生命周期管理
- Categories 的管理功能

### ✅ 高级功能
- 分页支持 (基于游标)
- 排序和筛选
- 点赞功能
- 锁定/解锁
- 标记最佳答案

### ✅ 错误处理
- GraphQL 错误捕获
- HTTP 状态码规范
- 详细错误信息

### ✅ 性能优化
- 异步操作
- 合理的数据结构
- GraphQL 查询优化

### ✅ 可扩展性
- 接口分离
- 依赖注入
- 模块化设计
