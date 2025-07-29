using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Options;
using DemoServer.Services.Services;

namespace DemoServer.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 GitHub OAuth 服务到依赖注入容器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddGitHubOAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置 GitHub 选项
        services.Configure<GitHubOptions>(configuration.GetSection(GitHubOptions.SectionName));
        
        // 注册 GitHub OAuth 服务
        services.AddScoped<IGitHubOAuthService, GitHubOAuthService>();
        
        // 注册 GitHub Issues 服务
        services.AddScoped<IGitHubIssuesService, GitHubIssuesService>();
        
        // 注册 GitHub Pull Request 服务
        services.AddScoped<IGitHubPullRequestService, GitHubPullRequestService>();
        
        // 注册 GitHub Commit 服务
        services.AddScoped<IGitHubCommitService, GitHubCommitService>();
        
        // 注册 GitHub Timeline 服务
        services.AddScoped<IGitHubTimeLineService, GitHubTimeLineService>();
        
        // 注册 GitHub Project 服务
        services.AddScoped<IGitHubProjectService, GitHubProjectService>();
        
        // 注册 GitHub Repository 服务
        services.AddScoped<IGitHubRepositoryService, GitHubRepositoryService>();
        
        return services;
    }
}
