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
    }
};
