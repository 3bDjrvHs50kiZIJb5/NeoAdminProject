using System.Net.Mime;
using System.Text.Encodings.Web;
using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Services;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Middlewares;

/// <summary>
/// 基于数据库 IP 白名单表的请求拦截中间件。
/// </summary>
public sealed class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipWhitelist(context.Request.Path))
        {
            await _next(context);
            return;
        }

        string clientIp = IpHelper.GetClientIpAddress(context, _logger);
        if (string.IsNullOrWhiteSpace(clientIp) || clientIp == "unknown")
        {
            _logger.LogWarning("IP 白名单校验失败：无法识别客户端 IP，路径：{Path}", context.Request.Path);
            await RejectAsync(context, "unknown", manualApproval: true);
            return;
        }

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IFreeSql fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();

        List<SysIpWhitelist> enabledEntries = await fsql.Select<SysIpWhitelist>()
            .Where(x => x.IsEnabled)
            .ToListAsync();

        if (enabledEntries.Count == 0)
        {
            await _next(context);
            return;
        }

        SysIpWhitelist? matchedEntry = enabledEntries
            .FirstOrDefault(x => IpHelper.NormalizeIp(x.IpAddress) == clientIp);

        if (matchedEntry is null)
        {
            _logger.LogWarning("IP 白名单拦截：IP={ClientIp}，路径={Path}", clientIp, context.Request.Path);
            SysSiteSettings siteSettings = await scope.ServiceProvider
                .GetRequiredService<SiteSettingsService>()
                .GetAsync();
            await RejectAsync(context, IpHelper.ToIpv4Display(clientIp), siteSettings.IpWhitelistManualApproval);
            return;
        }

        try
        {
            await fsql.Update<SysIpWhitelist>()
                .Set(a => a.LastAccessTime, DateTime.Now)
                .Set(a => a.AccessCount, matchedEntry.AccessCount + 1)
                .Where(a => a.Id == matchedEntry.Id)
                .ExecuteAffrowsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IP 白名单命中后更新访问统计失败：IP={ClientIp}", clientIp);
        }

        await _next(context);
    }

    private static bool ShouldSkipWhitelist(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/profile", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase);

    private static async Task RejectAsync(HttpContext context, string clientIpv4, bool manualApproval = true)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = MediaTypeNames.Text.Html + "; charset=utf-8";

        string encodedIp = HtmlEncoder.Default.Encode(clientIpv4);
        string modalHint = manualApproval
            ? "提交当前 IP，待审核"
            : "提交当前 IP，立即生效";
        string encodedModalHint = HtmlEncoder.Default.Encode(modalHint);
        await context.Response.WriteAsync($$"""
<!doctype html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>403 Forbidden</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #f7f8fb;
      --panel: #ffffff;
      --text: #1f2937;
      --muted: #6b7280;
      --line: #e5e7eb;
      --accent: #2563eb;
      --accent-hover: #1d4ed8;
      --danger: #dc2626;
      --success: #059669;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 32px 16px;
      background: var(--bg);
      color: var(--text);
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif;
      line-height: 1.7;
    }
    main {
      width: min(680px, 100%);
      padding: 36px;
      border: 1px solid var(--line);
      border-radius: 12px;
      background: var(--panel);
      box-shadow: 0 18px 50px rgba(15, 23, 42, 0.08);
    }
    .code {
      margin: 0 0 18px;
      color: var(--accent);
      font-size: 15px;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }
    h1 {
      margin: 0 0 20px;
      font-size: clamp(26px, 4vw, 38px);
      line-height: 1.2;
    }
    p {
      margin: 0 0 14px;
      font-size: 17px;
    }
    .note {
      margin-top: 26px;
      padding-top: 18px;
      border-top: 1px solid var(--line);
      color: var(--muted);
      font-size: 14px;
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 6px 10px;
    }
    .apply-link {
      color: var(--accent);
      font-size: 14px;
      text-decoration: none;
      cursor: pointer;
    }
    .apply-link:hover {
      text-decoration: underline;
    }
    .modal {
      position: fixed;
      inset: 0;
      z-index: 50;
      display: none;
      align-items: center;
      justify-content: center;
      padding: 16px;
      background: rgba(15, 23, 42, 0.35);
    }
    .modal.open {
      display: flex;
    }
    .modal-card {
      width: min(420px, 100%);
      padding: 16px 18px;
      border: 1px solid var(--line);
      border-radius: 10px;
      background: var(--panel);
      box-shadow: 0 16px 40px rgba(15, 23, 42, 0.12);
    }
    .modal-head {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 12px;
      font-size: 14px;
      color: var(--text);
    }
    .modal-close {
      border: none;
      background: transparent;
      color: var(--muted);
      font-size: 18px;
      line-height: 1;
      cursor: pointer;
      padding: 0 4px;
    }
    .apply-row {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 8px;
    }
    .captcha-wrap {
      display: flex;
      align-items: center;
      min-height: 32px;
      padding: 2px 4px;
      border: 1px solid var(--line);
      border-radius: 6px;
      background: #f9fafb;
      cursor: pointer;
    }
    .captcha-wrap svg {
      display: block;
      width: 100px;
      height: 32px;
    }
    .captcha-input {
      width: 72px;
      height: 32px;
      padding: 0 8px;
      border: 1px solid var(--line);
      border-radius: 6px;
      font-size: 13px;
      outline: none;
    }
    .captcha-input:focus {
      border-color: var(--accent);
      box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.12);
    }
    .submit-btn {
      height: 32px;
      padding: 0 12px;
      border: none;
      border-radius: 6px;
      background: var(--accent);
      color: #fff;
      font-size: 13px;
      cursor: pointer;
    }
    .submit-btn:hover:not(:disabled) {
      background: var(--accent-hover);
    }
    .submit-btn:disabled {
      opacity: 0.65;
      cursor: not-allowed;
    }
    .feedback {
      margin-top: 10px;
      font-size: 12px;
    }
    .feedback.error { color: var(--danger); }
    .feedback.success { color: var(--success); }
  </style>
</head>
<body>
  <main>
    <p class="code">403 Forbidden</p>
    <h1>你访问的页面暂时进不去</h1>
    <p>当前请求没有通过 IP 白名单校验。</p>
    <p class="note">
      <span>当前客户端 IP：{{encodedIp}}</span>
      <a class="apply-link" id="apply-link" href="#">申请加入</a>
    </p>
  </main>
  <div class="modal" id="apply-modal" aria-hidden="true">
    <div class="modal-card" role="dialog" aria-modal="true" aria-label="申请白名单">
      <div class="modal-head">
        <span>{{encodedModalHint}}</span>
        <button type="button" class="modal-close" id="modal-close" aria-label="关闭">×</button>
      </div>
      <div class="apply-row">
        <div class="captcha-wrap" id="captcha-box" title="点击刷新" role="button" tabindex="0" aria-label="验证码"></div>
        <input id="captcha-code" class="captcha-input" type="text" maxlength="8" placeholder="验证码" autocomplete="off" />
        <button type="button" class="submit-btn" id="submit-btn">提交</button>
      </div>
      <p class="feedback" id="feedback" hidden></p>
    </div>
  </div>
  <script>
    (function () {
      var captchaId = "";
      var modal = document.getElementById("apply-modal");
      var applyLink = document.getElementById("apply-link");
      var modalClose = document.getElementById("modal-close");
      var captchaBox = document.getElementById("captcha-box");
      var captchaInput = document.getElementById("captcha-code");
      var submitBtn = document.getElementById("submit-btn");
      var feedback = document.getElementById("feedback");

      function showFeedback(text, type) {
        feedback.textContent = text;
        feedback.className = "feedback " + type;
        feedback.hidden = false;
      }

      function openModal() {
        modal.classList.add("open");
        modal.setAttribute("aria-hidden", "false");
        feedback.hidden = true;
        captchaInput.value = "";
        loadCaptcha();
        setTimeout(function () { captchaInput.focus(); }, 0);
      }

      function closeModal() {
        modal.classList.remove("open");
        modal.setAttribute("aria-hidden", "true");
      }

      function loadCaptcha() {
        captchaBox.innerHTML = "<span style=\"font-size:12px;color:#6b7280\">…</span>";
        fetch("/api/ip-whitelist/captcha", { credentials: "same-origin" })
          .then(function (res) { return res.json(); })
          .then(function (result) {
            if (!result || result.code !== 0 || !result.data) {
              captchaBox.textContent = "失败";
              return;
            }
            captchaId = result.data.id || "";
            captchaBox.innerHTML = result.data.svg || "";
          })
          .catch(function () {
            captchaBox.textContent = "失败";
          });
      }

      function submitApply() {
        if (!captchaId) {
          showFeedback("请刷新验证码", "error");
          return;
        }
        var code = (captchaInput.value || "").trim();
        if (!code) {
          showFeedback("请输入验证码", "error");
          return;
        }

        submitBtn.disabled = true;
        feedback.hidden = true;

        fetch("/api/ip-whitelist/apply", {
          method: "POST",
          credentials: "same-origin",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ captchaId: captchaId, captchaCode: code })
        })
          .then(function (res) { return res.json(); })
          .then(function (result) {
            if (!result) {
              showFeedback("提交失败", "error");
              return;
            }
            if (result.code === 0) {
              showFeedback(result.message || "已提交", "success");
              captchaInput.value = "";
            } else {
              showFeedback(result.message || "提交失败", "error");
            }
            loadCaptcha();
          })
          .catch(function () {
            showFeedback("提交失败", "error");
            loadCaptcha();
          })
          .finally(function () {
            submitBtn.disabled = false;
          });
      }

      applyLink.addEventListener("click", function (e) {
        e.preventDefault();
        openModal();
      });
      modalClose.addEventListener("click", closeModal);
      modal.addEventListener("click", function (e) {
        if (e.target === modal) {
          closeModal();
        }
      });
      document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && modal.classList.contains("open")) {
          closeModal();
        }
      });
      captchaBox.addEventListener("click", loadCaptcha);
      captchaBox.addEventListener("keydown", function (e) {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          loadCaptcha();
        }
      });
      submitBtn.addEventListener("click", submitApply);
      captchaInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
          submitApply();
        }
      });
    })();
  </script>
</body>
</html>
""");
    }
}
