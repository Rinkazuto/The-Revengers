# QA test cases: Solar check

**Feature:** Solar check (main feature), comparing the bill without and with solar, monthly savings and payback years
**Endpoint:** `POST /api/solar/estimate`
**Owner:** Design, QA and Pitch lead
**Status:** Ready to run. The Solar check uses real data and tariff 150A (merged into `main`, 14 Nov 2026; checked against `SolarEndpoints.cs`). The Data Lead confirmed SC-10 to SC-16 match the live API. ⚠️ The website still has `"UseMockApi": true`, so test through Swagger until the Frontend Lead sets it to `false`.

**Expected numbers for SC-10 to SC-18** come from the Data Lead's file [`solar_expected_results.csv`](solar_expected_results.csv), which shows the working for each case. The IDs in this file match it.

---

## Before you start

**What the backend does** (Backend Lead, 14 Nov 2026):
- Uses the real **CEB 2026 solar scheme**: CEB system prices (midpoint of each range), **25% grant (max Rs 75,000)**, **Rs 2,000 fee**, monthly PVGIS sun data, and **0.5% panel ageing per year**.
- **Inputs:** `monthlyKwh` (required), `systemSizeKwp` (optional) and `phase` (`"single"` or `"three"`, optional). The size must be a CEB size: **1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7.5, 10 or 15**. If it's left out, the backend picks the smallest size that covers the usage.
- **`installCostRs` is the cost after the grant and the fee.**
- **`withSolar.exportCreditRs`** is shown **separately** and is **not** counted in the savings.
- The response has an **`assumptions`** list that explains every number (shown under the result) and a **20-year `projection`** for the graph.

**The "with solar" bill uses prosumer tariff 150A** (single-phase) or **150B** (three-phase). Source: GN 473/2026, Appendix VIII, p. 2230.
- Net energy is charged at a **flat Rs 8.58/kWh if total use is up to 300 kWh**, and by the 150A bands above that.
- **Minimum charge by TOTAL use:** Rs 50.60 (0–75 kWh), **Rs 211.60 (76–300 kWh)**, Rs 424.35 (over 300 kWh). The "with solar" bill can never go below it.
- Export credit is not used to pay the minimum.

**Still to confirm with the developers:**
- [ ] Is meter rent (Rs 20) included in "Today"? The Bill calculator test cases exclude it.

## How to use this file

1. Run each case on a real phone (or browser dev tools in phone size). For number checks, use Swagger at `http://localhost:5012/swagger`.
2. Write **P** (pass) or **F** (fail) in the Result column.
3. For every fail, log a bug (template at the bottom). The Backend Lead wants: **endpoint, what you sent, what came back**.
4. Re-test fixed bugs and update the result.

**Test devices / sizes**

| Device / size | Browser | Tested by | Date |
|---|---|---|---|
| 390×844 (iPhone 13/14) | | | |
| 360×800 (Android) | | | |
| Real phone: ________ | | | |

---

## 1. Normal use (happy path)

| ID | Steps | Input | Expected | Result | Notes |
|---|---|---|---|---|---|
| SC-01 | Bill tab → enter kWh → Calculate → See solar savings | 200 kWh | Solar check screen opens and shows Today, With solar, savings per month and payback years | | |
| SC-02 | Open the Solar tab directly from the bottom nav | — | Screen asks for kWh, or uses the last kWh entered (agree which with the developers) | | |
| SC-03 | Compare "Today" with the Bill calculator result | 200 kWh | "Today" equals the Bill calculator total for the same kWh | | |
| SC-04 | Check the savings arithmetic | 200 kWh | Savings per month = Today − With solar (export credit **not** included) | | |
| SC-05 | Open and use the size dropdown | 200 kWh, try 3 then 5 kWp | Shows exactly the 12 CEB sizes, no free typing; all figures update and the chosen size is shown | | |
| SC-06 | Don't choose a size (dropdown left on "Recommended for my usage") | 200 kWh | The app picks **2 kWp** and says "2 kWp makes about 227 kWh a month on average" | | Checked in the running app on 14 Nov 2026 |
| SC-07 | Export credit | 200 kWh, 3 kWp | Shown on its own line, e.g. "+ Rs 423.37 credit from CEB", and not added to the savings | | |
| SC-08 | Assumptions list and graph | 200 kWh, 3 kWp | The **assumptions** list is visible under the result; the 20-year projection graph shows | | |
| SC-09 | Go back to the Bill tab, change the kWh, return to Solar | 200 → 300 kWh | Solar check uses the new kWh | | |

## 2. Correct numbers (most important)

Values are an average month (12 PVGIS months, panels −0.5% a year), single phase. Full working: [`solar_expected_results.csv`](solar_expected_results.csv).

