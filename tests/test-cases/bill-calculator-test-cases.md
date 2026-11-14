# QA test cases: Bill calculator

**Feature:** Bill calculator. Enter monthly kWh and see the bill split by CEB tariff band.
**Endpoint:** `POST /api/bill/calculate` (also `GET /api/tariffs`)
**Owner:** Design, QA and Pitch lead
**Status:** Ready to run. The backend uses the real CEB data (merged into `main`, 14 Nov 2026). ⚠️ The website still has `"UseMockApi": true` in `frontend/PowerCheck.Web/appsettings.json`, so **in-app** checks only test the real numbers once the Frontend Lead sets it to `false`. API checks in Swagger work now.

**API test run (14 Nov 2026, `main` at 94c3497, fresh database):** every case checkable through the API **passed** (109 checks across the four files, plus the restart test RD-31). In the Result column, **P (API)** = passed through the API; **API ✓, check on phone** = the numbers passed, and the screen still needs checking. Blank results are phone and screen checks for the phone test run (board #30).

---

## Before you start

- **Tariff used by the app:** domestic tariff **110** by default, rates in force **today** (from 1 May 2026). The API also accepts an optional `tariffCode`: `110`, `110A`, `120` or `140` (see section 2b).
- **Bands start at 0 in the API** (0–25, 25–50…), the same maths as CEB's "first 25 kWh, next 25 kWh". The loader converts CEB's 1–25 / 26–50 labels, so the band-edge cases in section 3 check this.
- **Where the expected numbers come from:** `data/seed/tariff_tiers.csv` and `data/seed/minimum_charges.csv`, sourced from https://ceb.mu/tariffs (see `data/sources/README.md`). The full list of 40 expected bills is in [`bill_test_cases.csv`](bill_test_cases.csv). This checklist uses the tariff 110 / 2026 rows from that file, plus extra boundary values worked out the same way.
- **2026 tariff 110 bands:**

| Band | kWh | Rs per kWh |
|---|---|---|
| 1 | 1–25 | 3.63 |
| 2 | 26–50 | 5.04 |
| 3 | 51–75 | 5.45 |
| 4 | 76–100 | 6.27 |
| 5 | 101–200 | 7.07 |
| 6 | 201–250 | 8.07 |
| 7 | 251–300 | 9.09 |
| 8 | 301–500 | 12.03 |
| 9 | 501–1000 | 12.28 |
| 10 | 1001–1500 | 12.55 |
| 11 | 1501–2000 | 12.80 |
| 12 | 2001 and above | 13.06 |

**Minimum monthly charge:** Rs 50.60. If the band total is lower, the bill is Rs 50.60.

**Decisions:**

- [x] **Rounding:** the app shows money with **2 decimals (F2)** (team agreement, API contract change 10). Expected values below are **to the cent**; the nearest-rupee value is in brackets for reference only. Note for Q&A: real CEB bills round to the rupee (CEB's sample bill, BC-21, shows Rs 769 for a calculated Rs 769.20).
- [ ] **Meter rent:** CEB charges Rs 20.00/month for a single-phase domestic meter (`data/seed/meter_rentals.csv`; CEB's sample bill also shows "Meter Rental 20.00" as a separate line). Is it included in the total? Expected values below **exclude** it.
- [ ] **VAT:** Data Lead to confirm that domestic electricity has no VAT (`data/sources/README.md` cites the VAT Act).

## How to use this file

1. Test in two ways: **in the app** on a phone, and **through the API** (Swagger at `http://localhost:5012/swagger` or `backend/PowerCheck.Api/PowerCheck.Api.http`).
2. Write **P** (pass) or **F** (fail) in the Result column.
3. For every fail, open a GitHub issue with the **Bug** template, then put its ID in Notes. The Backend Lead wants: **endpoint, what you sent, what came back**.
4. Re-test fixed bugs and update the result.

**Test devices / sizes**

| Device / size | Browser | Tested by | Date |
|---|---|---|---|
| iPhone | Safari | Laura Isabelle | 14 Nov 2026 |
| Android phone | Chrome | Laura Isabelle | 14 Nov 2026 |
| Real phone: ________ | | | |

---

## 1. Normal use (happy path)

| ID | Steps | Input | Expected | Result | Notes |
|---|---|---|---|---|---|
| BC-01 | Open the app → Bill tab → enter kWh → Calculate | 200 kWh | Total and a breakdown by band appear | P (phone) | |
| BC-02 | Check the breakdown | 200 kWh | 5 band lines: 25×3.63, 25×5.04, 25×5.45, 25×6.27, 100×7.07 | P (API) | |
| BC-03 | Check the lines add up | 200 kWh | Sum of band amounts = total shown | P (API) | |
| BC-04 | Change the kWh and calculate again | 200 → 300 | Total and breakdown update; no old lines left behind | P (phone) | |
| BC-05 | Tap "See solar savings" | 200 kWh | Solar check opens with **Today = the same total** as this screen | P (phone) | |
| BC-06 | Call `GET /api/tariffs` | — | 12 bands matching the table above, sorted from lowest, last band "and above" (`toKwh: null`) | P (API) | |

## 2. Correct numbers (most important)

Tariff 110, 2026 rates. Meter rent excluded. Nearest-rupee value in brackets.

| ID | Input (kWh) | Why this value | Expected total | Result | Notes |
|---|---|---|---|---|---|
| BC-10 | 200 | Pitch example (was Rs 1,058.25 before 1 May 2026) | Rs 1,216.75 (1,217) | P (API) | |
| BC-11 | 201.5 | Average home, 2025 | Rs 1,228.86 (1,229) | P (API) | |
| BC-12 | 50 | | Rs 216.75 (217) | P (API) | |
| BC-13 | 100 | | Rs 509.75 (510) | P (API) | |
| BC-14 | 150 | | Rs 863.25 (863) | P (API) | |
| BC-15 | 250 | | Rs 1,620.25 (1,620) | P (API) | |
| BC-16 | 300 | | Rs 2,074.75 (2,075) | P (API) | |
| BC-17 | 450 | | Rs 3,879.25 (3,879) | P (API) | |
| BC-18 | 600 | | Rs 5,708.75 (5,709) | P (API) | |
| BC-19 | 1200 | | Rs 13,130.75 (13,131) | P (API) | |
| BC-20 | 2500 | Reaches the top band | Rs 29,825.75 (29,826) | P (API) | |
| BC-21 | 153, using the **2023** rates (tariff 110, from 1 Feb 2023) | Matches a real CEB-issued bill: CEB's sample bill dated 3 Feb 2024 shows 153 kWh → Consumption Charges **Rs 769** (`docs/screenshots/ceb-ebill-sample-2024.jpg`, from CEB's official Facebook e-billing post) | Rs 769.20 (769) | P (API + phone) | Only testable if the app or API can use the 2023 rates; otherwise check by hand |
| BC-22 | 153 (2026 rates) | Same household after 1 May 2026 | Rs 884.46 (884) | P (API) | |

## 2b. Other domestic tariff codes (API only)

From [`bill_test_cases.csv`](bill_test_cases.csv). Send `{ "monthlyKwh": …, "tariffCode": "…" }`.

| ID | Tariff | kWh | Expected total | Why | Result | Notes |
|---|---|---|---|---|---|---|
| BC-25 | 110A | 200 | **Rs 963.75** | Small-consumer tariff, lower first bands | P (API) | |
| BC-26 | 110A | 5 | **Rs 31.00** | 110A minimum charge | P (API) | |
| BC-27 | 120 | 5 | **Rs 211.60** | Same rates as 110, higher minimum | P (API) | |
| BC-28 | 140 | 5 | **Rs 424.35** | Same rates as 110, highest minimum | P (API) | |
| BC-29 | 120 or 140 | 200 | **Rs 1,216.75** | Same bands as 110 above the minimum | P (API) | |
| BC-29b | `"999"` | 200 | **400**, "Unknown tariffCode '999'. Known codes: …" | | P (API) | `GET /api/tariffs?tariffCode=999` returns **404** instead |

## 3. Band edges

These catch "off by one kWh" mistakes. Each pair should differ by exactly the next band's price.

| ID | Input (kWh) | Expected total | Check | Result | Notes |
|---|---|---|---|---|---|
| BC-30 | 25 | Rs 90.75 | All in band 1 | P (API) | |
| BC-31 | 26 | Rs 95.79 | +5.04, 1 kWh in band 2 | P (API) | |
| BC-32 | 75 | Rs 353.00 | | P (API) | |
| BC-33 | 76 | Rs 359.27 | +6.27, 1 kWh in band 4 | P (API) | |
| BC-34 | 100 | Rs 509.75 | | P (API) | |
| BC-35 | 101 | Rs 516.82 | +7.07, 1 kWh in band 5 | P (API) | |
| BC-36 | 300 | Rs 2,074.75 | | P (API) | |
| BC-37 | 301 | Rs 2,086.78 | +12.03, 1 kWh in band 8 | P (API) | |
| BC-38 | 1000 | Rs 10,620.75 | | P (API) | |
| BC-39 | 1001 | Rs 10,633.30 | +12.55, 1 kWh in band 10 | P (API) | |

## 4. Minimum charge

| ID | Input (kWh) | Expected | Result | Notes |
|---|---|---|---|---|
| BC-40 | 0 | Rs 50.60 (minimum charge), with a note explaining it | P (API) | |
| BC-41 | 5 | Rs 50.60 (band total would be Rs 18.15) | P (API) | |
| BC-42 | 13 | Rs 50.60 (band total would be Rs 47.19) | P (API) | |
| BC-43 | 14 | Rs 50.82 (band total is now above the minimum) | P (API) | |

## 5. Bad input

The API accepts 0 to 5000 kWh and returns **400** otherwise.

| ID | Input | Expected | Result | Notes |
|---|---|---|---|---|
| BC-50 | Empty field, tap Calculate | Inline message next to the field, e.g. "Enter how many kWh you used"; nothing is calculated | P (phone) | |
| BC-51 | −50 | Inline message; API returns 400 | P (API + phone) | |
| BC-52 | 5000 | Accepted: Rs 62,475.75 (62,476) | P (API) | |
| BC-53 | 5001 | Inline message; API returns 400 "monthlyKwh must be between 0 and 5000." | P (API + phone) | |
| BC-54 | Letters ("abc") | Rejected, or the phone shows a number keypad so letters can't be typed | P (phone) | |
| BC-55 | 150.5 | Either calculated correctly, or a clear message saying whole numbers only (agree which) | P (phone) | |
| BC-56 | Fix the bad input after an error | The error message disappears | P (phone) | |
| BC-57 | Backend stopped, tap Calculate | Friendly "Something went wrong, try again"; no blank screen or crash | P (phone) | |

## 6. Display and wording

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| BC-60 | Source line | "Source: CEB tariffs 2026" visible; link opens https://ceb.mu/tariffs | P (API) | |
| BC-61 | No test text | The word "FAKE" or "mock" appears nowhere on screen or in the API response (check after `UseMockApi` is set to `false`) | P (API + phone) | |
| BC-62 | Money format | "Rs" with thousands separators: Rs 1,216.75, not Rs 1216.75 or Rs 1216.7500 | P (phone) | |
| BC-63 | Band labels | Match CEB wording: 1–25, 26–50, …, "2001 and above" | P (phone) | |
| BC-64 | Band bars | Higher bands look darker; label and amount readable on each | P (phone) | |
| BC-65 | Wording | Says "calculated" (not "estimate"); if meter rent is excluded, the screen says so | P (phone) | |
| BC-66 | Fits the phone | No sideways scrolling at 360 px wide, even with 12 band lines (2500 kWh) | P (phone) | |
| BC-67 | Number keypad | Tapping the kWh field opens a number keypad on the phone | P (phone) | |
| BC-68 | Tap targets | Calculate button about 44 px tall, easy to tap | P (phone) | |
| BC-69 | Bottom nav | Bill tab highlighted on this screen | P (phone) | |

## 7. Demo path (run before every rehearsal and on stage day)

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| BC-70 | Enter 200 kWh → Calculate | Rs 1,216.75 (or Rs 1,217 if rounding to the rupee), matching the slides exactly | P (phone) | |
| BC-71 | Same on the demo device and network | Works; no pop-ups or login in the way | P (phone) | |
| BC-72 | Run 3 times in a row | Same result every time | To do at rehearsal | |

---

## Status of the issues found earlier

Checked against the code on `main` on 14 Nov 2026.

| # | Issue (13 Nov) | Status now |
|---|---|---|
| 1 | Backend used fake prices | ✅ Fixed: prices come from the real CEB data in the database (`TariffData`) |
| 2 | Minimum charge not applied | ✅ Fixed: the bill is never below the tariff's minimum charge (`minimumChargeRs`) |
| 3 | Risk of being 1 kWh short per band | ✅ Fixed: the loader subtracts 1 from CEB's band starts (`Seeder.cs`). BC-30 to BC-39 still check it |
| 4 | Rounding decision | ✅ Decided: the app shows 2 decimals (F2) |
| 5 | Source link | ✅ Fixed: tariffs from 2026 link to https://ceb.mu/tariffs; 2023 rates link to the Feb 2023 tariff schedule |
| 6 | **New:** the website still has `"UseMockApi": true` | ⏳ Frontend Lead to set it to `false`. Until then, in-app results are mock data; test numbers through Swagger |

---

## Bug template

Prefer a GitHub issue with the **Bug** template. If you can't, copy this:

```
Bug ID:      BUG-___
Test case:   BC-__
Device/size:
Steps:       1.  2.  3.
Input:
Expected:
Actual:
Screenshot:
Severity:    Blocker (breaks the demo) / Major / Minor
Assigned to:
Status:      Open / Fixed / Re-tested OK
```

## Sign-off

| | Passed | Failed | Not run |
|---|---|---|---|
| Totals | 59 | 0 | 1 |

Ready for demo? ☒ Yes ☐ No. Signed: __________ Date: __________
