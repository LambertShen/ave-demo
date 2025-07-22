namespace DemoServer.Services.Models;

public class CommitAuthor
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset? Date { get; set; }
}

public class FileChange
{
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public FileChangeType Type { get; set; } = FileChangeType.Modified;
}

public enum FileChangeType
{
    Added,
    Modified,
    Deleted
}

public class CommitFilesRequest
{
    public Dictionary<string, string> FileChanges { get; set; } = new();
    public List<string> FilesToDelete { get; set; } = new();
    public string CommitMessage { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public CommitAuthor? Author { get; set; }
    public CommitAuthor? Committer { get; set; }
}
