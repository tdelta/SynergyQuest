import shell from 'shelljs';
import genDeps from './gen-deps';
import genDisclaimer from './gen-disclaimer';

const commandMap: Record<string, () => unknown> = {
    'gen-deps': genDeps,
    'gen-disclaimer': genDisclaimer
}

const keys = Object.keys(commandMap);
const command = process.argv.length >= 3 ? process.argv[2] : undefined;

if (command == null || !(command in commandMap)) {
    shell.echo("Usage: node build-scripts <command> <parameters>");
    shell.echo(`Where <command> is one of ${keys}`)
    shell.exit(1);
}

const run = commandMap[command];
run();
