import path from 'node:path';
import { defineConfig, devices } from '@playwright/test';

const neoAdminDir = path.join(__dirname, '..');
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5040';

/** 通过环境变量覆盖 NeoAdmin 配置，避免在应用项目内放置 E2E 专用 appsettings。 */
const neoAdminE2eEnv: Record<string, string> = {
  // Development：dotnet run 即可提供 _framework 静态资源；Production 需先 publish
  ASPNETCORE_ENVIRONMENT: 'Development',
  ASPNETCORE_URLS: baseURL,
  NeoAdmin__DataType: 'Sqlite',
  NeoAdmin__ConnectionString: 'Data Source=neoadmin.e2e.db',
  NeoAdmin__AutoSyncStructure: 'true',
  NeoAdmin__EnableSeedData: 'true',
  NeoAdmin__SeedAdminUserName: 'admin',
  NeoAdmin__SeedAdminPassword: 'admin',
  NeoAdmin__MonitorCommand: 'false',
};

export default defineConfig({
  testDir: './tests',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: [['list'], ['html', { open: 'never', outputFolder: 'output/playwright/report' }]],
  outputDir: 'output/playwright/test-results',

  use: {
    baseURL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    { name: 'setup', testMatch: /.*\.setup\.ts/ },
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'output/playwright/.auth/admin.json',
      },
      dependencies: ['setup'],
      testIgnore: /.*\.setup\.ts/,
    },
  ],

  webServer: {
    command: `dotnet run --no-launch-profile --urls ${baseURL}`,
    cwd: neoAdminDir,
    url: baseURL,
    reuseExistingServer: !process.env.CI,
    timeout: 120_000,
    env: {
      ...process.env,
      ...neoAdminE2eEnv,
    },
  },
});
