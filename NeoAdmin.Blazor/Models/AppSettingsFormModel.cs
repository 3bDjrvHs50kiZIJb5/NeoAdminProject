namespace NeoAdmin.Blazor.Models;

/// <summary>
/// appsettings 固定字段表单模型。值为 <c>null</c> 表示该键不在当前文件中（保存时不写入 / 会移除）。
/// 含框架通用项与常见宿主扩展（Sms / Pandoc / DeepSeek / OpenAI）。
/// </summary>
public sealed class AppSettingsFormModel
{
    // NeoAdmin 基础
    public string? DataType { get; set; }
    public string? ConnectionString { get; set; }
    public bool? AutoSyncStructure { get; set; }
    public bool? EnableSeedData { get; set; }
    public string? HomePath { get; set; }
    public bool? MonitorCommand { get; set; }
    public string? SeedAdminUserName { get; set; }
    public string? SeedAdminPassword { get; set; }
    public int? WorkId { get; set; }
    public bool? EnableIpWhitelist { get; set; }
    public bool? IsSwagger { get; set; }
    public string? SwaggerHides { get; set; }
    public bool? SchedulerAutoLoad { get; set; }
    public string? LogDirectory { get; set; }
    public string? LogFilePrefix { get; set; }

    // FileUpload
    public string? FileUploadDirectory { get; set; }
    public string? FileUploadDateTimeDirectory { get; set; }
    public bool? FileUploadMd5 { get; set; }
    public long? FileUploadMaxSize { get; set; }
    public string? FileUploadIncludeExtension { get; set; }
    public string? FileUploadExcludeExtension { get; set; }

    // OSS
    public string? OssEndpoint { get; set; }
    public string? OssAccessKeyId { get; set; }
    public string? OssAccessKeySecret { get; set; }
    public string? OssBucketName { get; set; }
    public string? OssCustomDomain { get; set; }
    public string? OssPrefix { get; set; }

    // DashScope
    public string? DashScopeApiKey { get; set; }
    public string? DashScopeRealtimeModel { get; set; }
    public int? DashScopeSampleRate { get; set; }

    // Logging
    public string? LoggingDefault { get; set; }
    public string? LoggingAspNetCore { get; set; }

    // Sms（宿主扩展，如 NovoLab）
    public string? SmsAccessKeyId { get; set; }
    public string? SmsAccessKeySecret { get; set; }
    public string? SmsSignName { get; set; }
    public string? SmsPasswordResetTemplateCode { get; set; }
    public string? SmsRegisterTemplateCode { get; set; }
    public int? SmsCodeExpireMinutes { get; set; }
    public int? SmsSendIntervalSeconds { get; set; }

    // Pandoc
    public string? PandocExecutablePath { get; set; }

    // DeepSeek
    public string? DeepSeekApiKey { get; set; }
    public string? DeepSeekBaseUrl { get; set; }
    public string? DeepSeekModel { get; set; }

    // OpenAI
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIBaseUrl { get; set; }
    public string? OpenAIModel { get; set; }
}

/// <summary>可编辑的 appsettings 文件描述。</summary>
public sealed record AppSettingsFileInfo(string FileName, string DisplayName, bool Optional);
