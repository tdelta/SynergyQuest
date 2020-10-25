import shell from 'shelljs';
import fs from 'fs';

export default function run() {
    // just executes yarn list and puts the output into a file.
    // The filepath is read from a cli parameter

    if (process.argv.length < 4) {
        shell.echo('Usage: node build-scripts gen-deps <target file path>');
    }

    const target = process.argv[3];

    if (!shell.which('yarn')) {
        shell.echo('Sorry, this script requires yarn to be available in PATH.');
        shell.exit(1);
    }

    const result = shell
        .exec('yarn list', {silent: true})
        .stdout

    fs.writeFileSync(target, result);
}
