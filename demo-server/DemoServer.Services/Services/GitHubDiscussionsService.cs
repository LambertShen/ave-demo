using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Options;
using DemoServer.Services.Interfaces;
using DemoServer.Services.Models;
using DemoServer.Services.Options;

namespace DemoServer.Services.Services;

public class GitHubDiscussionsService : IGitHubDiscussionsService
{
    private readonly GitHubOptions _options;
    private const string GraphQLEndpoint = "https://api.github.com/graphql";

    public GitHubDiscussionsService(IOptions<GitHubOptions> options)
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
        return client;
    }

    #region Discussions CRUD

    public async Task<DiscussionsResponse> GetDiscussionsAsync(
        string owner,
        string name,
        int first = 20,
        string? after = null,
        string? categoryId = null,
        string orderBy = "UPDATED_AT",
        string direction = "DESC")
    {
        using var client = CreateGraphQLClient();

        var categoryFilter = !string.IsNullOrEmpty(categoryId) ? $", categoryId: \"{categoryId}\"" : "";

        var query = new GraphQLRequest
        {
            Query = $@"
                query GetDiscussions($owner: String!, $name: String!, $first: Int!, $after: String) {{
                    repository(owner: $owner, name: $name) {{
                        discussions(first: $first, after: $after, orderBy: {{field: {orderBy}, direction: {direction}}}{categoryFilter}) {{
                            totalCount
                            pageInfo {{
                                hasNextPage
                                endCursor
                            }}
                            nodes {{
                                id
                                title
                                body
                                url
                                number
                                locked
                                createdAt
                                updatedAt
                                upvoteCount
                                viewerHasUpvoted
                                comments {{
                                    totalCount
                                }}
                                category {{
                                    id
                                    name
                                    description
                                    emoji
                                    emojiHTML
                                    slug
                                    isAnswerable
                                    createdAt
                                    updatedAt
                                }}
                                author {{
                                    login
                                    ... on User {{
                                        id
                                        avatarUrl
                                        url
                                    }}
                                    ... on Organization {{
                                        id
                                        avatarUrl
                                        url
                                    }}
                                }}
                                labels(first: 10) {{
                                    nodes {{
                                        id
                                        name
                                        color
                                        description
                                        url
                                    }}
                                }}
                                answer {{
                                    id
                                    body
                                    bodyHTML
                                    url
                                    createdAt
                                    updatedAt
                                    upvoteCount
                                    viewerHasUpvoted
                                    author {{
                                        login
                                        ... on User {{
                                            id
                                            avatarUrl
                                            url
                                        }}
                                    }}
                                }}
                                answerChosenAt
                            }}
                        }}
                    }}
                }}",
            Variables = new
            {
                owner,
                name,
                first,
                after
            }
        };

        var response = await client.SendQueryAsync<dynamic>(query);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var discussions = new List<GitHubDiscussion>();
        var discussionsData = response.Data.repository.discussions;

        foreach (var node in discussionsData.nodes)
        {
            var discussion = new GitHubDiscussion
            {
                Id = node.id,
                Title = node.title,
                Body = node.body ?? "",
                Url = node.url,
                Number = node.number,
                Locked = node.locked,
                CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
                UpvoteCount = node.upvoteCount,
                ViewerHasUpvoted = node.viewerHasUpvoted,
                CommentsTotalCount = node.comments.totalCount,
                AnswerChosenAt = node.answerChosenAt != null
            };

            if (node.category != null)
            {
                discussion.Category = new DiscussionCategory
                {
                    Id = node.category.id,
                    Name = node.category.name,
                    Description = node.category.description ?? "",
                    Emoji = node.category.emoji ?? "",
                    EmojiHTML = node.category.emojiHTML ?? "",
                    Slug = node.category.slug,
                    IsAnswerable = node.category.isAnswerable,
                    CreatedAt = DateTimeOffset.Parse(node.category.createdAt.ToString()),
                    UpdatedAt = DateTimeOffset.Parse(node.category.updatedAt.ToString())
                };
                discussion.CategoryId = discussion.Category.Id;
            }

            if (node.author != null)
            {
                discussion.Author = new DiscussionAuthor
                {
                    Login = node.author.login,
                    Id = node.author.id ?? "",
                    AvatarUrl = node.author.avatarUrl ?? "",
                    Url = node.author.url ?? "",
                    Type = "User" // GraphQL fragment determines this
                };
            }

            if (node.labels?.nodes != null)
            {
                foreach (var label in node.labels.nodes)
                {
                    discussion.Labels.Add(new DiscussionLabel
                    {
                        Id = label.id,
                        Name = label.name,
                        Color = label.color,
                        Description = label.description ?? "",
                        Url = label.url
                    });
                }
            }

            if (node.answer != null)
            {
                discussion.Answer = new DiscussionComment
                {
                    Id = node.answer.id,
                    Body = node.answer.body,
                    BodyHTML = node.answer.bodyHTML,
                    Url = node.answer.url,
                    CreatedAt = DateTimeOffset.Parse(node.answer.createdAt.ToString()),
                    UpdatedAt = DateTimeOffset.Parse(node.answer.updatedAt.ToString()),
                    UpvoteCount = node.answer.upvoteCount,
                    ViewerHasUpvoted = node.answer.viewerHasUpvoted,
                    IsAnswer = true
                };

                if (node.answer.author != null)
                {
                    discussion.Answer.Author = new DiscussionAuthor
                    {
                        Login = node.answer.author.login,
                        Id = node.answer.author.id ?? "",
                        AvatarUrl = node.answer.author.avatarUrl ?? "",
                        Url = node.answer.author.url ?? "",
                        Type = "User"
                    };
                }
            }

            discussions.Add(discussion);
        }

        return new DiscussionsResponse
        {
            Discussions = discussions,
            HasNextPage = discussionsData.pageInfo.hasNextPage,
            EndCursor = discussionsData.pageInfo.endCursor,
            TotalCount = discussionsData.totalCount
        };
    }

    public async Task<GitHubDiscussion?> GetDiscussionByIdAsync(string discussionId)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetDiscussion($discussionId: ID!) {
                    node(id: $discussionId) {
                        ... on Discussion {
                            id
                            title
                            body
                            url
                            number
                            locked
                            createdAt
                            updatedAt
                            upvoteCount
                            viewerHasUpvoted
                            comments {
                                totalCount
                            }
                            category {
                                id
                                name
                                description
                                emoji
                                emojiHTML
                                slug
                                isAnswerable
                                createdAt
                                updatedAt
                            }
                            author {
                                login
                                ... on User {
                                    id
                                    avatarUrl
                                    url
                                }
                                ... on Organization {
                                    id
                                    avatarUrl
                                    url
                                }
                            }
                            labels(first: 10) {
                                nodes {
                                    id
                                    name
                                    color
                                    description
                                    url
                                }
                            }
                            answer {
                                id
                                body
                                bodyHTML
                                url
                                createdAt
                                updatedAt
                                upvoteCount
                                viewerHasUpvoted
                                author {
                                    login
                                    ... on User {
                                        id
                                        avatarUrl
                                        url
                                    }
                                }
                            }
                            answerChosenAt
                        }
                    }
                }",
            Variables = new { discussionId }
        };

        var response = await client.SendQueryAsync<dynamic>(query);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.node;
        if (node == null) return null;

        var discussion = new GitHubDiscussion
        {
            Id = node.id,
            Title = node.title,
            Body = node.body ?? "",
            Url = node.url,
            Number = node.number,
            Locked = node.locked,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
            UpvoteCount = node.upvoteCount,
            ViewerHasUpvoted = node.viewerHasUpvoted,
            CommentsTotalCount = node.comments.totalCount,
            AnswerChosenAt = node.answerChosenAt != null
        };

        if (node.category != null)
        {
            discussion.Category = new DiscussionCategory
            {
                Id = node.category.id,
                Name = node.category.name,
                Description = node.category.description ?? "",
                Emoji = node.category.emoji ?? "",
                EmojiHTML = node.category.emojiHTML ?? "",
                Slug = node.category.slug,
                IsAnswerable = node.category.isAnswerable,
                CreatedAt = DateTimeOffset.Parse(node.category.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.category.updatedAt.ToString())
            };
            discussion.CategoryId = discussion.Category.Id;
        }

        if (node.author != null)
        {
            discussion.Author = new DiscussionAuthor
            {
                Login = node.author.login,
                Id = node.author.id ?? "",
                AvatarUrl = node.author.avatarUrl ?? "",
                Url = node.author.url ?? "",
                Type = "User"
            };
        }

        if (node.labels?.nodes != null)
        {
            foreach (var label in node.labels.nodes)
            {
                discussion.Labels.Add(new DiscussionLabel
                {
                    Id = label.id,
                    Name = label.name,
                    Color = label.color,
                    Description = label.description ?? "",
                    Url = label.url
                });
            }
        }

        if (node.answer != null)
        {
            discussion.Answer = new DiscussionComment
            {
                Id = node.answer.id,
                Body = node.answer.body,
                BodyHTML = node.answer.bodyHTML,
                Url = node.answer.url,
                CreatedAt = DateTimeOffset.Parse(node.answer.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.answer.updatedAt.ToString()),
                UpvoteCount = node.answer.upvoteCount,
                ViewerHasUpvoted = node.answer.viewerHasUpvoted,
                IsAnswer = true
            };

            if (node.answer.author != null)
            {
                discussion.Answer.Author = new DiscussionAuthor
                {
                    Login = node.answer.author.login,
                    Id = node.answer.author.id ?? "",
                    AvatarUrl = node.answer.author.avatarUrl ?? "",
                    Url = node.answer.author.url ?? "",
                    Type = "User"
                };
            }
        }

        return discussion;
    }

    public async Task<GitHubDiscussion?> GetDiscussionByNumberAsync(string owner, string name, int number)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetDiscussionByNumber($owner: String!, $name: String!, $number: Int!) {
                    repository(owner: $owner, name: $name) {
                        discussion(number: $number) {
                            id
                            title
                            body
                            url
                            number
                            locked
                            createdAt
                            updatedAt
                            upvoteCount
                            viewerHasUpvoted
                            comments {
                                totalCount
                            }
                            category {
                                id
                                name
                                description
                                emoji
                                emojiHTML
                                slug
                                isAnswerable
                                createdAt
                                updatedAt
                            }
                            author {
                                login
                                ... on User {
                                    id
                                    avatarUrl
                                    url
                                }
                                ... on Organization {
                                    id
                                    avatarUrl
                                    url
                                }
                            }
                            labels(first: 10) {
                                nodes {
                                    id
                                    name
                                    color
                                    description
                                    url
                                }
                            }
                            answer {
                                id
                                body
                                bodyHTML
                                url
                                createdAt
                                updatedAt
                                upvoteCount
                                viewerHasUpvoted
                                author {
                                    login
                                    ... on User {
                                        id
                                        avatarUrl
                                        url
                                    }
                                }
                            }
                            answerChosenAt
                        }
                    }
                }",
            Variables = new { owner, name, number }
        };

        var response = await client.SendQueryAsync<dynamic>(query);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.repository.discussion;
        if (node == null) return null;

        var discussion = new GitHubDiscussion
        {
            Id = node.id,
            Title = node.title,
            Body = node.body ?? "",
            Url = node.url,
            Number = node.number,
            Locked = node.locked,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
            UpvoteCount = node.upvoteCount,
            ViewerHasUpvoted = node.viewerHasUpvoted,
            CommentsTotalCount = node.comments.totalCount,
            AnswerChosenAt = node.answerChosenAt != null
        };

        if (node.category != null)
        {
            discussion.Category = new DiscussionCategory
            {
                Id = node.category.id,
                Name = node.category.name,
                Description = node.category.description ?? "",
                Emoji = node.category.emoji ?? "",
                EmojiHTML = node.category.emojiHTML ?? "",
                Slug = node.category.slug,
                IsAnswerable = node.category.isAnswerable,
                CreatedAt = DateTimeOffset.Parse(node.category.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.category.updatedAt.ToString())
            };
            discussion.CategoryId = discussion.Category.Id;
        }

        if (node.author != null)
        {
            discussion.Author = new DiscussionAuthor
            {
                Login = node.author.login,
                Id = node.author.id ?? "",
                AvatarUrl = node.author.avatarUrl ?? "",
                Url = node.author.url ?? "",
                Type = "User"
            };
        }

        if (node.labels?.nodes != null)
        {
            foreach (var label in node.labels.nodes)
            {
                discussion.Labels.Add(new DiscussionLabel
                {
                    Id = label.id,
                    Name = label.name,
                    Color = label.color,
                    Description = label.description ?? "",
                    Url = label.url
                });
            }
        }

        if (node.answer != null)
        {
            discussion.Answer = new DiscussionComment
            {
                Id = node.answer.id,
                Body = node.answer.body,
                BodyHTML = node.answer.bodyHTML,
                Url = node.answer.url,
                CreatedAt = DateTimeOffset.Parse(node.answer.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.answer.updatedAt.ToString()),
                UpvoteCount = node.answer.upvoteCount,
                ViewerHasUpvoted = node.answer.viewerHasUpvoted,
                IsAnswer = true
            };

            if (node.answer.author != null)
            {
                discussion.Answer.Author = new DiscussionAuthor
                {
                    Login = node.answer.author.login,
                    Id = node.answer.author.id ?? "",
                    AvatarUrl = node.answer.author.avatarUrl ?? "",
                    Url = node.answer.author.url ?? "",
                    Type = "User"
                };
            }
        }

        return discussion;
    }

    public async Task<GitHubDiscussion> CreateDiscussionAsync(CreateDiscussionRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation CreateDiscussion($input: CreateDiscussionInput!) {
                    createDiscussion(input: $input) {
                        discussion {
                            id
                            title
                            body
                            url
                            number
                            locked
                            createdAt
                            updatedAt
                            upvoteCount
                            viewerHasUpvoted
                            comments {
                                totalCount
                            }
                            category {
                                id
                                name
                                description
                                emoji
                                emojiHTML
                                slug
                                isAnswerable
                                createdAt
                                updatedAt
                            }
                            author {
                                login
                                ... on User {
                                    id
                                    avatarUrl
                                    url
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    repositoryId = request.RepositoryId,
                    title = request.Title,
                    body = request.Body,
                    categoryId = request.CategoryId
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.createDiscussion.discussion;
        var discussion = new GitHubDiscussion
        {
            Id = node.id,
            Title = node.title,
            Body = node.body ?? "",
            Url = node.url,
            Number = node.number,
            Locked = node.locked,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
            UpvoteCount = node.upvoteCount,
            ViewerHasUpvoted = node.viewerHasUpvoted,
            CommentsTotalCount = node.comments.totalCount
        };

        if (node.category != null)
        {
            discussion.Category = new DiscussionCategory
            {
                Id = node.category.id,
                Name = node.category.name,
                Description = node.category.description ?? "",
                Emoji = node.category.emoji ?? "",
                EmojiHTML = node.category.emojiHTML ?? "",
                Slug = node.category.slug,
                IsAnswerable = node.category.isAnswerable,
                CreatedAt = DateTimeOffset.Parse(node.category.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.category.updatedAt.ToString())
            };
            discussion.CategoryId = discussion.Category.Id;
        }

        if (node.author != null)
        {
            discussion.Author = new DiscussionAuthor
            {
                Login = node.author.login,
                Id = node.author.id ?? "",
                AvatarUrl = node.author.avatarUrl ?? "",
                Url = node.author.url ?? "",
                Type = "User"
            };
        }

        return discussion;
    }

    public async Task<GitHubDiscussion> UpdateDiscussionAsync(UpdateDiscussionRequest request)
    {
        using var client = CreateGraphQLClient();

        var inputFields = new List<string>();
        var inputValues = new Dictionary<string, object>
        {
            ["discussionId"] = request.DiscussionId
        };

        if (!string.IsNullOrEmpty(request.Title))
        {
            inputFields.Add("title: $title");
            inputValues["title"] = request.Title;
        }

        if (!string.IsNullOrEmpty(request.Body))
        {
            inputFields.Add("body: $body");
            inputValues["body"] = request.Body;
        }

        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            inputFields.Add("categoryId: $categoryId");
            inputValues["categoryId"] = request.CategoryId;
        }

        var inputString = string.Join(", ", inputFields);

        var mutation = new GraphQLRequest
        {
            Query = $@"
                mutation UpdateDiscussion($discussionId: ID!{(inputValues.ContainsKey("title") ? ", $title: String!" : "")}{(inputValues.ContainsKey("body") ? ", $body: String!" : "")}{(inputValues.ContainsKey("categoryId") ? ", $categoryId: ID!" : "")}) {{
                    updateDiscussion(input: {{discussionId: $discussionId{(inputFields.Any() ? ", " + inputString : "")}}} ) {{
                        discussion {{
                            id
                            title
                            body
                            url
                            number
                            locked
                            createdAt
                            updatedAt
                            upvoteCount
                            viewerHasUpvoted
                            comments {{
                                totalCount
                            }}
                            category {{
                                id
                                name
                                description
                                emoji
                                emojiHTML
                                slug
                                isAnswerable
                                createdAt
                                updatedAt
                            }}
                            author {{
                                login
                                ... on User {{
                                    id
                                    avatarUrl
                                    url
                                }}
                            }}
                        }}
                    }}
                }}",
            Variables = inputValues
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.updateDiscussion.discussion;
        var discussion = new GitHubDiscussion
        {
            Id = node.id,
            Title = node.title,
            Body = node.body ?? "",
            Url = node.url,
            Number = node.number,
            Locked = node.locked,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
            UpvoteCount = node.upvoteCount,
            ViewerHasUpvoted = node.viewerHasUpvoted,
            CommentsTotalCount = node.comments.totalCount
        };

        if (node.category != null)
        {
            discussion.Category = new DiscussionCategory
            {
                Id = node.category.id,
                Name = node.category.name,
                Description = node.category.description ?? "",
                Emoji = node.category.emoji ?? "",
                EmojiHTML = node.category.emojiHTML ?? "",
                Slug = node.category.slug,
                IsAnswerable = node.category.isAnswerable,
                CreatedAt = DateTimeOffset.Parse(node.category.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.category.updatedAt.ToString())
            };
            discussion.CategoryId = discussion.Category.Id;
        }

        if (node.author != null)
        {
            discussion.Author = new DiscussionAuthor
            {
                Login = node.author.login,
                Id = node.author.id ?? "",
                AvatarUrl = node.author.avatarUrl ?? "",
                Url = node.author.url ?? "",
                Type = "User"
            };
        }

        return discussion;
    }

    public async Task<bool> DeleteDiscussionAsync(string discussionId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation DeleteDiscussion($input: DeleteDiscussionInput!) {
                    deleteDiscussion(input: $input) {
                        clientMutationId
                    }
                }",
            Variables = new
            {
                input = new { discussionId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.deleteDiscussion != null;
    }

    public async Task<bool> LockDiscussionAsync(string discussionId, string? lockReason = null)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation LockLockable($input: LockLockableInput!) {
                    lockLockable(input: $input) {
                        lockedRecord {
                            locked
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    lockableId = discussionId,
                    lockReason = lockReason ?? "OFF_TOPIC"
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.lockLockable?.lockedRecord?.locked == true;
    }

    public async Task<bool> UnlockDiscussionAsync(string discussionId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation UnlockLockable($input: UnlockLockableInput!) {
                    unlockLockable(input: $input) {
                        unlockedRecord {
                            locked
                        }
                    }
                }",
            Variables = new
            {
                input = new { lockableId = discussionId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.unlockLockable?.unlockedRecord?.locked == false;
    }

    public async Task<bool> UpvoteDiscussionAsync(string discussionId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation AddUpvote($input: AddUpvoteInput!) {
                    addUpvote(input: $input) {
                        subject {
                            upvoteCount
                            viewerHasUpvoted
                        }
                    }
                }",
            Variables = new
            {
                input = new { subjectId = discussionId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.addUpvote?.subject?.viewerHasUpvoted == true;
    }

    public async Task<bool> RemoveDiscussionUpvoteAsync(string discussionId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation RemoveUpvote($input: RemoveUpvoteInput!) {
                    removeUpvote(input: $input) {
                        subject {
                            upvoteCount
                            viewerHasUpvoted
                        }
                    }
                }",
            Variables = new
            {
                input = new { subjectId = discussionId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.removeUpvote?.subject?.viewerHasUpvoted == false;
    }

    #endregion

    #region Discussion Comments CRUD

    public async Task<DiscussionCommentsResponse> GetDiscussionCommentsAsync(
        string discussionId,
        int first = 20,
        string? after = null)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetDiscussionComments($discussionId: ID!, $first: Int!, $after: String) {
                    node(id: $discussionId) {
                        ... on Discussion {
                            comments(first: $first, after: $after) {
                                totalCount
                                pageInfo {
                                    hasNextPage
                                    endCursor
                                }
                                nodes {
                                    id
                                    body
                                    bodyHTML
                                    url
                                    createdAt
                                    updatedAt
                                    upvoteCount
                                    viewerHasUpvoted
                                    isAnswer
                                    isMinimized
                                    minimizedReason
                                    author {
                                        login
                                        ... on User {
                                            id
                                            avatarUrl
                                            url
                                        }
                                        ... on Organization {
                                            id
                                            avatarUrl
                                            url
                                        }
                                    }
                                    replies(first: 10) {
                                        nodes {
                                            id
                                            body
                                            bodyHTML
                                            url
                                            createdAt
                                            updatedAt
                                            upvoteCount
                                            viewerHasUpvoted
                                            author {
                                                login
                                                ... on User {
                                                    id
                                                    avatarUrl
                                                    url
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }",
            Variables = new { discussionId, first, after }
        };

        var response = await client.SendQueryAsync<dynamic>(query);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var comments = new List<DiscussionComment>();
        var commentsData = response.Data.node.comments;

        foreach (var node in commentsData.nodes)
        {
            var comment = new DiscussionComment
            {
                Id = node.id,
                Body = node.body,
                BodyHTML = node.bodyHTML,
                Url = node.url,
                CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
                UpvoteCount = node.upvoteCount,
                ViewerHasUpvoted = node.viewerHasUpvoted,
                IsAnswer = node.isAnswer,
                IsMinimized = node.isMinimized,
                MinimizedReason = node.minimizedReason
            };

            if (node.author != null)
            {
                comment.Author = new DiscussionAuthor
                {
                    Login = node.author.login,
                    Id = node.author.id ?? "",
                    AvatarUrl = node.author.avatarUrl ?? "",
                    Url = node.author.url ?? "",
                    Type = "User"
                };
            }

            if (node.replies?.nodes != null)
            {
                foreach (var reply in node.replies.nodes)
                {
                    var replyComment = new DiscussionComment
                    {
                        Id = reply.id,
                        Body = reply.body,
                        BodyHTML = reply.bodyHTML,
                        Url = reply.url,
                        CreatedAt = DateTimeOffset.Parse(reply.createdAt.ToString()),
                        UpdatedAt = DateTimeOffset.Parse(reply.updatedAt.ToString()),
                        UpvoteCount = reply.upvoteCount,
                        ViewerHasUpvoted = reply.viewerHasUpvoted
                    };

                    if (reply.author != null)
                    {
                        replyComment.Author = new DiscussionAuthor
                        {
                            Login = reply.author.login,
                            Id = reply.author.id ?? "",
                            AvatarUrl = reply.author.avatarUrl ?? "",
                            Url = reply.author.url ?? "",
                            Type = "User"
                        };
                    }

                    comment.Replies.Add(replyComment);
                }
            }

            comments.Add(comment);
        }

        return new DiscussionCommentsResponse
        {
            Comments = comments,
            HasNextPage = commentsData.pageInfo.hasNextPage,
            EndCursor = commentsData.pageInfo.endCursor,
            TotalCount = commentsData.totalCount
        };
    }

    public async Task<DiscussionComment> CreateDiscussionCommentAsync(CreateDiscussionCommentRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation AddDiscussionComment($input: AddDiscussionCommentInput!) {
                    addDiscussionComment(input: $input) {
                        comment {
                            id
                            body
                            bodyHTML
                            url
                            createdAt
                            updatedAt
                            upvoteCount
                            viewerHasUpvoted
                            isAnswer
                            isMinimized
                            minimizedReason
                            author {
                                login
                                ... on User {
                                    id
                                    avatarUrl
                                    url
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    discussionId = request.DiscussionId,
                    body = request.Body,
                    replyToId = request.ReplyToId
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.addDiscussionComment.comment;
        var comment = new DiscussionComment
        {
            Id = node.id,
            Body = node.body,
            BodyHTML = node.bodyHTML,
            Url = node.url,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
            UpvoteCount = node.upvoteCount,
            ViewerHasUpvoted = node.viewerHasUpvoted,
            IsAnswer = node.isAnswer,
            IsMinimized = node.isMinimized,
            MinimizedReason = node.minimizedReason
        };

        if (node.author != null)
        {
            comment.Author = new DiscussionAuthor
            {
                Login = node.author.login,
                Id = node.author.id ?? "",
                AvatarUrl = node.author.avatarUrl ?? "",
                Url = node.author.url ?? "",
                Type = "User"
            };
        }

        return comment;
    }

    public async Task<DiscussionComment> UpdateDiscussionCommentAsync(UpdateDiscussionCommentRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation UpdateDiscussionComment($input: UpdateDiscussionCommentInput!) {
                    updateDiscussionComment(input: $input) {
                        comment {
                            id
                            body
                            bodyHTML
                            url
                            createdAt
                            updatedAt
                            upvoteCount
                            viewerHasUpvoted
                            isAnswer
                            isMinimized
                            minimizedReason
                            author {
                                login
                                ... on User {
                                    id
                                    avatarUrl
                                    url
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    commentId = request.CommentId,
                    body = request.Body
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.updateDiscussionComment.comment;
        var comment = new DiscussionComment
        {
            Id = node.id,
            Body = node.body,
            BodyHTML = node.bodyHTML,
            Url = node.url,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString()),
            UpvoteCount = node.upvoteCount,
            ViewerHasUpvoted = node.viewerHasUpvoted,
            IsAnswer = node.isAnswer,
            IsMinimized = node.isMinimized,
            MinimizedReason = node.minimizedReason
        };

        if (node.author != null)
        {
            comment.Author = new DiscussionAuthor
            {
                Login = node.author.login,
                Id = node.author.id ?? "",
                AvatarUrl = node.author.avatarUrl ?? "",
                Url = node.author.url ?? "",
                Type = "User"
            };
        }

        return comment;
    }

    public async Task<bool> DeleteDiscussionCommentAsync(string commentId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation DeleteDiscussionComment($input: DeleteDiscussionCommentInput!) {
                    deleteDiscussionComment(input: $input) {
                        clientMutationId
                    }
                }",
            Variables = new
            {
                input = new { id = commentId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.deleteDiscussionComment != null;
    }

    public async Task<bool> UpvoteDiscussionCommentAsync(string commentId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation AddUpvote($input: AddUpvoteInput!) {
                    addUpvote(input: $input) {
                        subject {
                            upvoteCount
                            viewerHasUpvoted
                        }
                    }
                }",
            Variables = new
            {
                input = new { subjectId = commentId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.addUpvote?.subject?.viewerHasUpvoted == true;
    }

    public async Task<bool> RemoveDiscussionCommentUpvoteAsync(string commentId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation RemoveUpvote($input: RemoveUpvoteInput!) {
                    removeUpvote(input: $input) {
                        subject {
                            upvoteCount
                            viewerHasUpvoted
                        }
                    }
                }",
            Variables = new
            {
                input = new { subjectId = commentId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.removeUpvote?.subject?.viewerHasUpvoted == false;
    }

    public async Task<bool> MarkCommentAsAnswerAsync(string commentId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation MarkDiscussionCommentAsAnswer($input: MarkDiscussionCommentAsAnswerInput!) {
                    markDiscussionCommentAsAnswer(input: $input) {
                        discussion {
                            answer {
                                id
                                isAnswer
                            }
                            answerChosenAt
                        }
                    }
                }",
            Variables = new
            {
                input = new { id = commentId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.markDiscussionCommentAsAnswer?.discussion?.answer?.isAnswer == true;
    }

    public async Task<bool> UnmarkCommentAsAnswerAsync(string commentId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation UnmarkDiscussionCommentAsAnswer($input: UnmarkDiscussionCommentAsAnswerInput!) {
                    unmarkDiscussionCommentAsAnswer(input: $input) {
                        discussion {
                            answer {
                                id
                                isAnswer
                            }
                            answerChosenAt
                        }
                    }
                }",
            Variables = new
            {
                input = new { id = commentId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.unmarkDiscussionCommentAsAnswer?.discussion?.answer == null;
    }

    #endregion

    #region Discussion Categories CRUD

    public async Task<DiscussionCategoriesResponse> GetDiscussionCategoriesAsync(string owner, string name)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetDiscussionCategories($owner: String!, $name: String!) {
                    repository(owner: $owner, name: $name) {
                        discussionCategories(first: 100) {
                            totalCount
                            nodes {
                                id
                                name
                                description
                                emoji
                                emojiHTML
                                slug
                                isAnswerable
                                createdAt
                                updatedAt
                            }
                        }
                    }
                }",
            Variables = new { owner, name }
        };

        var response = await client.SendQueryAsync<dynamic>(query);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var categories = new List<DiscussionCategory>();
        var categoriesData = response.Data.repository.discussionCategories;

        foreach (var node in categoriesData.nodes)
        {
            categories.Add(new DiscussionCategory
            {
                Id = node.id,
                Name = node.name,
                Description = node.description ?? "",
                Emoji = node.emoji ?? "",
                EmojiHTML = node.emojiHTML ?? "",
                Slug = node.slug,
                IsAnswerable = node.isAnswerable,
                CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
                UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString())
            });
        }

        return new DiscussionCategoriesResponse
        {
            Categories = categories,
            TotalCount = categoriesData.totalCount
        };
    }

    public async Task<DiscussionCategory?> GetDiscussionCategoryByIdAsync(string categoryId)
    {
        using var client = CreateGraphQLClient();

        var query = new GraphQLRequest
        {
            Query = @"
                query GetDiscussionCategory($categoryId: ID!) {
                    node(id: $categoryId) {
                        ... on DiscussionCategory {
                            id
                            name
                            description
                            emoji
                            emojiHTML
                            slug
                            isAnswerable
                            createdAt
                            updatedAt
                        }
                    }
                }",
            Variables = new { categoryId }
        };

        var response = await client.SendQueryAsync<dynamic>(query);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.node;
        if (node == null) return null;

        return new DiscussionCategory
        {
            Id = node.id,
            Name = node.name,
            Description = node.description ?? "",
            Emoji = node.emoji ?? "",
            EmojiHTML = node.emojiHTML ?? "",
            Slug = node.slug,
            IsAnswerable = node.isAnswerable,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString())
        };
    }

    public async Task<DiscussionCategory> CreateDiscussionCategoryAsync(CreateDiscussionCategoryRequest request)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation CreateDiscussionCategory($input: CreateDiscussionCategoryInput!) {
                    createDiscussionCategory(input: $input) {
                        category {
                            id
                            name
                            description
                            emoji
                            emojiHTML
                            slug
                            isAnswerable
                            createdAt
                            updatedAt
                        }
                    }
                }",
            Variables = new
            {
                input = new
                {
                    repositoryId = request.RepositoryId,
                    name = request.Name,
                    description = request.Description,
                    emoji = request.Emoji,
                    isAnswerable = request.IsAnswerable
                }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.createDiscussionCategory.category;
        return new DiscussionCategory
        {
            Id = node.id,
            Name = node.name,
            Description = node.description ?? "",
            Emoji = node.emoji ?? "",
            EmojiHTML = node.emojiHTML ?? "",
            Slug = node.slug,
            IsAnswerable = node.isAnswerable,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString())
        };
    }

    public async Task<DiscussionCategory> UpdateDiscussionCategoryAsync(UpdateDiscussionCategoryRequest request)
    {
        using var client = CreateGraphQLClient();

        var inputFields = new List<string>();
        var inputValues = new Dictionary<string, object>
        {
            ["categoryId"] = request.CategoryId
        };

        if (!string.IsNullOrEmpty(request.Name))
        {
            inputFields.Add("name: $name");
            inputValues["name"] = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            inputFields.Add("description: $description");
            inputValues["description"] = request.Description;
        }

        if (!string.IsNullOrEmpty(request.Emoji))
        {
            inputFields.Add("emoji: $emoji");
            inputValues["emoji"] = request.Emoji;
        }

        if (request.IsAnswerable.HasValue)
        {
            inputFields.Add("isAnswerable: $isAnswerable");
            inputValues["isAnswerable"] = request.IsAnswerable.Value;
        }

        var inputString = string.Join(", ", inputFields);

        var mutation = new GraphQLRequest
        {
            Query = $@"
                mutation UpdateDiscussionCategory($categoryId: ID!{(inputValues.ContainsKey("name") ? ", $name: String!" : "")}{(inputValues.ContainsKey("description") ? ", $description: String!" : "")}{(inputValues.ContainsKey("emoji") ? ", $emoji: String!" : "")}{(inputValues.ContainsKey("isAnswerable") ? ", $isAnswerable: Boolean!" : "")}) {{
                    updateDiscussionCategory(input: {{categoryId: $categoryId{(inputFields.Any() ? ", " + inputString : "")}}} ) {{
                        category {{
                            id
                            name
                            description
                            emoji
                            emojiHTML
                            slug
                            isAnswerable
                            createdAt
                            updatedAt
                        }}
                    }}
                }}",
            Variables = inputValues
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var node = response.Data.updateDiscussionCategory.category;
        return new DiscussionCategory
        {
            Id = node.id,
            Name = node.name,
            Description = node.description ?? "",
            Emoji = node.emoji ?? "",
            EmojiHTML = node.emojiHTML ?? "",
            Slug = node.slug,
            IsAnswerable = node.isAnswerable,
            CreatedAt = DateTimeOffset.Parse(node.createdAt.ToString()),
            UpdatedAt = DateTimeOffset.Parse(node.updatedAt.ToString())
        };
    }

    public async Task<bool> DeleteDiscussionCategoryAsync(string categoryId)
    {
        using var client = CreateGraphQLClient();

        var mutation = new GraphQLRequest
        {
            Query = @"
                mutation DeleteDiscussionCategory($input: DeleteDiscussionCategoryInput!) {
                    deleteDiscussionCategory(input: $input) {
                        clientMutationId
                    }
                }",
            Variables = new
            {
                input = new { categoryId }
            }
        };

        var response = await client.SendMutationAsync<dynamic>(mutation);
        if (response.Errors != null && response.Errors.Any())
        {
            throw new InvalidOperationException($"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        return response.Data.deleteDiscussionCategory != null;
    }

    #endregion
}