| ID | kWh | Size | Today | With solar | Export credit (separate) | Savings / month | Install cost | Payback | Result | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| SC-10 | **200** (pitch example) | 3 kWp | **Rs 1,216.75** | **Rs 211.60** | Rs 423.37 | **Rs 1,005.15** | Rs 193,250 | **16.0 years** | | Flat Rs 8.58 rule; bill sits at the 150A minimum. Only when 3 kWp is chosen; the demo uses the default 2 kWp (SC-18) |
| SC-11 | 201.5 (average home) | 3 kWp | Rs 1,228.86 | Rs 211.60 | Rs 418.87 | Rs 1,017.26 | Rs 193,250 | 15.8 years | | |
| SC-12 | 50 (low use) | 3 kWp | Rs 216.75 | Rs 50.60 | Rs 873.37 | Rs 166.15 | Rs 193,250 | **none within 30 years** | | The screen must say solar doesn't pay back at this usage (see SC-31) |
| SC-13 | 350 | 3 kWp | Rs 2,676.25 | Rs 553.51 | Rs 48.38 | Rs 2,122.74 | Rs 193,250 | 7.7 years | | Over 300 kWh: 150A bands, minimum Rs 424.35 |
| SC-14 | 300 (band edge) | 1.5 kWp | Rs 2,074.75 | Rs 1,110.58 | Rs 0.00 | Rs 964.17 | Rs 137,000 | 12.4 years | | Last kWh on the flat Rs 8.58 rule |
| SC-15 | 301 (one above) | 1.5 kWp | Rs 2,086.78 | Rs 1,569.18 | Rs 0.00 | Rs 517.60 | Rs 137,000 | **none within 30 years** | | Crossing 300 kWh total moves the bill to the 150A bands; savings drop sharply |
| SC-16 | 500 | 3 kWp | Rs 4,480.75 | Rs 1,911.30 | Rs 0.00 | Rs 2,569.45 | Rs 193,250 | 6.4 years | | |
| SC-17 | 650, no size chosen | **6 kWp** (picked by the app) | Rs 6,322.75 | **Rs 850.49** | Rs 181.70 | **Rs 5,472.26** | Rs 377,000 | **5.8 years** | | Checked in the live API and by hand by the Data Lead (replaces the pre-150A values Rs 505.94 / 5.4 years). Over 500 kWh, so the CST of Rs 0.82 per kWh covered by solar applies (leaflet, pending URA approval) |
| SC-18 | **200, no size chosen** | **2 kWp** (picked by the app) | Rs 1,216.75 | **Rs 211.60** | Rs 93.92 | **Rs 1,005.15** | Rs 155,750 | **13.0 years** | | Same bill and saving as SC-10 (3 kWp) because the 150A minimum sets the bill; only install, payback and credit differ |

**Note for the pitch:** SC-14 and SC-15 show a real cliff. At 300 kWh solar pays back in 12.4 years; at 301 kWh it doesn't pay back within 30 years on 1.5 kWp. Worth knowing if a judge asks about a home around 300 kWh.

## 3. Install cost (after grant and fee)

Formula: midpoint of CEB's price range − grant (25%, max Rs 75,000) + Rs 2,000 fee. Checked against `data/seed/solar_system_prices_2026.csv`.

| ID | Size | CEB price range | Midpoint | Grant | Expected `installCostRs` | Checks | Result |
|---|---|---|---|---|---|---|---|
| SC-20 | 6 kWp | Rs 420,000–480,000 | Rs 450,000 | Rs 75,000 (capped) | **Rs 377,000** | Grant cap applied | |
| SC-21 | 1.5 kWp | Rs 165,000–195,000 | Rs 180,000 | Rs 45,000 (25%) | **Rs 137,000** | 25% applied, not the cap | |
| SC-22 | 3 kWp | Rs 240,000–270,000 | Rs 255,000 | Rs 63,750 (25%) | **Rs 193,250** | 25% applied | |
| SC-23 | Any size | — | — | — | Payback uses `installCostRs` (after grant and fee), not the full price | | |

## 4. Edge cases

