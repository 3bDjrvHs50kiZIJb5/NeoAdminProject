import { test, expect } from './fixtures';

const stepTimeout = 5_000;

test.describe('系统日志', () => {
  test('已登录用户可访问系统日志页', async ({ page }) => {
    await page.goto('/admin/system-log');

    await expect(page).toHaveURL(/\/admin\/system-log/);
    await expect(page.getByRole('heading', { name: '系统日志', level: 2 })).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByRole('button', { name: '刷新' })).toBeVisible({ timeout: stepTimeout });
  });
});
