using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Data.Entities;

public sealed class SysUserLoginLog : Entity
{
    public enum LogType
    {
        登陆成功,
        登陆失败
    }

    public DateTime LoginTime { get; set; } = DateTime.Now;

    [Column(StringLength = 50)]
    public string Username { get; set; } = string.Empty;

    public LogType Type { get; set; }

    [Column(StringLength = -1)]
    public string Extra { get; set; } = string.Empty;

    [Column(StringLength = 100)]
    public string Ip { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string UserAgent { get; set; } = string.Empty;
}
