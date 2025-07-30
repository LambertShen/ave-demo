using Microsoft.AspNetCore.Mvc;
using Octokit;
using DemoServer.Services.Interfaces;

namespace DemoServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GitHubRepositoryController : ControllerBase
{
    private readonly IGitHubRepositoryService _repositoryService;

    public GitHubRepositoryController(IGitHubRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    /// <summary>
    /// 获取当前用户的仓库列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Repository>>> GetRepositories(
        [FromQuery] string? username = null,
        [FromQuery] int pageSize = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            var repositories = await _repositoryService.GetRepositoriesAsync(username, pageSize, page);
            return Ok(repositories);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving repositories: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取组织的仓库列表
    /// </summary>
    [HttpGet("organization/{organizationName}")]
    public async Task<ActionResult<IReadOnlyList<Repository>>> GetOrganizationRepositories(
        string organizationName,
        [FromQuery] int pageSize = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            var repositories = await _repositoryService.GetOrganizationRepositoriesAsync(organizationName, pageSize, page);
            return Ok(repositories);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving organization repositories: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据 ID 获取仓库详情
    /// </summary>
    [HttpGet("{repositoryId:long}")]
    public async Task<ActionResult<Repository>> GetRepositoryById(long repositoryId)
    {
        try
        {
            var repository = await _repositoryService.GetRepositoryByIdAsync(repositoryId);
            return Ok(repository);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving repository: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据所有者和仓库名获取仓库详情
    /// </summary>
    [HttpGet("{owner}/{repositoryName}")]
    public async Task<ActionResult<Repository>> GetRepository(string owner, string repositoryName)
    {
        try
        {
            var repository = await _repositoryService.GetRepositoryAsync(owner, repositoryName);
            return Ok(repository);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving repository: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新仓库
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Repository>> CreateRepository([FromBody] NewRepository newRepository)
    {
        try
        {
            var repository = await _repositoryService.CreateRepositoryAsync(newRepository);
            return CreatedAtAction(nameof(GetRepositoryById), new { repositoryId = repository.Id }, repository);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating repository: {ex.Message}");
        }
    }

    /// <summary>
    /// 在组织下创建新仓库
    /// </summary>
    [HttpPost("organization/{organizationName}")]
    public async Task<ActionResult<Repository>> CreateOrganizationRepository(
        string organizationName,
        [FromBody] NewRepository newRepository)
    {
        try
        {
            var repository = await _repositoryService.CreateOrganizationRepositoryAsync(organizationName, newRepository);
            return CreatedAtAction(nameof(GetRepositoryById), new { repositoryId = repository.Id }, repository);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating organization repository: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新仓库信息
    /// </summary>
    [HttpPut("{owner}/{repositoryName}")]
    public async Task<ActionResult<Repository>> UpdateRepository(
        string owner,
        string repositoryName,
        [FromBody] RepositoryUpdate repositoryUpdate)
    {
        try
        {
            var repository = await _repositoryService.UpdateRepositoryAsync(owner, repositoryName, repositoryUpdate);
            return Ok(repository);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating repository: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除仓库
    /// </summary>
    [HttpDelete("{owner}/{repositoryName}")]
    public async Task<ActionResult> DeleteRepository(string owner, string repositoryName)
    {
        try
        {
            var success = await _repositoryService.DeleteRepositoryAsync(owner, repositoryName);
            if (success)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to delete repository");
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error deleting repository: {ex.Message}");
        }
    }

    /// <summary>
    /// Fork 仓库
    /// </summary>
    [HttpPost("{owner}/{repositoryName}/fork")]
    public async Task<ActionResult<Repository>> ForkRepository(
        string owner,
        string repositoryName,
        [FromBody] NewRepositoryFork? forkRepository = null)
    {
        try
        {
            var repository = await _repositoryService.ForkRepositoryAsync(owner, repositoryName, forkRepository);
            return CreatedAtAction(nameof(GetRepositoryById), new { repositoryId = repository.Id }, repository);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error forking repository: {ex.Message}");
        }
    }

    /// <summary>
    /// 搜索仓库
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<SearchRepositoryResult>> SearchRepositories([FromQuery] string query)
    {
        try
        {
            var searchRequest = new SearchRepositoriesRequest(query);
            var result = await _repositoryService.SearchRepositoriesAsync(searchRequest);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error searching repositories: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取仓库的分支列表
    /// </summary>
    [HttpGet("{owner}/{repositoryName}/branches")]
    public async Task<ActionResult<IReadOnlyList<Branch>>> GetBranches(
        string owner,
        string repositoryName,
        [FromQuery] int pageSize = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            var branches = await _repositoryService.GetBranchesAsync(owner, repositoryName, pageSize, page);
            return Ok(branches);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving branches: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取仓库的标签列表
    /// </summary>
    [HttpGet("{owner}/{repositoryName}/tags")]
    public async Task<ActionResult<IReadOnlyList<RepositoryTag>>> GetTags(
        string owner,
        string repositoryName,
        [FromQuery] int pageSize = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            var tags = await _repositoryService.GetTagsAsync(owner, repositoryName, pageSize, page);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving tags: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取仓库的协作者列表
    /// </summary>
    [HttpGet("{owner}/{repositoryName}/collaborators")]
    public async Task<ActionResult<IReadOnlyList<Collaborator>>> GetCollaborators(
        string owner,
        string repositoryName,
        [FromQuery] int pageSize = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            var collaborators = await _repositoryService.GetCollaboratorsAsync(owner, repositoryName, pageSize, page);
            return Ok(collaborators);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving collaborators: {ex.Message}");
        }
    }

    /// <summary>
    /// 添加协作者
    /// </summary>
    [HttpPost("{owner}/{repositoryName}/collaborators/{username}")]
    public async Task<ActionResult> AddCollaborator(
        string owner,
        string repositoryName,
        string username,
        [FromQuery] InvitationPermissionType permission = InvitationPermissionType.Write)
    {
        try
        {
            var success = await _repositoryService.AddCollaboratorAsync(owner, repositoryName, username, permission);
            if (success)
            {
                return Ok(new { message = "Collaborator added successfully" });
            }
            else
            {
                return BadRequest("Failed to add collaborator");
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error adding collaborator: {ex.Message}");
        }
    }

    /// <summary>
    /// 移除协作者
    /// </summary>
    [HttpDelete("{owner}/{repositoryName}/collaborators/{username}")]
    public async Task<ActionResult> RemoveCollaborator(
        string owner,
        string repositoryName,
        string username)
    {
        try
        {
            var success = await _repositoryService.RemoveCollaboratorAsync(owner, repositoryName, username);
            if (success)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to remove collaborator");
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error removing collaborator: {ex.Message}");
        }
    }
}
