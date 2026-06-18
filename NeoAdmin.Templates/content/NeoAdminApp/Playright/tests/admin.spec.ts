import { test, expect } from './fixtures';

test.describe('后台首页', () => {
  test('已登录用户可访问后台首页', async ({ page }) => {
    await page.goto('/Admin');

    await expect(page).toHaveURL(/\/Admin/);
    await expect(page.getByRole('heading', { name: '基础设施监控', level: 2 })).toBeVisible();
  });

  test('展示服务器健康概览', async ({ page }) => {
    await page.goto('/Admin');

    await expect(page.getByText('系统运行正常')).toBeVisible();
    await expect(page.getByText('服务器健康')).toBeVisible();
    await expect(page.getByText('web-prod-01')).toBeVisible();
  });

  test('展示统计卡片', async ({ page }) => {
    await page.goto('/Admin');

    await expect(page.getByText('平均 CPU')).toBeVisible();
    await expect(page.getByText('平均内存')).toBeVisible();
    await expect(page.getByText('可用率')).toBeVisible();
  });
});
