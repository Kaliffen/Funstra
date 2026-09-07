#!/usr/bin/env node
// Builds the Funstra site into site/dist.
//
// The download list is not written by hand: it is generated from the live
// GitHub Releases of the repo named in site/content/site.json. Publishing a
// release is therefore the only step needed to update this page.
//
//   node site/build.mjs                 # fetch releases from the GitHub API
//   node site/build.mjs --offline       # reuse site/.cache/releases.json
//
// GITHUB_TOKEN lifts the anonymous API rate limit but is not required for a
// public repo.

import { readFileSync, writeFileSync, mkdirSync, copyFileSync, existsSync, rmSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { retainedReleases } from '../Tools/release-policy.mjs';

const siteDir = dirname(fileURLToPath(import.meta.url));
const root = resolve(siteDir, '..');
const dist = join(siteDir, 'dist');
const cache = join(siteDir, '.cache', 'releases.json');
const offline = process.argv.includes('--offline');

const content = JSON.parse(readFileSync(join(siteDir, 'content', 'site.json'), 'utf8'));
const process_content = JSON.parse(readFileSync(join(siteDir, 'content', 'process.json'), 'utf8'));
const repo = process.env.GITHUB_REPOSITORY || content.repo;

/* ---------------------------------------------------------------- helpers */

const esc = (s) => String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

const bytes = (n) => {
  if (!Number.isFinite(n) || n <= 0) return 'unknown size';
  const mb = n / 1024 / 1024;
  return mb >= 1024 ? `${(mb / 1024).toFixed(2)} GB` : `${mb.toFixed(1)} MB`;
};

const day = (iso) => (iso ? new Date(iso).toISOString().slice(0, 10) : 'unreleased');

// Deliberately small Markdown subset: enough for a release body, no dependencies.
function markdown(src) {
  if (!src || !src.trim()) return '';
  const inline = (t) =>
    esc(t)
      .replace(/`([^`]+)`/g, '<code>$1</code>')
      .replace(/\[([^\]]+)\]\((https?:[^)\s]+)\)/g, '<a href="$2">$1</a>')
      .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
      .replace(/(^|[\s(])\*([^*\n]+)\*/g, '$1<em>$2</em>');

  const out = [];
  let list = null;
  const closeList = () => { if (list) { out.push(`</${list}>`); list = null; } };

  for (const raw of src.replace(/\r/g, '').split('\n')) {
    const line = raw.trim();
    if (!line) { closeList(); continue; }
    const heading = /^(#{1,6})\s+(.*)$/.exec(line);
    const bullet = /^[-*]\s+(.*)$/.exec(line);
    const numbered = /^\d+[.)]\s+(.*)$/.exec(line);
    if (heading) {
      closeList();
      const level = Math.min(heading[1].length + 2, 6);
      out.push(`<h${level}>${inline(heading[2])}</h${level}>`);
    } else if (bullet || numbered) {
      const want = bullet ? 'ul' : 'ol';
      if (list !== want) { closeList(); out.push(`<${want}>`); list = want; }
      out.push(`<li>${inline((bullet || numbered)[1])}</li>`);
    } else {
      closeList();
      out.push(`<p>${inline(line)}</p>`);
    }
  }
  closeList();
  return out.join('\n');
}

/* ------------------------------------------------------------- release data */

async function releases() {
  if (offline) {
    if (!existsSync(cache)) {
      console.warn('! --offline and no cached releases; building with an empty download list');
      return [];
    }
    return JSON.parse(readFileSync(cache, 'utf8'));
  }
  const headers = { accept: 'application/vnd.github+json', 'user-agent': 'funstra-site-build' };
  if (process.env.GITHUB_TOKEN) headers.authorization = `Bearer ${process.env.GITHUB_TOKEN}`;

  const res = await fetch(`https://api.github.com/repos/${repo}/releases?per_page=50`, { headers });
  if (!res.ok) throw new Error(`GitHub API ${res.status} ${res.statusText} for ${repo}`);

  const data = (await res.json())
    .filter((r) => !r.draft)
    .map((r) => ({
      tag: r.tag_name,
      name: r.name || r.tag_name,
      body: r.body || '',
      prerelease: r.prerelease,
      published_at: r.published_at,
      url: r.html_url,
      assets: (r.assets || [])
        .filter((a) => /\.(zip|7z)$/i.test(a.name))
        .map((a) => ({ name: a.name, size: a.size, downloads: a.download_count, url: a.browser_download_url })),
    }));

  mkdirSync(dirname(cache), { recursive: true });
  writeFileSync(cache, JSON.stringify(data, null, 2));
  return data;
}

