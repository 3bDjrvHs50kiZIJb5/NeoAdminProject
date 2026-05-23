using System;
using System.Security.Cryptography;

namespace NoAdmin.Blazor.Infrastructure.Encrypt;

/// <summary>
/// 安全的 UUID v7 生成器
/// 基于 RFC 9562 规范，结合时间戳和加密安全随机数
/// </summary>
public static class GuidGenerator
{
	/// <summary>
	/// 生成 UUID v7（时间有序 + 加密安全随机）
	/// 格式: TTTTTTTT-TTTT-7RRR-VRRR-RRRRRRRRRRRR
	/// T = 时间戳 (48 bit, milliseconds since Unix epoch)
	/// 7 = 版本号 (固定为 7)
	/// V = 变体位 (固定为 10xx)
	/// R = 加密安全随机位
	/// </summary>
	public static Guid CreateVersion7()
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		Span<byte> span = stackalloc byte[16];
		RandomNumberGenerator.Fill(span);
		span[0] = (byte)(num >> 40);
		span[1] = (byte)(num >> 32);
		span[2] = (byte)(num >> 24);
		span[3] = (byte)(num >> 16);
		span[4] = (byte)(num >> 8);
		span[5] = (byte)num;
		span[6] = (byte)((span[6] & 0xF) | 0x70);
		span[8] = (byte)((span[8] & 0x3F) | 0x80);
		return new Guid(span);
	}

	/// <summary>
	/// 从 UUID v7 中提取时间戳
	/// </summary>
	public static DateTimeOffset? ExtractTimestamp(Guid uuid)
	{
		byte[] array = uuid.ToByteArray();
		if (array[7] >> 4 != 7)
		{
			return null;
		}
		long milliseconds = (long)(((ulong)array[3] << 40) | ((ulong)array[2] << 32) | ((ulong)array[1] << 24) | ((ulong)array[0] << 16) | ((ulong)array[5] << 8) | array[4]);
		return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
	}

	/// <summary>
	/// 验证是否为有效的 UUID v7
	/// </summary>
	public static bool IsVersion7(Guid uuid)
	{
		byte[] array = uuid.ToByteArray();
		return array[7] >> 4 == 7;
	}
}
