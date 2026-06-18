import { test, expect } from './fixtures';
import { expectCrudPage, searchCrudTable } from './helpers/page';

test.describe('技术频道', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/Blog/Channel');
    await expectCrudPage(page, '技术频道');
  });

  test('列表展示种子频道数据', async ({ page }) => {
    await expect(page.getByText('.NET')).toBeVisible();
    await expect(page.getByText('前端')).toBeVisible();
    await expect(page.getByText('数据库')).toBeVisible();
  });

  test('可按频道名称搜索', async ({ page }) => {
    await searchCrudTable(page, /频道名称/, '数据库');

    await expect(page.getByText('数据库')).toBeVisible();
    await expect(page.getByText('.NET')).not.toBeVisible();
  });
});
