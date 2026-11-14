# QA test cases: Cyclone mode

**Feature:** Cyclone mode, the team's answer to the surprise card "Made for Mauritius". In a cyclone the CEB grid goes down and grid-tied rooftop solar switches off with it; only a battery keeps the power on. Cyclone mode shows how long a battery keeps the chosen appliances running, and (if built in time) when to charge it.
**Endpoints:** `GET /api/cyclone/appliances`, `POST /api/cyclone/backup`, and optionally `GET /api/cyclone/outlook`
**Owner:** Design, QA and Pitch lead
**Status:** Ready to run. All backend fixes are on `main` (14 Nov 2026), and the team reports all 11 cases CY-01 to CY-95 pass. The wind outlook is built and works offline from a saved forecast (CY-30 to CY-38). The website uses the real API (`"UseMockApi": false`).

**Expected numbers for CY-01 to CY-05 and CY-90 to CY-95** come from the Data Lead's file [`cyclone_backup_cases.csv`](cyclone_backup_cases.csv), using its **97.8% column**. The IDs in this file match it.

---

## Before you start

**Team decision (14 Nov 2026): 97.8% of the battery is usable.** Source: Huawei SUN2000-5KTL-L1, European weighted efficiency 97.8% (`data/seed/cyclone_reference_values.csv`). A 5 kWh battery delivers **4.89 kWh** to the appliances.

**How the result is worked out** (API contract, section 7):
- `kwhPerDay` = sum of (watts × hours per day) ÷ 1000
- `hoursCovered` = usable kWh ÷ `kwhPerDay` × 24, to 1 decimal
- The battery is full when the power cut starts; solar recharging is not counted.

**Sourced appliance watts** (`data/seed/cyclone_appliances.csv`):

