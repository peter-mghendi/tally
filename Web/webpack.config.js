const path = require('path');

module.exports = {
    entry: {
        details: './wwwroot/js/polls/details.js'
    },
    output: {
        filename: '[name].g.js',
        path: path.resolve(__dirname, 'wwwroot', 'js', 'polls')
    },
    devtool: 'source-map',
    mode: 'development',
    module: {
        rules: [
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader'],
            },
            {
                test: /\.(eot|woff(2)?|ttf|otf|svg)$/i,
                type: 'asset'
            },
        ]
    }
};
