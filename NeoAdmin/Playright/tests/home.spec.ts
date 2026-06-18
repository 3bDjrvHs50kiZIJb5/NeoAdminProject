import { test, expect } from './fixtures';
import { configureGuestTests } from './helpers/guest-setup';

configureGuestTests();

test.describe('首页', () => {
  test('展示站点标题与产品介绍', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('heading', { name: 'NeoAdmin', level: 1 })).toBeVisible();
    await expect(page.getByText('NeoAdmin 是一个基于 Blazor Server、NeoUI 与 FreeSql 的后台管理框架。')).toBeVisible();
    await expect(page.getByText('快速开始')).toBeVisible();
  });

  test('从首页进入登录页', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: '进入后台' }).click();

    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByText('欢迎回来')).toBeVisible();
  });
});
