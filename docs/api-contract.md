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
| Money | Mauritian rupees (`Rs`), decimal, rounded to 2 places |
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
- `toKwh: null` means "and above".
- Bands are sorted by `fromKwh`. The newest `effective_from` in force today wins.

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
| `systemSizeKwp` | Optional; if missing, use the scheme's default size. Must be > 0 and ≤ the scheme maximum. |
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
    "Solar output: 113.71 kWh per kWp per month (PVGIS, European Commission JRC).",
    "Install cost of Rs X per kWp",
    "Exported kWh credited at Rs Y each"
  ],
  "projection": [
    { "year": 1, "cumulativeSavingsRs": 0.00, "netGainRs": 0.00 },
    { "year": 2, "cumulativeSavingsRs": 0.00, "netGainRs": 0.00 }
  ]
}
```
- `withoutSolar.monthlyBillRs` must equal `totalRs` from `/api/bill/calculate` for the same kWh and tariff. **QA test case.**
- `withSolar.monthlyBillRs` = bill for `kwhFromGrid` minus credit for exported kWh, never below 0.
- `paybackYears` = `installCostRs / yearlySavingsRs`, 1 decimal. If savings are 0 or negative it is `null`, and the UI shows "Solar doesn't pay back at this usage".
- `projection` is one row per year for the graph: `cumulativeSavingsRs` = yearly savings × year, `netGainRs` = that minus `installCostRs` (negative until payback).
- `assumptions` is shown on screen under the result, so users and judges can see where the numbers come from.

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

## Known gaps (to fix before the demo)

| Gap | Owner |
|---|---|
| `/api/solar/estimate` and the `billRs` in readings still use `FakeSolarData` / `FakeTariffData`. Switch them to `TariffData`, then delete the Fake files. | Backend Lead |
| `/api/solar/estimate` doesn't use `tariffCode` yet (it only checks it). | Backend Lead |
| Readings are kept in memory (`ReadingStore`), so they're lost on restart. Move them to the `Readings` table. | Backend Lead + Data Lead |
