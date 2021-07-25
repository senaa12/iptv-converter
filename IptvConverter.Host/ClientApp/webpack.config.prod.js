const path = require("path");

const rules = require("./webpack/rules");
const plugins = require("./webpack/plugins");

module.exports = {
    mode: 'production',
    entry: path.resolve(__dirname, 'src/index.tsx'),
    output: {
        path: path.resolve(__dirname, 'build'),
        filename: '[name].[contenthash].js',
        publicPath: '/'
    },
    optimization: {
        runtimeChunk: 'single',
        splitChunks: {
            chunks: 'all' // vendor.js posebno izdvaja
        },
        minimizer: [
           plugins.uglifyJsPlugin
        ]
    },
    resolve: {
        extensions: ['.js', '.ts', '.tsx']
    },
    // devtool: "source-map", DO NOT TURN ON; don't want pretty files in production
    module: {
        rules: [
            rules.typescript,
            rules.scssProdLoader,
            rules.svgLoader
        ]
    },
    plugins: [
        plugins.htmlWebpackPlugin,
        plugins.definePluginProd,
        plugins.hashedModulePlugin,
        plugins.miniCssPlugin,
        plugins.copyWebpackPlugin
    ],
}