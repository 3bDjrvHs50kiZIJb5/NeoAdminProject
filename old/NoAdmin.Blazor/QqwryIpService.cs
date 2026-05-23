using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FreeRedis;
using Microsoft.Extensions.Logging;

public class QqwryIpService
{
	private class IpRecord
	{
		public ulong StartIpNum { get; set; }

		public ulong EndIpNum { get; set; }

		public string Country { get; set; }

		public string Area { get; set; }
	}

	private const string VersionUrl = "https://raw.githubusercontent.com/metowolf/qqwry.dat/main/version.json";

	private const string DownloadUrl = "https://github.com/metowolf/qqwry.dat/releases/latest/download/qqwry.dat";

	private const string RedisKey = "qqwry_ip";

	private const string VersionKey = "qqwry_ip_version";

	private const string UpdatePassword = "freesql";

	private const byte REDIRECT_MODE_1 = 1;

	private const byte REDIRECT_MODE_2 = 2;

	private static readonly Encoding GB2312 = Encoding.GetEncoding("GB2312");

	private readonly RedisClient _redis;

	private readonly HttpClient _httpClient;

	private readonly ILogger<QqwryIpService> _logger;

	private static readonly string[] ChineseProvinces = new string[34]
	{
		"北京", "上海", "天津", "重庆", "香港", "澳门", "台湾", "内蒙古", "新疆", "西藏",
		"宁夏", "广西", "河北", "山西", "辽宁", "吉林", "黑龙江", "江苏", "浙江", "安徽",
		"福建", "江西", "山东", "河南", "湖北", "湖南", "广东", "海南", "四川", "贵州",
		"云南", "陕西", "甘肃", "青海"
	};

	public QqwryIpService(RedisClient redis, HttpClient httpClient, ILogger<QqwryIpService> logger)
	{
		_redis = redis;
		_httpClient = httpClient;
		_logger = logger;
	}

	/// <summary>
	/// 将 IPv4 地址字符串转换为 ulong 整数（强制大端/网络字节序）
	/// </summary>
	private static ulong Ip2Long(string ip)
	{
		string[] array = ip.Split('.');
		if (array.Length != 4)
		{
			return 0uL;
		}
		if (ulong.TryParse(array[0], out var result) && ulong.TryParse(array[1], out var result2) && ulong.TryParse(array[2], out var result3) && ulong.TryParse(array[3], out var result4))
		{
			return (result << 24) | (result2 << 16) | (result3 << 8) | result4;
		}
		return 0uL;
	}

	/// <summary>
	/// 将 ulong 整数（大端/网络字节序）转换为 IPv4 地址字符串
	/// </summary>
	private static string Long2Ip(ulong ipInt)
	{
		return $"{(ipInt >> 24) & 0xFF}.{(ipInt >> 16) & 0xFF}.{(ipInt >> 8) & 0xFF}.{ipInt & 0xFF}";
	}

	/// <summary>
	/// 读取 3 字节无符号整数（小端存储）
	/// </summary>
	private static uint ReadUInt24(BinaryReader br)
	{
		byte b = br.ReadByte();
		byte b2 = br.ReadByte();
		byte b3 = br.ReadByte();
		return (uint)(b | (b2 << 8) | (b3 << 16));
	}

	/// <summary>
	/// 读取以 0x00 结尾的 GB2312 字符串 (含乱码净化)
	/// </summary>
	private static string ReadString(BinaryReader br, int? startPosition = null)
	{
		if (startPosition.HasValue)
		{
			br.BaseStream.Seek(startPosition.Value, SeekOrigin.Begin);
		}
		List<byte> list = new List<byte>();
		byte item;
		while (br.BaseStream.Position < br.BaseStream.Length && (item = br.ReadByte()) != 0)
		{
			list.Add(item);
		}
		if (list.Count == 0)
		{
			return string.Empty;
		}
		byte[] bytes = list.Where((byte by) => by >= 32 || by == 10 || by == 13 || by >= 128).ToArray();
		return GB2312.GetString(bytes);
	}

	/// <summary>
	/// 从 BinaryReader 中读取 4 字节的 IP 地址，并将其转换为大端（网络字节序）ulong
	/// QQWry 文件中的 IP 地址是小端存储的。
	/// </summary>
	private static ulong ReadIpAsBigEndian(BinaryReader br)
	{
		byte[] array = br.ReadBytes(4);
		Array.Reverse(array);
		return ((ulong)array[0] << 24) | ((ulong)array[1] << 16) | ((ulong)array[2] << 8) | array[3];
	}

