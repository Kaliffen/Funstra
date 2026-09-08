import { readFileSync, readdirSync, statSync } from 'node:fs';
import assert from 'node:assert/strict';
import { dirname, join, resolve, relative, isAbsolute, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { retainedReleases } from '../Tools/release-policy.mjs';

const list = JSON.parse(readFileSync(new URL('./dist/releases.json', import.meta.url), 'utf8'));
const html = readFileSync(new URL('./dist/index.html', import.meta.url), 'utf8');
const escape = value => value.replace(/&/g, '&amp;').replace(/"/g, '&quot;');
assert.deepEqual(list, retainedReleases(list), 'Published site must contain only the newest five demos in order');
if (list.length > 1) {
  assert(html.includes(`href="${escape(list[1].url)}">Previous version — ${list[1].tag}</a>`), 'Previous version must open the preceding release, not the current page');
  assert(html.includes('href="#archive"') && html.includes('id="archive"'), 'Previous versions navigation must reach a rendered section');
}
for (const release of list.slice(1)) {
  assert(html.includes(`href="${escape(release.url)}"`), `Missing release notes for ${release.tag}`);
  if (release.assets.length) assert(html.includes(`href="${escape(release.assets[0].url)}"`), `Missing previous download for ${release.tag}`);
}
console.log(`Site retention and previous-version links verified for ${list.length} release(s).`);

// Check all generated pages, including evidence-linked dossiers, so a relocated
// dossier or nested screenshot cannot silently ship a broken relative URL.
const dist = fileURLToPath(new URL('./dist/', import.meta.url));
let checked = 0;
function verifyLocalLinks(directory) {
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const file = join(directory, entry.name);
    if (entry.isDirectory()) { verifyLocalLinks(file); continue; }
    if (!/\.html?$/i.test(entry.name)) continue;
    const document = readFileSync(file, 'utf8');
    for (const [, , url] of document.matchAll(/\b(?:href|src)\s*=\s*(["'])(.*?)\1/gi)) {
      if (!url || /^(?:#|\/\/|[a-z][a-z0-9+.-]*:)/i.test(url)) continue;
      const path = decodeURIComponent(url.split(/[?#]/, 1)[0].replace(/&amp;/g, '&'));
      if (!path) continue;
      assert(!path.startsWith('/') && !path.includes('\\'), `Site URL must be relative: ${url} in ${file}`);
      const target = resolve(dirname(file), path);
      const rel = relative(dist, target);
      assert(rel !== '..' && !rel.startsWith(`..${sep}`) && !isAbsolute(rel), `Site URL escapes dist: ${url} in ${file}`);
      assert(statSync(target, { throwIfNoEntry: false })?.isFile(), `Missing local target: ${url} in ${file}`);
      checked++;
    }
  }
}
verifyLocalLinks(dist);
console.log(`Verified ${checked} local page and evidence links.`);
