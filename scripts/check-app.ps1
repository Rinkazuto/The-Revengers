# Checks that ReVolt runs on this laptop, exactly as the README says.
# 1. Builds both projects.
# 2. Starts the API (port 5012) and the website (port 5062) with REAL data (UseMockApi=false).
# 3. Checks the key numbers against the real CEB data.
# 4. Opens the website in your browser, then stops both when you press Enter.
#
# Run from the repo root (close any running API / website first):
#   powershell -ExecutionPolicy Bypass -File scripts\check-app.ps1
# Add -NoWait to skip the browser and stop straight away.

param([switch]$NoWait)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
$api = "http://localhost:5012"
$web = "http://localhost:5062"
$script:fails = 0
$procs = @()

function Ok($name)            { Write-Host "  OK    $name" -ForegroundColor Green }
function Bad($name, $detail)  { Write-Host "  FAIL  $name : $detail" -ForegroundColor Red; $script:fails++ }
function Check($name, $expected, $actual) {
    if ("$expected" -eq "$actual") { Ok "$name = $actual" } else { Bad $name "expected $expected, got $actual" }
}
function WaitFor($url, $seconds) {
    for ($i = 0; $i -lt $seconds; $i++) {
        try { Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2 | Out-Null; return $true } catch { Start-Sleep 1 }
    }
    return $false
}
function Post($path, $body) {
    Invoke-RestMethod "$api$path" -Method Post -ContentType "application/json" -Body $body
}
function StopTree($id) {
    Get-CimInstance Win32_Process -Filter "ParentProcessId=$id" -ErrorAction SilentlyContinue |
        ForEach-Object { StopTree $_.ProcessId }
    Stop-Process -Id $id -Force -ErrorAction SilentlyContinue
}

try {
    # ---------- 0. Ports free? ----------
    foreach ($port in 5012, 5062) {
        if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) {
            throw "Port $port is already in use. Close the running API / website (Ctrl+C in its terminal) and try again."
        }
    }

    # ---------- 1. Build ----------
    Write-Host "1. Building..." -ForegroundColor Cyan
    dotnet build PowerCheck.slnx -v q -nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Build failed. Run 'dotnet build PowerCheck.slnx' to see the error." }
    Ok "build"

    # ---------- 2. Start the API ----------
    Write-Host "2. Starting the API on $api ..." -ForegroundColor Cyan
    $apiLog = Join-Path $env:TEMP "revolt-api.log"
    $procs += Start-Process dotnet -ArgumentList "run", "--project", "backend/PowerCheck.Api", "--no-build" `
        -PassThru -WindowStyle Hidden -RedirectStandardOutput $apiLog -RedirectStandardError "$apiLog.err"
    if (-not (WaitFor "$api/health" 60)) { throw "The API did not start. See $apiLog" }
    Ok "API is up"

    # ---------- 3. Check the API ----------
    Write-Host "3. Checking real numbers..." -ForegroundColor Cyan
    Check "bill 200 kWh (Rs)"                   1216.75 (Post "/api/bill/calculate" '{"monthlyKwh":200}').totalRs
    $s = Post "/api/solar/estimate" '{"monthlyKwh":200,"systemSizeKwp":3}'
    Check "solar without (= bill, Rs)"          1216.75 $s.withoutSolar.monthlyBillRs
    Check "solar with, tariff 150A (Rs)"        211.6   $s.withSolar.monthlyBillRs
    Check "solar savings per month (Rs)"        1005.15 $s.monthlySavingsRs
    $c = Invoke-RestMethod "$api/api/cyclone/appliances"
    if ($c.appliances.Count -gt 0) { Ok "cyclone appliances = $($c.appliances.Count)" } else { Bad "cyclone appliances" "empty list" }
    try { Invoke-RestMethod "$api/api/readings" | Out-Null; Ok "readings list answers" } catch { Bad "readings list" $_.Exception.Message }

    # ---------- 4. Start the website with real data ----------
    Write-Host "4. Starting the website on $web (real data)..." -ForegroundColor Cyan
    $oldMock = $env:UseMockApi
    $env:UseMockApi = "false"
    $webLog = Join-Path $env:TEMP "revolt-web.log"
    $procs += Start-Process dotnet -ArgumentList "run", "--project", "frontend/PowerCheck.Web", "--no-build" `
        -PassThru -WindowStyle Hidden -RedirectStandardOutput $webLog -RedirectStandardError "$webLog.err"
    $env:UseMockApi = $oldMock
    if (-not (WaitFor $web 60)) { throw "The website did not start. See $webLog" }
    $html = (Invoke-WebRequest $web -UseBasicParsing).Content
    if ($html -match "ReVolt") { Ok "website is up and says ReVolt" } else { Bad "website" "page doesn't mention ReVolt" }

    # ---------- Result ----------
    Write-Host ""
    if ($script:fails -eq 0) { Write-Host "ALL CHECKS PASSED" -ForegroundColor Green }
    else { Write-Host "$($script:fails) CHECK(S) FAILED" -ForegroundColor Red }

    if (-not $NoWait) {
        Write-Host ""
        Write-Host "Opening $web in your browser. Try the Bill, Solar, Readings and Cyclone pages."
        Write-Host "Note: the appsettings.json file is NOT changed; this run just uses real data."
        Start-Process $web
        Read-Host "Press Enter to stop the API and the website"
    }
}
catch {
    Write-Host ""
    Write-Host "STOPPED: $($_.Exception.Message)" -ForegroundColor Red
    $script:fails++
}
finally {
    foreach ($p in $procs) { if ($p) { StopTree $p.Id } }
    Write-Host "API and website stopped."
}

if ($script:fails -gt 0) { exit 1 } else { exit 0 }
