using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GitHubDiscussionsController : ControllerBase
{
    private readonly IGitHubDiscussionsService _discussionsService;

    public GitHubDiscussionsController(IGitHubDiscussionsService discussionsService)
    {
        _discussionsService = discussionsService;
    }

    #region Discussions CRUD

    /// <summary>
    /// 获取仓库的讨论列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="first">获取的讨论数量，默认20</param>
    /// <param name="after">分页游标</param>
    /// <param name="categoryId">分类ID过滤</param>
    /// <param name="orderBy">排序方式 (CREATED_AT, UPDATED_AT)，默认UPDATED_AT</param>
    /// <param name="direction">排序方向 (ASC, DESC)，默认DESC</param>
    /// <returns>讨论列表</returns>
    [HttpGet("{owner}/{name}")]
    public async Task<ActionResult<DiscussionsResponse>> GetDiscussions(
        string owner,
        string name,
        [FromQuery] int first = 20,
        [FromQuery] string? after = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] string orderBy = "UPDATED_AT",
        [FromQuery] string direction = "DESC")
    {
        try
        {
            var result = await _discussionsService.GetDiscussionsAsync(owner, name, first, after, categoryId, orderBy, direction);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取单个讨论详情
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>讨论详情</returns>
    [HttpGet("by-id/{discussionId}")]
    public async Task<ActionResult<GitHubDiscussion>> GetDiscussionById(string discussionId)
    {
        try
        {
            var result = await _discussionsService.GetDiscussionByIdAsync(discussionId);
            if (result == null)
            {
                return NotFound(new { error = "Discussion not found" });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 根据仓库和讨论编号获取讨论详情
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <param name="number">讨论编号</param>
    /// <returns>讨论详情</returns>
    [HttpGet("{owner}/{name}/{number:int}")]
    public async Task<ActionResult<GitHubDiscussion>> GetDiscussionByNumber(string owner, string name, int number)
    {
        try
        {
            var result = await _discussionsService.GetDiscussionByNumberAsync(owner, name, number);
            if (result == null)
            {
                return NotFound(new { error = "Discussion not found" });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 创建新讨论
    /// </summary>
    /// <param name="request">创建讨论请求</param>
    /// <returns>创建的讨论</returns>
    [HttpPost]
    public async Task<ActionResult<GitHubDiscussion>> CreateDiscussion([FromBody] CreateDiscussionRequest request)
    {
        try
        {
            var result = await _discussionsService.CreateDiscussionAsync(request);
            return CreatedAtAction(nameof(GetDiscussionById), new { discussionId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <param name="request">更新讨论请求</param>
    /// <returns>更新后的讨论</returns>
    [HttpPut("{discussionId}")]
    public async Task<ActionResult<GitHubDiscussion>> UpdateDiscussion(string discussionId, [FromBody] UpdateDiscussionRequest request)
    {
        try
        {
            request.DiscussionId = discussionId;
            var result = await _discussionsService.UpdateDiscussionAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{discussionId}")]
    public async Task<ActionResult> DeleteDiscussion(string discussionId)
    {
        try
        {
            var success = await _discussionsService.DeleteDiscussionAsync(discussionId);
            if (success)
            {
                return NoContent();
            }
            return BadRequest(new { error = "Failed to delete discussion" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 锁定讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <param name="lockReason">锁定原因</param>
    /// <returns>锁定结果</returns>
    [HttpPost("{discussionId}/lock")]
    public async Task<ActionResult> LockDiscussion(string discussionId, [FromBody] string? lockReason = null)
    {
        try
        {
            var success = await _discussionsService.LockDiscussionAsync(discussionId, lockReason);
            if (success)
            {
                return Ok(new { message = "Discussion locked successfully" });
            }
            return BadRequest(new { error = "Failed to lock discussion" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 解锁讨论
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>解锁结果</returns>
    [HttpPost("{discussionId}/unlock")]
    public async Task<ActionResult> UnlockDiscussion(string discussionId)
    {
        try
        {
            var success = await _discussionsService.UnlockDiscussionAsync(discussionId);
            if (success)
            {
                return Ok(new { message = "Discussion unlocked successfully" });
            }
            return BadRequest(new { error = "Failed to unlock discussion" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 为讨论点赞
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>点赞结果</returns>
    [HttpPost("{discussionId}/upvote")]
    public async Task<ActionResult> UpvoteDiscussion(string discussionId)
    {
        try
        {
            var success = await _discussionsService.UpvoteDiscussionAsync(discussionId);
            if (success)
            {
                return Ok(new { message = "Discussion upvoted successfully" });
            }
            return BadRequest(new { error = "Failed to upvote discussion" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 取消讨论点赞
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <returns>取消点赞结果</returns>
    [HttpDelete("{discussionId}/upvote")]
    public async Task<ActionResult> RemoveDiscussionUpvote(string discussionId)
    {
        try
        {
            var success = await _discussionsService.RemoveDiscussionUpvoteAsync(discussionId);
            if (success)
            {
                return Ok(new { message = "Discussion upvote removed successfully" });
            }
            return BadRequest(new { error = "Failed to remove discussion upvote" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Discussion Comments CRUD

    /// <summary>
    /// 获取讨论的评论列表
    /// </summary>
    /// <param name="discussionId">讨论ID</param>
    /// <param name="first">获取的评论数量，默认20</param>
    /// <param name="after">分页游标</param>
    /// <returns>评论列表</returns>
    [HttpGet("{discussionId}/comments")]
    public async Task<ActionResult<DiscussionCommentsResponse>> GetDiscussionComments(
        string discussionId,
        [FromQuery] int first = 20,
        [FromQuery] string? after = null)
    {
        try
        {
            var result = await _discussionsService.GetDiscussionCommentsAsync(discussionId, first, after);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 创建讨论评论
    /// </summary>
    /// <param name="request">创建评论请求</param>
    /// <returns>创建的评论</returns>
    [HttpPost("comments")]
    public async Task<ActionResult<DiscussionComment>> CreateDiscussionComment([FromBody] CreateDiscussionCommentRequest request)
    {
        try
        {
            var result = await _discussionsService.CreateDiscussionCommentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新讨论评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="request">更新评论请求</param>
    /// <returns>更新后的评论</returns>
    [HttpPut("comments/{commentId}")]
    public async Task<ActionResult<DiscussionComment>> UpdateDiscussionComment(string commentId, [FromBody] UpdateDiscussionCommentRequest request)
    {
        try
        {
            request.CommentId = commentId;
            var result = await _discussionsService.UpdateDiscussionCommentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除讨论评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("comments/{commentId}")]
    public async Task<ActionResult> DeleteDiscussionComment(string commentId)
    {
        try
        {
            var success = await _discussionsService.DeleteDiscussionCommentAsync(commentId);
            if (success)
            {
                return NoContent();
            }
            return BadRequest(new { error = "Failed to delete comment" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 为评论点赞
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>点赞结果</returns>
    [HttpPost("comments/{commentId}/upvote")]
    public async Task<ActionResult> UpvoteDiscussionComment(string commentId)
    {
        try
        {
            var success = await _discussionsService.UpvoteDiscussionCommentAsync(commentId);
            if (success)
            {
                return Ok(new { message = "Comment upvoted successfully" });
            }
            return BadRequest(new { error = "Failed to upvote comment" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 取消评论点赞
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>取消点赞结果</returns>
    [HttpDelete("comments/{commentId}/upvote")]
    public async Task<ActionResult> RemoveDiscussionCommentUpvote(string commentId)
    {
        try
        {
            var success = await _discussionsService.RemoveDiscussionCommentUpvoteAsync(commentId);
            if (success)
            {
                return Ok(new { message = "Comment upvote removed successfully" });
            }
            return BadRequest(new { error = "Failed to remove comment upvote" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 标记评论为答案
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>标记结果</returns>
    [HttpPost("comments/{commentId}/mark-as-answer")]
    public async Task<ActionResult> MarkCommentAsAnswer(string commentId)
    {
        try
        {
            var success = await _discussionsService.MarkCommentAsAnswerAsync(commentId);
            if (success)
            {
                return Ok(new { message = "Comment marked as answer successfully" });
            }
            return BadRequest(new { error = "Failed to mark comment as answer" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 取消标记评论为答案
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>取消标记结果</returns>
    [HttpPost("comments/{commentId}/unmark-as-answer")]
    public async Task<ActionResult> UnmarkCommentAsAnswer(string commentId)
    {
        try
        {
            var success = await _discussionsService.UnmarkCommentAsAnswerAsync(commentId);
            if (success)
            {
                return Ok(new { message = "Comment unmarked as answer successfully" });
            }
            return BadRequest(new { error = "Failed to unmark comment as answer" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Discussion Categories CRUD

    /// <summary>
    /// 获取仓库的讨论分类列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="name">仓库名称</param>
    /// <returns>分类列表</returns>
    [HttpGet("{owner}/{name}/categories")]
    public async Task<ActionResult<DiscussionCategoriesResponse>> GetDiscussionCategories(string owner, string name)
    {
        try
        {
            var result = await _discussionsService.GetDiscussionCategoriesAsync(owner, name);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取讨论分类详情
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <returns>分类详情</returns>
    [HttpGet("categories/{categoryId}")]
    public async Task<ActionResult<DiscussionCategory>> GetDiscussionCategoryById(string categoryId)
    {
        try
        {
            var result = await _discussionsService.GetDiscussionCategoryByIdAsync(categoryId);
            if (result == null)
            {
                return NotFound(new { error = "Category not found" });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 创建讨论分类
    /// </summary>
    /// <param name="request">创建分类请求</param>
    /// <returns>创建的分类</returns>
    [HttpPost("categories")]
    public async Task<ActionResult<DiscussionCategory>> CreateDiscussionCategory([FromBody] CreateDiscussionCategoryRequest request)
    {
        try
        {
            var result = await _discussionsService.CreateDiscussionCategoryAsync(request);
            return CreatedAtAction(nameof(GetDiscussionCategoryById), new { categoryId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新讨论分类
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <param name="request">更新分类请求</param>
    /// <returns>更新后的分类</returns>
    [HttpPut("categories/{categoryId}")]
    public async Task<ActionResult<DiscussionCategory>> UpdateDiscussionCategory(string categoryId, [FromBody] UpdateDiscussionCategoryRequest request)
    {
        try
        {
            request.CategoryId = categoryId;
            var result = await _discussionsService.UpdateDiscussionCategoryAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除讨论分类
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("categories/{categoryId}")]
    public async Task<ActionResult> DeleteDiscussionCategory(string categoryId)
    {
        try
        {
            var success = await _discussionsService.DeleteDiscussionCategoryAsync(categoryId);
            if (success)
            {
                return NoContent();
            }
            return BadRequest(new { error = "Failed to delete category" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion
}
