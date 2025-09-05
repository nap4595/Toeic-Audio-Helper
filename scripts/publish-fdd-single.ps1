Param(
  [string]$Runtime = "win-x64",
  [string]$Configuration = "Release"
)

# Framework-dependent, single-file publish (requires .NET Desktop Runtime on target)
dotnet publish -c $Configuration -r $Runtime --self-contained:$false `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishTrimmed=false `
  -nologo -clp:Summary

Write-Host "Output:" -ForegroundColor Cyan
$out = Join-Path "bin" (Join-Path $Configuration (Join-Path "net6.0-windows" (Join-Path $Runtime "publish")))
Write-Host $out
Get-ChildItem $out | Select-Object Name,Length | Format-Table -Auto

