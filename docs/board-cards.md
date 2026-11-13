# Board cards: ready to fill in

For every card: **Issues → New issue → Feature / Task**, then copy the fields below.
Sidebar each time: **Labels** as listed · **Projects: Power Check** · **Assignees: empty**.
Create them in this order so the To do column is in the right order.

---

# 🔧 SETUP

## Card 1
- **Title:** `[Integrator] Labels + branch protection on main`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, integrator
- **What needs to be done:**
  ```
  Create labels: must, should, could, blocked, frontend, backend, data, qa, integrator.
  Add a branch protection rule on main: require a pull request before merging.
  ```
- **Done when:**
  ```
  - [ ] All labels exist
  - [ ] Pushing straight to main is blocked
  ```

## Card 2
- **Title:** `[Integrator] Merge submain into main`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, integrator
- **What needs to be done:**
  ```
  Open a PR submain → main once submain is tested and working.
  submain stays the integration branch: everyone branches from submain and PRs into submain.
  main only gets tested PRs from submain, so the working demo on main never breaks.
  ```
- **Done when:**
  ```
  - [ ] main has the tested submain changes
  - [ ] main builds (dotnet build PowerCheck.slnx) and the app runs
  - [ ] Team told: branch from submain, PR into submain, never into main
  ```

## Card 3
- **Title:** `[Backend] Agree API contract with Data + Frontend`
- **MVP feature:** Setup / other
- **Area:** Backend
- **Labels:** must, backend
- **What needs to be done:**
  ```
  Review docs/api-contract.md with the Data Lead and Frontend Lead.
  Answer the 6 open questions, adjust endpoints if needed, mark it AGREED.
  ```
- **Done when:**
  ```
  - [ ] Contract says AGREED
  - [ ] Open questions answered or assigned
  - [ ] Decision log row added
  ```

## Card 4
- **Title:** `[Data] Collect CEB tariff + 2026 solar scheme sources`
- **MVP feature:** Setup / other
- **Area:** Data
- **Labels:** must, data
- **What needs to be done:**
  ```
  Find the official CEB domestic tariff and the 2026 solar scheme leaflet.
  Save copies in data/sources/ and list the links in the README.
  Answer API contract questions 1–4 (fixed charge/VAT, how bands work, scheme rules, generation per kWp).
  ```
- **Done when:**
  ```
  - [ ] Source files in data/sources/
  - [ ] Links + date accessed in README
  - [ ] Contract questions 1–4 answered
  ```

