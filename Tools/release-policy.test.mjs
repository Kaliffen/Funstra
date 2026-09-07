import { test } from 'node:test';
import assert from 'node:assert/strict';
import { retainedReleases } from './release-policy.mjs';

const release = n => ({ id: n, tag_name: `v0.${n}.0`, published_at: `2026-09-${String(n).padStart(2, '0')}T00:00:00Z` });
for (const count of [0, 2, 5, 7]) {
  test(`retain newest min(5, ${count}) published releases`, () => {
    const all = Array.from({ length: count }, (_, i) => release(i + 1));
    const original = [...all];
    assert.deepEqual(retainedReleases(all).map(r => r.id), Array.from({ length: Math.min(5, count) }, (_, i) => count - i));
    assert.deepEqual(all, original);
  });
}
test('drafts do not evict published demos; ordering also works for cached site records', () => {
  const all = [release(2), { ...release(9), draft: true }, release(1), release(3)];
  assert.deepEqual(retainedReleases(all).map(r => r.id), [3, 2, 1]);
  assert.deepEqual(retainedReleases(all.filter(r => !r.draft).map(r => ({ tag: r.tag_name, published_at: r.published_at }))).map(r => r.tag), ['v0.3.0', 'v0.2.0', 'v0.1.0']);
});
