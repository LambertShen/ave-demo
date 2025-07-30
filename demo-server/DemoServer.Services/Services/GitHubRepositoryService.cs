using Octokit;
using DemoServer.Services.Interfaces;

namespace DemoServer.Services.Services;

public class GitHubRepositoryService : IGitHubRepositoryService
{
    private readonly IGitHubOAuthService _oauthService;

    public GitHubRepositoryService(IGitHubOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    public async Task<IReadOnlyList<Repository>> GetRepositoriesAsync(string? username = null, int pageSize = 30, int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        if (string.IsNullOrEmpty(username))
        {
            // 获取当前认证用户的仓库
            return await client.Repository.GetAllForCurrent(options);
        }
        else
        {
            // 获取指定用户的仓库
            return await client.Repository.GetAllForUser(username, options);
        }
    }

    public async Task<IReadOnlyList<Repository>> GetOrganizationRepositoriesAsync(string organizationName, int pageSize = 30, int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Repository.GetAllForOrg(organizationName, options);
    }

    public async Task<Repository> GetRepositoryByIdAsync(long repositoryId)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Get(repositoryId);
    }

    public async Task<Repository> GetRepositoryAsync(string owner, string repositoryName)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Get(owner, repositoryName);
    }

    public async Task<Repository> CreateRepositoryAsync(NewRepository newRepository)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Create(newRepository);
    }

    public async Task<Repository> CreateOrganizationRepositoryAsync(string organizationName, NewRepository newRepository)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Create(organizationName, newRepository);
    }

    public async Task<Repository> UpdateRepositoryAsync(long repositoryId, RepositoryUpdate repositoryUpdate)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Edit(repositoryId, repositoryUpdate);
    }

    public async Task<Repository> UpdateRepositoryAsync(string owner, string repositoryName, RepositoryUpdate repositoryUpdate)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Edit(owner, repositoryName, repositoryUpdate);
    }

    public async Task<bool> DeleteRepositoryAsync(long repositoryId)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            await client.Repository.Delete(repositoryId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteRepositoryAsync(string owner, string repositoryName)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            await client.Repository.Delete(owner, repositoryName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Repository> ForkRepositoryAsync(string owner, string repositoryName, NewRepositoryFork? forkRepository = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        if (forkRepository != null)
        {
            return await client.Repository.Forks.Create(owner, repositoryName, forkRepository);
        }
        else
        {
            return await client.Repository.Forks.Create(owner, repositoryName, new NewRepositoryFork());
        }
    }

    public async Task<SearchRepositoryResult> SearchRepositoriesAsync(SearchRepositoriesRequest searchRequest)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Search.SearchRepo(searchRequest);
    }

    public async Task<IReadOnlyList<Branch>> GetBranchesAsync(string owner, string repositoryName, int pageSize = 30, int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Repository.Branch.GetAll(owner, repositoryName, options);
    }

    public async Task<IReadOnlyList<RepositoryTag>> GetTagsAsync(string owner, string repositoryName, int pageSize = 30, int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Repository.GetAllTags(owner, repositoryName, options);
    }

    public async Task<IReadOnlyList<Collaborator>> GetCollaboratorsAsync(string owner, string repositoryName, int pageSize = 30, int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Repository.Collaborator.GetAll(owner, repositoryName, options);
    }

    public async Task<bool> AddCollaboratorAsync(string owner, string repositoryName, string username, InvitationPermissionType permission = InvitationPermissionType.Write)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            var collaboratorRequest = new CollaboratorRequest(permission.ToString().ToLower());
            await client.Repository.Collaborator.Add(owner, repositoryName, username, collaboratorRequest);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveCollaboratorAsync(string owner, string repositoryName, string username)
    {
        try
        {
            var client = _oauthService.CreateClientFromAccessTokenAsync();
            await client.Repository.Collaborator.Delete(owner, repositoryName, username);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
