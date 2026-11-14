# ReVolt

**Know what your CEB bill really costs, and what solar could save your family.**

Team **The Revengers** · Theme 3: *Powered to Live Better*

**Try it on your phone:** <https://revolt-rb.azurewebsites.net> (no account needed)
API and Swagger: <https://revolt-api-rb.azurewebsites.net/swagger>
**Demo video (about 2 min):** [`docs/demo/ReVolt_Demo.mp4`](docs/demo/ReVolt_Demo.mp4)
**Pitch slides:** [`docs/slides/ReVolt_Pitch_Deck.pptx`](docs/slides/ReVolt_Pitch_Deck.pptx) · Decision log: [`docs/decision-log.md`](docs/decision-log.md)
*(Free hosting plan: the first load after a quiet spell can take about 20 seconds.)*

> The app started as "Power Check", so the code folders are still called `PowerCheck.Api` and `PowerCheck.Web`. Same app.

---

## The problem
On **1 May 2026** CEB electricity rates went up by **about 15%** (General Notice No. 473 of 2026). An average Mauritian home uses about **201.5 kWh a month** (Statistics Mauritius, 2025). At 200 kWh, a tariff 110 bill went from **Rs 1,058.25 to Rs 1,216.75**: Rs 158.50 more a month, **Rs 1,902 more a year**.

Families see one total on the bill, but not *why* it is so high: the price per kWh climbs band by band. Solar is pushed as the answer, but the 2026 household scheme is hard to judge: a 25% grant capped at Rs 75,000, a processing fee, and a **different tariff once you have solar** (prosumer tariff 150A, with its own minimum charge). And every cyclone season the grid goes down, and ordinary rooftop solar switches off with it.

## Who it is for
A Mauritian household that pays its own CEB bill (tariff 110, 110A, 120 or 140), on a phone, that wants to understand its bill and decide **whether solar is worth it** before talking to an installer.

## What it does
| Screen | What you get |
|---|---|
| **Welcome** | What the app does, one tap to start. No account. |
| **Bill calculator** | Type the kWh from your CEB bill: the bill **band by band** with the real 2026 CEB rates, the minimum charge, and a tariff picker (110 / 110A / 120 / 140). |
| **Solar check** ⭐ | Your bill **today vs with solar** under the 2026 scheme: the solar home's bill on **prosumer tariff 150A**, monthly and yearly savings, install cost **after the 25% grant, incl. the Rs 2,000 fee**, years until it pays back, the **export credit shown separately** (not counted in savings), panel size (the CEB price-guide sizes), a link to the official CEB application leaflet, and "How we worked this out" with every assumption and source. |
| **Cyclone mode** 🌀 | Tick the appliances to keep running and for how many hours: **how long a 5 kWh home battery lasts** in a power cut (e.g. fridge + Wi-Fi + one LED light: *up to about 4.1 days*). Plus a **48-hour wind outlook** turned into battery advice, and a line in Kreol. |
| **My readings** | Save each month's kWh: the bill for that month (priced with the tariff **in force that month**), a 6-month chart and the trend ("Up 14% vs Sep 2026"). |

Example: 200 kWh a month, tariff 110 → **Rs 1,216.75** today. With a 2 kWp system: **Rs 211.60** a month (tariff 150A), **saves Rs 1,005.15 a month**, cost after grant and fee Rs 155,750, back in about 13 years.

## Surprise card: "Made for Mauritius" → cyclones
In a cyclone the CEB grid goes down, and grid-tied rooftop solar is required to switch off with it (CEB SSDG Grid Code, sections 2.5.1 and 2.5.2). **Only a battery keeps the fridge cold.** Cyclone mode answers "how long will my battery keep us going?" from real appliance figures (the fridge comes from the energy label of a fridge sold in Mauritius).

The **48-hour wind outlook** (OpenWeather, required by the organisers) turns the forecast into **battery advice only** ("charge your battery now"), using the WMO Beaufort scale on sustained wind. It always ends with *"For official cyclone warnings, check MMS"*. If OpenWeather cannot be reached, it shows a real saved forecast and says so, so it never breaks on stage.

