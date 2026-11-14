# QA test cases: My readings

**Feature:** My readings. Save each month's usage, see the bill for that month and whether usage is going up or down.
**Endpoints:** `POST /api/readings`, `GET /api/readings`
**Owner:** Design, QA and Pitch lead
**Status:** Ready to run (merged into `main`, 14 Nov 2026; checked against `ReadingEndpoints.cs`). ⚠️ The website still has `"UseMockApi": true`, so test through Swagger until the Frontend Lead sets it to `false`.

---

## Before you start

**What the backend does** (API contract + Backend Lead, 14 Nov 2026):
- `POST /api/readings` with `{ "month": "YYYY-MM", "kwh": 0–5000 }`. The month can't be in the future.
- **One reading per month:** saving the same month again **overwrites** it (201 = new month, 200 = overwritten).
- `GET /api/readings` lists months **oldest → newest**, with `trend`: `"up"`, `"down"` or `"flat"`, comparing the **last two** months; `null` if there are fewer than 2.
- Readings are **saved in the database** and survive a restart.
- **The bill uses the tariff in force that month** (domestic tariff 110): months before **1 May 2026** use the 2023 rates; May 2026 onwards uses the 2026 rates.
- **The trend compares kWh**, not rupees.

**Rules for test and demo data:** readings are **example inputs** typed into the app. Never save made-up readings into `data/seed` as a real home's history.

**Expected bills** come from `data/seed/tariff_tiers.csv`, `minimum_charges.csv` and `chart_bill_vs_kwh.csv` (tariff 110).

## How to use this file

