import { test, expect } from './fixtures';
import { expectCrudPage } from './helpers/page';
import { uniqueUsername } from './helpers/user';

const stepTimeout = 5_000;

test.describe('用户管理', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/user');
    await expectCrudPage(page, '用户管理');
  });

  test('列表展示种子管理员用户', async ({ page }) => {
    await expect(page.getByRole('row').filter({ hasText: 'admin' }).first()).toBeVisible({ timeout: stepTimeout });
  });

  test('新建用户未填备注时保存后默认为手动添加', async ({ page }) => {
    const username = uniqueUsername();
    const password = 'Test1234';
    const nickname = `E2E用户${Date.now()}`;

    await page.getByRole('button', { name: '新增' }).click();
    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible({ timeout: stepTimeout });

    const textboxes = dialog.getByRole('textbox');
    await textboxes.nth(0).fill(username);
    await textboxes.nth(1).fill(nickname);
    await dialog.getByPlaceholder('请输入密码').fill(password);

    await page.getByRole('button', { name: '保存' }).click();
    await expect(dialog).toBeHidden({ timeout: stepTimeout });

    const row = page.getByRole('row').filter({ hasText: username }).first();
    await expect(row).toBeVisible({ timeout: stepTimeout });
    await expect(row).toContainText('手动添加', { timeout: stepTimeout });
  });

  test('登录日志按钮打开弹窗', async ({ page }) => {
    await page.getByRole('row').filter({ hasText: 'admin' }).getByRole('button', { name: '日志' }).click();

    const logDialog = page.getByRole('dialog').filter({ hasText: '登录日志' });
    await expect(logDialog).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText(/共 \d+ 条记录/)).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByRole('columnheader', { name: '时间' })).toBeVisible({ timeout: stepTimeout });
  });
});