/* ------------------------------------------------------------------ render */

function renderDownload(latest) {
  if (!latest) {
    return `<div class="download">
    <div class="download-head"><h2>No published build yet</h2></div>
    <p class="empty">The first release will appear here automatically once it is published.</p>
  </div>`;
  }
  const asset = latest.assets[0];
  const notes = markdown(latest.body);
  const button = asset
    ? `<a class="btn" href="${esc(asset.url)}">Download for Windows</a>
      <div class="asset-meta">
        <b>${esc(asset.name)}</b> · ${bytes(asset.size)}<br>
        Released ${day(latest.published_at)} · ${asset.downloads.toLocaleString('en-US')} downloads
      </div>`
    : `<p class="empty">This release has no downloadable package attached.</p>`;

  return `<div class="download" id="download">
    <div class="download-head">
      <h2>${esc(latest.name)}</h2>
      <span class="ver mono">${esc(latest.tag)}${latest.prerelease ? ' · prerelease' : ''}</span>
    </div>
    <div class="dl-body">${notes || '<p>No release notes were filed for this build.</p>'}</div>
    <div class="asset-row">${button}</div>
    <p class="note">Unsigned Windows x64 build. SmartScreen will warn on first run — choose <b>More info → Run anyway</b>, or build it yourself from source. Extract the whole archive before launching <b>Funstra.exe</b>; the data folder and DLLs must sit beside it.</p>
  </div>`;
}

function renderArchive(older) {
  if (!older.length) return '';
  const rows = older
    .map((r) => {
      const asset = r.assets[0];
      const link = asset
        ? `<a href="${esc(asset.url)}">Download ${esc(r.tag)} · ${bytes(asset.size)} zip</a>`
        : `<a href="${esc(r.url)}">notes</a>`;
      return `      <div class="item">
        <span class="tag mono">${esc(r.tag)}</span>
        <span class="desc">${esc(r.name)}<span class="sub">${day(r.published_at)}</span></span>
        <span class="who">${link} · <a href="${esc(r.url)}">Release notes</a></span>
      </div>`;
    })
    .join('\n');

  return `
  <div class="section" id="archive">
    <h2>Previous versions</h2>
    <p class="lede">We keep the latest five published demos, or all of them while fewer than five exist. Download an earlier version below; these are snapshots, not supported versions.</p>
    <div class="items">
${rows}
    </div>
  </div>`;
}

