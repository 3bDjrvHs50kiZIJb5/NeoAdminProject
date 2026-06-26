import { test, expect } from './fixtures';
import {
  expectApiLoginFailure,
  expectApiLoginSuccess,
  registerViaApi,
} from './helpers/api-login';
import { expectCrudPage, searchCrudTable } from './helpers/page';
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

    await row.getByRole('button', { name: '编辑' }).click();
    const editDialog = page.getByRole('dialog');
    await expect(editDialog).toBeVisible({ timeout: stepTimeout });
    await expect(editDialog.getByText('手动添加')).toBeVisible({ timeout: stepTimeout });
    await editDialog.getByRole('button', { name: '取消' }).click();
  });

  test('登录日志按钮打开弹窗', async ({ page }) => {
    await page.getByRole('row').filter({ hasText: 'admin' }).getByRole('button', { name: '日志' }).click();

    const logDialog = page.getByRole('dialog').filter({ hasText: '登录日志' });
    await expect(logDialog).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText(/共 \d+ 条记录/)).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByRole('columnheader', { name: '时间' })).toBeVisible({ timeout: stepTimeout });
  });

  test('API 登录会写入登录日志', async ({ page, request }) => {
    const username = uniqueUsername();
    const password = 'Test1234';

    await registerViaApi(request, username, password);
    await expectApiLoginSuccess(request, username, password);
    await expectApiLoginFailure(request, username, 'wrong-password');

    await page.goto('/admin/user');
    await expectCrudPage(page, '用户管理');
    await searchCrudTable(page, '账号/姓名..', username);

    const row = page.getByRole('row').filter({ hasText: username }).first();
    await expect(row).toBeVisible({ timeout: stepTimeout });
    await row.getByRole('button', { name: '日志' }).click();

    const logDialog = page.getByRole('dialog').filter({ hasText: '登录日志' });
    await expect(logDialog).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText(/共 [1-9]\d* 条记录/)).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText('登陆成功').first()).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText('登陆失败').first()).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText('API register')).toBeVisible({ timeout: stepTimeout });
    await expect(logDialog.getByText(/failed:\d+/).first()).toBeVisible({ timeout: stepTimeout });
  });
});
