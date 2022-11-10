const {createProxyMiddleware} = require('http-proxy-middleware');
const settings = require("../package.json");

module.exports = function (app) {
    const proxy = createProxyMiddleware({
        target: settings.proxy,
        changeOrigin: true,
        secure: false
    });

    app.use('/pay', proxy);
    app.use('/withdraw', proxy);
};