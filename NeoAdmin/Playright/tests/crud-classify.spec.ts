import { test, expect } from './fixtures';
import { expectCrudPage, searchCrudTable } from './helpers/page';

const stepTimeout = 5_000;

async function createClassify(page: import('@playwright/test').Page, name: string) {
  await page.getByRole('button', { name: '新增' }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible({ timeout: stepTimeout });
  await dialog.getByLabel('分类专栏名称').fill(name);
  await page.getByRole('button', { name: '保存' }).click();
  await expect(dialog).toBeHidden({ timeout: stepTimeout });
  await expect(page.getByRole('row').filter({ hasText: name }).first()).toBeVisible({ timeout: stepTimeout });
}

async function selectRowByName(page: import('@playwright/test').Page, name: string) {
  const row = page.getByRole('row').filter({ hasText: name }).first();
  await row.getByRole('checkbox', { name: 'Select this row' }).click();
}

async function getToolbarDeleteButton(page: import('@playwright/test').Page) {
  return page.locator('.crud-table').getByRole('button', { name: /^删除( \(\d+\))?$/ }).first();
}

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

  test('批量删除后再次勾选时删除计数从0重新计算', async ({ page }) => {
    const prefix = `E2E删除计数${Date.now()}`;
    const firstBatch = [`${prefix}A`, `${prefix}B`];
    const secondBatch = [`${prefix}C`, `${prefix}D`];

    for (const name of firstBatch) {
      await createClassify(page, name);
    }

    await searchCrudTable(page, /专栏名称/, prefix);

    for (const name of firstBatch) {
      await selectRowByName(page, name);
    }

    const deleteButton = await getToolbarDeleteButton(page);
    await expect(deleteButton).toHaveText('删除 (2)', { timeout: stepTimeout });
    await deleteButton.click();

    const confirmDialog = page.getByRole('dialog').filter({ hasText: '确认批量删除' });
    await expect(confirmDialog).toBeVisible({ timeout: stepTimeout });
    await expect(confirmDialog).toContainText('确定要删除 2 行记录');
    await confirmDialog.getByRole('button', { name: '删除' }).click();
    await expect(confirmDialog).toBeHidden({ timeout: stepTimeout });

    for (const name of firstBatch) {
      await expect(page.getByRole('row').filter({ hasText: name })).toHaveCount(0, { timeout: stepTimeout });
    }

    for (const name of secondBatch) {
      await createClassify(page, name);
    }

    await searchCrudTable(page, /专栏名称/, prefix);

    for (const name of secondBatch) {
      await selectRowByName(page, name);
    }

    await expect(deleteButton).toHaveText('删除 (2)', { timeout: stepTimeout });
    await expect(deleteButton).not.toHaveText('删除 (4)');

    await deleteButton.click();
    await expect(confirmDialog).toBeVisible({ timeout: stepTimeout });
    await expect(confirmDialog).toContainText('确定要删除 2 行记录');
    await confirmDialog.getByRole('button', { name: '删除' }).click();
    await expect(confirmDialog).toBeHidden({ timeout: stepTimeout });
  });
});
