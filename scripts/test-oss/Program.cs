using System.Net.Http;
using System.Text;
using System.Text.Json;
using Aliyun.OSS;

const string appsettingsPath = "NeoAdmin/appsettings.json";
if (!File.Exists(appsettingsPath))
{
    Console.Error.WriteLine($"找不到配置文件：{appsettingsPath}");
    return 1;
}

using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
JsonElement oss = doc.RootElement
    .GetProperty("NeoAdmin")
    .GetProperty("FileUpload")
    .GetProperty("Oss");

string endpoint = oss.GetProperty("Endpoint").GetString() ?? "";
string accessKeyId = oss.GetProperty("AccessKeyId").GetString() ?? "";
string accessKeySecret = oss.GetProperty("AccessKeySecret").GetString() ?? "";
string bucketName = oss.GetProperty("BucketName").GetString() ?? "";
string customDomain = oss.GetProperty("CustomDomain").GetString() ?? "";
string prefix = oss.GetProperty("Prefix").GetString() ?? "";

Console.WriteLine("=== NeoAdmin OSS 配置测试 ===");
Console.WriteLine($"Endpoint      : {endpoint}");
Console.WriteLine($"BucketName    : {bucketName}");
Console.WriteLine($"CustomDomain  : {customDomain}");
Console.WriteLine($"Prefix        : {prefix}");
Console.WriteLine();

if (string.IsNullOrWhiteSpace(endpoint)
    || string.IsNullOrWhiteSpace(accessKeyId)
    || string.IsNullOrWhiteSpace(accessKeySecret)
    || string.IsNullOrWhiteSpace(bucketName))
{
    Console.Error.WriteLine("失败：OSS 必填项未完整配置。");
    return 1;
}

string objectKey = $"{prefix.TrimEnd('/')}/_test/neoadmin-oss-test-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
byte[] content = Encoding.UTF8.GetBytes($"NeoAdmin OSS test at {DateTime.Now:O}");

try
{
    OssClient client = new(endpoint, accessKeyId, accessKeySecret);

    Console.WriteLine("1. 上传测试文件…");
    using (MemoryStream stream = new(content))
    {
        client.PutObject(bucketName, objectKey, stream);
    }
    Console.WriteLine($"   上传成功，ObjectKey={objectKey}");

    Console.WriteLine("2. 校验对象是否存在…");
    bool exists = client.DoesObjectExist(bucketName, objectKey);
    Console.WriteLine($"   DoesObjectExist = {exists}");
    if (!exists)
    {
        Console.Error.WriteLine("失败：上传后对象不存在。");
        return 1;
    }

    string publicUrl = string.IsNullOrWhiteSpace(customDomain)
        ? $"https://{bucketName}.{endpoint.TrimStart("https://").TrimStart("http://").TrimEnd('/')}/{objectKey}"
        : $"https://{customDomain.Trim().TrimStart("https://").TrimStart("http://").TrimEnd('/')}/{objectKey}";

    Console.WriteLine("3. 访问公开 URL…");
    Console.WriteLine($"   URL: {publicUrl}");

    using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(10) };
    using HttpResponseMessage response = await http.GetAsync(publicUrl);
    string body = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"   HTTP {(int)response.StatusCode} {response.StatusCode}");
    Console.WriteLine($"   Body: {body}");

    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine("警告：公开 URL 无法访问（Bucket 可能未开启公共读，上传本身仍可能成功）。");
    }
    else if (!body.Contains("NeoAdmin OSS test", StringComparison.Ordinal))
    {
        Console.Error.WriteLine("警告：公开 URL 返回内容与上传不一致。");
    }
    else
    {
        Console.WriteLine("   公开访问正常。");
    }

    Console.WriteLine("4. 删除测试文件…");
    client.DeleteObject(bucketName, objectKey);
    bool stillExists = client.DoesObjectExist(bucketName, objectKey);
    Console.WriteLine($"   删除后仍存在 = {stillExists}");

    Console.WriteLine();
    Console.WriteLine(stillExists ? "失败：测试文件未能删除。" : "全部通过：OSS 配置可用。");
    return stillExists ? 1 : 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"失败：{ex.GetType().Name}: {ex.Message}");
    return 1;
}
