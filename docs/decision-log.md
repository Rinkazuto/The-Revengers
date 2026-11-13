# Decision Log

5–10 key decisions, with times. Add a row whenever the team makes a choice that shapes the app.

| # | Date & time | Decision | Why | Who |
|---|---|---|---|---|
| 1 | 13 Nov 2026, 12:43 | Chose **Power Check** (Theme 3: Powered to Live Better) | Clear problem every Mauritian family has (rising CEB bills), small official dataset, short demo path | Team |
| 2 | 13 Nov 2026, 12:46 | Main feature: compare monthly cost **without solar vs with solar** | Pushes households to switch to solar, saving money and imported fuel for Mauritius | Team |
| 3 | 13 Nov 2026, 12:46 | MVP limited to 3 features: bill calculator, solar check, my readings | Avoid "too many features, none finished" | Team |
| 4 | 13 Nov 2026, HH:MM | Two-branch flow: feature branches → **submain** → **main**. Both protected (PR required, no force push, no deletion) | A faulty push or a breaking change lands on submain first and gets fixed there, so the working demo on main is never broken | Integrator |
| 5 | 13 Nov 2026, HH:MM | API endpoints and contract **agreed** (`docs/api-contract.md`): 5 endpoints under `/api`, optional `tariffCode` (default 110), `minimumChargeRs`, solar output from PVGIS (113.71 kWh/kWp/month), year-by-year `projection` for the graph | Frontend and backend build against the same shapes; every number traces back to an official source | Backend, Data, Frontend, Integrator |
| 6 | 13 Nov 2026, HH:MM | Minimal API endpoint files (`Endpoints/`) instead of controllers | Less boilerplate, faster to build in 24 hours, easy to read | Backend Lead |
| 7 | 13 Nov 2026, HH:MM | Logo chosen: DESCRIBE IT HERE | WHY THIS ONE | Team |
