const path = require("path");

//#region  LOADERS (skip basically)
/** not sure if module loader is working */
const CSSModuleLoader = {
    loader: 'css-loader',
    options: {
        modules: {
            auto: true,
            localIdentName: '[name]_[local]--[hash:base64:5]',
        },
        sourceMap: true, // turned off as causes delay
   }
 }

 const CSSLoaderProd = {
    loader: 'css-loader',
    options: {
        modules: "global",
        importLoaders: 2,
        sourceMap: false, // turned off as causes delay
    }
 }

//#endregion

//#region LANGUAGE
exports.babel = {
    test: /\.(js|jsx)$/,
    exclude: /node_modules/,
    loader: "babel-loader"
};

exports.typescript = {
    test: /\.(tsx|ts)$/,
    exclude: /node_modules/,
    loader: "ts-loader"
}

//#endregion

//#region STYLE
exports.scssDevLoader = {
    test: /\.s?css$/,
    exclude: /node_modules/,
    use: ["style-loader", "css-loader", "sass-loader"]
};

exports.cssDevLoader = {
    test: /\.css$/,
    use: ['style-loader', 'css-loader'],
};

exports.scssProdLoader = {
    test: /\.scss$/,
    exclude: /node_modules/,
    use: ["style-loader", CSSLoaderProd, "sass-loader"]
};

/** not sure if working */
exports.scssModuleLoader = {
    test: /\.module.scss$/,
    exclude: /node_modules/,
    use: ['style-loader', CSSModuleLoader, "sass-loader"]
}

//#endregion

//#region OTHER LOADERS

exports.svgLoader = {
  test: /\.svg$/,
  loader: "svg-sprite-loader",
  include: [path.resolve(__dirname, '../src/assets/icons')]
}

//#endregion