function renderPage(list) {
  const [latest, ...older] = list;
  const c = content;

  const meta = [
    ['Build', latest ? latest.tag : 'unreleased'],
    ['Engine', c.engine],
    ['Platform', c.platform],
    ['Licence', c.license],
    ['Price', 'Free'],
  ]
    .map(([k, v]) => `<div class="meta-item"><span class="k">${esc(k)}</span><span class="v">${esc(v)}</span></div>`)
    .join('\n      ');

  const nav = c.links.map((l) => `<a href="${esc(l.href)}">${esc(l.label)}</a>`).join('\n      ');

  const li = (items) => items.map((t) => `<li>${t}</li>`).join('\n          ');

  const tiles = c.panel
    .map(
      (p) => `      <div class="score-tile" style="--tile-accent:var(--accent-${esc(p.accent)})">
        <div class="who">${esc(p.who)}</div>
        <div class="score num">${esc(p.score)}<sup>/10</sup></div>
        <div class="lens">${esc(p.lens)}</div>
      </div>`
    )
    .join('\n');

  const shots = c.shots
    .filter((s) => existsSync(join(root, 'Evidence', s.file)))
    .map(
      (s) => `      <figure class="shot">
        <img src="shots/${esc(s.file)}" alt="${esc(s.caption)}" loading="lazy" width="1600" height="900">
        <figcaption>${esc(s.caption)}</figcaption>
      </figure>`
    )
    .join('\n');

  const steps = c.pipeline
    .map(
      (s, i) => `      <div class="item">
        <span class="tag mono">${esc(s.tag)}</span>
        <span class="desc">${s.desc}</span>
        <span class="who">step ${i + 1}</span>
      </div>`
    )
    .join('\n');

  return `<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>${esc(c.title)} — playable prototype</title>
<meta name="description" content="${esc(c.tagline)}">
<meta property="og:title" content="Funstra — playable prototype">
<meta property="og:description" content="${esc(c.tagline)}">
<link rel="stylesheet" href="assets/styles.css">

<div class="wrap">

  <div class="masthead">
    <span class="stamp">${esc(c.stamp)}</span>
    <h1>${esc(c.title)}</h1>
    <p class="subtitle">${esc(c.tagline)}</p>
    <div class="meta-row">
      ${meta}
    </div>
    <div class="navlinks">
      ${nav}
    </div>
  </div>

${renderDownload(latest)}
${older.length ? `<p class="note"><a href="${esc(older[0].url)}">Previous version — ${esc(older[0].tag)}</a> · <a href="#archive">All retained versions ↓</a></p>` : ''}

  <div class="section" id="honest">
    <h2>${esc(c.honesty.heading)}</h2>
    <p class="lede">${esc(c.honesty.lede)}</p>
    <div class="twoup">
      <div class="card" style="--card-accent:var(--accent-dag)">
        <h3>What it is</h3>
        <ul class="findings">
          ${li(c.honesty.is)}
        </ul>
      </div>
      <div class="card" style="--card-accent:var(--accent-nell)">
        <h3>What it is not</h3>
        <ul class="findings">
          ${li(c.honesty.isNot)}
        </ul>
      </div>
    </div>
  </div>
${shots ? `
  <div class="section" id="shots">
    <h2>The build, as it looks</h2>
    <p class="lede">Captures from the shipped executable. Nothing here is a mockup or a target render.</p>
    <div class="shots">
${shots}
    </div>
  </div>` : ''}

  <div class="section" id="panel">
    <h2>What reviewers said</h2>
    <p class="mono">${esc(c.panelBuild)}</p>
    <p class="lede">${esc(c.panelNote)}</p>
    <div class="scorecard">
${tiles}
    </div>
    <p class="note"><a href="dossier.html">Read the ${esc(c.panelBuild)} dossier →</a>${c.previousDossiers.map((d) => `<br><a href="${esc(d.output)}">${esc(d.label)}</a>`).join('')}</p>
  </div>

  <div class="section" id="pipeline">
    <h2>How a demo reaches this page</h2>
    <p class="lede">Every build travels the same route, and no step of it is manual once a demo is finished.</p>
    <div class="items">
${steps}
    </div>
    <p class="note">That is the delivery half. The agents that write and review the demo run their own loop before it — <a href="process.html">how Funstra is made →</a></p>
  </div>

${renderArchive(older)}

  <footer>
    <span>Funstra · ${esc(c.license)} · <a href="https://github.com/${esc(repo)}">github.com/${esc(repo)}</a></span>
    <span>Page generated ${new Date().toISOString().slice(0, 10)} from ${list.length} published release${list.length === 1 ? '' : 's'}</span>
  </footer>

</div>
`;
}


/* ---------------------------------------------------------- process page */