	/// <summary>
	/// 解析 QQWry.dat 二进制文件，提取 IP 记录 (核心修正)
	/// </summary>
	private List<IpRecord> ParseQqwry(byte[] datBytes)
	{
		List<IpRecord> list = new List<IpRecord>();
		if (datBytes == null || datBytes.Length < 8)
		{
			return list;
		}
		using MemoryStream input = new MemoryStream(datBytes);
		BinaryReader br = new BinaryReader(input, GB2312);
		try
		{
			try
			{
				uint num = br.ReadUInt32();
				uint num2 = br.ReadUInt32();
				uint num3 = (num2 - num) / 7 + 1;
				_logger.LogInformation("开始解析 QQWry.dat，预计记录数: {Count}", num3);
				for (uint num4 = 0u; num4 < num3; num4++)
				{
					br.BaseStream.Seek(num + num4 * 7, SeekOrigin.Begin);
					ulong startIpNum = ReadIpAsBigEndian(br);
					uint num5 = ReadUInt24(br);
					br.BaseStream.Seek(num5, SeekOrigin.Begin);
					ulong endIpNum = ReadIpAsBigEndian(br);
					string empty = string.Empty;
					string empty2 = string.Empty;
					byte b = br.ReadByte();
					long position = br.BaseStream.Position;
					switch (b)
					{
					case 1:
					{
						uint num6 = ReadUInt24(br);
						br.BaseStream.Seek(num6, SeekOrigin.Begin);
						byte b2 = br.ReadByte();
						if (b2 == 2)
						{
							uint value2 = ReadUInt24(br);
							empty = ReadString(br, (int)value2);
							empty2 = ReadArea((int)br.BaseStream.Position);
						}
						else
						{
							empty = ReadString(br, (int)num6);
							empty2 = ReadArea((int)br.BaseStream.Position);
						}
						break;
					}
					case 2:
					{
						uint value = ReadUInt24(br);
						empty = ReadString(br, (int)value);
						empty2 = ReadArea((int)(num5 + 4 + 3));
						break;
					}
					default:
						empty = ReadString(br, (int)(num5 + 4));
						empty2 = ReadArea((int)br.BaseStream.Position);
						break;
					}
					list.Add(new IpRecord
					{
						StartIpNum = startIpNum,
						EndIpNum = endIpNum,
						Country = empty.Trim(),
						Area = empty2.Trim()
					});
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "IP库解析失败，请检查文件格式或解析逻辑！");
			}
			return list;
		}
		finally
		{
			if (br != null)
			{
				((IDisposable)br).Dispose();
			}
		}
		string ReadArea(int offset)
		{
			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			byte b3 = br.ReadByte();
			if (b3 == 1 || b3 == 2)
			{
				uint num7 = ReadUInt24(br);
				if (num7 == 0)
				{
					return string.Empty;
				}
				return ReadString(br, (int)num7);
			}
			return ReadString(br, offset);
		}
	}

	/// <summary>
	/// 第一步：检查版本，如果需要更新，则执行下载和更新到 Redis
	/// </summary>
	public async Task<bool> CheckAndUpdateAsync(string password)
	{
		if (password != "freesql")
		{
			_logger.LogWarning("IP库更新尝试失败：密码不匹配");
			return false;
		}
		string currentVersion = _redis.Get<string>("qqwry_ip_version") ?? "0";
		string latestVersion = await GetLatestVersionAsync();
		if (string.IsNullOrEmpty(latestVersion))
		{
			_logger.LogError("无法获取最新版本号，更新操作中止。");
			return false;
		}
		if (latestVersion == currentVersion)
		{
			_logger.LogInformation("IP库版本: {CurrentVersion}，已是最新版本。", currentVersion);
			return false;
		}
		_logger.LogInformation("检测到新版本IP库: {NewVersion} (当前: {CurrentVersion})，开始更新...", latestVersion, currentVersion);
		return await DownloadAndSaveToRedisAsync(latestVersion);
	}

