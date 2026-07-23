import { test, expect } from './fixtures';

test.describe('定时任务', () => {
  test('开发环境展示自动加载执行提示', async ({ page }) => {
    test.setTimeout(5_000);

    await page.goto('/admin/task-scheduler', { waitUntil: 'domcontentloaded', timeout: 5_000 });

    await expect(page.getByText('开发环境默认不自动加载执行任务')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByText('未启动任务（开发环境）')).toHaveCount(0, { timeout: 5_000 });
  });
});
