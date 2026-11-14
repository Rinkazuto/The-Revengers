# Board update 2 (14 Nov 2026, before the feature freeze).
# 1. Closes the cards that are already done (with a comment saying where).
# 2. Creates the new cards and adds them to the "Power Check" project.
#
# Run from the repo root:
#   powershell -ExecutionPolicy Bypass -File scripts\create-board-cards-2.ps1
#
# Afterwards, drag the cards into their columns (see the table in the chat / PR).

$Repo    = "Rinkazuto/The-Revengers"
$Project = "Power Check"

# Use the full path if "gh" is not on PATH in this terminal
$gh = "gh"
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { $gh = "C:\Program Files\GitHub CLI\gh.exe" }

# ---------- 1. Close cards that are already done ----------
$done = @(
    @{ n = 2;  c = "Done: labels created and the 'Protect Main' ruleset protects main and submain." },
    @{ n = 6;  c = "Done in #70 and #75 (submain merged into main)." },
    @{ n = 27; c = "Done in #79 (My readings page)." },
    @{ n = 45; c = "Duplicate of #46 (already closed). Solar estimate is done (#66)." },
    @{ n = 47; c = "Done: contract changes merged (#68, #80)." },
    @{ n = 52; c = "Done in #71 (logo files in docs/logo/)." },
    @{ n = 54; c = "Done in #81: CI builds, starts the API and checks 11 results on every PR." }
)
foreach ($d in $done) {
    & $gh issue close $d.n --repo $Repo --comment $d.c | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-Host "Closed #$($d.n)" } else { Write-Host "Could not close #$($d.n) (maybe already closed)" -ForegroundColor Yellow }
}

