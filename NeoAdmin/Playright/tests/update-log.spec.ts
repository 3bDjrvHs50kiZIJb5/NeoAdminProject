import { test, expect } from './fixtures';

const stepTimeout = 5_000;

test.describe('NeoUpdateLog 更新日志', () => {
  test('演示页从 JSON 文件展示更新日志时间线', async ({ page }) => {
    await page.goto('/neo-demo/comp/update-log', { timeout: stepTimeout });
    await expect(page.getByText('NeoUpdateLog 更新日志', { exact: true })).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByText('读取 JSON 文件', { exact: true })).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByText('最近提交（最多 15 条）')).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByText('chore: 发布 v1.0.36')).toBeVisible({ timeout: stepTimeout });
  });
});
