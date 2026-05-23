using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NoAdmin.Blazor.Infrastructure.Encrypt;

/// <summary>
/// MD5加密
/// </summary>
public class Md5Encrypt
{
	/// <summary>
	/// 16位MD5加密
	/// </summary>
	/// <param name="password"></param>
	/// <param name="lowerCase"></param>
	/// <returns></returns>
	public static string Encrypt16(string password, bool lowerCase = false)
	{
		if (password.IsNull())
		{
			return null;
		}
		using MD5 mD = MD5.Create();
		return mD.ComputeHash(Encoding.UTF8.GetBytes(password)).ToHex(lowerCase);
	}

	/// <summary>
	/// 32位MD5加密
	/// </summary>
	/// <param name="password"></param>
	/// <param name="lowerCase"></param>
	/// <returns></returns>
	public static string Encrypt32(string password = "", bool lowerCase = false)
	{
		if (password.IsNull())
		{
			return null;
		}
		using MD5 mD = MD5.Create();
		string text = string.Empty;
		byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(password));
		string text2 = (lowerCase ? "x2" : "X2");
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += b.ToString(text2);
		}
		return text;
	}

	/// <summary>
	/// 64位MD5加密
	/// </summary>
	/// <param name="password"></param>
	/// <returns></returns>
	public static string Encrypt64(string password)
	{
		if (password.IsNull())
		{
			return null;
		}
		using MD5 mD = MD5.Create();
		byte[] inArray = mD.ComputeHash(Encoding.UTF8.GetBytes(password));
		return Convert.ToBase64String(inArray);
	}

	public static string GetHash(Stream stream)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using MD5 mD = MD5.Create();
		byte[] array = mD.ComputeHash(stream);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
