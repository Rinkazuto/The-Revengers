# ReVolt: API Contract

**Status:** AGREED (13 Nov 2026) by the Backend Lead, Data Lead, Frontend Lead and Integrator. See `docs/decision-log.md`.
**Living document:** last updated 14 Nov 2026 (after the `v1-demo` feature freeze). Everything in this contract is built and live. Every change is listed in "Changes since the first draft" at the bottom.

**Owner:** Integrator. Changes go through a PR and get announced in the team chat. The C# shapes live in `backend/PowerCheck.Api/Contracts/ApiContracts.cs`; this file and that file must always match.

---

## Basics

| Item | Value |
|---|---|
| Base URL (local) | `http://localhost:5012` |
| Base URL (live) | `https://revolt-api-rb.azurewebsites.net` (Azure App Service). Swagger: `/swagger` |
| Prefix | All endpoints start with `/api` |
| Format | JSON, property names in `camelCase` |
| Money | Mauritian rupees (`Rs`), decimal, rounded to 2 places. **Frontend: always display with `F2`** (e.g. `value.ToString("F2")`), because SQLite returns `12.8` for `12.80`. |
| Energy | kWh, decimal |
| Month | String `"YYYY-MM"`, e.g. `"2026-10"` |
| Tariff code | Optional everywhere. `110` (default), `110A`, `120` or `140`. Unknown code → 400 (404 on GET /api/tariffs). |
| Login | None in the MVP. Readings are one shared list for the demo. |
| Swagger | `http://localhost:5012/swagger` |
| Health check | `GET /health` → `{ "status": "ok" }` |

> ⚠️ **Numbers in the examples below only show the shape.** Real values come from the database, seeded from `data/seed/` (CEB tariffs, CEB 2026 solar scheme leaflet, CEB price guide, PVGIS). Sources are listed in `data/sources/README.md`.

### Errors (every endpoint)

