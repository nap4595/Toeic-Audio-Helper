Param(
  [ValidateSet('x64','x86')] [string]$Arch = 'x64'
)

function Test-DesktopRuntime6Installed {
  $regPath = "HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\$Arch\sharedfx\Microsoft.WindowsDesktop.App"
  if (Test-Path $regPath) {
    $versions = (Get-Item $regPath).GetValueNames() | Sort-Object -Descending
    foreach ($v in $versions) { if ($v -match '^6\.') { return $true } }
  }
  return $false
}

if (Test-DesktopRuntime6Installed) {
  Write-Host ".NET 6 Windows Desktop Runtime ($Arch) already installed." -ForegroundColor Green
  exit 0
}

Write-Host ".NET 6 Windows Desktop Runtime ($Arch) not found." -ForegroundColor Yellow

# Try winget first (silent)
if (Get-Command winget -ErrorAction SilentlyContinue) {
  $id = ($Arch -eq 'x86') ? 'Microsoft.DotNet.DesktopRuntime.6-x86' : 'Microsoft.DotNet.DesktopRuntime.6'
  Write-Host "Installing via winget ($id)..." -ForegroundColor Cyan
  winget install --id $id -e --source winget --accept-source-agreements --accept-package-agreements
  if (Test-DesktopRuntime6Installed) { Write-Host "Installation succeeded." -ForegroundColor Green; exit 0 }
  Write-Host "winget install did not complete. Falling back to web download..." -ForegroundColor Yellow
}

# Fallback: open official download page for manual install
Write-Host "Opening the official download page in your browser..." -ForegroundColor Cyan
Start-Process "https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime?term=windowsdesktop"
exit 1

