namespace NeoAdmin.Blazor.Api.Dto;

public sealed class UserDetailResponse
{
    public long Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }

    public DateTime LoginTime { get; init; }

    public string Description { get; init; } = string.Empty;

    public List<string> Roles { get; init; } = [];
}

public sealed class UserCardResponse
{
    public long Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTime LastLoginTime { get; init; }
}

public sealed class UserCardListResponse
{
    public int TotalCount { get; init; }

    public List<UserCardResponse> Items { get; init; } = [];
}
