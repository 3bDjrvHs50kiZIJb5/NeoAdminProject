import { expect, type APIRequestContext } from '@playwright/test';

type ApiResult<T = unknown> = {
  code: number;
  message: string;
  data?: T;
  succeeded?: boolean;
};

type LoginResponse = {
  token: string;
  user: {
    id: number;
    username: string;
    nickname: string;
  };
};

/** 通过 API 注册新用户 */
export async function registerViaApi(
  request: APIRequestContext,
  username: string,
  password: string,
  nickname?: string,
) {
  const response = await request.post('/api/login/@Register', {
    data: { username, password, nickname: nickname ?? username },
  });
  expect(response.ok()).toBeTruthy();
  const body = (await response.json()) as ApiResult;
  expect(body.code).toBe(0);
  return body;
}

/** 通过 API 登录，返回响应体 */
export async function loginViaApi(
  request: APIRequestContext,
  username: string,
  password: string,
) {
  const response = await request.post('/api/login/@Login', {
    data: { username, password },
  });
  expect(response.ok()).toBeTruthy();
  return (await response.json()) as ApiResult<LoginResponse>;
}

/** 断言 API 登录成功 */
export async function expectApiLoginSuccess(
  request: APIRequestContext,
  username: string,
  password: string,
) {
  const body = await loginViaApi(request, username, password);
  expect(body.code).toBe(0);
  expect(body.data?.user.username).toBe(username);
  expect(body.data?.token).toBeTruthy();
  return body;
}

/** 断言 API 登录失败 */
export async function expectApiLoginFailure(
  request: APIRequestContext,
  username: string,
  password: string,
) {
  const body = await loginViaApi(request, username, password);
  expect(body.code).not.toBe(0);
  expect(body.message).toContain('用户名或密码错误');
  return body;
}
