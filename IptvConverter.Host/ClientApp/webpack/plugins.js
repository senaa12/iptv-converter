const webpack = require("webpack");
const HtmlWebpackPlugin = require("html-webpack-plugin");

/**
 * extra node modules to install:
 * uglifyjs-webpack-plugin mini-css-extract-plugin
 */
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const path = require("path");

//#region BASIC 
exports.htmlWebpackPlugin = new HtmlWebpackPlugin({
    template: "./src/index.html"
    // TODO: add favicon here
});

exports.definePluginProd = new webpack.DefinePlugin({
    'process.env': {
        'NODE_ENV': JSON.stringify('production') 
    },
    '__REACT_DEVTOOLS_GLOBAL_HOOK__': '({ isDisabled: true })'
})

exports.definePluginDev = new webpack.DefinePlugin({
    'process.env': {
        'NODE_ENV': JSON.stringify('development') // also can be set with webpack config => mode: 'development'
    },
    '__REACT_DEVTOOLS_GLOBAL_HOOK__': '({ isDisabled: true })'
})

exports.copyWebpackPluginBuild = new CopyWebpackPlugin({
  patterns: [
    { from: 'src/public', to: '' }
  ]
})

exports.copyWebpackPluginDev = new CopyWebpackPlugin({
  patterns: [
    { from: 'src/public', to: path.resolve(__dirname, '../../dist') }
  ]
})

//#endregion

//#region extras DEV
exports.hotModuleReplacementPlugin = new webpack.HotModuleReplacementPlugin();

//#endregion

//#region extras PROD
exports.hashedModulePlugin = new webpack.ids.HashedModuleIdsPlugin();

exports.miniCssPlugin = new MiniCssExtractPlugin({
    filename: "[name].[hash].css",
    chunkFilename: "[name].[hash].css"
});

exports.uglifyJsPlugin = new UglifyJsPlugin({
    chunkFilter: (chunk) => {
        // Exclude uglification for the `vendor` chunk
        if (chunk.name === 'vendor') {
          return false;
        }
        return true;
      },
    uglifyOptions: {
        output: {
          comments: false,
        },
      },
  });

//#endregion
