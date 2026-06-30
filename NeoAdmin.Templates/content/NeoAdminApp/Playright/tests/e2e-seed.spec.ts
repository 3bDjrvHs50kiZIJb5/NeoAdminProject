import { expect, test } from '@playwright/test';

test('E2E 种子接口可调用', async ({ request }) => {
  const response = await request.post('/api/e2e/seed/ensure');
  expect(response.ok()).toBeTruthy();
});