| id | Appliance | Watts | Source |
|---|---|---|---|
| `fridge` | Fridge | 35.5023 (311 kWh a year) | Samsung RB29FERNDSA energy label, Courts Mammouth |
| `led_light` | LED light (one bulb) | 9 | US FTC, "Shopping for Light Bulbs" |
| `wifi_router` | Wi-Fi router | 12 | TP-Link Archer C6 (12 V, 1 A) |
| `ceiling_fan` | Ceiling fan | 175 | Virginia Tech (top of 65–175 W range) |
| `tv` | TV (flat screen) | 120 | Virginia Tech |
| `laptop` | Laptop | 50 | Virginia Tech |
| `phone_charger` | USB-C charger | 35 | Apple 35W adapter label (team member's charger) |

**Wording rules (team decision):**
- Always **"up to about…"**: the app says **"up to about 4.1 days"** for the demo case; "up to about 4 days" is fine when speaking. Never just "4 days".
- On screen: *"Assumes a new, fully charged battery. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less."*
- Battery advice only. Never anything that looks like an official cyclone warning; always "check MMS".

## ✅ Fixes since the first check (merged into `main`, 14 Nov 2026)

Reported by the team after the `submain` → `main` merge: **all 11 cases CY-01 to CY-95 pass**, using the 97.8% column.

| What | Before | Now | Cases |
|---|---|---|---|
| Usable battery | 80% | **97.8%**, read from `data/seed` | CY-01 to CY-05 |
| Appliance watts | Hard-coded in the code (Wi-Fi 10 W, fan 120 W, charger 7.5 W) | **Read from `data/seed/cyclone_appliances.csv`** (12 W, 175 W, 35 W) | CY-02 to CY-05 |
| Result wording | "about 4.1 days" | **"up to about 4.1 days"** | CY-50, CY-70 |
| Caveat in assumptions | Missing | **In the assumptions list** | CY-51, CY-52 |
| Wind outlook | Not built | **Built**: works offline from a real saved forecast; live needs the API key in user-secrets | CY-30 to CY-38 |

If a case fails on your PC, first check you've pulled the latest `main`.

## How to use this file

1. Run each case on a real phone (or browser dev tools in phone size). For API checks, use Swagger at `http://localhost:5012/swagger`.
2. Write **P** (pass) or **F** (fail) in the Result column.
3. For every fail, log a bug (template at the bottom). The Backend Lead wants: **endpoint, what you sent, what came back**.

**Test devices / sizes**

| Device / size | Browser | Tested by | Date |
|---|---|---|---|
| 390×844 (iPhone 13/14) | | | |
| 360×800 (Android) | | | |
| Real phone: ________ | | | |

---

## 1. Appliance list

| ID | Steps | Expected | Result | Notes |
|---|---|---|---|---|
| CY-A1 | `GET /api/cyclone/appliances` | 7 appliances with the ids in the table above; every one has `watts` and a `source` | | |
| CY-A2 | Open the Cyclone tab | Every appliance shows its name and watts, with a way to see the source | | |
| CY-A3 | Check the watts | Match the sourced table above (the API now reads them from `cyclone_appliances.csv`) | | |

## 2. Battery hours (most important)

Battery 5 kWh, 97.8% usable. Hours per day as listed.

| ID | Appliances (hours a day) | Expected kWh a day (API shows 2 decimals) | Expected hours covered | Result | Notes |
|---|---|---|---|---|---|
| CY-01 | Fridge 24 h | 0.8521 → **0.85** | **137.7** | | |
| CY-02 | Fridge 24 h, Wi-Fi 24 h | 1.1401 → **1.14** | **102.9** | | |
| **CY-03** | **Fridge 24 h, Wi-Fi 24 h, LED light 6 h** (the demo case) | **1.1941 → 1.19** | **98.3 → "up to about 4 days"** | | Must match the demo and slides |
| CY-04 | Fridge 24 h, Wi-Fi 24 h, LED 6 h, ceiling fan 8 h | 2.5941 → **2.59** | **45.2** | | Shows how a fan cuts the time in half |
| CY-05 | Fridge 24 h, TV 4 h, laptop 4 h, phone charger 2 h | 1.6021 → **1.60** | **73.3** | | |
| CY-06 | Untick an appliance after a result | — | Hours update straight away | | |
| CY-07 | Leave `batteryKwh` out of the request | — | Uses the default **5 kWh** | | |

## 3. Bad input (400)

| ID | Request | Expected | Result | Notes |
|---|---|---|---|---|
| CY-90 | Empty appliance list | 400 "Choose at least one appliance." | | |
| CY-91 | Same appliance twice (fridge 24 h, fridge 10 h) | 400 "Each appliance can only be listed once: fridge." | | The app's tick boxes should prevent this |
| CY-92 | Fridge 25 h | 400 "hoursPerDay must be between 0 and 24." | | |
| CY-93 | Fridge 0 h, Wi-Fi 0 h | 400 "At least one appliance needs more than 0 hours a day." | | |
| CY-94 | Battery 0 kWh | 400 "batteryKwh must be more than 0 and at most 50." | | |
| CY-95 | Battery 51 kWh | 400 "batteryKwh must be more than 0 and at most 50." | | |
| CY-96 | Unknown appliance id (`"oven"`) | 400 "Unknown appliance id: oven. Use GET /api/cyclone/appliances for the list." | | |

## 4. Wind outlook (built; works offline from a saved forecast)

`GET /api/cyclone/outlook` uses OpenWeather's free 5-day / 3-hour forecast and gives **battery advice**, not cyclone warnings. Levels use the Beaufort scale on **sustained** wind (`data/seed/cyclone_reference_values.csv`): `ok` below 62.97 km/h, `charge` from 62.97 km/h (Beaufort 8, gale), `chargeNow` from 88.90 km/h (Beaufort 10, storm). Gusts only appear in the advice text. The result is cached for 15 minutes.

**Two modes:**
- **Live:** needs the OpenWeather API key in the backend's user-secrets (`OpenWeather:ApiKey`). Without the key, the API goes straight to offline mode.
- **Offline:** uses the **real saved forecast** `data/sources/raw/openweather_forecast_-20.25_57.55.json` (first entry 14 Nov 2026, 10:00 Mauritius time). It's a calm day, so the offline result should be: `level: "ok"`, `isOffline: true`, strongest wind about **16 km/h** (15.8), gusts about **21 km/h** (21.2), advice starting "No strong winds in the next 48 hours…".

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| CY-30 | Calm forecast (the saved forecast is one) | `level: "ok"`; "No strong winds in the next 48 hours (strongest wind about 16 km/h, gusts 21 km/h). Keep your battery charged during cyclone season."; calm colours, no alarm | | Expected offline result today |
| CY-31 | Strong winds (62.97–88.89 km/h) | `level: "charge"`; "Strong winds forecast … Charge your battery." | | Only testable live on a windy day, or with a test forecast file from the developers |
| CY-32 | Storm winds (88.90 km/h or more) | `level: "chargeNow"`; "Storm-force winds forecast … Charge your battery now." | | Same as CY-31 |
| CY-33 | Every result | `advice` ends with "For official cyclone warnings, check MMS." and the screen shows it | | |
| CY-34 | Times | Shown in Mauritius time (UTC+4), not UTC | | |
| CY-35 | **No API key, or Wi-Fi / internet off**, then open the Cyclone tab | Answers within about 3 seconds with `isOffline: true`; the screen says it's the last saved forecast and shows its date (14 Nov 2026). It's a **real saved forecast**, never a made-up storm | | Key "never breaks on stage" check |
| CY-36 | With the key set, open the tab twice within a few minutes | Second time is instant (15-minute cache); `isOffline: false` | | Only once the key is set |
| CY-37 | Security | No OpenWeather API key in any committed file (`appsettings.json`, code, docs). Search the repo for "OpenWeather" before every merge | | Key lives only in user-secrets |
| CY-38 | Saved forecast file missing | The API still answers without crashing, or the screen hides the outlook card with a friendly note | | Agree the expected behaviour with the Backend Lead |

**Pitch note:** on stage the outlook will most likely say **"No strong winds"** (calm weather, or the offline calm forecast). That's honest; don't promise judges a "charge your battery now" message. The wireframe's "Strong winds expected Tuesday night" banner is only an example.

## 5. Display and wording

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| CY-50 | Result wording | Starts with "up to about…", e.g. **"up to about 4.1 days"** for CY-03; never a bare promise like "4 days" | | |
| CY-51 | Caveat | Shows: "Assumes a new, fully charged battery. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less." | | |
| CY-52 | Assumptions list | Also keeps today's lines "Without a battery, ordinary rooftop solar switches off when the CEB grid goes down." and "For official cyclone warnings, check the Mauritius Meteorological Services (MMS)." Lists the appliance sources, "Usable battery: 97.8% of 5 kWh (Huawei SUN2000-5KTL-L1)", "Battery full when the power cut starts. Solar recharging after the storm is not counted.", **and the caveat** "Assumes a new, fully charged battery. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less." | | The caveat must be in the list itself, not only elsewhere on screen |
| CY-53 | No battery | Choosing "No battery" (if offered) shows: solar panels switch off when the CEB power goes off; no hours shown | | |
| CY-54 | Kreol | If the screen is in Kreol, the message reads naturally and the numbers match the English version (e.g. "…pandan apepre 98 erdtan") | | Ask a Kreol speaker on the team to check |
| CY-55 | Long results | Very long times (e.g. fridge only, 137.7 hours) are shown readably, e.g. "up to about 5½ days", not "137.7 hours" only | | Agree the format with Frontend |
| CY-56 | Money/number format | Hours to 1 decimal at most; kWh to 2 decimals | | |
| CY-57 | Fits the phone | No sideways scrolling at 360 px; tick boxes and hour inputs easy to tap (about 44 px) | | |
| CY-58 | Bottom nav | The Cyclone tab is highlighted on this screen | | |
| CY-59 | Backend down | Loading state, then a friendly error; no blank screen or crash | | |

## 6. Demo path (run before every rehearsal and on stage day)

| ID | Check | Expected | Result | Notes |
|---|---|---|---|---|
| CY-70 | Open Cyclone mode, tick fridge, Wi-Fi and LED light (6 h) | **"up to about 4.1 days"** (98.3 hours) within a few seconds | | Matches CY-03; on stage, read exactly what the screen says |
| CY-71 | Run with the demo phone's Wi-Fi off | Battery result still works (offline); outlook shows "offline" | | |
| CY-72 | Run 3 times in a row | Same result every time | | |

---

## Bug template

Prefer a GitHub issue with the **Bug** template. If you can't, copy this:

```
Bug ID:      BUG-___
Test case:   CY-__
Device/size:
Endpoint:    (e.g. POST /api/cyclone/backup)
What I sent: (e.g. { "appliances": [ { "id": "fridge", "hoursPerDay": 24 } ], "batteryKwh": 5 })
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
