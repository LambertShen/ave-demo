using DemoServer.Services.Models;

namespace DemoServer.Services.Interfaces;

public interface IGitHubProjectService
{
    /// <summary>
    /// 获取当前用户的项目列表
    /// </summary>
    /// <param name="first">获取的项目数量</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目列表</returns>
    Task<(List<GitHubProject> Projects, bool HasNextPage, string? EndCursor)> GetProjectsAsync(
        int first = 20, 
        string? after = null);

    /// <summary>
    /// 获取用户信息和项目访问权限
    /// </summary>
    /// <returns>用户登录名、项目数量和是否有项目访问权限</returns>
    Task<(string Login, int ProjectCount, bool HasProjectAccess)> GetUserInfoAsync();

    /// <summary>
    /// 获取组织的项目列表
    /// </summary>
    /// <param name="organizationLogin">组织登录名</param>
    /// <param name="first">获取的项目数量</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目列表</returns>
    Task<(List<GitHubProject> Projects, bool HasNextPage, string? EndCursor)> GetOrganizationProjectsAsync(
        string organizationLogin, 
        int first = 20, 
        string? after = null);

    /// <summary>
    /// 获取用户的项目列表
    /// </summary>
    /// <param name="userLogin">用户登录名</param>
    /// <param name="first">获取的项目数量</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目列表</returns>
    Task<(List<GitHubProject> Projects, bool HasNextPage, string? EndCursor)> GetUserProjectsAsync(
        string userLogin, 
        int first = 20, 
        string? after = null);

    /// <summary>
    /// 根据ID获取项目详情
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>项目详情</returns>
    Task<GitHubProject?> GetProjectByIdAsync(string projectId);

    /// <summary>
    /// 创建新项目
    /// </summary>
    /// <param name="request">创建项目请求</param>
    /// <returns>创建的项目</returns>
    Task<GitHubProject?> CreateProjectAsync(CreateProjectRequest request);

    /// <summary>
    /// 更新项目
    /// </summary>
    /// <param name="request">更新项目请求</param>
    /// <returns>更新后的项目</returns>
    Task<GitHubProject?> UpdateProjectAsync(UpdateProjectRequest request);

    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteProjectAsync(string projectId);

    /// <summary>
    /// 获取项目的所有者信息
    /// </summary>
    /// <param name="login">所有者登录名</param>
    /// <returns>所有者信息</returns>
    Task<ProjectOwner?> GetOwnerAsync(string login);

    /// <summary>
    /// 获取项目字段
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <returns>项目字段列表</returns>
    Task<List<ProjectField>> GetProjectFieldsAsync(string projectId);

    /// <summary>
    /// 获取项目条目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="first">获取的条目数量</param>
    /// <param name="after">分页游标</param>
    /// <returns>项目条目列表</returns>
    Task<(List<ProjectItem> Items, bool HasNextPage, string? EndCursor)> GetProjectItemsAsync(
        string projectId, 
        int first = 20, 
        string? after = null);

    /// <summary>
    /// 添加项目条目
    /// </summary>
    /// <param name="request">添加条目请求</param>
    /// <returns>添加的条目</returns>
    Task<ProjectItem?> AddProjectItemAsync(AddProjectItemRequest request);

    /// <summary>
    /// 更新项目条目
    /// </summary>
    /// <param name="request">更新条目请求</param>
    /// <returns>更新后的条目</returns>
    Task<ProjectItem?> UpdateProjectItemAsync(UpdateProjectItemRequest request);

    /// <summary>
    /// 删除项目条目
    /// </summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="itemId">条目ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteProjectItemAsync(string projectId, string itemId);
}
