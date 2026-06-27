namespace NeoAdmin.Components.Pages.DemoComp.Snippets;

internal static class FileUploadApiDemoSnippets
{
    public const string CSharp = """
        @inject FileApiClient FileApiClient

        private async Task UploadCoverAsync(IBrowserFile file)
        {
            ApiResult<FileUploadResponse> result = await FileApiClient.UploadAsync(
                file,
                directory: "blog/cover");

            if (!result.Succeeded || result.Data is null)
            {
                ToastService.Error("上传失败", result.Message);
                return;
            }

            coverUrl = result.Data.LinkUrl;
            // result.Data.Provider == "AliyunOss" 表示已走 OSS
        }
        """;

    public const string Curl = """
        # 1. 登录拿 Token
        curl -s -X POST 'https://your-host/api/login/@Login' \
          -H 'Content-Type: application/json' \
          -d '{"username":"admin","password":"admin"}'

        # 2. 上传文件（multipart，字段名 file）
        curl -X POST 'https://your-host/api/file/@Upload' \
          -H 'Authorization: Bearer <token>' \
          -F 'file=@./cover.png' \
          -F 'directory=blog/cover'
        """;

    public const string Fetch = """
        const token = localStorage.getItem('neoadmin:token');

        const form = new FormData();
        form.append('file', fileInput.files[0]);
        form.append('directory', 'blog/cover');

        const response = await fetch('/api/file/@Upload', {
          method: 'POST',
          headers: { Authorization: `Bearer ${token}` },
          body: form
        });

        const result = await response.json();
        if (result.code === 0) {
          console.log(result.data.linkUrl);
        }
        """;
}
