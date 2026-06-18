import { test, expect } from './fixtures';
import { expectCrudPage, openSidebarPage } from './helpers/page';

test.describe('后台导航', () => {
  test('侧边栏可进入分类页', async ({ page }) => {
    await page.goto('/Admin');
    await openSidebarPage(page, '分类');

    await expect(page).toHaveURL(/\/Blog\/Classify/);
    await expectCrudPage(page, '随笔专栏');
  });

  test('侧边栏可进入频道页', async ({ page }) => {
    await page.goto('/Admin');
    await openSidebarPage(page, '频道');

    await expect(page).toHaveURL(/\/Blog\/Channel/);
    await expectCrudPage(page, '技术频道');
  });

  test('侧边栏可进入文章页', async ({ page }) => {
    await page.goto('/Admin');
    await openSidebarPage(page, '文章');

    await expect(page).toHaveURL(/\/Blog\/Article/);
    await expectCrudPage(page, '随笔文章');
  });

  test('侧边栏可进入用户管理页', async ({ page }) => {
    await page.goto('/Admin');
    await openSidebarPage(page, '用户管理');

    await expect(page).toHaveURL(/\/admin\/user/);
    await expectCrudPage(page, '用户管理');
  });

  test('侧边栏可进入角色管理页', async ({ page }) => {
    await page.goto('/Admin');
    await openSidebarPage(page, '角色管理');

    await expect(page).toHaveURL(/\/admin\/role/);
    await expectCrudPage(page, '角色管理');
  });

  test('侧边栏可进入字典管理页', async ({ page }) => {
    await page.goto('/Admin');
    await openSidebarPage(page, '字典管理');

    await expect(page).toHaveURL(/\/admin\/dict/);
    await expectCrudPage(page, '字典分类');
  });
});
