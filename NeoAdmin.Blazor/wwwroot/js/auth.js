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
    }
};
