# 第一阶段：构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 先复制项目文件，充分利用 Docker 缓存
COPY ["NovaAdmin/NovaAdmin.csproj", "NovaAdmin/"]
COPY ["NoAdmin.Blazor/NoAdmin.Blazor.csproj", "NoAdmin.Blazor/"]

# 还原包
RUN dotnet restore "NovaAdmin/NovaAdmin.csproj"

# 复制所有源代码
COPY . .
WORKDIR /src/NovaAdmin

# 构建项目
RUN dotnet build "NovaAdmin.csproj" -c Release -o /app/build --no-restore

# 发布项目
FROM build AS publish
RUN dotnet publish "NovaAdmin.csproj" -c Release -o /app/publish --no-build

# 第二阶段：运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 安装多语言支持
RUN apt-get update && \
    apt-get install -y --no-install-recommends locales && \
    sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen && \
    sed -i '/zh_CN.UTF-8/s/^# //g' /etc/locale.gen && \
    locale-gen && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 从发布阶段复制文件
COPY --from=publish /app/publish .

# 创建必要的目录并设置权限
RUN mkdir -p /app/Logs && \
    mkdir -p /app/wwwroot/uploads && \
    mkdir -p /app/wwwroot/avatars && \
    mkdir -p /app/wwwroot/badge-photos && \
    mkdir -p /app/keys && \
    chmod -R 777 /app/Logs && \
    chmod -R 777 /app/wwwroot

# sqlite 数据文件由 docker-compose 绑定挂载到 /app/novaadmin.db
RUN rm -f /app/novaadmin.db || true

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
# 多语言环境变量设置（默认使用中文 UTF-8）
ENV LANG=zh_CN.UTF-8
ENV LC_ALL=zh_CN.UTF-8
ENV LC_CTYPE=zh_CN.UTF-8

# 暴露端口
EXPOSE 80

# 启动应用
ENTRYPOINT ["dotnet", "NovaAdmin.dll"]