function renderProcess(latest) {
  const p = process_content;
  const li = (items) => items.map((t) => `<li>${t}</li>`).join('\n          ');

  const meta = p.meta
    .map(([k, v]) => `<div class="meta-item"><span class="k">${esc(k)}</span><span class="v">${esc(v)}</span></div>`)
    .join('\n      ');

  const nav = p.links.map((l) => `<a href="${esc(l.href)}">${esc(l.label)}</a>`).join('\n      ');

  const phases = p.phases
    .map(
      (ph, i) => `      <div class="phase" style="--phase-accent:var(--accent-${esc(ph.accent)})">
        <div class="phase-head">
          <span class="phase-n mono num">${esc(ph.n)}</span>
          <div class="phase-titles">
            <span class="phase-who mono">${esc(ph.who)}</span>
            <h3>${esc(ph.title)}</h3>
            <span class="phase-kicker mono">${esc(ph.kicker)}</span>
          </div>
        </div>
        <p class="phase-body">${ph.body}</p>
        <ul class="findings">
          ${li(ph.points)}
        </ul>
      </div>${i < p.phases.length - 1 ? '\n      <div class="arrow" aria-hidden="true"></div>' : ''}`
    )
    .join('\n');

  const reviewers = p.reviewers
    .map(
      (r) => `      <div class="score-tile" style="--tile-accent:var(--accent-${esc(r.accent)})">
        <div class="who">${esc(r.who)}</div>
        <div class="arch">${esc(r.archetype)}</div>
        <div class="lens">${esc(r.lens)}</div>
        <div class="range mono">Typical ${esc(r.range)}</div>
      </div>`
    )
    .join('\n');

  const artefacts = p.artefacts
    .map(
      (a) => `      <div class="item">
        <span class="tag mono">${esc(a.tag)}</span>
        <span class="desc">${a.desc}</span>
        <span class="who">${esc(a.who)}</span>
      </div>`
    )
    .join('\n');

  return `<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>${esc(p.title)} — ${esc(content.title)}</title>
<meta name="description" content="${esc(p.tagline)}">
<meta property="og:title" content="${esc(p.title)} — ${esc(content.title)}">
<meta property="og:description" content="${esc(p.tagline)}">
<link rel="stylesheet" href="assets/styles.css">

<div class="wrap">

  <div class="masthead">
    <span class="stamp">${esc(p.stamp)}</span>
    <h1>${esc(p.title)}</h1>
    <p class="subtitle">${esc(p.tagline)}</p>
    <div class="meta-row">
      ${meta}
    </div>
    <div class="navlinks">
      ${nav}
    </div>
  </div>

  <div class="section" id="loop">
    <h2>The loop</h2>
    <p class="lede">${esc(p.lede)}</p>
    <div class="loop">
${phases}
    </div>
    <div class="loopback">
      <span class="mono">↻ back to 01</span>
      <p>${esc(p.loopNote)}</p>
    </div>
  </div>

  <div class="section" id="panel">
    <h2>The panel</h2>
    <p class="lede">${esc(p.reviewersNote)}</p>
    <div class="scorecard">
${reviewers}
    </div>
  </div>

  <div class="section" id="artefacts">
    <h2>What each turn leaves behind</h2>
    <p class="lede">Every phase commits something readable to the repository, so a finished demo can be traced back through its reviews to the brief that asked for it.</p>
    <div class="items">
${artefacts}
    </div>
  </div>

  <div class="section" id="current">
    <h2>Where the loop is now</h2>
    <div class="items">
      <div class="item">
        <span class="tag mono">Latest</span>
        <span class="desc">${latest ? `${esc(latest.name)}<span class="sub">published ${day(latest.published_at)}</span>` : 'No published release yet'}</span>
        <span class="who">${latest ? `<a href="index.html#download">download</a>` : '—'}</span>
      </div>
      <div class="item">
        <span class="tag mono">Reviews</span>
        <span class="desc">${esc(content.panelBuild)} — four verdicts, published unedited<span class="sub">${esc(content.panelSummary)}</span></span>
        <span class="who"><a href="dossier.html">dossier</a></span>
      </div>
    </div>
  </div>

  <footer>
    <span>${esc(content.title)} · ${esc(content.license)} · <a href="https://github.com/${esc(repo)}">github.com/${esc(repo)}</a></span>
    <span>Page generated ${new Date().toISOString().slice(0, 10)}</span>
  </footer>

</div>
`;
}

/* ------------------------------------------------------------------- build */

const list = retainedReleases(await releases());

rmSync(dist, { recursive: true, force: true });
mkdirSync(join(dist, 'assets'), { recursive: true });
mkdirSync(join(dist, 'shots'), { recursive: true });

writeFileSync(join(dist, 'index.html'), renderPage(list));
writeFileSync(join(dist, 'process.html'), renderProcess(list[0]));
writeFileSync(join(dist, 'releases.json'), JSON.stringify(list, null, 2));
writeFileSync(join(dist, '.nojekyll'), '');
copyFileSync(join(siteDir, 'assets', 'styles.css'), join(dist, 'assets', 'styles.css'));

// A demo owns one dossier; retain previous case files rather than overwriting history.
copyFileSync(join(root, 'Docs', content.dossierFile), join(dist, 'dossier.html'));
for (const dossier of content.previousDossiers) {
  copyFileSync(join(root, 'Docs', dossier.file), join(dist, dossier.output));
}

let copied = 0;
for (const shot of content.shots) {
  const from = join(root, 'Evidence', shot.file);
  if (existsSync(from)) { copyFileSync(from, join(dist, 'shots', shot.file)); copied++; }
  else console.warn(`! missing screenshot: Evidence/${shot.file}`);
}

console.log(`built site/dist — ${list.length} release(s), ${copied} screenshot(s), 2 pages`);
if (!list.length) console.log('  (no releases found; the download card will invite the first one)');
