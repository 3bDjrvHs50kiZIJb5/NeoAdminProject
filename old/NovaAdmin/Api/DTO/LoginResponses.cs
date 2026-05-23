namespace NovaAdmin.API.DTO;

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserSummaryResponse User { get; set; } = new();
}

public class UserSummaryResponse
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime LoginTime { get; set; }
    public List<string> Roles { get; set; } = new();
}

public sealed class UserDetailResponse : UserSummaryResponse
{
    public long OrgId { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class UserCardResponse
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime LastLoginTime { get; set; }
}

public sealed class UserCardListResponse
{
    public int TotalCount { get; set; }
    public List<UserCardResponse> Items { get; set; } = new();
}
