import { expect, type Page } from '@playwright/test';

const adminUsername = process.env.E2E_ADMIN_USER ?? 'admin';
const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? 'admin';

export async function loginAsAdmin(page: Page) {
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: '欢迎回来' })).toBeVisible();

  await page.locator('#userName').fill(adminUsername);
  await page.locator('#password').fill(adminPassword);
  await page.getByRole('button', { name: '登录' }).click();

  await expect(page).toHaveURL(/\/Admin/);
}