## Data sources
Every number in the app comes from an official or checkable source, stored in [`data/seed/`](data/seed/) and loaded into the database. The full list, with links, retrieval dates and the checks done on every file, is in **[`data/sources/README.md`](data/sources/README.md)**. Figures used in the pitch are in [`data/sources/pitch-figures.md`](data/sources/pitch-figures.md).

| Data | Source |
|---|---|
| 2026 tariffs and minimum charges | CEB, "Tariffs & Rates" ([ceb.mu/tariffs](https://ceb.mu/tariffs)); General Notice No. 473 of 2026 |
| 2023 tariffs and minimum charges | CEB tariff schedule; General Notice No. 1804 of 2022 |
| Prosumer tariffs 150A / 150B (homes with solar) | General Notice No. 473 of 2026, Appendix VIII, pp. 2230–2231 |
| 2026 household solar scheme (grant, fee, net metering, contract) | CEB Household Rooftop Solar PV Scheme leaflet, revised July 2026 |
| Solar system prices, panel ageing | CEB "Solar PV Comparison" price guide ([ceb.mu/pv_calculator](https://ceb.mu/pv_calculator)) |
| Solar output in Mauritius | PVGIS 5.3, European Commission JRC (113.71 kWh per kWp per month) |
| Households and average use | Statistics Mauritius, Energy and Water Statistics 2025, Table 11 |
| Cyclone mode: appliances, battery, inverter | Courts Mammouth (fridge energy label), Huawei LUNA2000 / SUN2000 datasheets, Virginia Tech BSE-301NP, US FTC, TP-Link |
| Cyclone mode: wind | OpenWeather 5-day / 3-hour forecast; Met Office Beaufort scale |

Known limits we show on screen: the CST charge above 500 kWh is "pending URA approval"; the battery time is "up to" (a new, fully charged battery); system prices are CEB estimates.

## Tech stack
| Part | Technology |
|---|---|
| Website (phone first) | **Blazor** (Interactive Server) + **MudBlazor**, .NET 10 |
| API | **ASP.NET Core minimal API**, .NET 10, Swagger |
| Database | **SQLite** with **EF Core** migrations, filled from the CSV files in `data/seed/` on start |
| Weather | OpenWeather API (key kept in user secrets / app settings, never in the repo) |
| Tests | **xUnit** data integrity tests (36), API checks in CI, manual test cases |
| CI | **GitHub Actions** on every pull request |
| Hosting | **Azure App Service** (Linux, free plan): one app for the website, one for the API |

```
Phone browser ──► Website (Blazor Server) ──► API (/api/...) ──► SQLite (seeded from data/seed)
                                                  └──► OpenWeather (wind outlook, saved forecast if offline)
```
The API's endpoints and rules are agreed in **[`docs/api-contract.md`](docs/api-contract.md)**.

## Testing
- **CI on every pull request** ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)): builds everything, runs the **36 data integrity tests** on `data/seed/`, starts the API on a fresh database and checks **16 real results**, e.g. 200 kWh = Rs 1,216.75 and 200 kWh + 3 kWp with solar = Rs 211.60. A red X means "don't merge".
- **Manual test cases** in [`tests/test-cases/`](tests/test-cases/) for the bill, solar, readings and cyclone screens, with expected values worked out from the seed data.
- **Phone tests** on iPhone and Android, using a dev tunnel ([`scripts/phone-test.ps1`](scripts/phone-test.ps1)).
- **One-command local check**: [`scripts/check-app.ps1`](scripts/check-app.ps1).

## How we worked
- **Board:** GitHub Projects ("Power Check" board), one card per task, labels by role and priority. Plan in [`docs/board.md`](docs/board.md).
- **Branches:** feature branch → **`submain`** (integration, tested) → **`main`** (working demo). Both protected: pull request required, no force push.
- **Decisions:** 18 key decisions with times in **[`docs/decision-log.md`](docs/decision-log.md)**.
- **Feature freeze:** tagged **`v1-demo`** on `main`; after it, only bug fixes, docs and deployment.

## AI tools used
We used **Claude (Anthropic)** as an assistant. The team chose the idea and the features, found and checked every data source, made the decisions, and tested every change before it was merged through a reviewed pull request.

- **Ryan, Integrator:** Claude helped with Git and GitHub commands, the board cards, drafting the API contract and this README, the CI workflow, the test and deployment scripts, and re-running checks against our own hand calculations. *I reviewed and tested every pull request, made the merge and freeze calls, and deployed the app.*
- **Rohini, Frontend Lead:** Claude gave step-by-step guidance on the Blazor / MudBlazor frontend: explaining tasks, suggesting code from our API contract and wireframes, and explaining build and CI errors. *I wrote, tested, committed and pushed the code myself.*
- **Lohit, Backend Lead:** Claude wrote most of the backend code (the API endpoints, the bill and solar calculations, the readings and cyclone endpoints, the OpenWeather outlook) and later the Solar and Readings page layouts and the small-phone polish. *I decided what to build with the team, gave it our API contract and the Data Lead's sources, chose the rules (the bill never goes below the CEB minimum charge, the export credit is kept out of savings, "up to about" for battery time), and tested every endpoint in Swagger and Postman against the official numbers before each pull request. When the Data Lead found prosumer tariff 150A in the Gazette, I had the solar maths rewritten to follow it and re-checked the result (200 kWh + 3 kWp = Rs 211.60).*
- **Vihaan, Data Lead:** Claude helped read the CEB, Government Gazette and PVGIS documents, drafted the first seed CSV files and expected test results, and rechecked our sums. *I checked each figure by hand against its source document before it went into the app.*
- **Laura, Design / QA / Pitch:** Claude helped with the initial repo structure, drafted test case tables from our seed data and double-checked some expected values, made early logo and wireframe sketches (redone by us in Figma), helped lay out the slides, and tidied the wording of docs such as the Q&A prep sheet. *The designs, test runs and pitch are our own.*

**Every number in the app comes from the sourced files in `data/`, not from AI.**

## How to run

### You need
- **.NET 10 SDK**: `dotnet --version` should show `10.x`
- **Git**

### 1. Get the code
```bash
git clone https://github.com/Rinkazuto/The-Revengers.git
cd The-Revengers
git switch main
```

> **Run both projects.** The website gets all its numbers from the API, so the API (step 2) must be running before you use the website (step 3). Two terminals, or Visual Studio below.

### 2. Start the API (terminal 1)
```bash
dotnet run --project backend/PowerCheck.Api
```
- Wait for `Now listening on: http://localhost:5012`.
- On the first start it creates `app.db` and fills it from `data/seed/` (CEB tariffs, 2026 solar scheme, prosumer tariff 150A/150B, PVGIS sun data, cyclone data).
- Check: <http://localhost:5012/health> shows `{"status":"ok"}`. All endpoints: <http://localhost:5012/swagger>.
- Optional live wind data: `dotnet user-secrets set "OpenWeather:ApiKey" "<your key>" --project backend/PowerCheck.Api`. Without a key it uses the saved real forecast.

### 3. Start the website (terminal 2)
```bash
dotnet run --project frontend/PowerCheck.Web
```
- Open <http://localhost:5062>.
- The website calls the API at `ApiBaseUrl` (`http://localhost:5012/`) in `frontend/PowerCheck.Web/appsettings.json`. `"UseMockApi": false` (the default) uses the real API. `true` shows built-in sample data, only for working on the layout without the API.

### Or: run both from Visual Studio
1. Open `PowerCheck.slnx`.
2. Right-click the solution in Solution Explorer → **Configure Startup Projects…**
3. Choose **Multiple startup projects**, set **PowerCheck.Api** and **PowerCheck.Web** to **Start**, put PowerCheck.Api first → **OK**.
4. Press **Ctrl+F5** (or F5 to debug). Both start; open <http://localhost:5062>.

This setting is saved on your own computer only, so each teammate does it once.

### 4. Check everything works (one command)
With both closed, from the repo root:
```bash
powershell -ExecutionPolicy Bypass -File scripts\check-app.ps1
```
It builds, starts the API and the website with real data, checks the key numbers (200 kWh bill = Rs 1,216.75; 200 kWh + 3 kWp with solar = Rs 211.60), opens the site in your browser, and stops both when you press Enter.

### 5. Open it on a phone
**Any network (dev tunnel), one command:**
```bash
winget install Microsoft.devtunnel
devtunnel user login
powershell -ExecutionPolicy Bypass -File scripts\phone-test.ps1 -Tunnel
```
Open the `https://...devtunnels.ms` link it prints on the phone (tap **Continue** on the warning page). The link works while that window stays open. Add `-Reset` for a clean database.

**Same Wi-Fi or a phone hotspot:** run `scripts\phone-test.ps1` without `-Tunnel` and open the `http://<laptop IP>:5062` link it prints. University Wi-Fi often blocks this; use a hotspot.

### Reset before a demo
The API rebuilds the database on start, so a reset gives clean real data and no saved readings:
1. Stop the API (Ctrl+C).
2. `Remove-Item backend/PowerCheck.Api/app.db*`
3. Start the API again (step 2).

### If something goes wrong
| You see | Fix |
|---|---|
| "Can't reach the ReVolt server" on the website | The API isn't running: do step 2 (or start both from Visual Studio). |
| Numbers look like samples | `UseMockApi` was set to `true` in `frontend/PowerCheck.Web/appsettings.json`. Set it back to `false`. |
| "address already in use" | Another copy is running. Close that terminal (Ctrl+C) and try again. |
| Old numbers after an update | Do the reset above so the new tables load. |

## Deploy (Azure)
The live app runs on **Azure App Service** (Linux, free F1 plan). To deploy or redeploy the code you have checked out (normally `main`):
```bash
winget install Microsoft.AzureCLI
az login --use-device-code
powershell -ExecutionPolicy Bypass -File scripts\deploy-azure.ps1 -Suffix rb
```
[`scripts/deploy-azure.ps1`](scripts/deploy-azure.ps1) creates (or reuses) the two apps, publishes the API and the website, sets `ApiBaseUrl` and WebSockets (needed by Blazor), and checks the bill for 200 kWh. The published API carries its own copy of `data/seed/` and the saved forecast.

## Limits and future ideas
- **Login per household**, so each family keeps its own readings (wireframe in `docs/wireframes/`). Today readings are one shared list for the demo.
- **Three-phase homes**: the API supports tariff 150B (`phase: "three"`); the screen assumes single-phase.
- Compare panel sizes side by side, share a result, and a "fuel saved for Mauritius" figure.
- Live wind data needs an OpenWeather key on the server; without it the app uses the saved real forecast.

## Team
| Name | Role |
|---|---|
| Ryan | Integrator / Testing |
| Rohini | Frontend Lead |
| Lohit | Backend Lead |
| Vihaan | Data Lead |
| Laura | Design / QA / Pitch |

## Repo layout
| Folder / file | Use |
|---|---|
| `backend/PowerCheck.Api/` | API: endpoints, calculations, database, seeder |
| `frontend/PowerCheck.Web/` | Phone-first website (Blazor + MudBlazor) |
| `data/seed/` | All figures as CSV, loaded into the database |
| `data/sources/` | Source list, checks, pitch figures and raw downloads (PVGIS, OpenWeather) |
| `tests/PowerCheck.DataTests/` | 36 automated data integrity tests (run in CI) |
| `tests/test-cases/` | Manual test cases with expected results |
| `scripts/` | `check-app.ps1`, `phone-test.ps1`, `deploy-azure.ps1`, board card scripts |
| `.github/workflows/ci.yml` | CI on every pull request |
| `docs/api-contract.md` | Agreed API endpoints, rules and change history |
| `docs/decision-log.md` | Key decisions with times |
| `docs/board.md` | Board plan, columns, labels and branch flow |
| `docs/slides/` | Pitch deck |
| `docs/demo/` | Demo video (`ReVolt_Demo.mp4`), pitch script and Q&A prep |
| `docs/wireframes/` | Figma screens and flows |
| `docs/logo/`, `docs/logo.png` | ReVolt logo files |
| `docs/screenshots/` | Screenshots used in the README and slides |
