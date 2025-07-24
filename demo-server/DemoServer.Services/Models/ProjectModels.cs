using Newtonsoft.Json;

namespace DemoServer.Services.Models;

public class GitHubProject
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? Owner { get; set; }
    public string? OwnerType { get; set; }
    public bool Public { get; set; }
    public string? Template { get; set; }
    public int Number { get; set; }
}

public class ProjectItem
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string? ContentId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? State { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Dictionary<string, object> FieldValues { get; set; } = new();
}

public class ProjectField
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public ProjectFieldSettings? Settings { get; set; }
}

public class ProjectFieldSettings
{
    public List<ProjectFieldOption>? Options { get; set; }
    public bool? Required { get; set; }
    public string? Format { get; set; }
}

public class ProjectFieldOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class CreateProjectRequest
{
    public string OwnerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Public { get; set; } = false;
    public string? Template { get; set; }
}

public class UpdateProjectRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? Public { get; set; }
    public string? State { get; set; }
}

public class AddProjectItemRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
}

public class UpdateProjectItemRequest
{
    public string ProjectId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public Dictionary<string, object> FieldValues { get; set; } = new();
}

public class ProjectOwner
{
    public string Id { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

// GraphQL Response Models
public class GraphQLResponse<T>
{
    [JsonProperty("data")]
    public T? Data { get; set; }
    
    [JsonProperty("errors")]
    public List<GraphQLError>? Errors { get; set; }
}

public class GraphQLError
{
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonProperty("path")]
    public List<object>? Path { get; set; }
}

public class ProjectsResponse
{
    [JsonProperty("viewer")]
    public ViewerProjects? Viewer { get; set; }
    
    [JsonProperty("organization")]
    public OrganizationProjects? Organization { get; set; }
    
    [JsonProperty("user")]
    public UserProjects? User { get; set; }
}

public class ViewerProjects
{
    [JsonProperty("login")]
    public string Login { get; set; } = string.Empty;
    
    [JsonProperty("projectsV2")]
    public ProjectConnection ProjectsV2 { get; set; } = new();
}

public class OrganizationProjects
{
    [JsonProperty("projectsV2")]
    public ProjectConnection ProjectsV2 { get; set; } = new();
}

public class UserProjects
{
    [JsonProperty("projectsV2")]
    public ProjectConnection ProjectsV2 { get; set; } = new();
}

public class ProjectConnection
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }
    
    [JsonProperty("nodes")]
    public List<ProjectNode> Nodes { get; set; } = new();
    
    [JsonProperty("pageInfo")]
    public PageInfo PageInfo { get; set; } = new();
}

public class ProjectNode
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonProperty("shortDescription")]
    public string? Description { get; set; }
    
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonProperty("closed")]
    public bool Closed { get; set; }
    
    public string State => Closed ? "CLOSED" : "OPEN";
    
    [JsonProperty("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
    
    [JsonProperty("public")]
    public bool Public { get; set; }
    
    [JsonProperty("number")]
    public int Number { get; set; }
    
    [JsonProperty("owner")]
    public OwnerNode? Owner { get; set; }
}

public class OwnerNode
{
    [JsonProperty("login")]
    public string Login { get; set; } = string.Empty;
    
    public string Type { get; set; } = "User"; // Default to User, can be User or Organization
}

public class PageInfo
{
    [JsonProperty("hasNextPage")]
    public bool HasNextPage { get; set; }
    
    [JsonProperty("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }
    
    [JsonProperty("startCursor")]
    public string? StartCursor { get; set; }
    
    [JsonProperty("endCursor")]
    public string? EndCursor { get; set; }
}

public class CreateProjectResponse
{
    [JsonProperty("createProjectV2")]
    public CreateProjectPayload CreateProjectV2 { get; set; } = new();
}

public class CreateProjectPayload
{
    [JsonProperty("projectV2")]
    public ProjectNode? ProjectV2 { get; set; }
}

public class UpdateProjectResponse
{
    [JsonProperty("updateProjectV2")]
    public UpdateProjectPayload UpdateProjectV2 { get; set; } = new();
}

public class UpdateProjectPayload
{
    [JsonProperty("projectV2")]
    public ProjectNode? ProjectV2 { get; set; }
}

public class DeleteProjectResponse
{
    [JsonProperty("deleteProjectV2")]
    public DeleteProjectPayload DeleteProjectV2 { get; set; } = new();
}

public class DeleteProjectPayload
{
    [JsonProperty("projectV2")]
    public ProjectNode? ProjectV2 { get; set; }
}
