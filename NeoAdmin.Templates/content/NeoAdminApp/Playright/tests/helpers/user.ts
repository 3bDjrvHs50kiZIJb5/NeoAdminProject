/** 生成唯一用户名，避免与种子数据冲突 */
export function uniqueUsername() {
  return `e2e_${Date.now()}`;
}