1. Run each case on a real phone (or browser dev tools in phone size). For API checks, use Swagger at `http://localhost:5012/swagger`.
2. **Reset the database before you start** (delete the local `app.db` or use the team's reset step), so old readings don't affect results.
3. Write **P** (pass) or **F** (fail) in the Result column.
4. For every fail, log a bug (template at the bottom). The Backend Lead wants: **endpoint, what you sent, what came back**.

**Test devices / sizes**

| Device / size | Browser | Tested by | Date |
|---|---|---|---|
| 390×844 (iPhone 13/14) | | | |
| 360×800 (Android) | | | |
| Real phone: ________ | | | |

---

## 1. Empty state

| ID | Steps | Expected | Result | Notes |
|---|---|---|---|---|
| RD-01 | Fresh database, open the Readings tab | A friendly empty state such as "Add your first reading", plus the **Add reading** button. No chart, no error | | |
| RD-02 | `GET /api/readings` on an empty database | `readings: []`, `trend: null` | | |
| RD-03 | Add only one reading | The reading shows; no trend line or "Up/Down %" yet (`trend: null`) | | |

## 2. Demo readings (must match the stage demo)

Enter them in this order. Expected bills use the **2026 rates**.

| ID | Month | kWh | Expected bill | Expected trend after saving | Result | Notes |
|---|---|---|---|---|---|---|
| RD-10 | Aug 2026 | 190 | **Rs 1,146.05** | `null` (only one month) | | Entered before going on stage |
| RD-11 | Sep 2026 | 201 | **Rs 1,224.82** | **up** | | Entered before going on stage |
| RD-12 | Oct 2026 | 230 | **Rs 1,458.85** | **up** | | Typed **live** on stage |
| RD-13 | Check the screen after RD-12 | — | Three months listed Aug → Oct, chart has 3 bars, the newest one highlighted, "Up 14.4% vs last month" if the screen shows a % (230 vs 201 kWh) | | The API only returns "up"; any % is worked out by the frontend from kWh |

## 3. Tariff in force that month (most important)

| ID | Month | kWh | Expected bill | Why | Result | Notes |
|---|---|---|---|---|---|---|
| RD-20 | **Apr 2026** | 200 | **Rs 1,058.25** | Before 1 May 2026: **2023 rates** | | Deliberate check that the old rates are used |
| RD-21 | May 2026 | 200 | **Rs 1,216.75** | From 1 May 2026: **2026 rates** | | Same kWh as RD-20, so the bill should be Rs 158.50 higher |
| RD-22 | Apr 2026 | 10 | **Rs 44.00** | 2023 minimum charge | | |
| RD-23 | May 2026 | 10 | **Rs 50.60** | 2026 minimum charge | | |

## 4. Saving, overwriting and restarts

| ID | Steps | Expected | Result | Notes |
|---|---|---|---|---|
| RD-30 | Save Sep 2026 = 201, then save Sep 2026 = 250 | Still **one** Sep 2026 entry, now 250 kWh with the new bill. API returns 201 the first time, 200 the second time | | "Same month twice" (board task Q3) |
| RD-31 | After RD-12, **restart the backend**, then reload the Readings tab | The same three readings are still there | | Key check from the Backend Lead |
| RD-32 | Save months out of order (Oct, then Aug, then Sep) | The list is still shown oldest → newest | | |
| RD-33 | Trend down: Sep = 201, then Oct = 190 | `trend: "down"` | | |
| RD-34 | Trend flat: Sep = 201, then Oct = 201 | `trend: "flat"` | | |

## 5. Bad input

The API returns **400** for these. The app should show a clear inline message and not save anything.

| ID | Input | Expected | Result | Notes |
|---|---|---|---|---|
| RD-40 | Month in the future (e.g. Dec 2026, as today is Nov 2026) | 400 "month cannot be in the future." The current month (Nov 2026) is allowed | | |
| RD-41 | Month missing or in the wrong format (`2026-13`, `Oct 2026`) | 400 "month must look like YYYY-MM, e.g. 2026-10." | | The app's month picker should prevent this |
| RD-42 | kWh empty | Inline message; nothing saved | | |
| RD-43 | kWh negative (−5) | 400 "kwh must be between 0 and 5000." | | |
| RD-44 | kWh 5001 | 400 "kwh must be between 0 and 5000." | | |
| RD-45 | kWh 0 | Accepted; bill = minimum charge (Rs 50.60 for May 2026 onwards) | | |

## 6. Display and wording

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| RD-50 | Money format | "Rs" with thousands separators and 2 decimals (F2): Rs 1,458.85 | | |
| RD-51 | Month labels | Readable, e.g. "Oct 2026", not "2026-10" | | |
| RD-52 | Chart | Bars in month order; the newest bar stands out; labels readable at 360 px | | |
| RD-53 | Fits the phone | No sideways scrolling; the Add reading button is easy to tap (about 44 px) | | |
| RD-54 | Bottom nav | The Readings tab is highlighted on this screen | | |
| RD-55 | Loading / backend down | Loading state, then a friendly error. No blank screen or crash | | |

## 7. Demo path (run before every rehearsal and on stage day)

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| RD-60 | Reset the database, enter Aug 2026 = 190 and Sep 2026 = 201 | Two months shown, trend up | | Do this just before going on stage |
| RD-61 | Type Oct 2026 = 230 live | Saves in a few seconds, bill Rs 1,458.85, trend still up | | |
| RD-62 | Run the full demo 3 times in a row | Same result every time | | Reset the database between runs |

---

## Bug template

Prefer a GitHub issue with the **Bug** template. If you can't, copy this:

```
Bug ID:      BUG-___
Test case:   RD-__
Device/size:
Endpoint:    (e.g. POST /api/readings)
What I sent: (e.g. { "month": "2026-04", "kwh": 200 })
What came back:
Expected:
Screenshot:
Severity:    Blocker (breaks the demo) / Major / Minor
Assigned to:
Status:      Open / Fixed / Re-tested OK
```

## Sign-off

| | Passed | Failed | Not run |
|---|---|---|---|
| Totals | | | |

Ready for demo? ☐ Yes ☐ No. Signed: __________ Date: __________
