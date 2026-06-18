import { test, expect } from './fixtures';
import { expectCrudPage, searchCrudTable } from './helpers/page';

test.describe('随笔专栏', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/Blog/Classify');
    await expectCrudPage(page, '随笔专栏');
  });

  test('列表展示种子专栏数据', async ({ page }) => {
    await expect(page.getByRole('cell', { name: 'FreeSql', exact: true })).toBeVisible();
    await expect(page.getByRole('cell', { name: 'FreeRedis', exact: true })).toBeVisible();
    await expect(page.getByRole('cell', { name: 'NeoAdmin', exact: true })).toBeVisible();
  });

  test('可按名称搜索过滤', async ({ page }) => {
    await searchCrudTable(page, /专栏名称/, 'NeoAdmin');

    await expect(page.getByRole('cell', { name: 'NeoAdmin', exact: true })).toBeVisible();
    await expect(page.getByRole('cell', { name: 'FreeSql', exact: true })).not.toBeVisible();
  });
});
