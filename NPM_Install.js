var https = require('https');
var fs = require('fs');
var path = require('path');

// Get the npm version from command-line arguments
var npmVersion = process.argv[2];
if (!npmVersion) {
    console.error('No npm version specified. Usage: node NPM_install.js <npm-version>');
    process.exit(1);
}


var npmUrl = 'https://registry.npmjs.org/npm/-/npm-' + npmVersion + '.tgz'; // Change URL if necessary
console.log('downloading from ' + npmUrl);
var npmPackage = path.join(__dirname, 'npm.tgz');
var extract = require('child_process').exec;

https.get(npmUrl, function (response) {
    var file = fs.createWriteStream(npmPackage);
    response.pipe(file);
    file.on('finish', function () {
        file.close(() => {
            extract('tar -xzf ' + npmPackage + ' -C ' + __dirname, function (err, stdout, stderr) {
                if (err) {
                    console.error(err);
                    process.exit(1);
                }
                console.log('npm installed successfully.');
            });
        });
    });
}).on('error', function (err) {
    fs.unlink(npmPackage);
    console.error(err.message);
    process.exit(1);
});
