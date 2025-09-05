Lightweight (Framework-Dependent) Deployment
===========================================

Summary
- Produces a single small EXE (~0.3 MB) that depends on the target PC having the .NET 6 Windows Desktop Runtime installed.
- Recommended when you can control/assume runtime installation on target machines.

Runtime Prerequisite
- Requires .NET 6 Windows Desktop Runtime (x64).
- Check/install helpers:
  - Minimal check+install: powershell -ExecutionPolicy Bypass -File scripts/check-or-install-desktop-runtime-6.ps1
  - Standalone installer launcher (winget or open official page):
    powershell -ExecutionPolicy Bypass -File scripts/runtime-installer-6.ps1

Notes
- This app targets net6.0-windows (WPF). Windows only; macOS is not supported for WPF apps.
- If you need a single-file that works without any runtime installed, use self-contained publish instead.