Bad input returns **400** with ASP.NET's standard `ValidationProblem`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "monthlyKwh": ["monthlyKwh must be between 0 and 5000."] }
}
```

| Code | Meaning |
|---|---|
| 200 | OK |
| 201 | Created (POST /api/readings, new month) |
| 400 | Bad input; show `errors` to the user |
| 404 | Not found (unknown tariff code on GET /api/tariffs) |
| 500 | Backend bug; show "Something went wrong, try again" |

---

## 1. GET `/api/tariffs?tariffCode=110`

The CEB domestic price bands in force today, from the database. `tariffCode` is optional (default `110`).

**Response 200**
```json
{
  "tariffCode": "110",
  "source": "CEB domestic tariff 110, in force from 1 May 2026",
  "sourceUrl": "https://ceb.mu/tariffs",
  "bands": [
    { "fromKwh": 0,  "toKwh": 25,   "pricePerKwh": 0.00 },
    { "fromKwh": 25, "toKwh": 50,   "pricePerKwh": 0.00 },
    { "fromKwh": 50, "toKwh": null, "pricePerKwh": 0.00 }
  ],
  "minimumChargeRs": 0.00
}
```
- Bands are **0-based**: 0–25, 25–50, 50–75… A band covers usage above `fromKwh` up to `toKwh`.
- `toKwh: null` means "and above" (the top band).
- Bands are sorted by `fromKwh`.
- When a tariff has prices with two dates (e.g. 2023 and 2026), **the newest one in force today wins**.

---

## 2. POST `/api/bill/calculate`

Work out a monthly bill **without solar**.

**Request**
```json
{ "monthlyKwh": 250, "tariffCode": "110" }
```
| Field | Rule |
|---|---|
| `monthlyKwh` | Required, 0 to 5000 |
| `tariffCode` | Optional, default `110` |

**Response 200**
```json
{
  "monthlyKwh": 250,
  "lines": [
    { "fromKwh": 0,  "toKwh": 25,  "kwh": 25,  "pricePerKwh": 0.00, "amountRs": 0.00 },
    { "fromKwh": 25, "toKwh": 50,  "kwh": 25,  "pricePerKwh": 0.00, "amountRs": 0.00 }
  ],
  "minimumChargeRs": 0.00,
  "totalRs": 0.00,
  "source": "CEB domestic tariff 110, in force from 1 May 2026"
}
```
- Charged **by band**: each band's kWh at that band's price. One line per band the usage reaches.
- `totalRs` = sum of `amountRs`, but **never less than `minimumChargeRs`**.
- Every endpoint uses the same `BillCalculator`, so bills always match.

---

## 3. POST `/api/solar/estimate` ⭐ main feature

Compare **without solar vs with solar** under the 2026 CEB solar scheme.

- **Without solar**: the home's normal tariff (`tariffCode`, default `110`).
- **With solar**: a solar home moves to the **prosumer tariff 150A** (single-phase) or **150B** (three-phase), whatever tariff it was on before (GN 473/2026, Appendix VIII, pp. 2230–2231). Tariff 110A also excludes solar owners (GN 1804/2022, p. 5005).

**Request**
```json
{ "monthlyKwh": 250, "systemSizeKwp": 3, "tariffCode": "110", "phase": "single" }
```
| Field | Rule |
|---|---|
| `monthlyKwh` | Required, 0 to 5000 |
| `systemSizeKwp` | Optional. Must be a **CEB price-guide size**: 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7.5, 10 or 15. Any other value → 400. If it's left out, the backend picks **the smallest size whose average output covers the usage** (or 15 if none does). |
| `tariffCode` | Optional, default `110`. **Only used for the without-solar bill.** |
| `phase` | Optional, `"single"` (default, tariff 150A) or `"three"` (tariff 150B). Any other value → 400. |

**Response 200**
```json
{
  "monthlyKwh": 250,
  "systemSizeKwp": 3,
  "withoutSolar": { "monthlyBillRs": 0.00 },
  "withSolar": {
    "monthlyGenerationKwh": 0,
    "kwhFromGrid": 0,
    "monthlyBillRs": 0.00,
    "exportCreditRs": 0.00
  },
  "monthlySavingsRs": 0.00,
  "yearlySavingsRs": 0.00,
  "installCostRs": 0.00,
  "paybackYears": 0.0,
  "source": "CEB Household Rooftop Solar PV Scheme leaflet (July 2026); CEB solar price guide; PVGIS 5.3, European Commission JRC",
  "assumptions": [
    "Solar output: 113.71 kWh per kWp per month on average, higher in summer and lower in winter (PVGIS, European Commission JRC).",
    "System price: Rs X to Rs Y for 3 kWp with a 5 kWh battery (CEB estimate, indicative only). We use the middle: Rs Z.",
    "Government grant: 25% of the cost, up to Rs 75,000 (eligible households). Processing fee: Rs 2,000.",
    "With solar you move to prosumer tariff 150A (single-phase), GN 473/2026. Net energy up to 300 kWh total use: flat Rs 8.58 per kWh. Minimum charge by total use: Rs 211.60.",
    "Extra solar sent to CEB is credited at Rs 3.00 per kWh. The credit is shown separately and is not counted in your savings.",
    "Panels lose 0.5% of their output each year (CEB estimate).",
    "Bill without solar uses CEB domestic tariff 110, in force from 1 May 2026."
  ],
  "projection": [
    { "year": 1, "cumulativeSavingsRs": 0.00, "netGainRs": 0.00 },
    { "year": 2, "cumulativeSavingsRs": 0.00, "netGainRs": 0.00 }
  ]
}
```
- `withoutSolar.monthlyBillRs` must equal `totalRs` from `/api/bill/calculate` for the same kWh and tariff. **QA test case.**
- **Year 1 is worked out month by month** with the 12 PVGIS monthly outputs (more sun in summer, less in winter).
- **With-solar bill, each month** (tariff 150A/150B, GN 473/2026; data from `TariffData.GetProsumerTariff(phase)`):
  1. **Total use** = `monthlyKwh` (CEB leaflet p.3: C = P + I − E).
  2. **Net energy** = total use − solar production, never below 0.
  3. **Energy charge**: on 150A, if total use ≤ 300 kWh, all net energy at the flat Rs 8.58 per kWh. Otherwise (and always on 150B), net energy goes through the prosumer bands from 0 kWh (12.03 / 12.28 / 12.55 / 12.80 / 13.06).
  4. **Minimum charge is picked by total use**: 150A: Rs 50.60 (0–75 kWh), Rs 211.60 (76–300 kWh), Rs 424.35 (above 300 kWh). 150B: Rs 424.35. The bill is the energy charge, but **never less than that minimum**.
  5. **CST**: GN 473's 150A has no CST. Only when total use is above 500 kWh, the leaflet's CST (Rs 0.82 for 501–1000 kWh, Rs 1.63 above 1000 kWh, per kWh covered by solar) is added and labelled "pending URA approval" in `assumptions`. At 500 kWh or less it is Rs 0.
  6. **Export credit**: extra solar sent to CEB (production − total use, if positive) × Rs 3.00. It goes in `exportCreditRs` and is **not taken off the bill and not counted in savings**.
- `withSolar.monthlyBillRs` = year-1 with-solar bill ÷ 12. `exportCreditRs` = year-1 export credit ÷ 12. `monthlyGenerationKwh` = size × 113.71 (the average month). `kwhFromGrid` = year-1 kWh bought from CEB ÷ 12.
- `monthlySavingsRs` = `withoutSolar.monthlyBillRs` − `withSolar.monthlyBillRs`. `yearlySavingsRs` = without-solar bill × 12 − year-1 bill with solar. **Neither includes the export credit**, so savings are on the safe side.
- **QA test case:** 200 kWh, 3 kWp, tariff 110, single-phase → without solar Rs 1,216.75, with solar **Rs 211.60**, savings **Rs 1,005.15 per month**.
- `installCostRs` = middle of CEB's price range for that size, **minus the 25% government grant (capped at Rs 75,000), plus the Rs 2,000 processing fee**.
- `paybackYears` = when the total savings reach `installCostRs`, 1 decimal. Panels lose 0.5% of their output each year, so later years save slightly less. If it doesn't pay back within 30 years it is `null`, and the UI shows "Solar doesn't pay back at this usage".
- `projection` is 20 rows (the 20-year CEB contract), one per year, for the graph: `cumulativeSavingsRs` = total saved by the end of that year, `netGainRs` = that minus `installCostRs` (negative until payback).
- `assumptions` lists every figure used and its source. It is shown on screen under the result, so users and judges can see where the numbers come from.
- All rates (150A/150B rates and minimums, grant, cap, fee, CST, export credit, degradation, prices, monthly output) come from `data/seed/`. None are typed into the code.

---

## 4. POST `/api/readings`

Save one month's usage.

**Request**
```json
{ "month": "2026-10", "kwh": 250 }
```
| Field | Rule |
|---|---|
| `month` | Required, `YYYY-MM`, not in the future |
| `kwh` | Required, 0 to 5000 |

**Response 201** (new month) or **200** (month already existed and was overwritten)
```json
{ "id": 1, "month": "2026-10", "kwh": 250, "billRs": 0.00 }
```
- One reading per month: saving the same month again overwrites it.
- `billRs` uses the same `BillCalculator` as `/api/bill/calculate`, with tariff `110` **as it was in force in that month** (e.g. a 2023 month uses the 2023 rates, a month from May 2026 onwards uses the 2026 rates).
- Readings are saved in the database, so they survive a restart.

---

## 5. GET `/api/readings`

List saved months with the trend.

**Response 200**
```json
{
  "readings": [
    { "id": 1, "month": "2026-08", "kwh": 230, "billRs": 0.00 },
    { "id": 2, "month": "2026-09", "kwh": 240, "billRs": 0.00 },
    { "id": 3, "month": "2026-10", "kwh": 250, "billRs": 0.00 }
  ],
  "trend": "up"
}
```
- Sorted oldest → newest.
- `trend`: `"up"`, `"down"` or `"flat"`, comparing the last two months. `null` if fewer than 2 readings.

---

## 🌀 Cyclone mode (surprise card: "Made for Mauritius")

In a cyclone the CEB grid goes down, and grid-tied rooftop solar switches off with it. Only a battery keeps the power on. These endpoints tell a family how long their battery keeps their essentials running, and when to charge it.

Sections 6 and 7 work **offline**. Section 8 calls OpenWeather but always answers, even with no internet.

## 6. GET `/api/cyclone/appliances`

Common home appliances with their power, read from the Data Lead's sourced file `data/seed/cyclone_appliances.csv` (#112). Change a number there and the API changes too.

> **Sourced values** (Data Lead, `data/seed/cyclone_*.csv`): fridge **35.5 W** average (Samsung RB29FERNDSA energy label, 311 kWh/year, sold by Courts Mammouth); usable battery **97.8%** of 5 kWh (Huawei SUN2000-5KTL-L1 weighted efficiency, decision log row 13). The result is shown as **"up to"** with the caveat: "Assumes a new, fully charged battery. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less."

**Response 200**
```json
{
  "appliances": [
    { "id": "fridge",      "name": "Fridge",               "watts": 0, "source": "..." },
    { "id": "led_light",   "name": "LED light (one bulb)", "watts": 0, "source": "..." },
    { "id": "wifi_router", "name": "Wi-Fi router",         "watts": 0, "source": "..." }
  ]
}
```
- Every `watts` value has a `source`. No unsourced "typical" values.
- The list and the watts come from `data/seed/cyclone_appliances.csv` (the one source of truth). The 7 ids are: `fridge`, `tv`, `laptop`, `ceiling_fan`, `wifi_router`, `phone_charger`, `led_light`.

## 7. POST `/api/cyclone/backup`

How long a battery keeps the chosen appliances running during a power cut.

**Request**
```json
{ "appliances": [ { "id": "fridge", "hoursPerDay": 24 }, { "id": "wifi_router", "hoursPerDay": 24 } ], "batteryKwh": 5 }
```
| Field | Rule → 400 if broken |
|---|---|
| `appliances` | Required, not empty. Each `id` must exist in `/api/cyclone/appliances`. No duplicate ids. |
| `hoursPerDay` | 0 to 24 |
| total of all `hoursPerDay` | Must be above 0 ("pick at least one appliance with hours above 0") |
| `batteryKwh` | Optional, default **5** (the battery in every system of the CEB price guide). Must be > 0 and ≤ 50. |

**Response 200**
```json
{
  "kwhPerDay": 0.00,
  "batteryKwh": 5,
  "hoursCovered": 0.0,
  "message": "Your battery keeps your fridge and Wi-Fi on for about X hours.",
  "assumptions": [
    "Appliance power from: ...",
    "Usable battery: X% of 5 kWh (source: ...).",
    "Battery full when the power cut starts. Solar recharging after the storm is not counted."
  ]
}
```
- `kwhPerDay` = sum of (`watts` × `hoursPerDay`) ÷ 1000.
- `hoursCovered` = usable kWh ÷ `kwhPerDay` × 24, 1 decimal. Usable kWh = `batteryKwh` × **97.8%** (Huawei SUN2000-5KTL-L1 weighted inverter efficiency; Huawei LUNA2000 lists 5 kWh usable), from `data/seed/cyclone_reference_values.csv`.
- `message` says **"up to about … days/hours"**: it assumes a new, fully charged battery. The assumptions add that real time is usually shorter (inverters lose more at small loads, older batteries hold less), and that recharging from the panels during the cyclone is not counted.

## 8. GET `/api/cyclone/outlook`

Wind outlook for the next 48 hours, turned into **battery advice**. It is not a cyclone warning. Shown as a banner at the top of the Cyclone page.

**Response 200**
```json
{
  "level": "charge",
  "maxWindKmh": 70,
  "maxGustKmh": 95,
  "maxWindAtUtc": "2026-11-17T17:00:00Z",
  "advice": "Strong winds forecast Tuesday night (gusts 95 km/h). Charge your battery now. For official cyclone warnings, check MMS.",
  "isOffline": false,
  "fetchedAtUtc": "2026-11-14T00:10:00Z",
  "source": "Wind forecast from OpenWeather (5 day / 3 hour forecast). Levels from the WMO Beaufort scale. For official cyclone warnings, check MMS."
}
```
- Data: OpenWeather's **free 5 day / 3 hour forecast** for the PVGIS point (-20.25, 57.55), the next 16 entries = 48 hours. Wind in m/s × 3.6 = km/h.
- `level` comes from **sustained wind** (`wind.speed`), Beaufort scale (Met Office): `"ok"` below 62.97 km/h, `"charge"` from 62.97 km/h (Beaufort 8, gale, 34 knots), `"chargeNow"` from 88.90 km/h (Beaufort 10, storm, 48 knots). Values from `data/seed/cyclone_reference_values.csv`. Gusts only appear in the `advice` text.
- `advice` is battery advice only and always ends with "For official cyclone warnings, check MMS."
- Times are UTC. The frontend shows them in Mauritius time (UTC+4).
- **Never breaks on stage:** 3-second timeout, result cached for about 15 minutes. If OpenWeather does not answer (or there is no key), the API returns a **real saved forecast** (`data/sources/raw/openweather_forecast_-20.25_57.55.json`) with `isOffline: true`, and the screen says "Last saved forecast from <time> (offline)".
- **API key:** only in the backend: user-secrets locally (`OpenWeather:ApiKey`), or the app setting `OpenWeather__ApiKey` on Azure. Never in `appsettings.json` or any committed file.
- If there is no live forecast **and** no saved forecast, the endpoint returns **503** with a ProblemDetails message.

---

## Shared C# records

The single source of truth is `backend/PowerCheck.Api/Contracts/ApiContracts.cs`. The frontend copies these shapes.

```csharp
public record TariffBandDto(decimal FromKwh, decimal? ToKwh, decimal PricePerKwh);
public record TariffsResponse(string TariffCode, string Source, string SourceUrl, List<TariffBandDto> Bands, decimal MinimumChargeRs);

