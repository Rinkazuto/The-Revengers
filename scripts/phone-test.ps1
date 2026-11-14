# Runs ReVolt on this laptop so you can test it on a phone.
# 1. Builds, starts the API (localhost:5012) and the website (port 5062, open to the network) with REAL data.
# 2. Shows the links to open on the phone.
# 3. With -Tunnel: also starts a dev tunnel, so the phone can use ANY network (4G, university Wi-Fi).
# Press Ctrl+C (tunnel) or Enter to stop everything.
#
# Run from the repo root:
#   powershell -ExecutionPolicy Bypass -File scripts\phone-test.ps1 -Tunnel        (any network, recommended)
#   powershell -ExecutionPolicy Bypass -File scripts\phone-test.ps1                (same Wi-Fi / phone hotspot)
# Add -Reset to start from a clean database (no saved readings), e.g. right before the demo.

param([switch]$Tunnel, [switch]$Reset)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
$procs = @()

function WaitFor($url, $seconds) {
    $deadline = (Get-Date).AddSeconds($seconds)
    while ((Get-Date) -lt $deadline) {
        try { Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 15 | Out-Null; return $true } catch { Start-Sleep 1 }
        Write-Host "." -NoNewline
    }
    return $false
}
function StopTree($id) {
    Get-CimInstance Win32_Process -Filter "ParentProcessId=$id" -ErrorAction SilentlyContinue |
        ForEach-Object { StopTree $_.ProcessId }
    Stop-Process -Id $id -Force -ErrorAction SilentlyContinue
}

try {
    $branch = git rev-parse --abbrev-ref HEAD
    Write-Host "Testing branch: $branch ($(git log -1 --format='%h %s'))" -ForegroundColor Cyan
    if ($branch -ne "main") { Write-Host "  Note: not on main. For the final phone test run 'git switch main; git pull' first." -ForegroundColor Yellow }

    foreach ($port in 5012, 5062) {
        if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) {
            throw "Port $port is already in use. Close the running API / website (Ctrl+C in its terminal) and try again."
        }
    }

    if ($Reset) {
        Remove-Item backend/PowerCheck.Api/app.db* -ErrorAction SilentlyContinue
        Write-Host "Database reset: clean real data, no saved readings." -ForegroundColor Cyan
    }

    Write-Host "Building..." -ForegroundColor Cyan
    dotnet build PowerCheck.slnx -v q -nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Build failed. Run 'dotnet build PowerCheck.slnx' to see the error." }

    Write-Host "Starting the API..." -ForegroundColor Cyan
    $procs += Start-Process dotnet -ArgumentList "run", "--project", "backend/PowerCheck.Api", "--no-build" `
        -PassThru -WindowStyle Hidden -RedirectStandardOutput (Join-Path $env:TEMP "revolt-api.log") -RedirectStandardError (Join-Path $env:TEMP "revolt-api.err")
    if (-not (WaitFor "http://localhost:5012/health" 60)) { throw "The API did not start. See $env:TEMP\revolt-api.log" }

    Write-Host "Starting the website (open to the network, real data)..." -ForegroundColor Cyan
    $oldMock = $env:UseMockApi; $env:UseMockApi = "false"
    $procs += Start-Process dotnet -ArgumentList "run", "--project", "frontend/PowerCheck.Web", "--no-build", "--urls", "http://0.0.0.0:5062" `
        -PassThru -WindowStyle Hidden -RedirectStandardOutput (Join-Path $env:TEMP "revolt-web.log") -RedirectStandardError (Join-Path $env:TEMP "revolt-web.err")
    $env:UseMockApi = $oldMock
    if (-not (WaitFor "http://localhost:5062" 120)) { throw "The website did not start. See $env:TEMP\revolt-web.log" }
    Write-Host ""

    Write-Host ""
    Write-Host "READY. Website on this laptop: http://localhost:5062" -ForegroundColor Green
    Write-Host "Same Wi-Fi or phone hotspot, open on the phone:" -ForegroundColor Green
    Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object { $_.IPAddress -notmatch '^(127|169\.254)\.' -and $_.InterfaceAlias -notmatch 'vEthernet|VirtualBox|VMware|Loopback' } |
        ForEach-Object { Write-Host "   http://$($_.IPAddress):5062   ($($_.InterfaceAlias))" -ForegroundColor Green }
    Write-Host "(If Windows asks to allow dotnet through the firewall, click Allow.)"
    Write-Host ""

    if ($Tunnel) {
        $dt = (Get-Command devtunnel -ErrorAction SilentlyContinue).Source
        if (-not $dt) {
            $links = Join-Path $env:LOCALAPPDATA "Microsoft\WinGet\Links\devtunnel.exe"
            if (Test-Path $links) { $dt = $links }
        }
        if (-not $dt) {
            throw "devtunnel is not installed. Run: winget install Microsoft.devtunnel   then: devtunnel user login   then run this script again."
        }
        Write-Host "Starting the dev tunnel. Open the https://...devtunnels.ms link it prints on the phone." -ForegroundColor Cyan
        Write-Host "The first time, the phone shows a 'developer tunnel' warning page: tap Continue." -ForegroundColor Cyan
        Write-Host "Press Ctrl+C here to stop everything." -ForegroundColor Cyan
        & $dt host -p 5062 --allow-anonymous
    }
    else {
        Read-Host "Press Enter to stop the API and the website"
    }
}
catch {
    Write-Host ""
    Write-Host "STOPPED: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    foreach ($p in $procs) { if ($p) { StopTree $p.Id } }
    Write-Host "API and website stopped."
}
