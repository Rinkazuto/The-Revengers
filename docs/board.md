# Power Check: Task Board

The live board is the **GitHub Project** in the repo's **Projects** tab. This file is the plan used to create it.
**Owner:** Integrator. Each card below becomes one GitHub issue (New issue → Feature / Task) added to the project.

## Columns

| Column | Meaning |
|---|---|
| **Backlog** | Ideas for later. Nobody works on these unless all Must cards are Done. |
| **To Do** | Ready to start. Most important at the top. |
| **In Progress** | Someone is working on it, assigned to them. One card per person. |
| **In Review** | PR open, waiting for the Integrator to check and merge. |
| **Done** | Merged into `main` and working. |

## Labels

| Label | Colour | Use |
|---|---|---|
| `must` | red | MVP: the demo fails without it |
| `should` | orange | Makes the demo better, do after the Musts |
| `could` | grey | Only if there's time (usually Backlog) |
| `blocked` | black | Waiting on someone. Comment why on the card. |
| `frontend` / `backend` / `data` / `qa` / `integrator` | any | Which role owns it |
| `feature` / `bug` / `testing` | (from templates) | Card type |

## Branches

| Branch | Use |
|---|---|
| `main` | Working demo only. Gets PRs from `submain` after they are tested. Never push or PR straight into it. |
| `submain` | Integration branch. Every feature branch is created from it and PRs back into it. |
| `feature/...`, `docs/...` | One per card. Created from `submain`. |

Why: a faulty push or a new change that breaks the build lands on `submain` first and gets fixed there, so the demo on `main` keeps working. Both branches are protected (PR required, no force push, no deletion).

## Card rules

- One card = one branch = one PR = one person. 30 min to 2 hours each.
- Assign yourself only when you **start** the card.
- PR description says `Closes #<card number>`, so the card moves to Done on merge.
- Blocked: comment on the card, add `blocked`, tell the chat, take another card.

---

## To Do (in this order, top first)

### Integrator
| # | Title | Labels | Done when |
|---|---|---|---|
| I1 | Board, labels and branch protection on `main` | must, integrator | Board has 5 columns, labels exist, `main` requires a PR |
| I2 | Solution file with Api + Web projects | must, integrator | Opening the `.slnx` loads both; both run together |
| I3 | Program.cs: controllers + DbContext registration + connection string | must, integrator | Builds; Swagger shows controllers; DB line ready for Data Lead |
| I4 | README "How to run" + phone link (dev tunnel) | must, integrator | A teammate follows the README and opens the app on a phone |
| I5 | Feature freeze at hour 18: `git tag v1-demo` | must, integrator | Tag pushed; only bug-fix PRs after this |

### Data Lead
| # | Title | Labels | Done when |
|---|---|---|---|
| D1 | Collect CEB tariff + 2026 solar scheme sources; answer API contract questions 1–4 | must, data | PDFs/links in `data/sources/`; answers written in `docs/api-contract.md` |
| D2 | Models (TariffBand, Reading) + AppDbContext + first migration | must, data | `dotnet ef database update` creates the tables |
| D3 | Seed tariff bands and solar scheme values from CSV | must, data | Fresh DB has real CEB values; CSVs in `data/seed/` |
| D4 | Clean demo readings (3–6 months) | should, data | Readings page shows a realistic trend on a fresh DB |

### Backend Lead
| # | Title | Labels | Done when |
|---|---|---|---|
| B1 | Agree the API contract with Frontend + Data | must, backend | `docs/api-contract.md` marked AGREED; decision log row |
| B2 | GET /api/tariffs | must, backend | Returns seeded bands in contract shape |
| B3 | Bill calculation + POST /api/bill/calculate | must, backend | Matches QA's hand-calculated test cases; 400 on bad input |
| B4 | Solar estimate + POST /api/solar/estimate | must, backend | Without-solar bill = bill endpoint; payback null when no savings |
| B5 | POST + GET /api/readings | must, backend | Same month twice replaces it; trend up/down/flat |
| B6 | Unit tests for bill and solar calculations | should, backend | Tests pass in `tests/` |

### Frontend Lead
| # | Title | Labels | Branch |
|---|---|---|---|
| F1 | Blazor project + MudBlazor + bottom bar (Bill / Solar / Readings) | must, frontend | `feature/frontend-skeleton` |
| F2 | API service with fake data (`UseMockApi` setting) | must, frontend | `feature/api-service` |
| F3 | Bill calculator page | must, frontend | `feature/bill-page` |
| F4 | Solar check page (depends on D1 question 3) | must, frontend | `feature/solar-page` |
| F5 | My readings page | must, frontend | `feature/readings-page` |
| F6 | Error handling + phone polish (Section 6 rules) | should, frontend | `feature/frontend-polish` |

### Design / QA / Pitch
| # | Title | Labels | Done when |
|---|---|---|---|
| Q1 | Phone wireframes for the 3 screens (390 px) | must, qa | In `docs/wireframes/`; Frontend Lead has seen them |
| Q2 | Test cases: bill calculator (expected bills worked out by hand) | must, qa, testing | 5+ Test Case issues: 0 kWh, each band edge, high usage, invalid input |
| Q3 | Test cases: solar check + readings | must, qa, testing | Test Case issues incl. "no savings" and "same month twice" |
| Q4 | Phone test runs at hours 7, 13, 18 (iPhone Safari + Android Chrome) | must, qa | Results on the test cards; bugs filed |
| Q5 | Pitch slides (max 6) | must, qa | In `docs/slides/` |
| Q6 | Demo script + backup video (~2 min) | must, qa | Script rehearsed 3×; video in `docs/demo/` |
| Q7 | README: problem, user, data sources, AI tools, surprise card | must, qa | All README sections filled |

---

## Backlog (only if all Musts are Done)

| # | Title | Labels |
|---|---|---|
| X1 | Readings trend chart (MudChart) | could, frontend |
| X2 | "Fuel saved for Mauritius" figure on the solar result (kWh from solar → fuel not imported) | could, backend, frontend |
| X3 | Compare panel sizes side by side | could, frontend |
| X4 | Login so each household has its own readings | could, backend |
| X5 | Share result as an image / link | could, frontend |
