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

  test('输入项目名称后创建命令同步更新', async ({ page }) => {
    await page.goto('/');

    const nameInput = page.getByRole('textbox', { name: '项目名称' });
    const command = page.locator('#create-project-command');

    await expect(nameInput).toHaveValue('MyAdmin');
    await expect(command).toHaveText('dotnet new neoadmin -n MyAdmin -o .');

    await nameInput.fill('ShopAdmin');
    await expect(command).toHaveText('dotnet new neoadmin -n ShopAdmin -o .');

    await nameInput.fill('   ');
    await expect(command).toHaveText('dotnet new neoadmin -n MyAdmin -o .');
  });

  test('从首页进入登录页', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: '进入后台' }).click();

    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByRole('heading', { name: '欢迎回来' })).toBeVisible();
  });
});
