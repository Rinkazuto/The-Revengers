# Creates the labels and all board cards (GitHub issues) for Power Check,
# and adds each one to the "Power Check" project.
#
# Needs the GitHub CLI:
#   winget install --id GitHub.cli
#   gh auth login
#   gh auth refresh -s project
#
# Run from the repo root:
#   powershell -ExecutionPolicy Bypass -File scripts\create-board-cards.ps1

$Repo    = "Rinkazuto/The-Revengers"
$Project = "Power Check"

# ---------- Labels ----------
$labels = @(
    @{ n = "must";       c = "d73a4a"; d = "MVP: demo fails without it" },
    @{ n = "should";     c = "fb8c00"; d = "Do after the Musts" },
    @{ n = "could";      c = "bdbdbd"; d = "Only if there is time" },
    @{ n = "blocked";    c = "000000"; d = "Waiting on someone" },
    @{ n = "frontend";   c = "1d76db"; d = "Frontend Lead" },
    @{ n = "backend";    c = "5319e7"; d = "Backend Lead" },
    @{ n = "data";       c = "0e8a16"; d = "Data Lead" },
    @{ n = "qa";         c = "e99695"; d = "Design / QA / Pitch" },
    @{ n = "integrator"; c = "fbca04"; d = "Integrator" },
    @{ n = "feature";    c = "a2eeef"; d = "Feature / task" },
    @{ n = "bug";        c = "b60205"; d = "Something is broken" },
    @{ n = "testing";    c = "c5def5"; d = "Test case" }
)
foreach ($l in $labels) {
    gh label create $l.n --repo $Repo --color $l.c --description $l.d --force | Out-Null
    Write-Host "Label: $($l.n)"
}

