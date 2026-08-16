import fs from 'node:fs';
import path from 'node:path';
import { test, expect } from './fixtures';

const stepTimeout = 5_000;

test.describe('NeoUpdateLog 更新日志', () => {
  test('演示页从 JSON 文件展示更新日志时间线', async ({ page }) => {
    const changelogPath = path.join(__dirname, '../../Data/git-changelog.json');
    const changelog = JSON.parse(fs.readFileSync(changelogPath, 'utf8')) as {
      subject: string;
      committedAt: string;
    }[];
    const first = changelog[0];
    expect(first?.subject).toBeTruthy();
    const firstDate = first!.committedAt.slice(0, 10);

    await page.goto('/neo-demo/comp/update-log', { timeout: stepTimeout });
    await expect(page.getByText('NeoUpdateLog 更新日志', { exact: true })).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByText('读取 JSON 文件', { exact: true })).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByText('最近提交（最多 15 条）')).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByRole('button', { name: `${first!.subject} ${firstDate}` })).toBeVisible({
      timeout: stepTimeout,
    });
  });
});
