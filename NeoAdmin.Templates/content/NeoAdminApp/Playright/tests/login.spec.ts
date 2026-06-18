import { test, expect } from './fixtures';
import { configureGuestTests } from './helpers/guest-setup';
import { expectLoginValidation, gotoLoginPage } from './helpers/login';

configureGuestTests();

test.describe('登录页', () => {
  test('展示登录表单', async ({ page }) => {
    await page.goto('/login');

    await expect(page.getByText('欢迎回来')).toBeVisible();
    await expect(page.locator('#userName')).toBeVisible();
    await expect(page.locator('#password')).toBeVisible();
    await expect(page.getByRole('button', { name: '登录' })).toBeVisible();
  });

  test('空表单显示校验提示', async ({ page }) => {
    test.setTimeout(10_000);

    await gotoLoginPage(page);

    // 先填密码再清空用户名，避免浏览器自动填充干扰校验断言
    await page.locator('#password').fill('test');
    await page.locator('#userName').clear();
    await expect(page.locator('#userName')).toHaveValue('');
    await page.getByRole('button', { name: '登录' }).click();
    await expectLoginValidation(page, 'username', '请输入用户名');

    await page.locator('#userName').fill('admin');
    await page.locator('#password').clear();
    await expect(page.locator('#password')).toHaveValue('');
    await page.getByRole('button', { name: '登录' }).click();
    await expectLoginValidation(page, 'password', '请输入密码');
  });

  test('错误密码提示登录失败', async ({ page }) => {
    await page.goto('/login');
    await page.locator('#userName').fill('admin');
    await page.locator('#password').fill('wrong-password');
    await page.getByRole('button', { name: '登录' }).click();

    await expect(page.getByText('用户名或密码错误')).toBeVisible();
    await expect(page).toHaveURL(/\/login/);
  });
});