public record BillRequest(decimal MonthlyKwh, string? TariffCode = null);
public record BillLineDto(decimal FromKwh, decimal? ToKwh, decimal Kwh, decimal PricePerKwh, decimal AmountRs);
public record BillResponse(decimal MonthlyKwh, List<BillLineDto> Lines, decimal MinimumChargeRs, decimal TotalRs, string Source);

public record SolarRequest(decimal MonthlyKwh, decimal? SystemSizeKwp, string? TariffCode = null, string? Phase = null);
public record WithoutSolarDto(decimal MonthlyBillRs);
public record WithSolarDto(decimal MonthlyGenerationKwh, decimal KwhFromGrid, decimal MonthlyBillRs, decimal ExportCreditRs);
public record ProjectionYearDto(int Year, decimal CumulativeSavingsRs, decimal NetGainRs);
public record SolarResponse(decimal MonthlyKwh, decimal SystemSizeKwp, WithoutSolarDto WithoutSolar, WithSolarDto WithSolar,
    decimal MonthlySavingsRs, decimal YearlySavingsRs, decimal InstallCostRs, decimal? PaybackYears,
    string Source, List<string> Assumptions, List<ProjectionYearDto> Projection);

public record ReadingRequest(string Month, decimal Kwh);
public record ReadingDto(int Id, string Month, decimal Kwh, decimal BillRs);
public record ReadingsResponse(List<ReadingDto> Readings, string? Trend);

