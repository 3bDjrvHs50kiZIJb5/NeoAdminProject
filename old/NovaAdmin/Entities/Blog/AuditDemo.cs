using FreeSql.DataAnnotations;

namespace LinCms.Entities.Blog
{
    /// <summary>
    /// 审计演示
    /// </summary>
    [Table(Name = "blog_audit_demo")]
    public class AuditDemo : EntityAudited
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Column(StringLength = 100)]
        public string Title { get; set; }

        /// <summary>
        /// 业务编号
        /// </summary>
        [Column(StringLength = 50)]
        public string BusinessNo { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [Column(StringLength = 500)]
        public string Remark { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}