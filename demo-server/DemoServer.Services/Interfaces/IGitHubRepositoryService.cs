using Octokit;

namespace DemoServer.Services.Interfaces;

public interface IGitHubRepositoryService
{
    /// <summary>
    /// 获取指定用户的仓库列表
    /// </summary>
    /// <param name="username">用户名，为空时获取当前认证用户的仓库</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="page">页码</param>
    /// <returns>仓库列表</returns>
    Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string? username = null, int pageSize = 30, int page = 1);

    /// <summary>
    /// 获取组织的仓库列表
    /// </summary>
    /// <param name="organizationName">组织名</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="page">页码</param>
    /// <returns>仓库列表</returns>
    Task<IReadOnlyList<Repository>> GetOrganizationRepositoriesAsync(string organizationName, int pageSize = 30, int page = 1);

    /// <summary>
    /// 根据仓库 ID 获取仓库详情
    /// </summary>
    /// <param name="repositoryId">仓库 ID</param>
    /// <returns>仓库详情</returns>
    Task<Repository> GetRepositoryByIdAsync(long repositoryId);

    /// <summary>
    /// 根据所有者和仓库名获取仓库详情
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <returns>仓库详情</returns>
    Task<Repository> GetRepositoryAsync(string owner, string repositoryName);

    /// <summary>
    /// 创建新仓库
    /// </summary>
    /// <param name="newRepository">新仓库信息</param>
    /// <returns>创建的仓库</returns>
    Task<Repository> CreateRepositoryAsync(NewRepository newRepository);

    /// <summary>
    /// 在指定组织下创建新仓库
    /// </summary>
    /// <param name="organizationName">组织名</param>
    /// <param name="newRepository">新仓库信息</param>
    /// <returns>创建的仓库</returns>
    Task<Repository> CreateOrganizationRepositoryAsync(string organizationName, NewRepository newRepository);

    /// <summary>
    /// 更新仓库信息
    /// </summary>
    /// <param name="repositoryId">仓库 ID</param>
    /// <param name="repositoryUpdate">更新信息</param>
    /// <returns>更新后的仓库</returns>
    Task<Repository> UpdateRepositoryAsync(long repositoryId, RepositoryUpdate repositoryUpdate);

    /// <summary>
    /// 更新仓库信息
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <param name="repositoryUpdate">更新信息</param>
    /// <returns>更新后的仓库</returns>
    Task<Repository> UpdateRepositoryAsync(string owner, string repositoryName, RepositoryUpdate repositoryUpdate);

    /// <summary>
    /// 删除仓库
    /// </summary>
    /// <param name="repositoryId">仓库 ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteRepositoryAsync(long repositoryId);

    /// <summary>
    /// 删除仓库
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteRepositoryAsync(string owner, string repositoryName);

    /// <summary>
    /// Fork 仓库
    /// </summary>
    /// <param name="owner">原仓库所有者</param>
    /// <param name="repositoryName">原仓库名</param>
    /// <param name="forkRepository">Fork 配置</param>
    /// <returns>Fork 后的仓库</returns>
    Task<Repository> ForkRepositoryAsync(string owner, string repositoryName, NewRepositoryFork? forkRepository = null);

    /// <summary>
    /// 搜索仓库
    /// </summary>
    /// <param name="searchRequest">搜索请求</param>
    /// <returns>搜索结果</returns>
    Task<SearchRepositoryResult> SearchRepositoriesAsync(SearchRepositoriesRequest searchRequest);

    /// <summary>
    /// 获取仓库的分支列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="page">页码</param>
    /// <returns>分支列表</returns>
    Task<IReadOnlyList<Branch>> GetBranchesAsync(string owner, string repositoryName, int pageSize = 30, int page = 1);

    /// <summary>
    /// 获取仓库的标签列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="page">页码</param>
    /// <returns>标签列表</returns>
    Task<IReadOnlyList<RepositoryTag>> GetTagsAsync(string owner, string repositoryName, int pageSize = 30, int page = 1);

    /// <summary>
    /// 获取仓库的协作者列表
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="page">页码</param>
    /// <returns>协作者列表</returns>
    Task<IReadOnlyList<Collaborator>> GetCollaboratorsAsync(string owner, string repositoryName, int pageSize = 30, int page = 1);

    /// <summary>
    /// 添加协作者
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <param name="username">用户名</param>
    /// <param name="permission">权限</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddCollaboratorAsync(string owner, string repositoryName, string username, InvitationPermissionType permission = InvitationPermissionType.Write);

    /// <summary>
    /// 移除协作者
    /// </summary>
    /// <param name="owner">仓库所有者</param>
    /// <param name="repositoryName">仓库名</param>
    /// <param name="username">用户名</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveCollaboratorAsync(string owner, string repositoryName, string username);
}