## Card 5
- **Title:** `[Design] Phone wireframes for Bill, Solar, Readings`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Figma phone frames (390 px) for the 3 screens + bottom bar.
  One column, big tap targets, main button near the bottom.
  ```
- **Done when:**
  ```
  - [ ] 3 screens drawn at phone size
  - [ ] Exported to docs/wireframes/
  - [ ] Frontend Lead has seen them
  ```

## Card 6
- **Title:** `[Frontend] Blazor project + MudBlazor + bottom bar`
- **MVP feature:** Setup / other
- **Area:** Frontend
- **Labels:** must, frontend
- **What needs to be done:**
  ```
  Create frontend/PowerCheck.Web (Blazor, Interactive Server) and add MudBlazor.
  Bottom app bar with Bill / Solar / Readings and 3 empty pages.
  Branch: feature/frontend-skeleton
  ```
- **Done when:**
  ```
  - [ ] Builds and runs
  - [ ] Bottom bar shows in Chrome device mode at 390 px
  - [ ] Opens on a real phone
  ```

## Card 7
- **Title:** `[Integrator] Solution file + Program.cs controllers + DB line`
- **MVP feature:** Setup / other
- **Area:** Backend
- **Labels:** must, integrator
- **What needs to be done:**
  ```
  Create one .slnx at the repo root with the Api project (and Web once it exists).
  Program.cs: register + map controllers, create the Controllers folder.
  Add the connection string to appsettings.json and the AddDbContext line (commented out until Card 9).
  ```
- **Done when:**
  ```
  - [ ] Opening the .slnx loads all projects
  - [ ] Api builds, Swagger opens at /swagger
  - [ ] Api + Web can run together
  ```

## Card 8
- **Title:** `[Frontend] API service with fake data`
- **MVP feature:** Setup / other
- **Area:** Frontend
- **Labels:** must, frontend
- **What needs to be done:**
  ```
  Copy the C# records from docs/api-contract.md.
  IPowerCheckApi with a fake version and a real HttpClient version, switched by UseMockApi in appsettings.
  Branch: feature/api-service
  ```
- **Done when:**
  ```
  - [ ] Pages get fake data shaped exactly like the contract
  - [ ] Switching to the real API is one setting
  ```

---

# ⚡ FEATURE 1: BILL CALCULATOR

## Card 9
- **Title:** `[Data] Models + AppDbContext + first migration`
- **MVP feature:** Bill calculator
- **Area:** Data
- **Labels:** must, data
- **What needs to be done:**
  ```
  Models: TariffBand, Reading (and SolarScheme if needed).
  AppDbContext in Data/. Create the first migration (only the Data Lead makes migrations).
  Tell the Integrator when merged so the AddDbContext line is switched on.
  ```
- **Done when:**
  ```
  - [ ] dotnet ef database update creates the tables
  - [ ] Teammate can pull main and get the same DB
  ```

## Card 10
- **Title:** `[Data] Seed CEB tariff bands from CSV`
- **MVP feature:** Bill calculator
- **Area:** Data
- **Labels:** must, data
- **What needs to be done:**
  ```
  Put the real CEB domestic bands in data/seed/tariffs.csv.
  Seeder loads them on startup if the table is empty.
  ```
- **Done when:**
  ```
  - [ ] Fresh DB has the real CEB bands
  - [ ] Numbers match the source in data/sources/
  ```

## Card 11
- **Title:** `[Backend] GET /api/tariffs`
- **MVP feature:** Bill calculator
- **Area:** Backend
- **Labels:** must, backend
- **What needs to be done:**
  ```
  TariffsController returns the seeded bands in the contract shape, sorted by fromKwh.
  Depends on: Card 9, Card 10.
  ```
- **Done when:**
  ```
  - [ ] Works in Swagger
  - [ ] Response matches docs/api-contract.md
  ```

## Card 12
- **Title:** `[Backend] POST /api/bill/calculate`
- **MVP feature:** Bill calculator
- **Area:** Backend
- **Labels:** must, backend
- **What needs to be done:**
  ```
  Bill calculation in Services/, used by BillController.
  One line per band reached, total, source. 400 on invalid kWh.
  ```
- **Done when:**
  ```
  - [ ] Matches QA's hand-calculated test cases (Card 13)
  - [ ] 400 for negative or > 5000 kWh
  - [ ] Works in Swagger
  ```

## Card 13
- **Title:** `[QA] Test cases: bill calculator`
- **MVP feature:** Bill calculator
- **Area:** Tests
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Work out expected bills by hand from the CEB tariff.
  Create one Test Case issue each for: 0 kWh, each band edge, typical usage, high usage, invalid input.
  ```
- **Done when:**
  ```
  - [ ] 5+ Test Case issues created
  - [ ] Each has input + expected Rs value
  - [ ] Run against the endpoint, results recorded
  ```

## Card 14
- **Title:** `[Frontend] Bill calculator page`
- **MVP feature:** Bill calculator
- **Area:** Frontend
- **Labels:** must, frontend
- **What needs to be done:**
  ```
  kWh input (number keyboard, 16 px+), Calculate button near the bottom,
  one card per tariff band, the total, and "Source: CEB tariffs 2026".
  Branch: feature/bill-page
  ```
- **Done when:**
  ```
  - [ ] Works with fake data, then the real POST /api/bill/calculate
  - [ ] Loading and error states
  - [ ] Checked on a phone
  ```

