using FreeSql;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class UserSeedData
{
    private const string DemoPassword = "123456";
    private const int DemoUserCount = 50;

    private static readonly string[] Nicknames =
    [
        "张伟", "王芳", "李娜", "刘洋", "陈静",
        "杨帆", "赵敏", "黄强", "周杰", "吴婷",
        "徐磊", "孙丽", "马超", "朱琳", "胡军",
        "郭佳", "何勇", "高雪", "林峰", "罗燕",
        "梁浩", "宋佳", "郑凯", "谢雨", "韩冰",
        "唐亮", "冯雪", "于波", "董洁", "萧然",
        "程远", "曹颖", "袁野", "邓华", "许晴",
        "傅强", "沈悦", "曾辉", "彭丽", "吕刚",
        "苏敏", "卢涛", "蒋欣", "蔡明", "贾玲",
        "丁磊", "魏晨", "薛峰", "叶青", "潘越"
    ];

    private static readonly string[] Departments =
    [
        "研发部", "产品部", "市场部", "运营部", "人事部",
        "财务部", "客服部", "设计部", "测试部", "行政部"
    ];

    public static void Ensure(IFreeSql freeSql)
    {
        long existingCount = freeSql.Select<SysUser>()
            .Where(a => a.Username.StartsWith("demo"))
            .Count();

        if (existingCount >= DemoUserCount)
        {
            return;
        }

        List<SysUser> users = CreateDemoUsers();
        List<string> existingUsernames = freeSql.Select<SysUser>()
            .Where(a => a.Username.StartsWith("demo"))
            .ToList(a => a.Username);

        List<SysUser> toInsert = users
            .Where(a => !existingUsernames.Contains(a.Username))
            .ToList();

        if (toInsert.Count == 0)
        {
            return;
        }

        freeSql.Insert(toInsert).ExecuteAffrows();
    }

    private static List<SysUser> CreateDemoUsers()
    {
        Random random = new(20260523);
        DateTime now = DateTime.Now;
        List<SysUser> users = new(DemoUserCount);

        for (int index = 0; index < DemoUserCount; index++)
        {
            int number = index + 1;
            string username = $"demo{number:D3}";
            string nickname = Nicknames[index];
            string department = Departments[index % Departments.Length];
            DateTime createdTime = now.AddDays(-random.Next(1, 365)).AddHours(-random.Next(0, 24));
            bool hasLogin = random.Next(100) < 70;

            users.Add(new SysUser
            {
                Username = username,
                Nickname = nickname,
                Password = DemoPassword,
                IsEnabled = random.Next(100) >= 10,
                IsSystem = false,
                LoginTime = hasLogin
                    ? createdTime.AddDays(random.Next(0, 30)).AddHours(random.Next(0, 24))
                    : default,
                Description = $"{department}模拟账号",
                CreatedTime = createdTime
            });
        }

        return users;
    }
}
