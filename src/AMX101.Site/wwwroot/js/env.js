//refer to http://www.jvandemo.com/how-to-configure-your-angularjs-application-using-environment-variables for implementation details
// possible values are aus,nz,sng


(function (window) {
    window.__env = window.__env || {};
    window.__env.region = "aus";
    window.__env.onlineMode = true;
}(this));
