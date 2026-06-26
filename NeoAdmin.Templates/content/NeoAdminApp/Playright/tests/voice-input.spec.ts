import { test, expect } from './fixtures';

const stepTimeout = 5_000;

test.describe('NeoVoiceInput 语音输入', () => {
  test('演示页渲染文本框与麦克风按钮', async ({ page }) => {
    await page.goto('/neo-demo/comp/voice-input', { timeout: stepTimeout });
    await expect(page.getByText('NeoVoiceInput 语音输入', { exact: true })).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByLabel('备注内容')).toBeVisible({ timeout: stepTimeout });
    await expect(page.getByRole('button', { name: '开始语音录入' })).toBeVisible({ timeout: stepTimeout });
  });
});
