// Builds a custom PrismJS bundle the same way the official download page
// (https://prismjs.com/download.html) does: it resolves the dependency
// closure of the requested languages with Prism's own "dependencies.js"
// loader, then concatenates the core and every resolved language's source in
// that order. Concatenation is what the languages need, since each one is a
// plain script of the shape `(function (Prism) { ... }(Prism))` that expects
// `Prism` to already be a variable in the surrounding scope, not something it
// imports.
//
// The `((window) => { ... })({})` wrapper is what keeps that scope private:
// Prism's core assigns itself to `window.Prism` for the browser, but passing
// in a plain object instead of the real `window` keeps that assignment local,
// and `return window.Prism` is what turns it into this module's export.
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { createRequire } from 'node:module';
import path from 'node:path';
import * as esbuild from 'esbuild';

const require = createRequire(import.meta.url);

// The languages the documentation actually fences code with. "markup" already
// covers its "html"/"xml"/"svg"/"mathml"/"ssml"/"atom"/"rss" aliases, and
// "json" covers "webmanifest"; every other fence language is normalized to
// one of the ids below by DocumentPostProcessor in the app (see AGENTS.md).
// Add a language here, not in the app, when a new fence language shows up
// that Prism already knows but this bundle does not yet.
const LANGUAGES = ['markup', 'css', 'javascript', 'csharp', 'bash', 'cshtml', 'typescript', 'markdown', 'json', 'nginx'];

const prismDir = path.dirname(require.resolve('prismjs/package.json'));
const components = require('prismjs/components.json');
const getLoader = require('prismjs/dependencies.js');

const readComponent = (id) => readFileSync(path.join(prismDir, 'components', `prism-${id}.js`), 'utf8');

const languageIds = getLoader(components, LANGUAGES, []).getIds();
const source = [readComponent('core'), ...languageIds.map(readComponent)].join('\n');
const wrapped = `export const Prism = ((window) => {\n${source}\nreturn window.Prism;\n})({});`;

const { code } = await esbuild.transform(wrapped, { minify: true, format: 'esm' });

mkdirSync('dist', { recursive: true });
writeFileSync('dist/prism.js', code);
