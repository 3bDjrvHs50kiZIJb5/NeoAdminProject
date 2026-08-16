window.neoAdminAuth = {
    getLoginFlag: function () {
        return window.localStorage.getItem("neoadmin:isLogin");
    },
    setLoginFlag: function (value) {
        window.localStorage.setItem("neoadmin:isLogin", value);
    },
    clearLoginFlag: function () {
        window.localStorage.removeItem("neoadmin:isLogin");
    },
    getToken: function () {
        return window.localStorage.getItem("neoadmin:token");
    },
    setToken: function (value) {
        window.localStorage.setItem("neoadmin:token", value);
        window.localStorage.setItem("neoadmin:isLogin", "1");
    },
    clearToken: function () {
        window.localStorage.removeItem("neoadmin:token");
        window.localStorage.removeItem("neoadmin:isLogin");
    },
    copyText: async function (text) {
        await navigator.clipboard.writeText(text);
    },
    copyElementText: async function (elementId) {
        const el = document.getElementById(elementId);
        if (!el) {
            return;
        }
        await this.copyText((el.textContent || "").trim());
    },
    /**
     * 首页「创建项目」命令：输入框变更时同步 -n 项目名（静态 SSR，无 Blazor 绑定）。
     */
    syncCreateProjectCommand: function (input, commandId) {
        const fallback = "MyAdmin";
        const name = (input && input.value ? String(input.value) : "").trim() || fallback;
        const el = document.getElementById(commandId);
        if (!el) {
            return;
        }
        el.textContent = "dotnet new neoadmin -n " + name + " -o .";
    },
    scrollIntoViewById: function (id) {
        const el = document.getElementById(id);
        if (!el) {
            return;
        }
        el.scrollIntoView({ behavior: "smooth", block: "start" });
    },
    scrollPageToTop: function () {
        const container = document.querySelector(".neo-admin-page-scroll");
        if (container) {
            container.scrollTo({ top: 0, behavior: "smooth" });
            return;
        }
        window.scrollTo({ top: 0, behavior: "smooth" });
    },
    /**
     * 监听标签页重新可见 / 窗口获焦，回调 Blazor 做登录态校验（单点登录互踢）。
     */
    watchSession: function (dotNetRef) {
        if (window.__neoAdminSessionWatchCleanup) {
            window.__neoAdminSessionWatchCleanup();
        }

        const trigger = function () {
            dotNetRef.invokeMethodAsync("OnSessionWatchAsync").catch(function () { });
        };

        const onVisibilityChange = function () {
            if (document.visibilityState === "visible") {
                trigger();
            }
        };

        document.addEventListener("visibilitychange", onVisibilityChange);
        window.addEventListener("focus", trigger);

        window.__neoAdminSessionWatchCleanup = function () {
            document.removeEventListener("visibilitychange", onVisibilityChange);
            window.removeEventListener("focus", trigger);
            window.__neoAdminSessionWatchCleanup = null;
        };
    },
    stopWatchSession: function () {
        if (window.__neoAdminSessionWatchCleanup) {
            window.__neoAdminSessionWatchCleanup();
        }
    },
    /**
     * 替换 textarea 当前选区（无选区则在光标处插入），返回新文本。
     * 用于 ApiExplorer 变量芯片点击写入请求 Body。
     */
    replaceTextareaSelection: function (elementId, text) {
        const el = document.getElementById(elementId);
        if (!el) {
            return null;
        }

        const value = typeof el.value === "string" ? el.value : "";
        const start = typeof el.selectionStart === "number" ? el.selectionStart : value.length;
        const end = typeof el.selectionEnd === "number" ? el.selectionEnd : start;
        const insert = text == null ? "" : String(text);
        const next = value.slice(0, start) + insert + value.slice(end);
        el.value = next;

        const cursor = start + insert.length;
        try {
            el.focus();
            el.setSelectionRange(cursor, cursor);
        } catch (_) {
            // ignore focus/selection failures on detached nodes
        }

        el.dispatchEvent(new Event("input", { bubbles: true }));
        return next;
    }
};
