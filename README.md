# ReVolt

Team **The Revengers**: Theme 3, *Powered to Live Better*.

> The app started as "Power Check", so the code folders are still called `PowerCheck.Api` and `PowerCheck.Web`. Same app.

## Problem
<!-- Who has the problem and what it is. Be specific (not "everyone in Mauritius"). -->

## User
<!-- Our target user, e.g. a household paying its own CEB bill and wondering if solar is worth it. -->

## What it does
1. **Bill calculator:** enter monthly kWh, see the bill broken down by CEB tariff band.
2. **Solar check:** compare the monthly cost without solar vs with solar, plus payback years under the 2026 scheme.
3. **My readings:** save each month's usage and see the trend.

## Data sources
<!-- Link each official source. Copies live in data/sources/. -->
- CEB domestic tariffs:
- CEB 2026 solar scheme leaflet:

## Tech stack
<!-- Frontend, backend, database. -->

## AI tools used
We used AI as a helper. The team chose the idea, the features and the data sources, and checked the results.

- **Claude (Integrator):** help with Git and GitHub commands, drafting the board cards, the API contract and this README, a first version of the CI workflow, and double-checking test results against our own hand calculations.
- <!-- Each teammate: add your tool and what you used it for, in one line. -->

All tariff, solar and cyclone figures come from the official sources listed above, not from AI.

## Surprise card
<!-- What the card was and how we handled it. -->

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
Only the website's port (5062) needs to reach the phone: the website talks to the API on the laptop itself.

**Same Wi-Fi:**
```bash
dotnet run --project frontend/PowerCheck.Web --urls http://0.0.0.0:5062
```
Find the laptop's IP with `ipconfig` (IPv4 Address), then open `http://<that IP>:5062` on the phone. Allow the Windows Firewall prompt.

**Any network (dev tunnel, used for the demo):**
```bash
winget install Microsoft.devtunnel
devtunnel user login
devtunnel host -p 5062 --allow-anonymous
```
Open the `https://...devtunnels.ms` link it prints on the phone. The link works while this window stays open.

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

## Team
| Name | Role |
|---|---|
| | Integrator / Testing |
| | |

## Repo layout
| Folder | Use |
|---|---|
| `backend/` | API code |
| `frontend/` | Mobile web app |
| `data/sources/` | Official CEB tariff and solar scheme documents |
| `data/seed/` | Tariff and solar data as CSV, loaded into the database |
| `tests/` | Automated tests |
| `tests/test-cases/` | Manual test cases (input, expected result, pass/fail) |
| `docs/decision-log.md` | Key decisions with times |
| `docs/slides/` | Pitch slides (max 6) |
| `docs/demo/` | Backup demo video (about 2 min) |
| `docs/wireframes/` | UI sketches |
| `docs/screenshots/` | App screenshots for README and slides |
