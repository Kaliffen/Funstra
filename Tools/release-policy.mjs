export const RELEASE_LIMIT = 5;

// Publication date, not commit date or semantic version, defines the latest demo.
export function publishedReleases(releases) {
  return releases.filter(r => !r.draft).sort((a, b) =>
    (Date.parse(b.published_at) || 0) - (Date.parse(a.published_at) || 0)
    || String(b.tag_name ?? b.tag).localeCompare(String(a.tag_name ?? a.tag)));
}

export function retainedReleases(releases) {
  return publishedReleases(releases).slice(0, RELEASE_LIMIT);
}
