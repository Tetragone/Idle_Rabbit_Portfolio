"""공개 폴더만 읽어 파일 목록과 참조를 점검한다. Unity 실행 검사는 아니다."""

from pathlib import Path
import hashlib
import json
import re
import sys
from urllib.parse import unquote


ROOT = Path(__file__).resolve().parents[1]
errors = []
# 사용자가 나중에 초기화한 로컬 Git 내부 파일은 공개 파일 검사에서 제외한다.
all_files = [path for path in ROOT.rglob('*') if path.is_file() and '.git' not in path.relative_to(ROOT).parts]
allowed = {'.cs', '.meta', '.prefab', '.md', '.json', '.py'}
blocked_names = {'CSVReader.cs', 'NewPlayerUnitBuilder.cs', 'NewSpriteLayer.cs', 'NewSpriteCollection.cs'}
blocked_dirs = {'Library', 'Temp', 'Logs', 'ProjectSettings', 'UserSettings', 'PackageCache', 'Assets'}

for path in all_files:
    relative = path.relative_to(ROOT)
    if path.name == '.gitignore':
        continue
    if path.suffix not in allowed or path.name in blocked_names:
        errors.append('허용 목록 밖 파일: ' + relative.as_posix())
    if blocked_dirs.intersection(relative.parts):
        errors.append('제외 대상 디렉터리: ' + relative.as_posix())

manifest = json.loads((ROOT / 'docs/SourceManifest.json').read_text(encoding='utf-8'))
listed = set()
for entry in manifest['files']:
    destination = ROOT / entry['destination']
    listed.add(destination)
    if not destination.is_relative_to(ROOT) or not destination.is_file():
        errors.append('원본 대응 파일 누락: ' + entry['destination'])
        continue
    digest = hashlib.sha256(destination.read_bytes()).hexdigest()
    if digest != entry['export_sha256']:
        errors.append('선별 기록과 해시 불일치: ' + entry['destination'])

for folder in ['Code', 'Prefabs']:
    for path in (ROOT / folder).rglob('*'):
        if path.is_file() and path.suffix != '.md' and path not in listed:
            errors.append('선별 기록 밖 파일: ' + path.relative_to(ROOT).as_posix())

guids = {}
for path in ROOT.rglob('*.meta'):
    text = path.read_text(encoding='utf-8')
    match = re.search(r'^guid: ([a-f0-9]{32})$', text, re.M)
    target = Path(str(path)[:-5])
    if not match or not target.is_file():
        errors.append('잘못된 메타 파일: ' + path.relative_to(ROOT).as_posix())
        continue
    if match[1] in guids:
        errors.append('중복 GUID: ' + path.relative_to(ROOT).as_posix())
    guids[match[1]] = target

packages = json.loads((ROOT / 'docs/PackageReferences.json').read_text(encoding='utf-8'))
for path in ROOT.rglob('*.prefab'):
    text = path.read_text(encoding='utf-8')
    for guid in set(re.findall(r'guid: ([a-f0-9]{32})', text)):
        if guid not in guids and guid not in packages and not guid.startswith('0' * 16):
            errors.append('미해결 외부 GUID: ' + path.relative_to(ROOT).as_posix() + ' ' + guid)
    # 비워진 원본 프리팹 연결은 계층 자체를 잃으므로 오류로 처리한다.
    if re.search(r'm_SourcePrefab:\s*\{fileID: 0\}', text):
        errors.append('비워진 중첩 프리팹 원본: ' + path.relative_to(ROOT).as_posix())
    anchors = set(re.findall(r'^--- !u!\d+ &(-?\d+)', text, re.M))
    for file_id in re.findall(r'\{fileID: (-?\d+)\}', text):
        if file_id != '0' and file_id not in anchors:
            errors.append('미해결 내부 fileID: ' + path.relative_to(ROOT).as_posix() + ' ' + file_id)

code_forbidden = re.compile(r'\b(?:NewPixel|Firearm|CSVReader|ENEMY_LIFECYCLE_TESTS)\b|Assets\.PixelFantasy')
secrets = re.compile(r'AKIA[0-9A-Z]{16}|gh[pousr]_[A-Za-z0-9_]{20,}|github_pat_[A-Za-z0-9_]+|sk-[A-Za-z0-9_-]{20,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|[A-Z]:[\\/]Users[\\/]', re.I)
for path in all_files:
    text = path.read_text(encoding='utf-8')
    if path.suffix == '.cs' and code_forbidden.search(text):
        errors.append('제외한 코드의 의존성: ' + path.relative_to(ROOT).as_posix())
    # 도구 자체의 검색 정규식은 검사 대상에서 제외한다.
    if path.suffix != '.py' and secrets.search(text):
        errors.append('인증 정보 또는 사용자 절대 경로 후보: ' + path.relative_to(ROOT).as_posix())
    if path.suffix == '.md':
        if '\ufffd' in text or re.search(r'\?{3,}', text):
            errors.append('문서 인코딩 손상 후보: ' + path.relative_to(ROOT).as_posix())
        for link in re.findall(r'\]\(([^)]+)\)', text):
            if '://' in link or link.startswith('#'):
                continue
            target = (path.parent / unquote(link.split('#')[0])).resolve()
            if not target.is_relative_to(ROOT) or not target.exists():
                errors.append('문서 연결 누락: ' + path.relative_to(ROOT).as_posix() + ' -> ' + link)

result = {'files': len(all_files), 'csharp': len(list(ROOT.rglob('*.cs'))), 'prefabs': len(list(ROOT.rglob('*.prefab'))), 'errors': errors}
print(json.dumps(result, ensure_ascii=False, indent=2))
sys.exit(1 if errors else 0)