# ---------- 2. New cards ----------
# t = title, f = MVP feature, a = area, l = labels, w = what, d = done-when items (separated by |)
$cards = @(
    # ----- Backend -----
    @{ t="[Backend] Solar: with-solar bill on prosumer tariff 150A/150B"; f="Solar check"; a="Backend"; l="must,backend,feature"
       w="Use TariffData.GetProsumerTariff(phase) for the with-solar bill (GN 473/2026): flat Rs 8.58 up to 300 kWh total use, bands above, minimum charge by TOTAL use, CST only above 500 kWh labelled 'pending URA approval'. Add request field phase ('single' default = 150A, 'three' = 150B) and withSolar.exportCreditRs (not counted in savings). tariffCode only affects the without-solar bill. See docs/api-contract.md section 3."
       d="200 kWh + 3 kWp gives with-solar bill Rs 211.60 and savings Rs 1,005.15/month|phase 'three' uses 150B, any other value gives 400|exportCreditRs shown, not in savings|CI green" },
    @{ t="[Backend] Cyclone: read fridge + battery from the seed files"; f="Setup / other"; a="Backend"; l="must,backend,feature"
       w="Replace the 2 placeholders in CycloneData.cs with the Data Lead's sourced values (#78): fridge from data/seed/cyclone_appliances.csv (311 kWh/year = 35.5 W average), battery from cyclone_reference_values.csv. Keep usable 80% but label it 'conservative: Huawei LUNA2000 lists 5 kWh usable; 20% allowed for inverter losses and ageing'."
       d="No 'pending source' left in the cyclone assumptions|Fridge shows 35.5 W with its source|CI green" },
    @{ t="[Backend] Cyclone outlook: GET /api/cyclone/outlook (OpenWeather)"; f="Setup / other"; a="Backend"; l="should,backend,feature"
       w="Required by the organisers. Free 5 day / 3 hour forecast at -20.25, 57.55, next 48 h. Level from sustained wind (wind.speed): ok < 62.97 km/h, charge from 62.97 (Beaufort 8), chargeNow from 88.90 (Beaufort 10), values from cyclone_reference_values.csv. 3 s timeout, 15 min cache, real saved forecast in data/sources/raw/ with isOffline=true. API key ONLY in user-secrets. See contract section 8."
       d="Returns a level with live data|Returns the saved forecast with isOffline=true when offline|No API key in any committed file|Advice ends with 'check MMS'" },

    # ----- Data -----
    @{ t="[Data] Cyclone: source the other appliance watts"; f="Setup / other"; a="Data"; l="should,data,feature"
       w="Add LED light, Wi-Fi router, ceiling fan, TV, laptop and phone charger to data/seed/cyclone_appliances.csv with a source each. Integrator drafts: LED 9 W (FTC / ENERGY STAR), router 10 W (maker measurements), ceiling fan 120 W, TV 120 W, laptop 50 W (Virginia Tech BSE-301NP / DOE table), phone charger 7.5 W (USB BC 1.2 max)."
       d="Every appliance row has a source|data/sources/README.md updated|Backend told the file is ready" },

    # ----- Frontend (from the Frontend Lead's plan) -----
    @{ t="[Frontend] New layout + theme + Cyclone tab"; f="Setup / other"; a="Frontend"; l="must,frontend,feature"
       w="New layout and ReVolt theme: green #2E9E4F, yellow #F7C61A, app bar dark green #2E7D32 with white text, font Calibri/Arial. Add a Cyclone tab to the bottom bar. Fixes the bug where the current page's button is invisible."
       d="All bottom-bar buttons readable, including the current page|Theme colours from the logo|Looks right at 390 px" },
    @{ t="[Frontend] Cyclone API service"; f="Setup / other"; a="Frontend"; l="must,frontend,feature"
       w="Add the cyclone calls to IPowerCheckApi (real + mock): GET /api/cyclone/appliances, POST /api/cyclone/backup (and /outlook if the backend has it). Copy the records from backend/PowerCheck.Api/Contracts/CycloneContracts.cs."
       d="Records match CycloneContracts.cs|400 errors shown as plain messages" },
    @{ t="[Frontend] Cyclone mode page"; f="Setup / other"; a="Frontend"; l="must,frontend,feature"
       w="Tick appliances + hours per day, show 'Your battery keeps these running for about X hours' and the assumptions list. 'For official cyclone warnings, check MMS' always visible. Optional: one Kreol line, checked by a native speaker. No EN | KR switch."
       d="Works with real API data|Assumptions shown under the result|Readable on a phone" },
    @{ t="[Frontend] Solar page: 150A + export credit"; f="Solar check"; a="Frontend"; l="must,frontend,feature"
       w="When the backend 150A card is merged: show withSolar.exportCreditRs separately ('plus Rs X credit for energy sent to CEB'), and 'Single-phase home (tariff 150A)' in the assumptions. Keep 'How to apply' at the bottom."
       d="200 kWh + 3 kWp shows Rs 211.60 with solar|Export credit shown separately, not in savings" },
    @{ t="[Frontend] Bill page restyle"; f="Bill calculator"; a="Frontend"; l="should,frontend,feature"
       w="Apply the new theme. Keep the tariff picker, small ('Tariff 110 - change')."
       d="Matches the new design|Tariff picker still works" },
    @{ t="[Frontend] Readings restyle"; f="My readings"; a="Frontend"; l="should,frontend,feature"
       w="Apply the new theme to the Readings page (bars, trend pill, list)."
       d="Matches the new design|Still works after a database reset" },
    @{ t="[Frontend] Welcome screen"; f="Setup / other"; a="Frontend"; l="could,frontend,feature"
       w="Short welcome screen with the ReVolt logo and one line on what the app does. No EN | KR switch."
       d="Shows the logo|One tap to the Bill page" },
    @{ t="[Bug] UseMockApi must be false + name says 'Power Check'"; f="Setup / other"; a="Frontend"; l="must,frontend,bug"
       w="appsettings.json still has UseMockApi: true, so the site shows fake data. The header says ReVolt, but the browser tab on Solar/Readings and the offline error message say 'Power Check'."
       d="UseMockApi is false and every page tested against the real API|Only 'ReVolt' shown to users" },

    # ----- QA / Pitch -----
    @{ t="[QA] Refresh every solar figure after the 150A change"; f="Solar check"; a="Docs / Pitch"; l="must,qa,feature"
       w="The 150A change makes the with-solar bill higher and payback longer. Recheck every solar number in the slides, demo script, pitch figures sheet and test cases against the real API after the backend 150A card is merged."
       d="Slides, script and test cases match the API|Pitch line: 'Our solar numbers follow the CEB prosumer tariff in the Government Gazette (GN 473/2026)'" },
    @{ t="[QA] Test cases: cyclone mode"; f="Setup / other"; a="Tests"; l="should,qa,testing"
       w="Test cases for /api/cyclone/backup and the Cyclone page: fridge + Wi-Fi 24 h, empty list (400), duplicate id (400), hours 25 (400), all hours 0 (400), batteryKwh 0 and 51 (400)."
       d="6+ test cases written|Run on a phone" },

    # ----- Integrator -----
    @{ t="[Integrator] CI: add the 150A check (Rs 211.60)"; f="Solar check"; a="Tests"; l="should,integrator,feature"
       w="After the backend 150A card is merged, add a 12th CI check: 200 kWh + 3 kWp gives withSolar.monthlyBillRs = 211.60. Then turn on 'Require status checks to pass' (build-and-check) in the ruleset."
       d="CI checks the 150A bill|Ruleset requires CI to pass" },
    @{ t="[Integrator] Rename Power Check to ReVolt in README and docs"; f="Setup / other"; a="Docs / Pitch"; l="should,integrator,feature"
       w="The app is called ReVolt (decision log row 6). Update the README title and docs that still say Power Check. Code and folder names (PowerCheck.Api, PowerCheck.Web) stay as they are."
       d="README and docs say ReVolt|Nothing in code renamed" },
    @{ t="[Integrator] Hosted deploy (app link that works after the demo)"; f="Setup / other"; a="Docs / Pitch"; l="could,integrator,feature"
       w="BACKLOG. Deploy the API and the website (e.g. Azure App Service). Copy data/seed into the published API so the database seeds. Set the website's ApiBaseUrl to the hosted API."
       d="Public link opens on a phone|Bill, Solar and Readings work with real data" }
)

$tmp = Join-Path $env:TEMP "revolt-card.md"
$i = 0
foreach ($c in $cards) {
    $i++
    $doneLines = ($c.d -split "\|" | ForEach-Object { "- [ ] $_" }) -join "`n"
    $body = "### MVP feature`n$($c.f)`n`n### Area`n$($c.a)`n`n### What needs to be done`n$($c.w)`n`n### Done when`n$doneLines`n"
    Set-Content -Path $tmp -Value $body -Encoding UTF8

    & $gh issue create --repo $Repo --title $c.t --body-file $tmp --label $c.l --project $Project | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-Host "[$i/$($cards.Count)] Created: $($c.t)" }
    else { Write-Host "[$i/$($cards.Count)] FAILED: $($c.t)" -ForegroundColor Red }
}
Remove-Item $tmp -ErrorAction SilentlyContinue
Write-Host "Done. Now drag the cards into their columns."
