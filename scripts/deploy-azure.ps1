# Deploys ReVolt to Azure App Service (Linux, Free F1): the API and the website as two web apps.
# Needs the Azure CLI (winget install Microsoft.AzureCLI, then open a NEW terminal).
# Run from the repo root, on the code you want to deploy (normally main):
#   powershell -ExecutionPolicy Bypass -File scripts\deploy-azure.ps1 -Suffix rb
# Run it again any time to redeploy the latest code (it reuses the same apps).
#
# -Suffix   makes the app names unique: revolt-api-<suffix> and revolt-<suffix>
# -Location Azure region (Azure for Students allows only some; the error message lists them)

param(
    [string]$Suffix = "rb",
    [string]$Location = "southafricanorth"
)

$ErrorActionPreference = "Continue"   # az prints warnings on stderr; we check exit codes instead
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$rg   = "revolt-rg"
$plan = "revolt-plan"
$api  = "revolt-api-$Suffix"
$web  = "revolt-$Suffix"
$out  = Join-Path $env:TEMP "revolt-deploy"

function Step($t) { Write-Host ""; Write-Host "== $t" -ForegroundColor Cyan }
function AzRun { & $azExe @args; if ($LASTEXITCODE -ne 0) { throw "az $($args -join ' ') failed" } }
# True if the az command succeeds (used for "does it exist yet?" checks; errors are hidden)
function AzOk { & $azExe @args -o none 2>&1 | Out-Null; return ($LASTEXITCODE -eq 0) }

# ---------- 0. Azure CLI + sign in ----------
$azExe = (Get-Command az -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1).Source
if (-not $azExe) { $p2 = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"; if (Test-Path $p2) { $azExe = $p2 } }
if (-not $azExe) {
    throw "Azure CLI not found. Run: winget install Microsoft.AzureCLI   then open a NEW terminal and run this script again."
}
Step "Signing in to Azure"
if (-not (AzOk account show)) { AzRun login -o none }
Write-Host ("Subscription: " + (& $azExe account show --query name -o tsv))

# ---------- 1. Which .NET runtime does App Service offer? ----------
Step "Checking the .NET runtimes on App Service (Linux)"
$runtimes = & $azExe webapp list-runtimes --os linux -o tsv 2>$null
# Azure lists them as "DOTNETCORE|10.0" (or "DOTNETCORE:10.0" in older CLIs)
$hasNet10 = [bool]($runtimes | Select-String -Pattern 'DOTNETCORE[|:]10\.0')
if ($hasNet10) { $runtime = "DOTNETCORE:10.0"; $selfContained = $false }
else {
    $m = [regex]::Match(($runtimes -join "`n"), 'DOTNETCORE[|:](\d+\.\d+)')
    if (-not $m.Success) { throw "No .NET runtime found on App Service Linux." }
    $runtime = "DOTNETCORE:" + $m.Groups[1].Value
    $selfContained = $true
}
Write-Host "Runtime: $runtime  (self-contained publish: $selfContained)"

# ---------- 2. Resource group, plan, two web apps ----------
Step "Creating (or reusing) $rg, $plan, $api, $web in $Location"
AzRun group create -n $rg -l $Location -o none
if (-not (AzOk appservice plan show -g $rg -n $plan)) { AzRun appservice plan create -g $rg -n $plan --is-linux --sku F1 -l $Location -o none }
foreach ($app in $api, $web) {
    if (-not (AzOk webapp show -g $rg -n $app)) { AzRun webapp create -g $rg -p $plan -n $app --runtime $runtime -o none }
    AzRun webapp update -g $rg -n $app --https-only true -o none
}

# ---------- 3. Publish + zip + deploy ----------
# Zip with "/" in the paths. Windows PowerShell's Compress-Archive writes "\", which Linux
# treats as part of the file name, so subfolders like data/seed would break on Azure.
function New-LinuxZip($sourceDir, $zipPath) {
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $base = (Resolve-Path $sourceDir).Path.TrimEnd('\') + '\'
    $zip = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        Get-ChildItem $sourceDir -Recurse -File | ForEach-Object {
            $name = $_.FullName.Substring($base.Length).Replace('\', '/')
            [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $name)
        }
    }
    finally { $zip.Dispose() }
}

function Deploy($project, $app, $exeName) {
    $dir = Join-Path $out $app
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
    $publishArgs = @("publish", $project, "-c", "Release", "-o", $dir, "-v", "q", "-nologo")
    if ($selfContained) { $publishArgs += @("-r", "linux-x64", "--self-contained", "true") }
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $project" }

    if ($selfContained) {
        # Run the self-contained app directly (no .NET runtime needed on the server)
        AzRun webapp config set -g $rg -n $app --startup-file "chmod +x /home/site/wwwroot/$exeName && /home/site/wwwroot/$exeName" -o none
    }
    $zip = "$dir.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    New-LinuxZip $dir $zip
    AzRun webapp deploy -g $rg -n $app --src-path $zip --type zip -o none
}

Step "Publishing the API to $api"
Deploy "backend/PowerCheck.Api" $api "PowerCheck.Api"

Step "Publishing the website to $web"
Deploy "frontend/PowerCheck.Web" $web "PowerCheck.Web"

# ---------- 4. Settings: the website calls the API; Blazor needs WebSockets ----------
Step "Setting ApiBaseUrl, UseMockApi=false and WebSockets on $web"
AzRun webapp config appsettings set -g $rg -n $web --settings "ApiBaseUrl=https://$api.azurewebsites.net/" "UseMockApi=false" -o none
AzRun webapp config set -g $rg -n $web --web-sockets-enabled true -o none
AzRun webapp restart -g $rg -n $web -o none

# ---------- 5. Check ----------
Step "Checking (the first start can take up to 2 minutes)"
$ok = $false
for ($i = 0; $i -lt 24; $i++) {
    try { $h = Invoke-RestMethod "https://$api.azurewebsites.net/health" -TimeoutSec 20; if ($h.status -eq "ok") { $ok = $true; break } } catch { Write-Host "." -NoNewline; Start-Sleep 5 }
}
Write-Host ""
if ($ok) {
    $bill = (Invoke-RestMethod "https://$api.azurewebsites.net/api/bill/calculate" -Method Post -ContentType "application/json" -Body '{"monthlyKwh":200}').totalRs
    Write-Host "API OK: bill for 200 kWh = Rs $bill (expected 1216.75)" -ForegroundColor Green
} else {
    Write-Host "API not answering yet. Check the log: az webapp log tail -g $rg -n $api" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "WEBSITE (submission link): https://$web.azurewebsites.net" -ForegroundColor Green
Write-Host "API:                       https://$api.azurewebsites.net/swagger" -ForegroundColor Green
