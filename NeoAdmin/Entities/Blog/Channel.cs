using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Entities.Blog;

partial class Tag2
{
    [Navigate(ManyToMany = typeof(ChannelTag2))]
    public List<Channel> Channels { get; set; } = [];

    [Table(Name = "blog_channel_tag")]
    public class ChannelTag2
    {
        public long ChannelId { get; set; }
        public long TagId { get; set; }

        public Channel Channel { get; set; } = default!;
        public Tag2 Tag { get; set; } = default!;
    }
}

/// <summary>
/// 技术频道
/// </summary>
[Table(Name = "blog_channel")]
public class Channel : EntityAudited
{
    [Navigate(ManyToMany = typeof(Tag2.ChannelTag2))]
    public List<Tag2> Tags { get; set; } = [];

    [Column(StringLength = 50)]
    public string ChannelName { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string ChannelCode { get; set; } = string.Empty;

    [Column(StringLength = 100)]
    public string Thumbnail { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Remark { get; set; } = string.Empty;

    public int SortCode { get; set; }

    public bool Status { get; set; }
}