# ---------- Cards ----------
# t = title, f = MVP feature, a = area, l = labels, w = what, d = done-when items (separated by |)
$cards = @(
    @{ t="[Integrator] Labels + branch protection on main"; f="Setup / other"; a="Docs / Pitch"; l="must,integrator,feature"
       w="Create labels: must, should, could, blocked, frontend, backend, data, qa, integrator. Add a branch rule on main: require a pull request before merging."
       d="All labels exist|Pushing straight to main is blocked" },
    @{ t="[Integrator] Merge submain into main"; f="Setup / other"; a="Docs / Pitch"; l="must,integrator,feature"
       w="Open a PR submain -> main so main has the API contract and the new Program.cs. Everyone branches from main after this."
       d="main has the contract + new Program.cs|main builds|Team told to branch from main" },
    @{ t="[Backend] Agree API contract with Data + Frontend"; f="Setup / other"; a="Backend"; l="must,backend,feature"
       w="Review docs/api-contract.md with the Data and Frontend Leads. Answer the 6 open questions and mark it AGREED."
       d="Contract marked AGREED|Questions answered|Decision log row added" },
    @{ t="[Data] Collect CEB tariff + 2026 solar scheme sources"; f="Setup / other"; a="Data"; l="must,data,feature"
       w="Find the official CEB domestic tariff and the 2026 solar scheme leaflet. Save copies in data/sources/ and add the links to the README. Answer contract questions 1-4."
       d="Files in data/sources/|Links + date in README|Questions 1-4 answered" },
    @{ t="[Design] Phone wireframes for Bill, Solar, Readings"; f="Setup / other"; a="Docs / Pitch"; l="must,qa,feature"
       w="Figma phone frames (390 px) for the 3 screens and the bottom bar. One column, big tap targets, main button near the bottom."
       d="3 screens drawn|Exported to docs/wireframes/|Frontend Lead has seen them" },
    @{ t="[Frontend] Blazor project + MudBlazor + bottom bar"; f="Setup / other"; a="Frontend"; l="must,frontend,feature"
       w="Create frontend/PowerCheck.Web (Blazor Interactive Server) and add MudBlazor. Bottom bar with Bill / Solar / Readings and 3 empty pages. Branch: feature/frontend-skeleton"
       d="Builds and runs|Bottom bar shows at 390 px|Opens on a real phone" },
    @{ t="[Integrator] Solution file + Program.cs controllers + DB line"; f="Setup / other"; a="Backend"; l="must,integrator,feature"
       w="One .slnx at the repo root with Api (and Web once it exists). Program.cs: register and map controllers, create the Controllers folder. Connection string in appsettings, plus the AddDbContext line commented out until the Data Lead's models card."
       d=".slnx loads all projects|Swagger opens at /swagger|Api + Web run together" },
    @{ t="[Frontend] API service with fake data"; f="Setup / other"; a="Frontend"; l="must,frontend,feature"
       w="Copy the C# records from docs/api-contract.md. IPowerCheckApi with a fake version and a real HttpClient version, switched by UseMockApi in appsettings. Branch: feature/api-service"
       d="Fake data matches the contract|Switching to the real API is one setting" },

    @{ t="[Data] Models + AppDbContext + first migration"; f="Bill calculator"; a="Data"; l="must,data,feature"
       w="Models: TariffBand, Reading (and SolarScheme if needed). AppDbContext in Data/. First migration (only the Data Lead makes migrations). Tell the Integrator when it's merged."
       d="dotnet ef database update creates the tables|A teammate gets the same DB after pulling main" },
    @{ t="[Data] Seed CEB tariff bands from CSV"; f="Bill calculator"; a="Data"; l="must,data,feature"
       w="Real CEB domestic bands in data/seed/tariffs.csv. A seeder loads them on startup if the table is empty."
       d="Fresh DB has the real bands|Numbers match data/sources/" },
    @{ t="[Backend] GET /api/tariffs"; f="Bill calculator"; a="Backend"; l="must,backend,feature"
       w="TariffsController returns the seeded bands in the contract shape, sorted by fromKwh. Depends on the Data models + seed cards."
       d="Works in Swagger|Matches the contract" },
    @{ t="[Backend] POST /api/bill/calculate"; f="Bill calculator"; a="Backend"; l="must,backend,feature"
       w="Bill calculation in Services/, used by BillController. One line per band reached, plus the total and the source. 400 on invalid kWh."
       d="Matches QA test cases|400 for negative or >5000 kWh|Works in Swagger" },
    @{ t="[QA] Test cases: bill calculator"; f="Bill calculator"; a="Tests"; l="must,qa,feature"
       w="Work out the expected bills by hand from the CEB tariff. One Test Case issue each for: 0 kWh, each band edge, typical usage, high usage, invalid input."
       d="5+ Test Case issues|Each has kWh + expected Rs|Run, results recorded" },
    @{ t="[Frontend] Bill calculator page"; f="Bill calculator"; a="Frontend"; l="must,frontend,feature"
       w="kWh input (number keyboard, 16 px+), Calculate button near the bottom, one card per band, the total, and 'Source: CEB tariffs 2026'. Branch: feature/bill-page"
       d="Works with fake data, then the real endpoint|Loading + error states|Checked on a phone" },

    @{ t="[Data] Seed 2026 solar scheme values"; f="Solar check"; a="Data"; l="must,data,feature"
       w="Scheme values in data/seed/: system sizes, install cost, export credit rate, generation per kWp. Seed them like the tariffs."
       d="Fresh DB has the values|Each value traced to a source" },
    @{ t="[Backend] POST /api/solar/estimate"; f="Solar check"; a="Backend"; l="must,backend,feature"
       w="Solar calculation in Services/, used by SolarController. Bill without vs with solar, monthly + yearly savings, payback years, assumptions list. Depends on the bill endpoint and solar seed cards."
       d="Without-solar bill = bill endpoint|paybackYears null when no savings|Works in Swagger" },
    @{ t="[QA] Test cases: solar check"; f="Solar check"; a="Tests"; l="must,qa,feature"
       w="Test Case issues for: a typical household, low usage (no savings), each panel size, invalid input. Expected values worked out by hand."
       d="4+ Test Case issues|Run, results recorded" },
    @{ t="[Frontend] Solar check page"; f="Solar check"; a="Frontend"; l="must,frontend,feature"
       w="kWh and panel size inputs. 'Without solar' and 'With solar' cards, monthly saving and payback years in big text, assumptions list. paybackYears null -> 'Solar doesn't pay back at this usage'. Branch: feature/solar-page"
       d="Works with the real endpoint|Loading + error states|Checked on a phone" },

    @{ t="[Backend] POST + GET /api/readings"; f="My readings"; a="Backend"; l="must,backend,feature"
       w="ReadingsController: save a month's kWh (the same month replaces it), list months oldest first with billRs, trend up/down/flat."
       d="Same month twice -> one reading|Trend correct, null with <2 readings|Works in Swagger" },
    @{ t="[Data] Demo readings (3-6 months)"; f="My readings"; a="Data"; l="should,data,feature"
       w="Seed 3-6 realistic months so the readings page looks good in the demo."
       d="Fresh DB shows a clear trend" },
    @{ t="[QA] Test cases: readings"; f="My readings"; a="Tests"; l="must,qa,feature"
       w="Test Case issues for: save a month, save the same month twice, a future month (rejected), trend up/down/flat."
       d="4+ Test Case issues|Run, results recorded" },
    @{ t="[Frontend] My readings page"; f="My readings"; a="Frontend"; l="must,frontend,feature"
       w="Month + kWh form with Save, saved months listed as cards, up/down/flat trend. Branch: feature/readings-page"
       d="Saves + lists via the API|Loading + error states|Checked on a phone" },

    @{ t="[Integrator] README how to run + phone link (dev tunnel)"; f="Setup / other"; a="Docs / Pitch"; l="must,integrator,feature"
       w="README 'How to run' with exact steps. A public Visual Studio dev tunnel for the Web project."
       d="A teammate runs it from the README|App opens on a phone via the tunnel" },
    @{ t="[Frontend] Error handling + phone polish"; f="Setup / other"; a="Frontend"; l="should,frontend,feature"
       w="ProblemDetails errors shown in plain words on every page. Section 6 rules: tap targets 44-48 px, text 16 px+, no hover-only controls. Branch: feature/frontend-polish"
       d="All pages pass Section 6|Checked with QA on both phones" },
    @{ t="[QA] Phone test runs: iPhone Safari + Android Chrome"; f="Setup / other"; a="Tests"; l="must,qa,feature"
       w="Run the full demo path on both phones at hours 7, 13 and 18. File a Bug issue for anything wrong."
       d="Hour 7 done|Hour 13 done|Hour 18 done, no must-fix bugs" },
    @{ t="[Backend] Unit tests for bill + solar"; f="Setup / other"; a="Tests"; l="should,backend,feature"
       w="xUnit tests in tests/ for the bill and solar calculations, using QA's test case values."
       d="dotnet test passes" },
    @{ t="[QA] Pitch slides (max 6)"; f="Setup / other"; a="Docs / Pitch"; l="must,qa,feature"
       w="Problem, solution, demo, tech diagram, impact. Max 6 slides. QR code to the app if it's reachable."
       d="Slides in docs/slides/|Team reviewed them" },
    @{ t="[QA] Demo script + backup video"; f="Setup / other"; a="Docs / Pitch"; l="must,qa,feature"
       w="Exact taps and data for a 90-120 s demo on a phone. A ~2 min backup video of the core feature."
       d="Rehearsed 3x with a timer|Video in docs/demo/" },
    @{ t="[QA] README: problem, user, sources, AI tools, surprise card"; f="Setup / other"; a="Docs / Pitch"; l="must,qa,feature"
       w="Fill in every README section the judges ask for."
       d="Problem, user, sources, stack, AI tools, surprise card, how to run all filled in" },
    @{ t="[Integrator] Feature freeze: tag v1-demo (hour 18)"; f="Setup / other"; a="Docs / Pitch"; l="must,integrator,feature"
       w="At hour 18: git tag v1-demo and push the tag. Only bug-fix PRs after that."
       d="Tag pushed|Team told: feature freeze" },

    # Backlog: after the script, drag these 5 into the Backlog column
    @{ t="[Frontend] Readings trend chart"; f="My readings"; a="Frontend"; l="could,frontend,feature"
       w="BACKLOG. A MudChart line chart of kWh per month. Only after hour 13."; d="Chart shows on the readings page" },
    @{ t="[Backend] Fuel saved for Mauritius figure on solar result"; f="Solar check"; a="Backend"; l="could,backend,feature"
       w="BACKLOG. Turn the solar kWh into fuel not imported, and show it on the solar result."; d="Figure shown with its source" },
    @{ t="[Frontend] Compare panel sizes"; f="Solar check"; a="Frontend"; l="could,frontend,feature"
       w="BACKLOG. Show 2-3 panel sizes side by side."; d="Comparison shows on the solar page" },
    @{ t="[Backend] Login per household"; f="My readings"; a="Backend"; l="could,backend,feature"
       w="BACKLOG. Each household gets its own readings."; d="Readings are separate per household" },
    @{ t="[Frontend] Share result"; f="Setup / other"; a="Frontend"; l="could,frontend,feature"
       w="BACKLOG. Share the solar result as a link or an image."; d="Result can be shared" }
)

$tmp = Join-Path $env:TEMP "powercheck-card.md"
$i = 0
foreach ($c in $cards) {
    $i++
    $doneLines = ($c.d -split "\|" | ForEach-Object { "- [ ] $_" }) -join "`n"
    $body = "### MVP feature`n$($c.f)`n`n### Area`n$($c.a)`n`n### What needs to be done`n$($c.w)`n`n### Done when`n$doneLines`n"
    Set-Content -Path $tmp -Value $body -Encoding UTF8

    gh issue create --repo $Repo --title $c.t --body-file $tmp --label $c.l --project $Project | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-Host "[$i/$($cards.Count)] Created: $($c.t)" }
    else { Write-Host "[$i/$($cards.Count)] FAILED: $($c.t)" -ForegroundColor Red }
}
Remove-Item $tmp -ErrorAction SilentlyContinue
Write-Host "Done. Drag the 5 BACKLOG cards into the Backlog column."