| ID | Input | Expected | Result | Notes |
|---|---|---|---|---|
| SC-30 | 0 kWh | No crash; a clear message such as "Enter how many kWh you used" | | |
| SC-31 | Usage where solar never pays back (SC-12, SC-15) | No negative, "infinite" or nonsense payback; a clear message such as "Solar doesn't pay back at this usage" (API returns `paybackYears: null`) | | |
| SC-32 | Solar produces more than the household uses | With solar never shows a negative bill: it shows the 150A minimum, with any export credit on a separate line. (CEB's own calculator showed Rs −388.40 for 200 kWh on 3 kWp because it ignores the minimum) | | |
| SC-33 | Very high usage (2000 kWh) | Picks a sensible size (up to 15 kWp); numbers correct; layout doesn't break | | |
| SC-34 | Decimal input (150.5) | Either accepted and calculated correctly, or a clear message. No crash | | |
| SC-35 | API only: `systemSizeKwp` not a CEB size (e.g. 2.2) | 400 "systemSizeKwp must be one of the CEB sizes: …" listing the 12 sizes. The app's dropdown should make this impossible | | |
| SC-35b | API only: `phase: "both"` | 400 'phase must be "single" (tariff 150A) or "three" (tariff 150B).' | | |
| SC-35c | API only: `tariffCode: "999"` | 400 "Unknown tariffCode '999'. Known codes: …" | | |
| SC-36 | 60 kWh, any size | With solar = **Rs 50.60** (minimum for 0–75 kWh) | | |
| SC-37 | 200 kWh, 3 kWp | With solar = **Rs 211.60** (minimum for 76–300 kWh) | | Same as SC-10 |
| SC-38 | 400 kWh, a size that covers all usage (e.g. 5 kWp or more) | With solar not below **Rs 424.35** (minimum for over 300 kWh) | | |
| SC-39 | 75 kWh then 76 kWh, large system | Minimum jumps from Rs 50.60 to Rs 211.60 (band edge on total use) | | |
| SC-39b | `phase: "three"` | Uses tariff **150B**; the screen or assumptions say so | | |

## 5. Bad input

| ID | Input | Expected | Result | Notes |
|---|---|---|---|---|
| SC-40 | Empty field, tap Calculate | Inline message next to the field; nothing is calculated | | |
| SC-41 | Negative number (−50) | Inline message; nothing is calculated | | |
| SC-42 | Letters ("abc") | Rejected, or the phone shows a number keypad so letters can't be typed | | |
| SC-43 | Absurd value (999999999) | 400 "monthlyKwh must be between 0 and 5000."; the app shows it inline, no crash | | |
| SC-44 | Fix the bad input after an error | The error message disappears | | |

## 6. Display and wording

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| SC-50 | Source line | "Source: CEB 2026 solar scheme" is visible and links to the official page | | |
| SC-51 | Estimate wording | The screen says the solar figures are an **estimate** | | |
| SC-52 | Assumptions list | Readable on a phone; plain language; mentions the grant, the Rs 2,000 fee, panel ageing, tariff 150A with its minimum charge, and "export credit shown separately, not used to pay the minimum" | | |
| SC-52b | "With solar" label | Says it includes CEB's minimum charge (tariff 150A), so a family isn't surprised it's not Rs 0 | | |
| SC-53 | Install cost label | Makes clear the cost is **after** the 25% grant and the Rs 2,000 fee | | |
| SC-54 | Money format | "Rs" with thousands separators and 2 decimals (F2): Rs 1,216.75 | | |
| SC-55 | Payback format | Readable, e.g. "16.0 years" or "about 16 years". Not "15.9999 years" | | |
| SC-56 | Fits the phone | No sideways scrolling at 360 px wide; key result visible without scrolling | | |
| SC-57 | Readability | Text at least about 14 px; savings card colours readable (contrast) | | |
| SC-58 | Tap targets | Buttons and the size dropdown are easy to tap with a thumb (about 44 px tall) | | |
| SC-59 | Bottom nav | The Solar tab is highlighted while on this screen | | |
| SC-60 | Loading / no connection | If the API is slow or down: a loading state, then a friendly error. No blank screen or crash | | |

## 7. Demo path (run before every rehearsal and on stage day)

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| SC-70 | Full flow: kWh → bill → See solar savings → result | Done in under 1 minute | | |
| SC-71 | Use the exact demo numbers: 200 kWh, size left on "Recommended" (**2 kWp**, team decision) | Rs 1,216.75 → Rs 211.60, save Rs 1,005.15 a month, **money back in 13 years**, export credit Rs 93.92, cost Rs 155,750; matches the slides and the slide 5 notes | | Same as SC-18 |
| SC-72 | Run on the demo device and network | Works; no login or other pop-ups get in the way | | |
| SC-73 | Run 3 times in a row | Same result every time | | |
| SC-74 | Backup ready | Screenshots or a screen recording of the flow saved in `docs/demo/`, and one screenshot on slide 5 of the pitch deck | | |

---

## Bug template

Prefer a GitHub issue with the **Bug** template. If you can't, copy this:

```
Bug ID:      BUG-___
Test case:   SC-__
Device/size:
Endpoint:    (e.g. POST /api/solar/estimate)
What I sent: (e.g. { "monthlyKwh": 200, "systemSizeKwp": 3 })
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
