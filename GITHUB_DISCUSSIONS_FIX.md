# GitHub Discussions 服务问题修复说明

## 问题描述
在使用 GitHub Discussions API 时遇到错误：
```
"error": "GraphQL errors: Field 'state' doesn't exist on type 'Discussion'"
```

## 问题原因
GitHub GraphQL API 中的 `Discussion` 类型没有 `state` 字段，这与 Issue 和 Pull Request 不同。

## 解决方案

### 1. 移除了所有 GraphQL 查询中的 `state` 字段
在以下方法的 GraphQL 查询中移除了 `state` 字段：
- `GetDiscussionsAsync`
- `GetDiscussionByIdAsync` 
- `GetDiscussionByNumberAsync`
- `CreateDiscussionAsync`
- `UpdateDiscussionAsync`

### 2. 更新了数据模型
将 `GitHubDiscussion` 模型中的 `State` 属性改为计算属性：

```csharp
/// <summary>
/// 讨论状态：根据是否被锁定判断
/// </summary>
public string State => Locked ? "LOCKED" : "OPEN";
```

这样：
- 当讨论被锁定时，状态为 "LOCKED"
- 当讨论未被锁定时，状态为 "OPEN"

### 3. 移除了服务实现中的 State 赋值
移除了所有服务方法中对 `State` 属性的手动赋值，因为现在它是只读的计算属性。

## 验证修复
项目现在可以成功编译，并且：
- ✅ 移除了所有对不存在的 `state` GraphQL 字段的引用
- ✅ 保持了 `State` 属性的向后兼容性
- ✅ 提供了基于 `Locked` 状态的合理状态值

## 使用说明
现在可以正常使用所有的 Discussions API，`State` 属性会自动根据 `Locked` 状态返回相应的值：

```csharp
var discussions = await _discussionsService.GetDiscussionsAsync("owner", "repo");
foreach (var discussion in discussions.Discussions)
{
    Console.WriteLine($"Discussion: {discussion.Title}");
    Console.WriteLine($"State: {discussion.State}"); // "OPEN" 或 "LOCKED"
    Console.WriteLine($"Locked: {discussion.Locked}"); // true 或 false
}
```

这个修复确保了与 GitHub GraphQL API 的完全兼容性，同时保持了代码的清晰和易用性。
