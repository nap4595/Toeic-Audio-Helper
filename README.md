# 🎧 TOEIC Audio Helper

> 토익 기출 MP3를 편리하게 관리하고 재생할 수 있는 Windows 데스크톱 앱

![screenshot](스크린샷%202025-09-04%20234249.png)

## ✨ 주요 기능

- **자동 스캔** — MP3 폴더를 선택하면 하위 폴더까지 재귀적으로 스캔하여 전체/문항/VOCA 트랙을 자동 인식
- **키보드 전용 워크플로우** — 마우스 없이도 모든 조작 가능
- **Full Test / Question 모드** — 전체 듣기 또는 특정 문항만 골라 듣기
- **VOCA LC / RC** — 어휘 트랙 바로 재생
- **다크/라이트 테마** — 🌙/☀ 토글로 전환, 다음 실행 시에도 유지
- **디렉토리 저장** — 한번 설정하면 다음 실행 시 자동 로드

## 🚀 실행 방법

### 사전 요구사항

- **Windows 10/11** (x64 권장)
- **.NET 6 Windows Desktop Runtime** 설치 필요

### 런타임 설치

런타임이 없는 경우 아래 방법 중 하나로 설치하세요:

```bash
# 방법 1: 배치 파일 실행 (권장)
scripts\install-runtime.bat

# 방법 2: PowerShell 스크립트 실행
scripts\runtime-installer-6.ps1
```

### 앱 실행

`ToeicAudioHelper.exe` 를 실행하면 됩니다. (exe 파일 위치는 무관)

## 📁 MP3 폴더 구성

[YBM 공식 홈페이지](https://www.ybmbooks.com/reader/reader.asp)에서 MP3를 다운로드하여 하나의 폴더에 압축 해제하세요.

### 파일명 인식 규칙

| 유형 | 패턴 | 예시 |
|------|------|------|
| 전체 | `TestXX-<문자>.mp3` | `Test01-전체.mp3`, `Test01-Overall.mp3` |
| 문항 | `TestXX-<시작>(-<끝>).mp3` | `Test01-001.mp3`, `Test01-001-003.mp3` |
| VOCA | `TestXX_LC_Voca.mp3`, `TestXX_RC_Voca.mp3` | 정확 일치 또는 경로에 VOCA+LC/RC 포함 |

> **참고:** 대소문자는 구분하지 않지만, 숫자와 구분자는 정확해야 합니다.

## ⌨️ 조작 방법

### 마우스 조작

| 요소 | 동작 |
|------|------|
| TEST 선택 | 01~10 중 선택 |
| LOAD (Full Test) | 선택한 TEST의 전체 트랙 로드 |
| Question | 숫자 입력 (Enter/포커스 아웃 시 3자리 보정), ▲/▼ 증감 |
| LOAD (Question) | 해당 문항 트랙 로드 |
| ▶ ⏸ ⏹ | 재생 / 일시정지 / 정지 |
| ⏪ ⏩ | 5초 뒤로 / 앞으로 |
| 🔁 | 반복 재생 토글 |

### 키보드 단축키

| 키 | 동작 |
|----|------|
| `Space` | 재생/일시정지 토글 |
| `← / →` | 5초 단위 탐색 |
| `Tab` | Full Test / Question 모드 전환 |
| `↑ / ↓` | 번호 조정 (모드에 따라 Test 번호 또는 문항 번호) |
| `]` | Full Test LOAD + 재생 버튼 포커스 |
| `'` | Question LOAD + 재생 버튼 포커스 |
| `/` | Question 모드에서 문항 번호 편집 토글 |
| `Esc` | 편집 모드 해제 + 재생 버튼 포커스 |
| `Enter` | 번호 확정 + LOAD + 재생 버튼 포커스 (편집 중) |

## 🛠️ 빌드 (개발자용)

```bash
dotnet build
dotnet run
```

## 📦 배포물 구성

```
ToeicAudioHelper.exe          # 앱 실행 파일
README.md                     # 이 파일
scripts/
  ├── install-runtime.bat     # 런타임 자동 설치
  └── runtime-installer-6.ps1 # PowerShell 설치 스크립트
```

## 🗺️ 로드맵

- [ ] Part별 재생 기능
- [ ] vol.3, 2, 1 지원 (현재 vol.4만 지원)
- [ ] ETS 기출 1000 외 다른 교재 지원

## 📝 FAQ

**Q: 앱이 .NET 런타임 메시지를 띄우고 실행되지 않아요.**
> `scripts\install-runtime.bat`를 먼저 실행하여 .NET 6 Windows Desktop Runtime(x64)을 설치하세요.

**Q: VOCA나 문항이 인식되지 않아요.**
> 파일명이 위의 인식 규칙을 만족하는지 확인하세요. YBM에서 파일 규칙이 변경되었다면 개발자에게 연락해 주세요.

## 📬 연락처

개발자: nap4595@gmail.com

---

> **시스템 참고:** 일부 Windows N/KN 에디션에서는 WPF MediaElement 사용을 위해 [Media Feature Pack](https://support.microsoft.com/ko-kr/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a) 설치가 필요할 수 있습니다.
