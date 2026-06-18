import { expect, type Page } from '@playwright/test';

/** 等待 CrudTable 页面标题与工具栏就绪 */
export async function expectCrudPage(page: Page, title: string) {
  await expect(page.getByRole('heading', { name: title, level: 2 })).toBeVisible();
  await expect(page.getByRole('button', { name: '新增' })).toBeVisible();
}

/** 在 CrudTable 搜索框输入关键词并提交，等待分页信息更新 */
export async function searchCrudTable(page: Page, placeholder: RegExp | string, keyword: string) {
  const search = page.getByPlaceholder(placeholder);
  await search.fill(keyword);
  await search.press('Enter');
  await expect(page.getByText(/共 \d+ 条/)).toBeVisible();
}

/** 通过侧边栏链接进入后台页面 */
export async function openSidebarPage(page: Page, label: string) {
  await page.getByRole('link', { name: label }).click();
}
