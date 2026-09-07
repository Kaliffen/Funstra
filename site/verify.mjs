import { readFileSync } from 'node:fs';
import assert from 'node:assert/strict';
import { retainedReleases } from '../Tools/release-policy.mjs';

const list = JSON.parse(readFileSync(new URL('./dist/releases.json', import.meta.url), 'utf8'));
const html = readFileSync(new URL('./dist/index.html', import.meta.url), 'utf8');
const escape = value => value.replace(/&/g, '&amp;').replace(/"/g, '&quot;');
assert.deepEqual(list, retainedReleases(list), 'Published site must contain only the newest five demos in order');
if (list.length > 1) {
  assert(html.includes('href="#archive"') && html.includes('id="archive"'), 'Previous versions navigation must reach a rendered section');
}
for (const release of list.slice(1)) {
  assert(html.includes(`href="${escape(release.url)}"`), `Missing release notes for ${release.tag}`);
  if (release.assets.length) assert(html.includes(`href="${escape(release.assets[0].url)}"`), `Missing previous download for ${release.tag}`);
}
console.log(`Site retention and previous-version links verified for ${list.length} release(s).`);
