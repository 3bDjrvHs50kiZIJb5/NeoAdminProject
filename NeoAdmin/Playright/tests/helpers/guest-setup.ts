import { test } from '../fixtures';

/** 未登录用例：使用空 storageState，与 setup 保存的管理员态隔离 */
export function configureGuestTests() {
  test.use({ storageState: { cookies: [], origins: [] } });
}