public record ApplianceDto(string Id, string Name, decimal Watts, string Source);
public record AppliancesResponse(List<ApplianceDto> Appliances);
public record BackupApplianceRequest(string Id, decimal HoursPerDay);
public record BackupRequest(List<BackupApplianceRequest>? Appliances, decimal? BatteryKwh);
public record BackupResponse(decimal KwhPerDay, decimal BatteryKwh, decimal HoursCovered, string Message, List<string> Assumptions);
public record OutlookResponse(string Level, decimal MaxWindKmh, decimal MaxGustKmh, DateTime MaxWindAtUtc, string Advice,
    bool IsOffline, DateTime FetchedAtUtc, string Source);
```

---

## Answered questions

| # | Question | Answer |
|---|---|---|
| 1 | Fixed/minimum charge, VAT or levies? | Minimum charge per tariff code (`minimumChargeRs`). The bill is never below it. |
| 2 | By band or by bracket? | By band: each band's kWh at its own price. |
| 3 | 2026 solar scheme values | From the CEB Household Rooftop Solar PV Scheme leaflet (July 2026) and the CEB price guide, in `data/seed/`. |
| 4 | Solar output per kWp | PVGIS 5.3 (European Commission JRC): 113.71 kWh per kWp per month on average. |
| 5 | Readings without login | One shared list for the demo. |
| 6 | Keep the `/api` prefix? | Yes. |

## Changes since the first draft

| # | Change | Where | Status |
|---|---|---|---|
| 1 | `fixedChargeRs` → `minimumChargeRs`. The total is the band total or the minimum, whichever is higher. | Tariffs + bill responses | ✅ Merged |
| 2 | New optional `tariffCode` (default `"110"`) | Bill + solar requests, `GET /api/tariffs?tariffCode=` | ✅ Merged |
| 3 | New `tariffCode` field in the tariffs response | `GET /api/tariffs` | ✅ Merged |
| 4 | New `projection` list (`year`, `cumulativeSavingsRs`, `netGainRs`) for the graph | Solar response | ✅ Merged |
| 5 | Bands are 0-based (0–25, 25–50…) and `toKwh` is `null` for the top band | Tariffs + bill | ✅ Agreed with the Data Lead |
| 6 | When a tariff has two dates, the newest one wins | Tariffs + bill | ✅ Agreed |
| 7 | Unknown `tariffCode` → 404 on `GET /api/tariffs`, 400 on POST | All | ✅ Merged |
| 8 | `systemSizeKwp` must be a CEB price-guide size; if left out, the backend picks the smallest size that covers the usage | Solar request | ✅ Merged (#66) |
| 9 | `installCostRs` is after the 25% grant (capped at Rs 75,000), plus the Rs 2,000 fee | Solar response | ✅ Merged (#66) |
| 10 | Frontend shows money with 2 decimals (`F2`) | Frontend | ✅ Agreed (Data Lead) |
| 11 | Solar uses real data: monthly PVGIS output, net metering credit, CST charge, panel ageing; payback counted year by year | Solar response | ✅ Merged (#66) |
| 11b | Readings saved in the database; `billRs` uses the tariff in force that month; fake data files removed | Readings | ✅ Merged (#69) |
| 12 | Interim fix: export credit can't take the bill below the minimum charge | Solar response | ✅ Merged (#72), replaced by row 13 |
| 13 | With solar the home is billed on **prosumer tariff 150A/150B** (GN 473/2026): flat Rs 8.58 up to 300 kWh total use, bands above, minimum charge by total use; CST only above 500 kWh, labelled pending URA approval | Solar response | ✅ Merged (#73 data, #103 bill). Checked: 200 kWh + 3 kWp = Rs 211.60, saving Rs 1,005.15 |
| 14 | New `exportCreditRs` in `withSolar`, shown separately, **not** counted in savings | Solar response | ✅ Merged (#103) |
| 15 | New optional `phase` (`"single"` → 150A default, `"three"` → 150B); `tariffCode` only affects the without-solar bill | Solar request | ✅ Merged (#103) |
| 16 | 🆕 Cyclone mode: `GET /api/cyclone/appliances`, `POST /api/cyclone/backup` | Sections 6–7 | ✅ Merged (#77). Sourced values: fridge 35.5 W (#83), all appliances read from `cyclone_appliances.csv` (#112), battery 97.8% (#115, decision log row 13) |
| 17 | 🆕 `GET /api/cyclone/outlook`: OpenWeather wind → battery advice (`ok` / `charge` / `chargeNow`, Beaufort on sustained wind), offline fallback | Section 8 | ✅ Merged (#121 endpoint, #123 saved forecast, #132 banner on the Cyclone page) |
| 18 | Website models match the backend (`Phase`, `ExportCreditRs`); Solar page shows the export credit and tariff 150A | Frontend | ✅ Merged (#125) |
| 19 | Data and the saved forecast are read next to the app when published (Azure), else from the repo | Hosting | ✅ Merged (#151), live on Azure |

## Known limits (after the freeze)

| Limit | Note |
|---|---|
| Readings are one shared list | No login in the MVP. Login per household is a future idea. |
| Three-phase homes | The API supports `phase: "three"` (tariff 150B); the website assumes single-phase. |
| Live wind data needs a key | Without `OpenWeather:ApiKey` the outlook uses the saved real forecast (`isOffline: true`). |
| CST above 500 kWh | Shown as "pending URA approval", as in the CEB leaflet. |
