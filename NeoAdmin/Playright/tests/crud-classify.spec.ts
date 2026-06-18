import { test, expect } from './fixtures';
import { expectCrudPage, searchCrudTable } from './helpers/page';

test.describe('随笔专栏', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/Blog/Classify');
    await expectCrudPage(page, '随笔专栏');
  });

  test('列表展示种子专栏数据', async ({ page }) => {
    await expect(page.getByText('FreeSql')).toBeVisible();
    await expect(page.getByText('FreeRedis')).toBeVisible();
    await expect(page.getByText('NeoAdmin')).toBeVisible();
  });

  test('可按名称搜索过滤', async ({ page }) => {
    await searchCrudTable(page, /专栏名称/, 'NeoAdmin');

    await expect(page.getByText('NeoAdmin')).toBeVisible();
    await expect(page.getByText('FreeSql')).not.toBeVisible();
  });
});
