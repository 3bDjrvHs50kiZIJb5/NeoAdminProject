import { mkdirSync } from 'node:fs';
import { dirname } from 'node:path';
import { test as setup } from '@playwright/test';
import { loginAsAdmin } from './helpers/auth';

const authFile = 'output/playwright/.auth/admin.json';

setup('保存管理员登录态', async ({ page }) => {
  mkdirSync(dirname(authFile), { recursive: true });
  await loginAsAdmin(page);
  await page.context().storageState({ path: authFile });
});
