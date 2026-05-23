using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NoAdmin.Blazor.Infrastructure.Encrypt;

/// <summary>
/// Des加解密
/// </summary>
public class DesEncrypt
{
	private const string Key = "freesql!";

	/// <summary>
	/// DES+Base64加密
	/// <para>采用ECB、PKCS7</para>
	/// </summary>
	/// <param name="encryptString">加密字符串</param>
	/// <param name="key">秘钥</param>
	/// <returns></returns>
	public static string Encrypt(string encryptString, string key = null)
	{
		return Encrypt(encryptString, key, hex: false, lowerCase: true);
	}

	/// <summary>
	/// DES+Base64解密
	/// <para>采用ECB、PKCS7</para>
	/// </summary>
	/// <param name="decryptString">解密字符串</param>
	/// <param name="key">秘钥</param>
	/// <returns></returns>
	public static string Decrypt(string decryptString, string key = null)
	{
		return Decrypt(decryptString, key, hex: false);
	}

	/// <summary>
	/// DES+16进制加密
	/// <para>采用ECB、PKCS7</para>
	/// </summary>
	/// <param name="encryptString">加密字符串</param>
	/// <param name="key">秘钥</param>
	/// <param name="lowerCase">是否小写</param>
	/// <returns></returns>
	public static string Encrypt4Hex(string encryptString, string key = null, bool lowerCase = false)
	{
		return Encrypt(encryptString, key, hex: true, lowerCase);
	}

	/// <summary>
	/// DES+16进制解密
	/// <para>采用ECB、PKCS7</para>
	/// </summary>
	/// <param name="decryptString">解密字符串</param>
	/// <param name="key">秘钥</param>
	/// <returns></returns>
	public static string Decrypt4Hex(string decryptString, string key = null)
	{
		return Decrypt(decryptString, key, hex: true);
	}

	/// <summary>
	/// DES加密
	/// </summary>
	/// <param name="encryptString"></param>
	/// <param name="key"></param>
	/// <param name="hex"></param>
	/// <param name="lowerCase"></param>
	/// <returns></returns>
	private static string Encrypt(string encryptString, string key, bool hex, bool lowerCase = false)
	{
		if (encryptString.IsNull())
		{
			return null;
		}
		if (key.IsNull())
		{
			key = "freesql!";
		}
		if (key.Length < 8)
		{
			throw new ArgumentException("秘钥长度为8位", "key");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
		byte[] bytes2 = Encoding.UTF8.GetBytes(encryptString);
		DES dES = DES.Create();
		dES.Mode = CipherMode.ECB;
		dES.Key = bytes;
		dES.Padding = PaddingMode.PKCS7;
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, dES.CreateEncryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(bytes2, 0, bytes2.Length);
		cryptoStream.FlushFinalBlock();
		byte[] array = memoryStream.ToArray();
		return hex ? array.ToHex(lowerCase) : Convert.ToBase64String(array);
	}

	/// <summary>
	/// DES解密
	/// </summary>
	/// <param name="decryptString"></param>
	/// <param name="key"></param>
	/// <param name="hex"></param>
	/// <returns></returns>
	private static string Decrypt(string decryptString, string key, bool hex)
	{
		if (decryptString.IsNull())
		{
			return null;
		}
		if (key.IsNull())
		{
			key = "freesql!";
		}
		if (key.Length < 8)
		{
			throw new ArgumentException("秘钥长度为8位", "key");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
		byte[] array = (hex ? decryptString.HexToBytes() : Convert.FromBase64String(decryptString));
		DES dES = DES.Create();
		dES.Mode = CipherMode.ECB;
		dES.Key = bytes;
		dES.Padding = PaddingMode.PKCS7;
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, dES.CreateDecryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(array, 0, array.Length);
		cryptoStream.FlushFinalBlock();
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}
