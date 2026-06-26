import { test, expect } from './fixtures';
import { configureGuestTests } from './helpers/guest-setup';

const defaultLogoPattern = /\/_content\/NeoAdmin\.Blazor\/images\/logo\.png/;
const defaultLoginBgPattern = /\/_content\/NeoAdmin\.Blazor\/images\/login_bg\.png/;

test.describe('站点品牌默认图', () => {
  configureGuestTests();

  test('登录页未配置时展示默认 LOGO 与背景', async ({ page }) => {
    test.setTimeout(5_000);

    await page.goto('/login');

    await expect(page.locator('img[src*="logo.png"]').first()).toBeVisible();
    await expect(page.locator('img[src*="login_bg.png"]').first()).toBeVisible();
    await expect(page.locator('img[src*="logo.png"]').first()).toHaveAttribute('src', defaultLogoPattern);
    await expect(page.locator('img[src*="login_bg.png"]').first()).toHaveAttribute('src', defaultLoginBgPattern);
  });
});

test.describe('站点设置默认图预览', () => {
  test('品牌图片区展示默认 LOGO 与登录页配图预览', async ({ page }) => {
    test.setTimeout(5_000);

    await page.goto('/admin/site-settings');

    await expect(page.locator('section').getByRole('heading', { name: '站点设置' })).toBeVisible();
    await expect(page.locator('img[alt="LOGO 预览"]')).toBeVisible();
    await expect(page.locator('img[alt="登录页预览"]')).toBeVisible();
    await expect(page.locator('img[alt="LOGO 预览"]')).toHaveAttribute('src', defaultLogoPattern);
    await expect(page.locator('img[alt="登录页预览"]')).toHaveAttribute('src', defaultLoginBgPattern);
  });

  test('说明字段使用 Textarea', async ({ page }) => {
    test.setTimeout(5_000);

    await page.goto('/admin/site-settings');

    const description = page.getByLabel('说明');
    await expect(description).toBeVisible();
    await expect(description).toHaveAttribute('maxlength', '500');
    await expect(page.locator('.ql-editor')).toHaveCount(0);
  });
});
