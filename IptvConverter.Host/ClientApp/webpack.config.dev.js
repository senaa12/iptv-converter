const path = require("path");

const rules = require("./webpack/rules");
const plugins = require("./webpack/plugins");

// port where application starts; if changing need to change port in Setup.cs
const webpackDevServerPort = 8008;
// API address
const proxyTarget = "https://[::1]:44313";


module.exports = {
    mode: "development",
    entry: path.resolve(__dirname, 'src/index.tsx'),
    output: {
        path: path.resolve(__dirname, './dist'),
        filename: 'bundle.js',
        publicPath: '/'
    },
    resolve: {
        extensions: ['.js', '.ts', '.tsx']
    },
    devtool: "source-map",
    devServer: {
        historyApiFallback: true,
        contentBase: path.resolve(__dirname, './dist'),
        open: true,
        compress: true,
        hot: true,
        port: webpackDevServerPort,
    },
    module: {
        rules: [
            rules.svgLoader,
            rules.typescript,
            rules.scssDevLoader,
        ]
    },
    plugins: [
        plugins.htmlWebpackPlugin,
        plugins.definePluginDev,
        plugins.hotModuleReplacementPlugin
    ]
}