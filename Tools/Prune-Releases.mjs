import { execFileSync } from 'node:child_process';
import { publishedReleases, RELEASE_LIMIT } from './release-policy.mjs';

const repo = process.env.GITHUB_REPOSITORY || 'Kaliffen/Funstra';
if (repo !== 'Kaliffen/Funstra') throw new Error(`Refusing retention writes outside Kaliffen/Funstra: ${repo}`);
const dryRun = process.argv.includes('--dry-run');
const gh = (...args) => execFileSync('gh', args, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'inherit'] });
const list = () => publishedReleases(JSON.parse(gh('api', '--paginate', '--slurp', `repos/${repo}/releases?per_page=100`)).flat());
const before = list();
const keep = before.slice(0, RELEASE_LIMIT);
for (const release of before.slice(RELEASE_LIMIT)) {
  console.log(`${dryRun ? 'Would remove' : 'Removing'} published release ${release.tag_name} (${release.id})`);
  if (!dryRun) gh('api', '--method', 'DELETE', `repos/${repo}/releases/${release.id}`);
}
if (!dryRun) {
  const after = list();
  if (after.length !== keep.length || after.some((r, i) => r.id !== keep[i].id)) {
    throw new Error('Release set changed or cleanup incomplete; inspect GitHub and rerun retention before deployment.');
  }
}
console.log(`${dryRun ? 'Would retain' : 'Verified retained'} ${keep.length} published release(s): ${keep.map(r => r.tag_name).join(', ')}`);
// Release deletion removes attached assets, but deliberately preserves source tags.
