using Microsoft.AspNetCore.Mvc;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GitHubProjectController : ControllerBase
{
    private readonly IGitHubProjectService _projectService;

    public GitHubProjectController(IGitHubProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// 获取当前用户的项目列表
    /// </summary>
    /// <param name="first">获取的项目数量，默认20</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目列表</returns>
    [HttpGet]
    public async Task<ActionResult<ProjectListResponse>> GetProjectsAsync(
        [FromQuery] int first = 20,
        [FromQuery] string? after = null)
    {
        try
        {
            var (projects, hasNextPage, endCursor) = await _projectService.GetProjectsAsync(first, after);
            
            return Ok(new ProjectListResponse
            {
                Projects = projects,
                HasNextPage = hasNextPage,
                EndCursor = endCursor
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取组织的项目列表
    /// </summary>
    /// <param name="organizationLogin">组织登录名</param>
    /// <param name="first">获取的项目数量，默认20</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目列表</returns>
    [HttpGet("organization/{organizationLogin}")]
    public async Task<ActionResult<ProjectListResponse>> GetOrganizationProjectsAsync(
        string organizationLogin,
        [FromQuery] int first = 20,
        [FromQuery] string? after = null)
    {
        try
        {
            if (string.IsNullOrEmpty(organizationLogin))
            {
                return BadRequest("Organization login is required");
            }

            var (projects, hasNextPage, endCursor) = await _projectService.GetOrganizationProjectsAsync(organizationLogin, first, after);
            
            return Ok(new ProjectListResponse
            {
                Projects = projects,
                HasNextPage = hasNextPage,
                EndCursor = endCursor
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户的项目列表
    /// </summary>
    /// <param name="userLogin">用户登录名</param>
    /// <param name="first">获取的项目数量，默认20</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目列表</returns>
    [HttpGet("user/{userLogin}")]
    public async Task<ActionResult<ProjectListResponse>> GetUserProjectsAsync(
        string userLogin,
        [FromQuery] int first = 20,
        [FromQuery] string? after = null)
    {
        try
        {
            if (string.IsNullOrEmpty(userLogin))
            {
                return BadRequest("User login is required");
            }

            var (projects, hasNextPage, endCursor) = await _projectService.GetUserProjectsAsync(userLogin, first, after);
            
            return Ok(new ProjectListResponse
            {
                Projects = projects,
                HasNextPage = hasNextPage,
                EndCursor = endCursor
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据ID获取项目详情
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>项目详情</returns>
    [HttpGet("{projectId}")]
    public async Task<ActionResult<GitHubProject>> GetProjectByIdAsync(string projectId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            var project = await _projectService.GetProjectByIdAsync(projectId);
            
            if (project == null)
            {
                return NotFound("Project not found");
            }

            return Ok(project);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新项目
    /// </summary>
    /// <param name="request">创建项目请求</param>
    /// <returns>创建的项目</returns>
    [HttpPost]
    public async Task<ActionResult<GitHubProject>> CreateProjectAsync([FromBody] CreateProjectRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrEmpty(request.Title))
            {
                return BadRequest("Project title is required");
            }

            if (string.IsNullOrEmpty(request.OwnerId))
            {
                return BadRequest("Owner ID is required");
            }

            var project = await _projectService.CreateProjectAsync(request);
            
            if (project == null)
            {
                return StatusCode(500, "Failed to create project");
            }

            return CreatedAtAction(nameof(GetProjectByIdAsync), new { projectId = project.Id }, project);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新项目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="request">更新项目请求</param>
    /// <returns>更新后的项目</returns>
    [HttpPut("{projectId}")]
    public async Task<ActionResult<GitHubProject>> UpdateProjectAsync(
        string projectId,
        [FromBody] UpdateProjectRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            // 设置项目ID
            request.ProjectId = projectId;

            var project = await _projectService.UpdateProjectAsync(request);
            
            if (project == null)
            {
                return NotFound("Project not found or failed to update");
            }

            return Ok(project);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{projectId}")]
    public async Task<ActionResult> DeleteProjectAsync(string projectId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            var result = await _projectService.DeleteProjectAsync(projectId);
            
            if (!result)
            {
                return NotFound("Project not found or failed to delete");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取所有者信息
    /// </summary>
    /// <param name="login">所有者登录名</param>
    /// <returns>所有者信息</returns>
    [HttpGet("owner/{login}")]
    public async Task<ActionResult<ProjectOwner>> GetOwnerAsync(string login)
    {
        try
        {
            if (string.IsNullOrEmpty(login))
            {
                return BadRequest("Login is required");
            }

            var owner = await _projectService.GetOwnerAsync(login);
            
            if (owner == null)
            {
                return NotFound("Owner not found");
            }

            return Ok(owner);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取项目字段
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>项目字段列表</returns>
    [HttpGet("{projectId}/fields")]
    public async Task<ActionResult<List<ProjectField>>> GetProjectFieldsAsync(string projectId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            var fields = await _projectService.GetProjectFieldsAsync(projectId);
            
            return Ok(fields);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取项目条目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="first">获取的条目数量，默认20</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目条目列表</returns>
    [HttpGet("{projectId}/items")]
    public async Task<ActionResult<ProjectItemListResponse>> GetProjectItemsAsync(
        string projectId,
        [FromQuery] int first = 20,
        [FromQuery] string? after = null)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            var (items, hasNextPage, endCursor) = await _projectService.GetProjectItemsAsync(projectId, first, after);
            
            return Ok(new ProjectItemListResponse
            {
                Items = items,
                HasNextPage = hasNextPage,
                EndCursor = endCursor
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加项目条目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="request">添加条目请求</param>
    /// <returns>添加的条目</returns>
    [HttpPost("{projectId}/items")]
    public async Task<ActionResult<ProjectItem>> AddProjectItemAsync(
        string projectId,
        [FromBody] AddProjectItemRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrEmpty(request.ContentId))
            {
                return BadRequest("Content ID is required");
            }

            // 设置项目ID
            request.ProjectId = projectId;

            var item = await _projectService.AddProjectItemAsync(request);
            
            if (item == null)
            {
                return StatusCode(500, "Failed to add project item");
            }

            return Created($"/api/githubproject/{projectId}/items", item);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除项目条目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="itemId">条目ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{projectId}/items/{itemId}")]
    public async Task<ActionResult> DeleteProjectItemAsync(
        string projectId,
        string itemId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required");
            }

            if (string.IsNullOrEmpty(itemId))
            {
                return BadRequest("Item ID is required");
            }

            var result = await _projectService.DeleteProjectItemAsync(projectId, itemId);
            
            if (!result)
            {
                return NotFound("Project item not found or failed to delete");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户信息和项目访问权限（调试用）
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("debug/userinfo")]
    public async Task<ActionResult> GetUserInfoAsync()
    {
        try
        {
            var (login, projectCount, hasAccess) = await _projectService.GetUserInfoAsync();
            
            return Ok(new 
            {
                Login = login,
                ProjectCount = projectCount,
                HasProjectAccess = hasAccess,
                Message = hasAccess 
                    ? $"用户 {login} 有 {projectCount} 个项目"
                    : $"用户 {login} 无法访问项目或权限不足"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

// Response models for API
public class ProjectListResponse
{
    public List<GitHubProject> Projects { get; set; } = new();
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
}

public class ProjectItemListResponse
{
    public List<ProjectItem> Items { get; set; } = new();
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
}
