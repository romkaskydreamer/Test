//refer to http://www.jvandemo.com/how-to-configure-your-angularjs-application-using-environment-variables for implementation details

(function (window) {
    window.__env = window.__env || {};
    window.__env.region = "sng";
    window.__env.onlineMode = true;
}(this));