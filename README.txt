TOEIC Audio Helper 안내 (경량 배포)
=================================

1) 실행 방법 (경량 배포 — Framework-Dependent)
- 먼저 .NET 6 Windows Desktop Runtime(x64)이 설치되어 있어야 합니다.
- 런타임이 없을 때는 아래 방법 중 하나로 설치하세요.
  a) scripts\install-runtime.bat 실행 (권장, 자동 설치 시도)
  b) PowerShell에서 scripts\runtime-installer-6.ps1 실행
     - winget이 없으면 공식 다운로드 페이지가 열립니다.
- 런타임 설치 후, ToeicAudioHelper.exe를 실행합니다.

2) MP3 폴더 구성 방법
- 토익 공식 홈페이지(교재 페이지)에서 MP3를 내려받아 하나의 루트 폴더 아래에 모두 압축 해제하세요.
- 앱은 하위 폴더까지 재귀적으로 스캔합니다. 아래 예시와 유사하면 인식됩니다.

  AudioRoot\
    Test_01\
      Test01-전체.mp3            (전체 재생 파일)
      Test01-001.mp3            (문항 001)
      Test01-001-003.mp3        (문항 001~003 구간)
    Test_02\
      Test02-전체.mp3
      Test02-001.mp3
    VOCA\
      Test01_LC_Voca.mp3       (LC VOCA)
      Test01_RC_Voca.mp3       (RC VOCA)

- 인식 규칙(파일명 패턴)
  • 전체:    TestXX-<문자>.mp3 (예: Test01-전체.mp3, Test01-Overall.mp3)
  • 문항:    TestXX-<시작>(-<끝>).mp3 (예: Test01-001.mp3, Test01-001-003.mp3)
  • VOCA:    TestXX_LC_Voca.mp3, TestXX_RC_Voca.mp3 (정확 일치)
            또는 경로에 VOCA와 LC/RC가 포함된 경우 보조 규칙으로 인식

3) 조작 방법
- TEST: 01~10 선택
- Play All: 선택한 TEST의 전체 트랙 재생
- Question: 숫자 입력(Enter/포커스 아웃 시 3자리로 보정), ▲/▼ 버튼으로 증감
- Play Part: 해당 문항(또는 범위) 트랙 재생
- ▶ ⏸ ⏹: 재생 / 일시정지 / 정지
- ⏪ ⏩: 10초 뒤로 / 10초 앞으로
- 🔁: 반복 재생 토글
- 재생 슬라이더: 얇게 보이지만 클릭/드래그 영역은 넓게 설계됨

4) 테마
- 상단바 토글(🌙/☀)로 Dark/Light 전환
- 색상 팔레트는 중앙화되어 있으며 선택은 다음 실행 시에도 유지됩니다.

5) 문제 해결(FAQ)
- 앱이 .NET 런타임 메시지를 띄우고 실행되지 않아요.
  → scripts\install-runtime.bat를 먼저 실행하여 .NET 6 Windows Desktop Runtime(x64)을 설치하세요.
- VOCA나 문항이 인식되지 않아요.
  → 파일명이 위의 규칙을 만족하는지 확인하세요. 대소문자는 구분하지 않지만, 숫자/구분자는 중요합니다.
- 재생이 되지만 진행 표시가 보이지 않아요.
  → 디자인 상 진행색은 숨기고 Thumb만 표시하도록 되어 있습니다(의도된 동작).

6) 배포물 구성(예)
- ToeicAudioHelper.exe      (앱 실행 파일)
- README.txt                (이 파일)
- scripts\install-runtime.bat
- scripts\runtime-installer-6.ps1

7) 시스템 요구사항
- Windows 10/11 (x64 권장)
- .NET 6 Windows Desktop Runtime (경량 배포 시 필수)
- WPF MediaElement 사용: 일부 N/KN 에디션은 Media Feature Pack이 필요할 수 있습니다.

