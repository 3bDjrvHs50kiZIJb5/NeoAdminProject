import { expect, type Page } from '@playwright/test';

/** 等待登录页 Blazor 表单就绪 */
export async function gotoLoginPage(page: Page) {
  await page.goto('/login');
  await expect(page.getByText('欢迎回来')).toBeVisible();
  await expect(page.locator('#userName')).toBeVisible();
}

function loginUsernameField(page: Page) {
  return page.getByRole('group').filter({ has: page.getByText('用户名', { exact: true }) });
}

function loginPasswordField(page: Page) {
  return page.getByRole('group').filter({ has: page.getByText('密码', { exact: true }) });
}

/** 断言登录表单某字段的校验提示（限定在 Field 内，避免与 placeholder 混淆） */
export async function expectLoginValidation(
  page: Page,
  field: 'username' | 'password',
  message: string,
) {
  const fieldLocator = field === 'username' ? loginUsernameField(page) : loginPasswordField(page);
  await expect(fieldLocator.getByText(message, { exact: true })).toBeVisible({ timeout: 10_000 });
}