---

# ☀️ FEATURE 2: SOLAR CHECK (main feature)

## Card 15
- **Title:** `[Data] Seed 2026 solar scheme values`
- **MVP feature:** Solar check
- **Area:** Data
- **Labels:** must, data
- **What needs to be done:**
  ```
  Put the scheme values in data/seed/ (system sizes, install cost, export credit rate, generation per kWp).
  Seed them like the tariffs.
  ```
- **Done when:**
  ```
  - [ ] Fresh DB has the scheme values
  - [ ] Each value traced to a source in data/sources/
  ```

## Card 16
- **Title:** `[Backend] POST /api/solar/estimate`
- **MVP feature:** Solar check
- **Area:** Backend
- **Labels:** must, backend
- **What needs to be done:**
  ```
  Solar calculation in Services/, used by SolarController.
  Without vs with solar bill, monthly + yearly savings, payback years, assumptions list.
  Depends on: Card 12, Card 15.
  ```
- **Done when:**
  ```
  - [ ] Without-solar bill equals /api/bill/calculate for the same kWh
  - [ ] paybackYears is null when there are no savings
  - [ ] Works in Swagger
  ```

## Card 17
- **Title:** `[QA] Test cases: solar check`
- **MVP feature:** Solar check
- **Area:** Tests
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Test Case issues for: typical household, low usage (no savings), each panel size, invalid input.
  Expected values worked out by hand from the scheme.
  ```
- **Done when:**
  ```
  - [ ] 4+ Test Case issues created
  - [ ] Run against the endpoint, results recorded
  ```

## Card 18
- **Title:** `[Frontend] Solar check page`
- **MVP feature:** Solar check
- **Area:** Frontend
- **Labels:** must, frontend
- **What needs to be done:**
  ```
  kWh and panel size inputs. "Without solar" and "With solar" cards,
  monthly saving and payback years in big text, assumptions list.
  paybackYears null → "Solar doesn't pay back at this usage".
  Branch: feature/solar-page
  ```
- **Done when:**
  ```
  - [ ] Works with the real POST /api/solar/estimate
  - [ ] Loading and error states
  - [ ] Checked on a phone
  ```

---

# 📈 FEATURE 3: MY READINGS

## Card 19
- **Title:** `[Backend] POST + GET /api/readings`
- **MVP feature:** My readings
- **Area:** Backend
- **Labels:** must, backend
- **What needs to be done:**
  ```
  ReadingsController: save a month's kWh (same month replaces it), list all months oldest first,
  billRs per month, trend up/down/flat.
  ```
- **Done when:**
  ```
  - [ ] Same month twice → one reading
  - [ ] Trend correct, null with < 2 readings
  - [ ] Works in Swagger
  ```

## Card 20
- **Title:** `[Data] Demo readings (3–6 months)`
- **MVP feature:** My readings
- **Area:** Data
- **Labels:** should, data
- **What needs to be done:**
  ```
  Seed 3–6 realistic months so the readings page looks good in the demo.
  ```
- **Done when:**
  ```
  - [ ] Fresh DB shows a clear trend on the readings page
  ```

## Card 21
- **Title:** `[QA] Test cases: readings`
- **MVP feature:** My readings
- **Area:** Tests
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Test Case issues for: save a month, save the same month twice, future month (rejected), trend up/down/flat.
  ```
- **Done when:**
  ```
  - [ ] 4+ Test Case issues created
  - [ ] Run against the endpoints, results recorded
  ```

## Card 22
- **Title:** `[Frontend] My readings page`
- **MVP feature:** My readings
- **Area:** Frontend
- **Labels:** must, frontend
- **What needs to be done:**
  ```
  Month + kWh form with Save, list of saved months as cards, up/down/flat trend.
  Branch: feature/readings-page
  ```
