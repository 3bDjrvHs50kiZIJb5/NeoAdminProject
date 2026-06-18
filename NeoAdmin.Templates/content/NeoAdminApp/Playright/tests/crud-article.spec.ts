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
});
