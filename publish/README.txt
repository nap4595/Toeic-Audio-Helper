TOEIC Audio Helper 안내 (경량 배포)
v1.1
2025-09-10
개발자: nap4595@gmail.com

=================================

1) 실행 방법 (경량 배포 — Framework-Dependent)
- exe파일 하나만 있으면 작동합니다! (해당 파일의 위치는 관련 없습니다.)
- 먼저 .NET 6 Windows Desktop Runtime(x64)이 설치되어 있어야 합니다.
- 런타임이 없을 때는 아래 방법 중 하나로 설치하세요.
  a) scripts\install-runtime.bat 실행 (권장, 자동 설치 시도)
  b) PowerShell에서 scripts\runtime-installer-6.ps1 실행
     - winget이 없으면 공식 다운로드 페이지가 열립니다.
- 런타임 설치 후, ToeicAudioHelper.exe를 실행합니다.

2) MP3 폴더 구성 방법
- 토익 공식 홈페이지( https://www.ybmbooks.com/reader/reader.asp )에서 MP3를 내려받아 하나의 루트 폴더 아래에 모두 압축 해제하세요.
- 앱은 하위 폴더까지 재귀적으로 스캔합니다. 폴더 구조와 상관없이 '파일명'을 기준으로 스캔하므로 루트 폴더 안에 아무렇게나 압축 해제하시면 됩니다.
- 디렉토리는 다음 실행 시에도 저장됩니다. 

- 인식 규칙(파일명 패턴)
  • 전체:    TestXX-<문자>.mp3 (예: Test01-전체.mp3, Test01-Overall.mp3)
  • 문항:    TestXX-<시작>(-<끝>).mp3 (예: Test01-001.mp3, Test01-001-003.mp3)
  • VOCA:    TestXX_LC_Voca.mp3, TestXX_RC_Voca.mp3 (정확 일치)
            또는 경로에 VOCA와 LC/RC가 포함된 경우 보조 규칙으로 인식

3) 조작 방법

■ 마우스 조작
- TEST: 01~10 선택
- LOAD (Full Test): 선택한 TEST의 전체 트랙 로드 
- Question: 숫자 입력(Enter/포커스 아웃 시 3자리로 보정), ▲/▼ 버튼으로 증감
- LOAD (Question): 해당 문항(또는 해당 문항이 포함된 범위) 트랙 로드
- ▶ ⏸ ⏹: 재생 / 일시정지 / 정지
- ⏪ ⏩: 5초 뒤로 / 5초 앞으로
- 🔁: 반복 재생 토글
- Full Test: Full Test/Question 모드 토글 버튼
- 재생 슬라이더: 클릭하여 이동 가능

■ 키보드 단축키 (완전한 키보드 전용 워크플로우 지원)
- 스페이스바: 재생/일시정지 토글
- ← / →: 5초 단위 탐색 (뒤로/앞으로)
- TAB: Full Test/Question 모드 전환
- ↑ / ↓: 번호 조정 (Full Test 모드: Test 번호, Question 모드: 문항 번호)
- ] (오른쪽 대괄호): Full Test LOAD + 재생 버튼 포커스
- ' (단일 따옴표): Question LOAD + 재생 버튼 포커스  
- / (슬래시): Question 모드에서 문항 번호 편집 토글 (포커스 in/out)
- ESC: 편집 모드 해제 및 재생 버튼 포커스
- Enter (문항 편집 중): 번호 확정 + LOAD + 재생 버튼 포커스

4) 테마
- 상단바 토글(🌙/☀)로 Dark/Light 전환
- 테마 선택은 다음 실행 시에도 유지됩니다.

5) 문제 해결(FAQ)
- 앱이 .NET 런타임 메시지를 띄우고 실행되지 않아요.
  → scripts\install-runtime.bat를 먼저 실행하여 .NET 6 Windows Desktop Runtime(x64)을 설치하세요.
- VOCA나 문항이 인식되지 않아요.
  → 파일명이 위의 규칙을 만족하는지 확인하세요. 대소문자는 구분하지 않지만, 숫자/구분자는 중요합니다. 만약 ybm에서 제공하는 파일 규칙이 변경되었다면 업데이트 버전을 확인한 후 다시 다운로드하거나 개발자에게 연락해 주세요.

6) 배포물 구성(예)
- ToeicAudioHelper.exe      (앱 실행 파일)
- README.txt                (이 파일)
- scripts\install-runtime.bat 
- scripts\runtime-installer-6.ps1

7) 시스템 요구사항
- Windows 10/11 (x64 권장)
- .NET 6 Windows Desktop Runtime (script 폴더 안의 bat파일로 설치 가능)
- WPF MediaElement 사용: 일부 N/KN 에디션은 Media Feature Pack이 필요할 수 있습니다.

---

개발 예정 기능
- part별 재생 기능
- 현재 vol.4만 지원함. 3,2,1 순으로 개발 예정
- 현재 ets 기출 1000 만 지원함. 다른 교재 지원 예정
---

[������ ����] UI ŰƮ ���� ���̵�� ../UiKit.Wpf/README.md ������ ��������.