- **Done when:**
  ```
  - [ ] Saves with POST and lists with GET /api/readings
  - [ ] Loading and error states
  - [ ] Checked on a phone
  ```

---

# 🎤 FINISH + PITCH

## Card 23
- **Title:** `[Integrator] README how to run + phone link (dev tunnel)`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, integrator
- **What needs to be done:**
  ```
  README "How to run" with exact steps. Set up a public Visual Studio dev tunnel for the Web project.
  ```
- **Done when:**
  ```
  - [ ] A teammate follows the README from scratch and it runs
  - [ ] App opens on a phone through the tunnel link
  ```

## Card 24
- **Title:** `[Frontend] Error handling + phone polish`
- **MVP feature:** Setup / other
- **Area:** Frontend
- **Labels:** should, frontend
- **What needs to be done:**
  ```
  Show ProblemDetails errors in plain words on every page.
  Check Section 6 rules: tap targets 44–48 px, text 16 px+, no hover-only controls.
  Branch: feature/frontend-polish
  ```
- **Done when:**
  ```
  - [ ] Every page passes the Section 6 rules
  - [ ] Checked with QA on both phones
  ```

## Card 25
- **Title:** `[QA] Phone test runs: iPhone Safari + Android Chrome`
- **MVP feature:** Setup / other
- **Area:** Tests
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Run the full demo path on both phones at hours 7, 13 and 18. File a Bug issue for anything wrong.
  ```
- **Done when:**
  ```
  - [ ] Hour 7 run done
  - [ ] Hour 13 run done
  - [ ] Hour 18 run done, no open must-fix bugs
  ```

## Card 26
- **Title:** `[Backend] Unit tests for bill + solar`
- **MVP feature:** Setup / other
- **Area:** Tests
- **Labels:** should, backend
- **What needs to be done:**
  ```
  xUnit tests in tests/ for the bill and solar calculations, using QA's test case values.
  ```
- **Done when:**
  ```
  - [ ] dotnet test passes
  ```

## Card 27
- **Title:** `[QA] Pitch slides (max 6)`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Problem, solution, demo, tech diagram, impact. Max 6 slides. QR code to the app if reachable.
  ```
- **Done when:**
  ```
  - [ ] Slides in docs/slides/
  - [ ] Team has reviewed them
  ```

## Card 28
- **Title:** `[QA] Demo script + backup video`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Exact taps and data for a 90–120 s demo on a phone. Record a ~2 min backup video of the core feature.
  ```
- **Done when:**
  ```
  - [ ] Script rehearsed 3 times with a timer
  - [ ] Video in docs/demo/
  ```

## Card 29
- **Title:** `[QA] README: problem, user, sources, AI tools, surprise card`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, qa
- **What needs to be done:**
  ```
  Fill every README section the judges ask for.
  ```
- **Done when:**
  ```
  - [ ] Problem, user, data sources, stack, AI tools, surprise card, how to run all filled
  ```

## Card 30
- **Title:** `[Integrator] Feature freeze: tag v1-demo (hour 18)`
- **MVP feature:** Setup / other
- **Area:** Docs / Pitch
- **Labels:** must, integrator
- **What needs to be done:**
  ```
  At hour 18: git tag v1-demo and push it. Only bug-fix PRs after this.
  ```
- **Done when:**
  ```
  - [ ] Tag pushed
  - [ ] Team told: feature freeze
  ```

---

# 🗂 BACKLOG (create these in the Backlog column; short form is fine)

| Title | MVP feature | Area | Labels |
|---|---|---|---|
| `[Frontend] Readings trend chart` | My readings | Frontend | could, frontend |
| `[Backend] Fuel saved for Mauritius figure on solar result` | Solar check | Backend | could, backend |
| `[Frontend] Compare panel sizes` | Solar check | Frontend | could, frontend |
| `[Backend] Login per household` | My readings | Backend | could, backend |
| `[Frontend] Share result` | Setup / other | Frontend | could, frontend |
