import { test, expect } from './fixtures';
import { expectCrudPage, searchCrudTable } from './helpers/page';

const stepTimeout = 5_000;

test.describe('随笔文章', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/Blog/Article');
    await expectCrudPage(page, '随笔文章');
  });

  test('列表展示种子文章数据', async ({ page }) => {
    await expect(page.getByRole('row', { name: /模拟文章 50/ }).first()).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByText(/共 50 条/)).toBeVisible({ timeout: stepTimeout });
  });

  test('可按标题搜索过滤', async ({ page }) => {
    await searchCrudTable(page, /标题 \/ 关键字/, '模拟文章 50');

    await expect(page.getByRole('row', { name: /模拟文章 50/ }).first()).toBeVisible({
      timeout: stepTimeout,
    });
    await expect(page.getByRole('row', { name: /模拟文章 49/ })).toHaveCount(0, { timeout: stepTimeout });
  });

  test('已通过审批记录双击打开查看模式', async ({ page }) => {
    const row = page.getByRole('row', { name: /模拟文章 50/ }).first();
    await row.dblclick({ timeout: stepTimeout });

    const dialog = page.getByRole('dialog');
    await expect(dialog.getByRole('heading', { name: /查看 随笔文章/ })).toBeVisible({ timeout: stepTimeout });
    await expect(dialog.getByRole('button', { name: '保存' })).toHaveCount(0, { timeout: stepTimeout });
    await expect(dialog.getByRole('button', { name: '关闭' })).toBeVisible({ timeout: stepTimeout });
  });

  test('已通过审批记录可切换查看弹窗标签页', async ({ page }) => {
    const row = page.getByRole('row', { name: /模拟文章 50/ }).first();
    await row.dblclick({ timeout: stepTimeout });

    const dialog = page.getByRole('dialog');
    await dialog.getByRole('tab', { name: '审批' }).click({ timeout: stepTimeout });
    await expect(dialog.getByText('审批状态')).toBeVisible({ timeout: stepTimeout });

    await dialog.getByRole('tab', { name: '设置' }).click({ timeout: stepTimeout });
    await expect(dialog.getByText('前台展示（审批通过后可勾选）')).toBeVisible({ timeout: stepTimeout });

    await dialog.getByRole('tab', { name: '随笔' }).click({ timeout: stepTimeout });
    await expect(dialog.getByLabel('标题')).toBeVisible({ timeout: stepTimeout });
  });
});
