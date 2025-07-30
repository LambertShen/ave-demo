using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/github/projects/{projectId}/views")]
public class GitHubProjectViewController : ControllerBase
{
    private readonly IGitHubProjectService _projectService;

    public GitHubProjectViewController(IGitHubProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// 获取项目的视图列表
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="first">获取数量（默认20）</param>
    /// <param name="after">分页游标</param>
    /// <returns>视图列表</returns>
    [HttpGet]
    public async Task<ActionResult<object>> GetProjectViews(
        string projectId,
        [FromQuery] int first = 20,
        [FromQuery] string? after = null)
    {
        try
        {
            var (views, hasNextPage, endCursor) = await _projectService.GetProjectViewsAsync(projectId, first, after);
            
            return Ok(new
            {
                data = views,
                pagination = new
                {
                    hasNextPage,
                    endCursor
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取项目视图详情
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="viewId">视图ID</param>
    /// <returns>视图详情</returns>
    [HttpGet("{viewId}")]
    public async Task<ActionResult<ProjectView>> GetProjectViewById(string projectId, string viewId)
    {
        try
        {
            var view = await _projectService.GetProjectViewByIdAsync(viewId);
            
            if (view == null)
            {
                return NotFound(new { error = "Project view not found" });
            }

            // Verify the view belongs to the specified project
            if (view.ProjectId != projectId)
            {
                return BadRequest(new { error = "View does not belong to the specified project" });
            }

            return Ok(view);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 创建项目视图
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="request">创建请求</param>
    /// <returns>创建的视图</returns>
    [HttpPost]
    public async Task<ActionResult<ProjectView>> CreateProjectView(
        string projectId,
        [FromBody] CreateProjectViewRequest request)
    {
        try
        {
            // Ensure the projectId in the URL matches the request
            request.ProjectId = projectId;

            var view = await _projectService.CreateProjectViewAsync(request);
            
            if (view == null)
            {
                return BadRequest(new { error = "Failed to create project view" });
            }

            return CreatedAtAction(
                nameof(GetProjectViewById),
                new { projectId = view.ProjectId, viewId = view.Id },
                view);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新项目视图
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="viewId">视图ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的视图</returns>
    [HttpPut("{viewId}")]
    public async Task<ActionResult<ProjectView>> UpdateProjectView(
        string projectId,
        string viewId,
        [FromBody] UpdateProjectViewRequest request)
    {
        try
        {
            // Ensure the viewId in the URL matches the request
            request.ViewId = viewId;

            var view = await _projectService.UpdateProjectViewAsync(request);
            
            if (view == null)
            {
                return NotFound(new { error = "Project view not found or failed to update" });
            }

            // Verify the view belongs to the specified project
            if (view.ProjectId != projectId)
            {
                return BadRequest(new { error = "View does not belong to the specified project" });
            }

            return Ok(view);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除项目视图
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="viewId">视图ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{viewId}")]
    public async Task<ActionResult> DeleteProjectView(string projectId, string viewId)
    {
        try
        {
            // First verify the view belongs to the specified project
            var view = await _projectService.GetProjectViewByIdAsync(viewId);
            if (view == null)
            {
                return NotFound(new { error = "Project view not found" });
            }

            if (view.ProjectId != projectId)
            {
                return BadRequest(new { error = "View does not belong to the specified project" });
            }

            var success = await _projectService.DeleteProjectViewAsync(viewId);
            
            if (!success)
            {
                return BadRequest(new { error = "Failed to delete project view" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 复制项目视图
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="viewId">源视图ID</param>
    /// <param name="request">复制请求</param>
    /// <returns>复制的视图</returns>
    [HttpPost("{viewId}/copy")]
    public async Task<ActionResult<ProjectView>> CopyProjectView(
        string projectId,
        string viewId,
        [FromBody] CopyProjectViewRequest request)
    {
        try
        {
            // First verify the source view belongs to the specified project
            var sourceView = await _projectService.GetProjectViewByIdAsync(viewId);
            if (sourceView == null)
            {
                return NotFound(new { error = "Source project view not found" });
            }

            if (sourceView.ProjectId != projectId)
            {
                return BadRequest(new { error = "Source view does not belong to the specified project" });
            }

            var copiedView = await _projectService.CopyProjectViewAsync(viewId, request.NewName);
            
            if (copiedView == null)
            {
                return BadRequest(new { error = "Failed to copy project view" });
            }

            return CreatedAtAction(
                nameof(GetProjectViewById),
                new { projectId = copiedView.ProjectId, viewId = copiedView.Id },
                copiedView);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// 复制项目视图请求
/// </summary>
public class CopyProjectViewRequest
{
    /// <summary>
    /// 新视图名称
    /// </summary>
    public string NewName { get; set; } = string.Empty;
}
