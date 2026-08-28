namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 全站 DiceBear 头像风格版本号。站点设置切换风格/预设时递增，用于缓存失效与前端 URL 刷新。
/// </summary>
public sealed class AvatarStyleRevision
{
    private long _revision;

    public long Current => Interlocked.Read(ref _revision);

    public long Bump() => Interlocked.Increment(ref _revision);
}
