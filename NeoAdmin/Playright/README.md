# NeoAdmin Playwright E2E

本目录位于 `NeoAdmin/Playright/`，专门存放 Playwright 端到端测试。

## 目录关系

```
NeoAdmin/
├── test.sh                  # 在根目录执行 ./test.sh 即可跑 E2E（无头）
├── ...                      # Blazor 宿主应用（webServer 启动此项目根目录）
└── Playright/               # 测试代码与 Playwright 配置
    ├── playwright.config.ts # 含 E2E 环境变量（独立 SQLite：neoadmin.e2e.db）
    └── tests/
```

## 前置条件

- Node.js / npm
- .NET SDK
- 首次运行 `./test.sh` 会自动 `npm install` 并安装 Chromium

## 运行

在 **NeoAdmin 根目录**：

```bash
cd NeoAdmin
chmod +x test.sh    # 首次赋予执行权限
./test.sh           # 无头测试（自动 dotnet run，环境 E2E）
./test.sh --grep 登录  # 透传 playwright 参数
```

也可在 `Playright/` 下直接用 npm：

```bash
cd Playright
npm test              # 无头
npm run test:headed   # 有头（每条用例独立上下文）
npm run test:ui       # Playwright UI
npm run report        # 查看 HTML 报告
```

本地若已启动 NeoAdmin，测试会复用现有服务（非 CI）。手动启动时需带上与 `playwright.config.ts` 相同的环境变量，或使用 `npm test` 自动启动。

## 环境变量

| 变量 | 默认 | 说明 |
|------|------|------|
| `PLAYWRIGHT_BASE_URL` | `http://localhost:5040` | 被测站点地址（E2E 默认端口，避免与开发 5038 冲突） |
| `E2E_ADMIN_USER` | `admin` | 登录账号 |
| `E2E_ADMIN_PASSWORD` | `admin` | 登录密码 |

## 认证说明

NeoAdmin 将 Token 存在浏览器 `localStorage`（`neoadmin:token`），且与服务端 `LoginTime` 绑定。
`auth.setup.ts` 会登录一次并保存 `storageState`；**不要在其他用例里再次成功登录**，否则会使已保存的 Token 失效。

未登录用例（首页、登录页等）在 spec 文件顶部调用 `configureGuestTests()`，使用空 `storageState` 与管理员态隔离。

## 版本管理说明

本目录与 NeoAdmin 宿主应用同属一个 Git 仓库，随应用代码一起提交与发布。
