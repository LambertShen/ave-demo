using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Options;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;
using DemoServer.Services.Options;

namespace DemoServer.Services.Services;

public class GitHubProjectService : IGitHubProjectService
{
    private readonly GitHubOptions _options;
    private const string GraphQLEndpoint = "https://api.github.com/graphql";

    public GitHubProjectService(IOptions<GitHubOptions> options)
    {
        _options = options.Value;
    }

    private GraphQLHttpClient CreateGraphQLClient()
    {
        if (string.IsNullOrEmpty(_options.AccessToken))
        {
            throw new InvalidOperationException("GitHub AccessToken is not configured");
        }

        var client = new GraphQLHttpClient(GraphQLEndpoint, new NewtonsoftJsonSerializer());
        client.HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);
        // client.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{_options.AppName}/1.0");
        return client;
    }

    public async Task<(List<GitHubProject> Projects, bool HasNextPage, string? EndCursor)> GetProjectsAsync(
        int first = 20, 
        string? after = null)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetProjects($first: Int!, $after: String) {
                    viewer {
                        login
                        projectsV2(first: $first, after: $after) {
                            totalCount
                            nodes {
                                id
                                title
                                url
                                closed
                                createdAt
                                updatedAt
                                number
                            }
                            pageInfo {
                                hasNextPage
                                endCursor
                            }
                        }
                    }
                }",
            Variables = new { first, after }
        };

        // 使用 dynamic 类型直接处理响应，避免复杂的类型映射问题
        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        // 直接从 dynamic 响应中提取数据
        var viewer = response.Data?.viewer;
        if (viewer == null)
        {
            return (new List<GitHubProject>(), false, null);
        }

        var projectsV2 = viewer.projectsV2;
        if (projectsV2 == null)
        {
            return (new List<GitHubProject>(), false, null);
        }

        // 添加调试信息
        var totalCount = (int)(projectsV2.totalCount ?? 0);
        var viewerLogin = viewer.login?.ToString() ?? "unknown";
        
        System.Diagnostics.Debug.WriteLine($"User {viewerLogin} has {totalCount} projects");

        var projects = new List<GitHubProject>();
        var nodes = projectsV2.nodes;
        
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var project = new GitHubProject
                {
                    Id = node.id?.ToString() ?? "",
                    Title = node.title?.ToString() ?? "",
                    Description = "", // 这个查询中没有 description 字段
                    Url = node.url?.ToString() ?? "",
                    State = (bool)(node.closed ?? false) ? "CLOSED" : "OPEN",
                    CreatedAt = DateTime.TryParse(node.createdAt?.ToString(), out DateTime created) ? created : DateTime.MinValue,
                    UpdatedAt = DateTime.TryParse(node.updatedAt?.ToString(), out DateTime updated) ? updated : DateTime.MinValue,
                    Public = false, // 这个查询中没有 public 字段
                    Number = (int)(node.number ?? 0),
                    Owner = viewerLogin,
                    OwnerType = "User"
                };
                
                projects.Add(project);
            }
        }

        var hasNextPage = (bool)(projectsV2.pageInfo?.hasNextPage ?? false);
        var endCursor = projectsV2.pageInfo?.endCursor?.ToString();

        return (projects, hasNextPage, endCursor);
    }

    // 添加一个测试方法来检查用户信息和权限
    public async Task<(string Login, int ProjectCount, bool HasProjectAccess)> GetUserInfoAsync()
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetUserInfo {
                    viewer {
                        login
                        projectsV2(first: 1) {
                            totalCount
                        }
                    }
                }"
        };

        try
        {
            var response = await client.SendQueryAsync<dynamic>(query);

            if (response.Errors?.Any() == true)
            {
                var errorMessages = string.Join(", ", response.Errors.Select(e => e.Message));
                return ("unknown", 0, false);
            }

            var login = response.Data?.viewer?.login?.ToString() ?? "unknown";
            var totalCount = response.Data?.viewer?.projectsV2?.totalCount ?? 0;
            
            return (login, totalCount, true);
        }
        catch (Exception)
        {
            return ("unknown", 0, false);
        }
    }

    public async Task<(List<GitHubProject> Projects, bool HasNextPage, string? EndCursor)> GetOrganizationProjectsAsync(
        string organizationLogin, 
        int first = 20, 
        string? after = null)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetOrganizationProjects($login: String!, $first: Int!, $after: String) {
                    organization(login: $login) {
                        projectsV2(first: $first, after: $after) {
                            nodes {
                                id
                                title
                                shortDescription
                                url
                                closed
                                createdAt
                                updatedAt
                                public
                                number
                                owner {
                                    ... on User {
                                        login
                                    }
                                    ... on Organization {
                                        login
                                    }
                                }
                            }
                            pageInfo {
                                hasNextPage
                                endCursor
                            }
                        }
                    }
                }",
            Variables = new { login = organizationLogin, first, after }
        };

        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var organization = response.Data?.organization;
        if (organization?.projectsV2 == null)
        {
            return (new List<GitHubProject>(), false, null);
        }

        var projects = new List<GitHubProject>();
        var nodes = organization.projectsV2.nodes;
        
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var project = new GitHubProject
                {
                    Id = node.id?.ToString() ?? "",
                    Title = node.title?.ToString() ?? "",
                    Description = node.shortDescription?.ToString() ?? "",
                    Url = node.url?.ToString() ?? "",
                    State = (bool)(node.closed ?? false) ? "CLOSED" : "OPEN",
                    CreatedAt = DateTime.TryParse(node.createdAt?.ToString(), out DateTime created) ? created : DateTime.MinValue,
                    UpdatedAt = DateTime.TryParse(node.updatedAt?.ToString(), out DateTime updated) ? updated : DateTime.MinValue,
                    Public = (bool)(node.@public ?? false),
                    Number = (int)(node.number ?? 0),
                    Owner = organizationLogin,
                    OwnerType = "Organization"
                };
                
                projects.Add(project);
            }
        }

        var hasNextPage = (bool)(organization.projectsV2.pageInfo?.hasNextPage ?? false);
        var endCursor = organization.projectsV2.pageInfo?.endCursor?.ToString();

        return (projects, hasNextPage, endCursor);
    }

    public async Task<(List<GitHubProject> Projects, bool HasNextPage, string? EndCursor)> GetUserProjectsAsync(
        string userLogin, 
        int first = 20, 
        string? after = null)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetUserProjects($login: String!, $first: Int!, $after: String) {
                    user(login: $login) {
                        projectsV2(first: $first, after: $after) {
                            nodes {
                                id
                                title
                                shortDescription
                                url
                                closed
                                createdAt
                                updatedAt
                                public
                                number
                                owner {
                                    ... on User {
                                        login
                                    }
                                    ... on Organization {
                                        login
                                    }
                                }
                            }
                            pageInfo {
                                hasNextPage
                                endCursor
                            }
                        }
                    }
                }",
            Variables = new { login = userLogin, first, after }
        };

        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var user = response.Data?.user;
        if (user?.projectsV2 == null)
        {
            return (new List<GitHubProject>(), false, null);
        }

        var projects = new List<GitHubProject>();
        var nodes = user.projectsV2.nodes;
        
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var project = new GitHubProject
                {
                    Id = node.id?.ToString() ?? "",
                    Title = node.title?.ToString() ?? "",
                    Description = node.shortDescription?.ToString() ?? "",
                    Url = node.url?.ToString() ?? "",
                    State = (bool)(node.closed ?? false) ? "CLOSED" : "OPEN",
                    CreatedAt = DateTime.TryParse(node.createdAt?.ToString(), out DateTime created) ? created : DateTime.MinValue,
                    UpdatedAt = DateTime.TryParse(node.updatedAt?.ToString(), out DateTime updated) ? updated : DateTime.MinValue,
                    Public = (bool)(node.@public ?? false),
                    Number = (int)(node.number ?? 0),
                    Owner = userLogin,
                    OwnerType = "User"
                };
                
                projects.Add(project);
            }
        }

        var hasNextPage = (bool)(user.projectsV2.pageInfo?.hasNextPage ?? false);
        var endCursor = user.projectsV2.pageInfo?.endCursor?.ToString();

        return (projects, hasNextPage, endCursor);
    }

    public async Task<GitHubProject?> GetProjectByIdAsync(string projectId)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetProject($id: ID!) {
                    node(id: $id) {
                        ... on ProjectV2 {
                            id
                            title
                            shortDescription
                            url
                            closed
                            createdAt
                            updatedAt
                            public
                            number
                            owner {
                                ... on User {
                                    login
                                }
                                ... on Organization {
                                    login
                                }
                            }
                        }
                    }
                }",
            Variables = new { id = projectId }
        };

        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var projectNode = response.Data?.node;
        if (projectNode == null) return null;

        return new GitHubProject
        {
            Id = projectNode.id,
            Title = projectNode.title,
            Description = projectNode.shortDescription,
            Url = projectNode.url,
            State = projectNode.closed ? "CLOSED" : "OPEN",
            CreatedAt = projectNode.createdAt,
            UpdatedAt = projectNode.updatedAt,
            Public = projectNode.@public,
            Number = projectNode.number,
            Owner = projectNode.owner?.login,
            OwnerType = projectNode.owner != null ? "User" : null
        };
    }

    public async Task<GitHubProject?> CreateProjectAsync(CreateProjectRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation CreateProject($input: CreateProjectV2Input!) {
                    createProjectV2(input: $input) {
                        projectV2 {
                            id
                            title
                            url
                            closed
                            createdAt
                            updatedAt
                            number
                            owner {
                                ... on User {
                                    login
                                }
                                ... on Organization {
                                    login
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    ownerId = request.OwnerId,
                    title = request.Title
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var projectNode = response.Data?.createProjectV2?.projectV2;
        if (projectNode == null) return null;

        return new GitHubProject
        {
            Id = projectNode.id?.ToString() ?? "",
            Title = projectNode.title?.ToString() ?? "",
            Description = "", // 创建时没有描述字段
            Url = projectNode.url?.ToString() ?? "",
            State = (bool)(projectNode.closed ?? false) ? "CLOSED" : "OPEN",
            CreatedAt = DateTime.TryParse(projectNode.createdAt?.ToString(), out DateTime created) ? created : DateTime.MinValue,
            UpdatedAt = DateTime.TryParse(projectNode.updatedAt?.ToString(), out DateTime updated) ? updated : DateTime.MinValue,
            Public = false, // 创建时没有public字段
            Number = (int)(projectNode.number ?? 0),
            Owner = projectNode.owner?.login?.ToString() ?? "",
            OwnerType = "User"
        };
    }

    public async Task<GitHubProject?> UpdateProjectAsync(UpdateProjectRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation UpdateProject($input: UpdateProjectV2Input!) {
                    updateProjectV2(input: $input) {
                        projectV2 {
                            id
                            title
                            url
                            closed
                            createdAt
                            updatedAt
                            number
                            owner {
                                ... on User {
                                    login
                                }
                                ... on Organization {
                                    login
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    projectId = request.ProjectId,
                    title = request.Title,
                    closed = request.State == "CLOSED"
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var projectNode = response.Data?.updateProjectV2?.projectV2;
        if (projectNode == null) return null;

        return new GitHubProject
        {
            Id = projectNode.id?.ToString() ?? "",
            Title = projectNode.title?.ToString() ?? "",
            Description = "", // 更新时没有描述字段
            Url = projectNode.url?.ToString() ?? "",
            State = (bool)(projectNode.closed ?? false) ? "CLOSED" : "OPEN",
            CreatedAt = DateTime.TryParse(projectNode.createdAt?.ToString(), out DateTime created) ? created : DateTime.MinValue,
            UpdatedAt = DateTime.TryParse(projectNode.updatedAt?.ToString(), out DateTime updated) ? updated : DateTime.MinValue,
            Public = false, // 更新时没有public字段
            Number = (int)(projectNode.number ?? 0),
            Owner = projectNode.owner?.login?.ToString() ?? "",
            OwnerType = "User"
        };
    }

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation DeleteProject($input: DeleteProjectV2Input!) {
                    deleteProjectV2(input: $input) {
                        projectV2 {
                            id
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    projectId
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data?.deleteProjectV2?.projectV2?.id != null;
    }

    public async Task<ProjectOwner?> GetOwnerAsync(string login)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetOwner($login: String!) {
                    repositoryOwner(login: $login) {
                        id
                        login
                        __typename
                    }
                }",
            Variables = new { login }
        };

        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var owner = response.Data?.repositoryOwner;
        if (owner == null) return null;

        return new ProjectOwner
        {
            Id = owner.id,
            Login = owner.login,
            Type = owner.__typename
        };
    }

    public async Task<List<ProjectField>> GetProjectFieldsAsync(string projectId)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetProjectFields($id: ID!) {
                    node(id: $id) {
                        ... on ProjectV2 {
                            fields(first: 100) {
                                nodes {
                                    ... on ProjectV2Field {
                                        id
                                        name
                                        dataType
                                    }
                                    ... on ProjectV2IterationField {
                                        id
                                        name
                                        dataType
                                    }
                                    ... on ProjectV2SingleSelectField {
                                        id
                                        name
                                        dataType
                                        options {
                                            id
                                            name
                                            color
                                        }
                                    }
                                }
                            }
                        }
                    }
                }",
            Variables = new { id = projectId }
        };

        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var fields = new List<ProjectField>();
        var fieldNodes = response.Data?.node?.fields?.nodes;

        if (fieldNodes != null)
        {
            foreach (var fieldNode in fieldNodes)
            {
                var field = new ProjectField
                {
                    Id = fieldNode.id,
                    Name = fieldNode.name,
                    DataType = fieldNode.dataType
                };

                if (fieldNode.options != null)
                {
                    field.Settings = new ProjectFieldSettings
                    {
                        Options = new List<ProjectFieldOption>()
                    };

                    foreach (var option in fieldNode.options)
                    {
                        field.Settings.Options.Add(new ProjectFieldOption
                        {
                            Id = option.id,
                            Name = option.name,
                            Color = option.color
                        });
                    }
                }

                fields.Add(field);
            }
        }

        return fields;
    }

    public async Task<(List<ProjectItem> Items, bool HasNextPage, string? EndCursor)> GetProjectItemsAsync(
        string projectId, 
        int first = 20, 
        string? after = null)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetProjectItems($id: ID!, $first: Int!, $after: String) {
                    node(id: $id) {
                        ... on ProjectV2 {
                            items(first: $first, after: $after) {
                                nodes {
                                    id
                                    createdAt
                                    updatedAt
                                    content {
                                        ... on Issue {
                                            id
                                            title
                                            body
                                            state
                                            url
                                        }
                                        ... on PullRequest {
                                            id
                                            title
                                            body
                                            state
                                            url
                                        }
                                        ... on DraftIssue {
                                            id
                                            title
                                            body
                                        }
                                    }
                                }
                                pageInfo {
                                    hasNextPage
                                    endCursor
                                }
                            }
                        }
                    }
                }",
            Variables = new { id = projectId, first, after }
        };

        var response = await client.SendQueryAsync<dynamic>(query);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var items = new List<ProjectItem>();
        var itemNodes = response.Data?.node?.items?.nodes;

        if (itemNodes != null)
        {
            foreach (var itemNode in itemNodes)
            {
                var item = new ProjectItem
                {
                    Id = itemNode.id,
                    ProjectId = projectId,
                    CreatedAt = itemNode.createdAt,
                    UpdatedAt = itemNode.updatedAt
                };

                var content = itemNode.content;
                if (content != null)
                {
                    item.ContentId = content.id;
                    item.Title = content.title;
                    item.Body = content.body;
                    item.State = content.state;
                    item.Url = content.url;
                    
                    // Determine content type based on available fields
                    if (content.url != null && content.url.ToString().Contains("/pull/"))
                    {
                        item.ContentType = "PullRequest";
                    }
                    else if (content.url != null && content.url.ToString().Contains("/issues/"))
                    {
                        item.ContentType = "Issue";
                    }
                    else
                    {
                        item.ContentType = "DraftIssue";
                    }
                }

                items.Add(item);
            }
        }

        var hasNextPage = response.Data?.node?.items?.pageInfo?.hasNextPage ?? false;
        var endCursor = response.Data?.node?.items?.pageInfo?.endCursor;

        return (items, hasNextPage, endCursor);
    }

    public async Task<ProjectItem?> AddProjectItemAsync(AddProjectItemRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation AddProjectItem($input: AddProjectV2ItemByIdInput!) {
                    addProjectV2ItemById(input: $input) {
                        item {
                            id
                            createdAt
                            updatedAt
                            content {
                                ... on Issue {
                                    id
                                    title
                                    body
                                    state
                                    url
                                }
                                ... on PullRequest {
                                    id
                                    title
                                    body
                                    state
                                    url
                                }
                                ... on DraftIssue {
                                    id
                                    title
                                    body
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    projectId = request.ProjectId,
                    contentId = request.ContentId
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var itemNode = response.Data?.addProjectV2ItemById?.item;
        if (itemNode == null) return null;

        var item = new ProjectItem
        {
            Id = itemNode.id,
            ProjectId = request.ProjectId,
            CreatedAt = itemNode.createdAt,
            UpdatedAt = itemNode.updatedAt
        };

        var content = itemNode.content;
        if (content != null)
        {
            item.ContentId = content.id;
            item.Title = content.title;
            item.Body = content.body;
            item.State = content.state;
            item.Url = content.url;
        }

        return item;
    }

    public Task<ProjectItem?> UpdateProjectItemAsync(UpdateProjectItemRequest request)
    {
        // Note: Updating project item field values requires specific field IDs and is more complex
        // This is a simplified implementation that would need to be expanded based on specific field types
        throw new NotImplementedException("UpdateProjectItemAsync requires field-specific implementation based on your project structure");
    }

    public async Task<bool> DeleteProjectItemAsync(string projectId, string itemId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation DeleteProjectItem($input: DeleteProjectV2ItemInput!) {
                    deleteProjectV2Item(input: $input) {
                        deletedItemId
                    }
                }",
            Variables = new
            {
                input = new
                {
                    projectId,
                    itemId
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);

        if (response.Errors?.Any() == true)
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data?.deleteProjectV2Item?.deletedItemId != null;
    }

    private static GitHubProject ConvertToGitHubProject(ProjectNode node)
    {
        return new GitHubProject
        {
            Id = node.Id,
            Title = node.Title,
            Description = node.Description ?? "",
            Url = node.Url,
            State = node.State,
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt,
            Public = node.Public,
            Number = node.Number,
            Owner = node.Owner?.Login ?? "",
            OwnerType = node.Owner?.Type ?? ""
        };
    }
}
