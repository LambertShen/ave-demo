using Octokit;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;

namespace DemoServer.Services.Services;

public class GitHubCommitService : IGitHubCommitService
{
    private readonly IGitHubOAuthService _oauthService;

    public GitHubCommitService(IGitHubOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    public async Task<IReadOnlyList<GitHubCommit>> GetRepositoryCommitsAsync(
        string owner,
        string name,
        string? sha = null,
        string? path = null,
        string? author = null,
        DateTimeOffset? since = null,
        DateTimeOffset? until = null,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var request = new CommitRequest
        {
            Sha = sha,
            Path = path,
            Author = author,
            Since = since,
            Until = until
        };

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100), // GitHub 限制最大100
            PageCount = 1,
            StartPage = page
        };

        return await client.Repository.Commit.GetAll(owner, name, request, options);
    }

    public async Task<GitHubCommit> GetCommitAsync(string owner, string name, string sha)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Commit.Get(owner, name, sha);
    }

    public async Task<Commit> CreateCommitAsync(
        string owner,
        string name,
        string message,
        string tree,
        IEnumerable<string> parents,
        Signature? author = null,
        Signature? committer = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var newCommit = new NewCommit(message, tree, parents);

        // Octokit 使用 Committer 类型而不是 Signature
        if (author != null)
            newCommit.Author = new Committer(author.Name, author.Email, DateTimeOffset.Now);
            
        if (committer != null)
            newCommit.Committer = new Committer(committer.Name, committer.Email, DateTimeOffset.Now);

        return await client.Git.Commit.Create(owner, name, newCommit);
    }

    public async Task<CompareResult> CompareCommitsAsync(string owner, string name, string @base, string head)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Commit.Compare(owner, name, @base, head);
    }

    public async Task<IReadOnlyList<CommitStatus>> GetCommitStatusesAsync(string owner, string name, string sha)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Status.GetAll(owner, name, sha);
    }

    public async Task<CommitStatus> CreateCommitStatusAsync(
        string owner,
        string name,
        string sha,
        CommitState state,
        string? targetUrl = null,
        string? description = null,
        string? context = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var newCommitStatus = new NewCommitStatus
        {
            State = state,
            TargetUrl = targetUrl,
            Description = description,
            Context = context
        };

        return await client.Repository.Status.Create(owner, name, sha, newCommitStatus);
    }

    public async Task<IReadOnlyList<CommitComment>> GetCommitCommentsAsync(string owner, string name, string sha)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Comment.GetAllForCommit(owner, name, sha);
    }

    public async Task<CommitComment> CreateCommitCommentAsync(
        string owner,
        string name,
        string sha,
        string body,
        string? path = null,
        int? line = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        var newCommitComment = new NewCommitComment(body)
        {
            Path = path
        };

        return await client.Repository.Comment.Create(owner, name, sha, newCommitComment);
    }

    public async Task<CommitComment> UpdateCommitCommentAsync(string owner, string name, long commentId, string body)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        return await client.Repository.Comment.Update(owner, name, commentId, body);
    }

    public async Task DeleteCommitCommentAsync(string owner, string name, long commentId)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        await client.Repository.Comment.Delete(owner, name, commentId);
    }

    public async Task<IReadOnlyList<CommitComment>> GetRepositoryCommitCommentsAsync(
        string owner,
        string name,
        DateTimeOffset? since = null,
        int pageSize = 30,
        int page = 1)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();

        var options = new ApiOptions
        {
            PageSize = Math.Min(pageSize, 100),
            PageCount = 1,
            StartPage = page
        };

        return await client.Repository.Comment.GetAllForRepository(owner, name, options);
    }

    public async Task<GitHubCommit> CommitFilesAsync(
        string owner, 
        string repo, 
        Dictionary<string, string> fileChanges, 
        string commitMessage, 
        string branch = "main",
        CommitAuthor? author = null,
        CommitAuthor? committer = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        try
        {
            // 1. 获取当前分支的最新 commit
            var latestCommit = await client.Repository.Commit.Get(owner, repo, branch);
            var baseTreeSha = latestCommit.Commit.Tree.Sha;
            
            // 2. 创建新的 Tree
            var newTree = new NewTree { BaseTree = baseTreeSha };
            
            // 3. 为每个文件变更创建 blob 并添加到 tree
            foreach (var fileChange in fileChanges)
            {
                // 创建 blob
                var blob = await client.Git.Blob.Create(owner, repo, new NewBlob
                {
                    Content = fileChange.Value,
                    Encoding = EncodingType.Utf8
                });
                
                // 添加到 tree
                newTree.Tree.Add(new NewTreeItem
                {
                    Path = fileChange.Key,
                    Mode = "100644", // 普通文件模式
                    Type = TreeType.Blob,
                    Sha = blob.Sha
                });
            }
            
            // 4. 创建新的 tree
            var createdTree = await client.Git.Tree.Create(owner, repo, newTree);
            
            // 5. 创建 commit
            var newCommit = new NewCommit(commitMessage, createdTree.Sha, latestCommit.Sha);
            
            // 设置作者信息
            if (author != null)
            {
                newCommit.Author = new Committer(
                    author.Name, 
                    author.Email, 
                    author.Date ?? DateTimeOffset.Now
                );
            }
            
            // 设置提交者信息
            if (committer != null)
            {
                newCommit.Committer = new Committer(
                    committer.Name, 
                    committer.Email, 
                    committer.Date ?? DateTimeOffset.Now
                );
            }
            
            // 6. 创建 commit
            var createdCommit = await client.Git.Commit.Create(owner, repo, newCommit);
            
            // 7. 更新分支引用指向新的 commit
            await client.Git.Reference.Update(owner, repo, $"heads/{branch}", new ReferenceUpdate(createdCommit.Sha));
            
            // 8. 获取完整的 commit 信息并返回
            return await client.Repository.Commit.Get(owner, repo, createdCommit.Sha);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"提交文件失败: {ex.Message}", ex);
        }
    }

    public async Task<GitHubCommit> CommitSingleFileAsync(
        string owner,
        string repo,
        string filePath,
        string fileContent,
        string commitMessage,
        string branch = "main",
        CommitAuthor? author = null,
        CommitAuthor? committer = null)
    {
        var fileChanges = new Dictionary<string, string> { { filePath, fileContent } };
        return await CommitFilesAsync(owner, repo, fileChanges, commitMessage, branch, author, committer);
    }

    public async Task<GitHubCommit> DeleteFilesAsync(
        string owner,
        string repo,
        List<string> filePaths,
        string commitMessage,
        string branch = "main",
        CommitAuthor? author = null,
        CommitAuthor? committer = null)
    {
        var client = _oauthService.CreateClientFromAccessTokenAsync();
        
        try
        {
            // 1. 获取当前分支的最新 commit
            var latestCommit = await client.Repository.Commit.Get(owner, repo, branch);
            var baseTreeSha = latestCommit.Commit.Tree.Sha;
            
            // 2. 获取当前 tree 的内容
            var currentTree = await client.Git.Tree.Get(owner, repo, baseTreeSha);
            
            // 3. 创建新的 tree，排除要删除的文件
            var newTree = new NewTree();
            
            foreach (var item in currentTree.Tree)
            {
                if (!filePaths.Contains(item.Path))
                {
                    newTree.Tree.Add(new NewTreeItem
                    {
                        Path = item.Path,
                        Mode = item.Mode,
                        Type = item.Type.Value,
                        Sha = item.Sha
                    });
                }
            }
            
            // 4. 创建新的 tree
            var createdTree = await client.Git.Tree.Create(owner, repo, newTree);
            
            // 5. 创建 commit
            var newCommit = new NewCommit(commitMessage, createdTree.Sha, latestCommit.Sha);
            
            // 设置作者和提交者信息
            if (author != null)
            {
                newCommit.Author = new Committer(author.Name, author.Email, author.Date ?? DateTimeOffset.Now);
            }
            if (committer != null)
            {
                newCommit.Committer = new Committer(committer.Name, committer.Email, committer.Date ?? DateTimeOffset.Now);
            }
            
            // 6. 创建 commit
            var createdCommit = await client.Git.Commit.Create(owner, repo, newCommit);
            
            // 7. 更新分支引用
            await client.Git.Reference.Update(owner, repo, $"heads/{branch}", new ReferenceUpdate(createdCommit.Sha));
            
            // 8. 获取完整的 commit 信息并返回
            return await client.Repository.Commit.Get(owner, repo, createdCommit.Sha);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"删除文件失败: {ex.Message}", ex);
        }
    }
}
