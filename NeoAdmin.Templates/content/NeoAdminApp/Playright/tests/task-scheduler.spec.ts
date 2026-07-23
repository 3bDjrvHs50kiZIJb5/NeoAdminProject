import { test, expect } from './fixtures';

test.describe('定时任务', () => {
  test('已登录用户可访问定时任务页', async ({ page }) => {
    test.setTimeout(15_000);

    await page.goto('/admin/task-scheduler', { waitUntil: 'domcontentloaded', timeout: 10_000 });

    await expect(page.getByRole('heading', { name: '定时任务' })).toBeVisible({ timeout: 10_000 });
    await expect(page.getByText('时区:')).toBeVisible({ timeout: 10_000 });

    // SchedulerAutoLoad=true（如 Development.json 显式开启）时不展示提示；
    // 未配置时开发环境默认关闭，应展示默认提示文案。
    const defaultTip = page.getByText('开发环境默认不自动加载执行任务');
    const configuredTip = page.getByText('当前配置不自动加载执行任务');
    const tipVisible = (await defaultTip.count()) > 0 || (await configuredTip.count()) > 0;

    if (tipVisible) {
      await expect(defaultTip.or(configuredTip).first()).toBeVisible();
    } else {
      await expect(page.getByText('未启动任务（开发环境）')).toHaveCount(0);
    }
  });
});
