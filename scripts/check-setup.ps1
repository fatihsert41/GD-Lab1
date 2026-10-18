# Lab 1 kurulum kontrolu. Calistirma:
#   powershell -ExecutionPolicy Bypass -File scripts\check-setup.ps1
# (Windows PowerShell 5.1 icin bilerek ASCII yazildi.)

function Show($name, $ok, $detail) {
    $mark = if ($ok) { "[OK]  " } else { "[YOK] " }
    $color = if ($ok) { "Green" } else { "Yellow" }
    Write-Host ($mark + $name.PadRight(28) + $detail) -ForegroundColor $color
}

function Find-Cmd($cmd) { Get-Command $cmd -ErrorAction SilentlyContinue | Select-Object -First 1 }

Write-Host "`n== Diskler ==" -ForegroundColor Cyan
foreach ($d in "C", "D") {
    $drive = Get-PSDrive $d -ErrorAction SilentlyContinue
    if ($drive) { Write-Host ("{0}: bos {1:N1} GB" -f $d, ($drive.Free / 1GB)) }
    else { Write-Host "${d}: bulunamadi" -ForegroundColor Red }
}

Write-Host "`n== Araclar ==" -ForegroundColor Cyan
$git = Find-Cmd git
Show "Git" ($null -ne $git) $(if ($git) { (git --version) + "  ->  " + $git.Source } else { "D:\Dev\Git'e kur" })

$code = Find-Cmd code
Show "VS Code" ($null -ne $code) $(if ($code) { $code.Source } else { "D:\Dev\VSCode'a kur (User Installer)" })

$dotnet = Find-Cmd dotnet
$sdks = if ($dotnet) { (dotnet --list-sdks) -join "; " } else { "" }
Show ".NET SDK" ([bool]$sdks) $(if ($sdks) { $sdks } else { "VS Code C# Dev Kit isteyince kur" })

if ($code) {
    $ext = (code --list-extensions) -join " "
    Show "VS Code Unity eklentisi" ($ext -match "visualstudiotoolsforunity.vstuc") ""
    Show "VS Code C# Dev Kit" ($ext -match "ms-dotnettools.csdevkit") ""
}

Write-Host "`n== Unity ==" -ForegroundColor Cyan
$upm = [Environment]::GetEnvironmentVariable("UPM_CACHE_ROOT", "User")
Show "UPM_CACHE_ROOT (paketler)" ($upm -like "D:*") $(if ($upm) { $upm } else { "SETUP.md adim 2b" })
$asc = [Environment]::GetEnvironmentVariable("ASSETSTORE_CACHE_PATH", "User")
Show "ASSETSTORE_CACHE_PATH" ($asc -like "D:*") $(if ($asc) { $asc } else { "SETUP.md adim 2b" })
$hubPaths = "D:\Unity\Hub\Unity Hub.exe", "C:\Program Files\Unity Hub\Unity Hub.exe"
$hub = $hubPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
Show "Unity Hub" ($null -ne $hub) $(if ($hub) { $hub } else { "D:\Unity\Hub'a kur" })
if ($hub -like "C:*") { Write-Host "      Hub C:'de. Sorun degil ama Editor'leri mutlaka D:'ye kur." -ForegroundColor Yellow }

$editorRoots = "D:\Unity\Editors", "C:\Program Files\Unity\Hub\Editor"
$found = @()
foreach ($r in $editorRoots) {
    if (Test-Path $r) { Get-ChildItem $r -Directory | ForEach-Object { $found += $_.FullName } }
}
Show "Unity 6000.6.6f1" (($found | Where-Object { $_ -like "*6000.6.6f1" }).Count -gt 0) ""
foreach ($f in $found) { Write-Host "      kurulu: $f" }
if ($found | Where-Object { $_ -like "C:*" }) {
    Write-Host "      C:'de Unity Editor var; yer acmak icin Hub'dan kaldirabilirsin." -ForegroundColor Yellow
}

Write-Host "`n== Depo ==" -ForegroundColor Cyan
Show "D:\Dev\GameDev\GD-Lab1" (Test-Path "D:\Dev\GameDev\GD-Lab1\.git") ""
Show "Unity projesi CipherQuest" (Test-Path "D:\Dev\GameDev\GD-Lab1\CipherQuest\ProjectSettings") ""
Write-Host ""