	/// <summary>
	/// 第一步辅助：获取最新版本号（手动解析 JSON）
	/// </summary>
	private async Task<string> GetLatestVersionAsync()
	{
		try
		{
			using JsonDocument doc = JsonDocument.Parse(await _httpClient.GetStringAsync("https://raw.githubusercontent.com/metowolf/qqwry.dat/main/version.json"));
			if (doc.RootElement.TryGetProperty("latest", out var latestElement))
			{
				return latestElement.GetString();
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError(ex2, "获取最新版本失败，URL: {Url}", "https://raw.githubusercontent.com/metowolf/qqwry.dat/main/version.json");
		}
		return string.Empty;
	}

	/// <summary>
	/// 第二步和第三步：下载、解析 QQWry.dat，然后批量存储到 Redis ZSet
	/// </summary>
	private async Task<bool> DownloadAndSaveToRedisAsync(string newVersion)
	{
		try
		{
			_logger.LogInformation("开始下载 QQWry.dat...");
			byte[] datBytes = await _httpClient.GetByteArrayAsync("https://github.com/metowolf/qqwry.dat/releases/latest/download/qqwry.dat");
			_logger.LogInformation("下载完成，文件大小: {Size} 字节", datBytes.Length);
			List<IpRecord> records = ParseQqwry(datBytes);
			if (records.Count == 0)
			{
				_logger.LogError("IP库解析记录数为 0，更新失败！");
				return false;
			}
			_redis.Del(new string[1] { "qqwry_ip" });
			List<ZMember> members = new List<ZMember>();
			int count = 0;
			foreach (IpRecord record in records)
			{
				string memberString = $"{record.StartIpNum}-{record.EndIpNum} {record.Country.Trim()} {record.Area.Trim()}";
				members.Add(new ZMember(memberString, (decimal)record.EndIpNum));
				count++;
				if (members.Count >= 50000)
				{
					await _redis.ZAddAsync("qqwry_ip", members.ToArray(), (ZAddThan?)null, false);
					members.Clear();
				}
			}
			if (members.Count > 0)
			{
				await _redis.ZAddAsync("qqwry_ip", members.ToArray(), (ZAddThan?)null, false);
			}
			_redis.Set<string>("qqwry_ip_version", newVersion, 0);
			_logger.LogInformation("IP库更新成功。总记录数: {Count}，新版本: {NewVersion}", count, newVersion);
			return true;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError(ex2, "IP库下载或保存到 Redis 过程中发生严重错误。");
			return false;
		}
	}

	/// <summary>
	/// 第四步：根据 IP 地址从 Redis 获取地理位置信息
	/// </summary>
	public string GetLocationByIpAddress(string ip)
	{
		if (string.IsNullOrEmpty(ip))
		{
			_logger.LogWarning("查询 IP 地址为空。");
			return "未知";
		}
		if (!IPAddress.TryParse(ip, out IPAddress address))
		{
			_logger.LogWarning("IP地址格式无法解析: {Ip}", ip);
			return "IP格式错误";
		}
		if (address.AddressFamily == AddressFamily.InterNetworkV6 || IPAddress.IsLoopback(address))
		{
			return "本地";
		}
		byte[] addressBytes = address.GetAddressBytes();
		if (addressBytes[0] == 10 || (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31) || (addressBytes[0] == 192 && addressBytes[1] == 168))
		{
			if (addressBytes[0] == 172 && addressBytes[1] == 17)
			{
				return "Docker";
			}
			return "内网";
		}
		if (addressBytes[0] == 0 || (addressBytes[0] == 169 && addressBytes[1] == 254) || addressBytes[0] >= 224)
		{
			return "保留";
		}
		ulong num = Ip2Long(ip);
		string[] array = _redis.ZRangeByScore("qqwry_ip", num.ToString("0"), "+inf", 0, 1);
		if (array != null && array.Any())
		{
			string text = array.First();
			int num2 = text.IndexOf(' ');
			if (num2 > 0)
			{
				string text2 = text.Substring(0, num2);
				string[] array2 = text2.Split('-');
				if (array2.Length == 2 && ulong.TryParse(array2[0], out var result) && result <= num)
				{
					string text3 = text.Substring(num2 + 1);
					string country = "未知";
					string area = string.Empty;
					int num3 = text3.IndexOf('–');
					if (num3 <= 0)
					{
						num3 = text3.IndexOf('-');
					}
					if (num3 <= 0)
					{
						num3 = text3.IndexOf(' ');
					}
					if (num3 > 0)
					{
						country = text3.Substring(0, num3).Trim();
						area = text3.Substring(num3 + 1).Trim();
					}
					else if (text3.Length > 0)
					{
						country = text3.Trim();
						area = string.Empty;
					}
					return GetProvince(country, area);
				}
			}
		}
		_logger.LogDebug("IP地址 {Ip} (Long: {IpLong}) 在 IP 库中未找到。", ip, num);
		return "未知";
	}

	/// <summary>
	/// 从 QQWry 的 Country 和 Area 字段中提取规范化的省份/直辖市名称（去后缀）
	/// </summary>
	public static string GetProvince(string country, string area)
	{
		if (!country.Contains("中国"))
		{
			return country.Trim();
		}
		string text = country.Trim() + area.Trim();
		string[] chineseProvinces = ChineseProvinces;
		foreach (string text2 in chineseProvinces)
		{
			if (text.Contains(text2))
			{
				return text2;
			}
		}
		return "中国";
	}

	/// <summary>
	/// 获取当前 IP 库版本号
	/// </summary>
	public string GetCurrentVersion()
	{
		return _redis.Get<string>("qqwry_ip_version") ?? "未安装";
	}
}
