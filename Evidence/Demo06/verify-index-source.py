"""Check staged release source against the exact exported input snapshot."""
import hashlib, json, pathlib, subprocess, zipfile, datetime

root = pathlib.Path(__file__).resolve().parents[2]
def git(*args):
    return subprocess.check_output(['git', *args], cwd=root)
def digest(data):
    return hashlib.sha256(data).hexdigest().upper()
manifest = json.loads((root/'Evidence/Demo06/integration-source.json').read_text(encoding='utf-8-sig'))
raw_same, normalized = [], []
with zipfile.ZipFile(root/'Evidence/Demo06/integration-source-files.zip') as archive:
    assert sorted(archive.namelist()) == sorted(f['path'] for f in manifest['files'])
    for item in manifest['files']:
        path = item['path']
        raw = (root/path).read_bytes()
        assert digest(raw) == item['sha256'], f'Working source drift: {path}'
        assert archive.read(path) == raw, f'Raw snapshot differs: {path}'
        staged = git('show', ':'+path)
        if staged == raw:
            raw_same.append(path)
        else:
            attributes = git('check-attr', '--cached', 'text', 'eol', '--', path).decode()
            assert ': text: unset' not in attributes, f'Unexpected binary normalization: {path}'
            assert staged == raw.replace(b'\r\n', b'\n'), f'Staged source differs beyond configured CRLF normalization: {path}'
            normalized.append(path)
index_tree = git('write-tree').decode().strip()
trees = {path:git('rev-parse', index_tree+':'+path).decode().strip() for path in ['Assets','Packages','ProjectSettings']}
receipt = dict(assemblySha256=manifest['assemblySha256'], verifiedAt=datetime.datetime.now(datetime.timezone.utc).isoformat(),
    sourceFiles=len(manifest['files']), rawIdentical=len(raw_same), configuredNewlineNormalizationOnly=len(normalized),
    normalizedFiles=normalized, otherDifferences=0, sourceTreeObjects=trees,
    exactRawSnapshot='Evidence/Demo06/integration-source-files.zip',
    method='Every staged source blob matches the actual exported input byte-for-byte, or solely through repository-configured CRLF-to-LF normalization. The exact raw226 inputs are independently preserved and verified in the source ZIP. No source behavior changes were made after B export.')
(root/'Evidence/Demo06/committed-source-verification.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
print(f"PASS: {len(raw_same)} raw-identical staged inputs; {len(normalized)} differ only by configured newline normalization; zero other differences.")
