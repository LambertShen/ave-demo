using DemoServer.Services.Models;

namespace DemoServer.Services.Interfaces;

public interface IGitHubDiscussionsService
{
    // Discussions CRUD operations
    
    /// <summary>
    /// 获取仓库的讨论列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="first">获取的讨论数量</param>
    /// <param name="after">分页游标</param>
    /// <param name="categoryId">分类ID过滤</param>
    /// <param name="orderBy">排序方式 (CREATED_AT, UPDATED_AT)</param>
    /// <param name="direction">排序方向 (ASC, DESC)</param>
    /// <returns>讨论列表</returns>
    Task<DiscussionsResponse> GetDiscussionsAsync(
        string owner,
        string name,
        int first = 20,
        string? after = null,
        string? categoryId = null,
        string orderBy = "UPDATED_AT",
        string direction = "DESC");

    /// <summary>
    /// 根据ID获取单个讨论详情
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>讨论详情</returns>
    Task<GitHubDiscussion?> GetDiscussionByIdAsync(string discussionId);

    /// <summary>
    /// 根据仓库和讨论编号获取讨论详情
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">讨论编号</param>
    /// <returns>讨论详情</returns>
    Task<GitHubDiscussion?> GetDiscussionByNumberAsync(string owner, string name, int number);

    /// <summary>
    /// 创建新讨论
    /// </summary>
    /// <param name="request">创建讨论请求</param>
    /// <returns>创建的讨论</returns>
    Task<GitHubDiscussion> CreateDiscussionAsync(CreateDiscussionRequest request);

    /// <summary>
    /// 更新讨论
    /// </summary>
    /// <param name="request">更新讨论请求</param>
    /// <returns>更新后的讨论</returns>
    Task<GitHubDiscussion> UpdateDiscussionAsync(UpdateDiscussionRequest request);

    /// <summary>
    /// 删除讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteDiscussionAsync(string discussionId);

    /// <summary>
    /// 锁定讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <param name="lockReason">锁定原因</param>
    /// <returns>锁定是否成功</returns>
    Task<bool> LockDiscussionAsync(string discussionId, string? lockReason = null);

    /// <summary>
    /// 解锁讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>解锁是否成功</returns>
    Task<bool> UnlockDiscussionAsync(string discussionId);

    /// <summary>
    /// 为讨论点赞
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>点赞是否成功</returns>
    Task<bool> UpvoteDiscussionAsync(string discussionId);

    /// <summary>
    /// 取消讨论点赞
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>取消点赞是否成功</returns>
    Task<bool> RemoveDiscussionUpvoteAsync(string discussionId);

    // Discussion Comments CRUD operations

    /// <summary>
    /// 获取讨论的评论列表
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <param name="first">获取的评论数量</param>
    /// <param name="after">分页游标</param>
    /// <returns>评论列表</returns>
    Task<DiscussionCommentsResponse> GetDiscussionCommentsAsync(
        string discussionId,
        int first = 20,
        string? after = null);

    /// <summary>
    /// 创建讨论评论
    /// </summary>
    /// <param name="request">创建评论请求</param>
    /// <returns>创建的评论</returns>
    Task<DiscussionComment> CreateDiscussionCommentAsync(CreateDiscussionCommentRequest request);

    /// <summary>
    /// 更新讨论评论
    /// </summary>
    /// <param name="request">更新评论请求</param>
    /// <returns>更新后的评论</returns>
    Task<DiscussionComment> UpdateDiscussionCommentAsync(UpdateDiscussionCommentRequest request);

    /// <summary>
    /// 删除讨论评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteDiscussionCommentAsync(string commentId);

    /// <summary>
    /// 为评论点赞
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>点赞是否成功</returns>
    Task<bool> UpvoteDiscussionCommentAsync(string commentId);

    /// <summary>
    /// 取消评论点赞
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>取消点赞是否成功</returns>
    Task<bool> RemoveDiscussionCommentUpvoteAsync(string commentId);

    /// <summary>
    /// 标记评论为答案
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>标记是否成功</returns>
    Task<bool> MarkCommentAsAnswerAsync(string commentId);

    /// <summary>
    /// 取消标记评论为答案
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>取消标记是否成功</returns>
    Task<bool> UnmarkCommentAsAnswerAsync(string commentId);

    // Discussion Categories CRUD operations

    /// <summary>
    /// 获取仓库的讨论分类列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <returns>分类列表</returns>
    Task<DiscussionCategoriesResponse> GetDiscussionCategoriesAsync(string owner, string name);

    /// <summary>
    /// 根据ID获取讨论分类详情
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <returns>分类详情</returns>
    Task<DiscussionCategory?> GetDiscussionCategoryByIdAsync(string categoryId);

    /// <summary>
    /// 创建讨论分类
    /// </summary>
    /// <param name="request">创建分类请求</param>
    /// <returns>创建的分类</returns>
    Task<DiscussionCategory> CreateDiscussionCategoryAsync(CreateDiscussionCategoryRequest request);

    /// <summary>
    /// 更新讨论分类
    /// </summary>
    /// <param name="request">更新分类请求</param>
    /// <returns>更新后的分类</returns>
    Task<DiscussionCategory> UpdateDiscussionCategoryAsync(UpdateDiscussionCategoryRequest request);

    /// <summary>
    /// 删除讨论分类
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <returns>删除是否成功</returns>
    Task<bool> DeleteDiscussionCategoryAsync(string categoryId);
}
