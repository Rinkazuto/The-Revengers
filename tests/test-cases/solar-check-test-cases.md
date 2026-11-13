# QA test cases: Solar check

**Feature:** Solar check (main feature), comparing the bill without and with solar, monthly savings and payback years
**Owner:** Design, QA and Pitch lead
**Status:** Draft. Fill in the expected values once the official data below is available.

---

## Before you start

You need these before you can fill in the **Expected** column:

- [ ] Official **CEB tariffs 2026** table (band limits and price per kWh). Source link: ____________
- [ ] Official **2026 solar scheme** rules (panel cost, subsidy, kWh produced per kWp per month). Source link: ____________
- [ ] From the developers: what the screen takes as input (kWh only? kWh plus panel size?) and the default panel size: ____________
- [ ] From the developers: whether the bill includes fixed charges or taxes, or only the energy charge: ____________
- [ ] A spreadsheet that works out the expected results by hand, kept next to this file in `tests/test-cases/` so anyone can check it

## How to use this file

1. Run each case on a real phone (or browser dev tools in phone size).
2. Write **P** (pass) or **F** (fail) in the Result column.
3. For every fail, log a bug: open a GitHub issue with the **Bug** template (or copy the template at the bottom), then put its ID in Notes.
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
| SC-04 | Check the solar figure | 200 kWh | "With solar" is lower than "Today" | | |
| SC-05 | Check the savings arithmetic | 200 kWh | Savings per month = Today − With solar | | |
| SC-06 | Change the panel size (if available) | 200 kWh, each size | All figures update straight away and a bigger panel gives more savings | | |
| SC-07 | Go back to the Bill tab, change the kWh, return to Solar | 200 → 300 kWh | Solar check uses the new kWh | | |

## 2. Correct numbers (most important)

Work out each expected value in the spreadsheet using only the official sources.

| ID | Input | Expected Today | Expected With solar | Expected savings / month | Expected payback (years) | Result | Notes |
|---|---|---|---|---|---|---|---|
| SC-10 | 200 kWh (pitch example) | Rs 1,216.75 *(confirm it's the same charge type as the app's)* | Rs ______ | Rs ______ | ______ | | |
| SC-11 | 201.5 kWh (average home) | Rs ______ | Rs ______ | Rs ______ | ______ | | |
| SC-12 | Low usage: ______ kWh | Rs ______ | Rs ______ | Rs ______ | ______ | | |
| SC-13 | High usage: ______ kWh | Rs ______ | Rs ______ | Rs ______ | ______ | | |
| SC-14 | Exactly on a band limit: ______ kWh | Rs ______ | Rs ______ | Rs ______ | ______ | | |
| SC-15 | One kWh above that band limit: ______ kWh | Rs ______ | Rs ______ | Rs ______ | ______ | | |

## 3. Edge cases

| ID | Input | Expected | Result | Notes |
|---|---|---|---|---|
| SC-20 | 0 kWh | No crash; a clear message such as "Enter how many kWh you used" | | |
| SC-21 | Very low usage where solar may never pay back | No negative, "infinite" or nonsense payback; a clear message instead | | |
| SC-22 | Usage so low the solar bill would go below Rs 0 | With solar never shows a negative bill (agree the rule with the developers) | | |
| SC-23 | Very high usage (2000 kWh) | Numbers are correct and the layout doesn't break | | |
| SC-24 | Decimal input (150.5) | Either accepted and calculated correctly, or a clear message. No crash | | |

## 4. Bad input

| ID | Input | Expected | Result | Notes |
|---|---|---|---|---|
| SC-30 | Empty field, tap Calculate | Inline message next to the field; nothing is calculated | | |
| SC-31 | Negative number (−50) | Inline message; nothing is calculated | | |
| SC-32 | Letters ("abc") | Rejected, or the phone shows a number keypad so letters can't be typed | | |
| SC-33 | Absurd value (999999999) | Message or sensible limit; no crash or overflow | | |
| SC-34 | Fix the bad input after an error | The error message disappears | | |

## 5. Display and wording

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| SC-40 | Source line | "Source: 2026 solar scheme" is visible and links to the official page | | |
| SC-41 | Estimate wording | The screen says the solar figures are an **estimate** | | |
| SC-42 | Money format | "Rs" with thousands separators, rounded (Rs 1,216.75, not Rs 1216.7499) | | |
| SC-43 | Payback format | Readable, e.g. "6.5 years" or "about 6 years". Not "6.4999 years" | | |
| SC-44 | Fits the phone | No sideways scrolling at 360 px wide; key result visible without scrolling | | |
| SC-45 | Readability | Text at least about 14 px; savings card colours readable (contrast) | | |
| SC-46 | Tap targets | Buttons and dropdown are easy to tap with a thumb (about 44 px tall) | | |
| SC-47 | Bottom nav | The Solar tab is highlighted while on this screen | | |
| SC-48 | Loading / no connection | If the API is slow or down: a loading state, then a friendly error. No blank screen or crash | | |

## 6. Demo path (run before every rehearsal and on stage day)

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| SC-50 | Full flow: kWh → bill → See solar savings → result | Done in under 1 minute | | |
| SC-51 | Use the exact demo numbers (200 kWh) | Numbers match the slides exactly | | |
| SC-52 | Run on the demo device and network | Works; no login or other pop-ups get in the way | | |
| SC-53 | Run 3 times in a row | Same result every time | | |
| SC-54 | Backup ready | Screenshots or a screen recording of the flow saved in `docs/demo/` | | |

---

## Bug template

Copy this for each failed case.

```
Bug ID:      BUG-___
Test case:   SC-__
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
| Totals | | | |

Ready for demo? ☐ Yes ☐ No. Signed: __________ Date: __________
