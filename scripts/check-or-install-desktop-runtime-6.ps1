<#
Checks if .NET 6 Windows Desktop Runtime is installed (x64). If missing
and winget is available, it will prompt to install via winget.

Note: WPF apps require WindowsDesktop runtime, not just .NET Runtime.
#>

function Test-DesktopRuntime6Installed {
  $key = 'HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App'
  if (Test-Path $key) {
    $versions = (Get-Item $key).GetValueNames() | Sort-Object -Descending
    foreach ($v in $versions) {
      if ($v -match '^6\.') { return $true }
    }
  }
  return $false
}

if (Test-DesktopRuntime6Installed) {
  Write-Host ".NET 6 Windows Desktop Runtime is already installed." -ForegroundColor Green
  exit 0
}

Write-Host ".NET 6 Windows Desktop Runtime not found." -ForegroundColor Yellow

# Try winget if available
if (Get-Command winget -ErrorAction SilentlyContinue) {
  Write-Host "Attempting to install via winget..." -ForegroundColor Cyan
  winget install --id Microsoft.DotNet.DesktopRuntime.6 -e --source winget
  if (Test-DesktopRuntime6Installed) {
    Write-Host "Installation succeeded." -ForegroundColor Green
    exit 0
  } else {
    Write-Host "Installation did not complete. Please install manually." -ForegroundColor Red
  }
} else {
  Write-Host "winget not found. Please install the runtime manually." -ForegroundColor Yellow
}

Write-Host "Manual download (official): https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime?term=windowsdesktop" -ForegroundColor Magenta
exit 1

