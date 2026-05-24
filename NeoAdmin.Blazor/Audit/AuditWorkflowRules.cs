using NeoAdmin.Blazor.Entities;
using NeoUI.Blazor;

namespace NeoAdmin.Blazor.Audit;

public static class AuditWorkflowRules
{
    public static bool CanEdit(EntityAudited item) =>
        item.AuditStatus == SysAuditStatus.待提交
        || (item.AuditStatus == SysAuditStatus.退回 && item.AuditStep == "audit_00");

    public static string GetStatusLabel(SysAuditStatus status) => status switch
    {
        SysAuditStatus.待提交 => "待提交",
        SysAuditStatus.审核中 => "审核中",
        SysAuditStatus.通过 => "已通过",
        SysAuditStatus.退回 => "已退回",
        SysAuditStatus.拒绝 => "已拒绝",
        _ => status.ToString()
    };

    public static BadgeVariant GetStatusBadgeVariant(SysAuditStatus status) => status switch
    {
        SysAuditStatus.通过 => BadgeVariant.Default,
        SysAuditStatus.审核中 => BadgeVariant.Secondary,
        SysAuditStatus.拒绝 => BadgeVariant.Destructive,
        SysAuditStatus.退回 => BadgeVariant.Outline,
        _ => BadgeVariant.Outline
    };

    /// <summary>通过、审核中、拒绝等需要更强视觉区分的状态附加样式。</summary>
    public static string GetStatusBadgeClass(SysAuditStatus status) => status switch
    {
        SysAuditStatus.通过 => "border-transparent bg-green-600 text-white hover:bg-green-600",
        SysAuditStatus.审核中 => "border-transparent bg-yellow-500 text-white hover:bg-yellow-500",
        SysAuditStatus.拒绝 => "border-transparent bg-red-600 text-white hover:bg-red-600",
        _ => string.Empty
    };
}
