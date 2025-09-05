@echo off
setlocal

REM Ensure .NET 6 Windows Desktop Runtime is installed (x64)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0runtime-installer-6.ps1" x64
if %ERRORLEVEL% NEQ 0 (
  echo.
  echo Failed to ensure .NET 6 Windows Desktop Runtime. A web page may have been opened for manual install.
  echo After installing, re-run this script or the app EXE.
  pause
)

endlocal
