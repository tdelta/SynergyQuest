import shell, {head} from 'shelljs';
import fs from 'fs';

export default function run() {
    // executes `yarn licenses generate-disclaimer`
    // removes leading sentence
    // and saves output to a file
    // (file path is given by cli parameter)

    if (process.argv.length < 4) {
        shell.echo('Usage: node build-scripts gen-disclaimer <target file path>');
    }

    const target = process.argv[3];

    if (!shell.which('yarn')) {
        shell.echo('Sorry, this script requires yarn to be available in PATH.');
        shell.exit(1);
    }

    const result = shell
        .exec('yarn --silent licenses generate-disclaimer', {silent: true});

    if (result.code !== 0) {
        shell.echo("Generating disclaimer failed:");
        shell.echo(result.stderr);
        shell.exit(result.code);
    }

    let disclaimer = result.stdout;

    // Remove header, if present
    const headerRe = /THE FOLLOWING SETS FORTH ATTRIBUTION NOTICES FOR THIRD PARTY SOFTWARE THAT MAY BE CONTAINED IN PORTIONS OF THE [^\n]+ PRODUCT.\n\n-----\n\n/m;
    if (headerRe.test(disclaimer)) {
        const split = disclaimer.split('\n');
        split.splice(0, 4);

        disclaimer = split.join('\n');
    }

    fs.writeFileSync(target, disclaimer);
}
