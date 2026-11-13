# Power Check: API Contract

**Status:** AGREED (13 Nov 2026) by the Backend Lead, Data Lead, Frontend Lead and Integrator. See `docs/decision-log.md`.

**Owner:** Integrator. Changes go through a PR and get announced in the team chat. The C# shapes live in `backend/PowerCheck.Api/Contracts/ApiContracts.cs`; this file and that file must always match.

---

## Basics

| Item | Value |
|---|---|
| Base URL (local) | `http://localhost:5012` |
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

**Request**
```json
{ "monthlyKwh": 250, "systemSizeKwp": 3, "tariffCode": "110" }
```
| Field | Rule |
|---|---|
| `monthlyKwh` | Required, 0 to 5000 |
| `systemSizeKwp` | Optional. Must be a **CEB price-guide size**: 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7.5, 10 or 15. Any other value → 400. If it's left out, the backend picks **the smallest size whose average output covers the usage** (or 15 if none does). |
| `tariffCode` | Optional, default `110` |

**Response 200**
```json
{
  "monthlyKwh": 250,
  "systemSizeKwp": 3,
  "withoutSolar": { "monthlyBillRs": 0.00 },
  "withSolar": {
    "monthlyGenerationKwh": 0,
    "kwhFromGrid": 0,
    "monthlyBillRs": 0.00
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
    "Net metering: extra solar energy is credited at Rs 3.00 per kWh. CST charge at your usage: Rs 0.00 per kWh of energy offset.",
    "Panels lose 0.5% of their output each year (CEB estimate).",
    "Bills use CEB domestic tariff 110, in force from 1 May 2026."
  ],
  "projection": [
    { "year": 1, "cumulativeSavingsRs": 0.00, "netGainRs": 0.00 },
    { "year": 2, "cumulativeSavingsRs": 0.00, "netGainRs": 0.00 }
  ]
}
```
- `withoutSolar.monthlyBillRs` must equal `totalRs` from `/api/bill/calculate` for the same kWh and tariff. **QA test case.**
- **Year 1 is worked out month by month** with the 12 PVGIS monthly outputs (more sun in summer, less in winter). Each month: bill for the kWh still bought from CEB, **plus** the CST charge on the kWh covered by solar, **minus** Rs 3.00 per kWh of extra solar sent to CEB (net metering). A month's bill is never below 0.
- CST charge per kWh covered by solar depends on monthly usage: Rs 0.00 up to 500 kWh, Rs 0.82 for 501–1000 kWh, Rs 1.63 above 1000 kWh.
- `withSolar.monthlyBillRs` = year-1 bill ÷ 12. `monthlyGenerationKwh` = size × 113.71 (the average month). `kwhFromGrid` = year-1 kWh bought from CEB ÷ 12.
- `monthlySavingsRs` = `withoutSolar.monthlyBillRs` − `withSolar.monthlyBillRs`. `yearlySavingsRs` = without-solar bill × 12 − year-1 bill with solar.
- `installCostRs` = middle of CEB's price range for that size, **minus the 25% government grant (capped at Rs 75,000), plus the Rs 2,000 processing fee**.
- `paybackYears` = when the total savings reach `installCostRs`, 1 decimal. Panels lose 0.5% of their output each year, so later years save slightly less. If it doesn't pay back within 30 years it is `null`, and the UI shows "Solar doesn't pay back at this usage".
- `projection` is 20 rows (the 20-year CEB contract), one per year, for the graph: `cumulativeSavingsRs` = total saved by the end of that year, `netGainRs` = that minus `installCostRs` (negative until payback).
- `assumptions` lists every figure used and its source. It is shown on screen under the result, so users and judges can see where the numbers come from.
- All rates (grant, cap, fee, CST, export credit, degradation, prices, monthly output) come from `data/seed/`. None are typed into the code.

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
- `billRs` uses the same `BillCalculator` as `/api/bill/calculate` (tariff `110`).

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

## Shared C# records

The single source of truth is `backend/PowerCheck.Api/Contracts/ApiContracts.cs`. The frontend copies these shapes.

```csharp
public record TariffBandDto(decimal FromKwh, decimal? ToKwh, decimal PricePerKwh);
public record TariffsResponse(string TariffCode, string Source, string SourceUrl, List<TariffBandDto> Bands, decimal MinimumChargeRs);

public record BillRequest(decimal MonthlyKwh, string? TariffCode = null);
public record BillLineDto(decimal FromKwh, decimal? ToKwh, decimal Kwh, decimal PricePerKwh, decimal AmountRs);
public record BillResponse(decimal MonthlyKwh, List<BillLineDto> Lines, decimal MinimumChargeRs, decimal TotalRs, string Source);

public record SolarRequest(decimal MonthlyKwh, decimal? SystemSizeKwp, string? TariffCode = null);
public record WithoutSolarDto(decimal MonthlyBillRs);
public record WithSolarDto(decimal MonthlyGenerationKwh, decimal KwhFromGrid, decimal MonthlyBillRs);
public record ProjectionYearDto(int Year, decimal CumulativeSavingsRs, decimal NetGainRs);
public record SolarResponse(decimal MonthlyKwh, decimal SystemSizeKwp, WithoutSolarDto WithoutSolar, WithSolarDto WithSolar,
    decimal MonthlySavingsRs, decimal YearlySavingsRs, decimal InstallCostRs, decimal? PaybackYears,
    string Source, List<string> Assumptions, List<ProjectionYearDto> Projection);

public record ReadingRequest(string Month, decimal Kwh);
public record ReadingDto(int Id, string Month, decimal Kwh, decimal BillRs);
public record ReadingsResponse(List<ReadingDto> Readings, string? Trend);
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

## Known gaps (to fix before the demo)

| Gap | Owner |
|---|---|
| The `billRs` in readings still uses `FakeTariffData`. Switch it to `TariffData`, then delete `FakeTariffData.cs` and `FakeSolarData.cs`. | Backend Lead |
| Readings are kept in memory (`ReadingStore`), so they're lost on restart. Move them to the `Readings` table. | Backend Lead + Data Lead |
| The website still uses fake data: `"UseMockApi": true` in `frontend/PowerCheck.Web/appsettings.json`. Set it to `false` and test every page against the real API before the demo. | Frontend Lead |
