// Focused publication fixtures: relocated evidence, preserved originals, and
// containment failures. No network or game build is involved.
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, readFileSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, resolve, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';

const sourceSite = dirname(fileURLToPath(import.meta.url));
const temporaryBase = resolve(tmpdir());
const fixture = mkdtempSync(join(temporaryBase, 'funstra-site-test-'));
const put = (path, body) => {
  const destination = join(fixture, path);
  mkdirSync(dirname(destination), { recursive: true });
  writeFileSync(destination, body);
};
const run = script => spawnSync(process.execPath, [join(fixture, 'site', script), '--offline'], { encoding: 'utf8' });
try {
  for (const file of ['build.mjs', 'verify.mjs', 'content/process.json', 'assets/styles.css']) {
    put(`site/${file}`, readFileSync(join(sourceSite, file)));
  }
  put('Tools/release-policy.mjs', readFileSync(join(sourceSite, '../Tools/release-policy.mjs')));
  put('site/.cache/releases.json', '[]');
  const content = JSON.parse(readFileSync(join(sourceSite, 'content/site.json'), 'utf8'));
  delete content.releaseVersion; // Empty-release fixture has no advertised production version.
  content.dossierFile = 'current.html';
  content.previousDossiers = [{ file: 'previous.html', output: 'dossier-previous.html', label: 'Previous' }];
  content.shots = [{ file: 'nested/receipt.png', caption: 'Nested screenshot' }];
  put('site/content/site.json', JSON.stringify(content));
  const dossier = '<a href="../reviews/full%20review.md?raw=1#score">Review</a><img src="../Evidence/nested/receipt.png"><a href="#local">Local</a><a href="https://example.com/external">External</a><a href="//example.com/protocol-relative">External</a>';
  put('Docs/current.html', dossier);
  put('Docs/previous.html', '<a href="../reviews/full%20review.md">Preserved previous review</a>');
  put('reviews/full review.md', '# Original verdict\nUnchanged.\n');
  put('Evidence/nested/receipt.png', 'fixture image bytes');
  let result = run('build.mjs');
  assert.equal(result.status, 0, result.stderr);
  const published = readFileSync(join(fixture, 'site/dist/dossier.html'), 'utf8');
  assert(published.includes('href="reviews/full%20review.md?raw=1#score"'));
  assert(published.includes('src="Evidence/nested/receipt.png"'));
  for (const url of ['#local', 'https://example.com/external', '//example.com/protocol-relative']) assert(published.includes(`href="${url}"`));
  assert.equal(readFileSync(join(fixture, 'Docs/current.html'), 'utf8'), dossier);
  assert.deepEqual(readFileSync(join(fixture, 'site/dist/reviews/full review.md')), readFileSync(join(fixture, 'reviews/full review.md')));
  assert.deepEqual(readFileSync(join(fixture, 'site/dist/shots/nested/receipt.png')), readFileSync(join(fixture, 'Evidence/nested/receipt.png')));
  result = run('verify.mjs');
  assert.equal(result.status, 0, result.stderr);
  content.releaseVersion = 'v99.0.0';
  put('site/content/site.json', JSON.stringify(content));
  result = run('verify.mjs');
  assert.notEqual(result.status, 0, 'An older download must not pass beside new release metadata');
  assert.match(result.stderr, /Download must match/);
  delete content.releaseVersion;
  put('site/content/site.json', JSON.stringify(content));
  put('Docs/current.html', '<a href="../../outside.txt">Escape</a>');
  result = run('build.mjs');
  assert.notEqual(result.status, 0);
  assert.match(result.stderr, /escapes/);
  put('Docs/current.html', '<a href="../missing.txt">Missing receipt</a>');
  result = run('build.mjs');
  assert.notEqual(result.status, 0);
  assert.match(result.stderr, /ENOENT/);
  console.log('Site dossier fixtures passed: nested files, previous dossier, unchanged originals, URL suffixes, external links, traversal and missing dependencies.');
} finally {
  const rel = relative(temporaryBase, resolve(fixture));
  assert(rel.startsWith('funstra-site-test-') && !rel.includes(sep));
  rmSync(fixture, { recursive: true, force: true });
}
