# Power Check: API Contract

**Status:** DRAFT. Confirm with the Backend Lead, Data Lead and Frontend Lead, then change this to **AGREED** and log it in `docs/decision-log.md`.

**Owner:** Integrator. Changes after AGREED go through a PR and get announced in the team chat.

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
| Login | None in the MVP |
| Swagger | `http://localhost:5012/swagger` |

> ⚠️ **All numbers in the examples below are made up to show the shape.** Real values come from the CEB tariff and the 2026 solar scheme in `data/seed/`.

### Errors (every endpoint)

Bad input returns **400** with ASP.NET's standard `ProblemDetails`:

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
| 201 | Created (POST /api/readings) |
| 400 | Bad input; show `errors` to the user |
| 404 | Not found |
| 500 | Backend bug; show "Something went wrong, try again" |

---

## 1. GET `/api/tariffs`

The CEB domestic tariff bands, from the database.

**Response 200**
```json
{
  "source": "CEB domestic tariff 2026",
  "sourceUrl": "https://...",
  "bands": [
    { "fromKwh": 0,   "toKwh": 25,   "pricePerKwh": 0.00 },
    { "fromKwh": 25,  "toKwh": 50,   "pricePerKwh": 0.00 },
    { "fromKwh": 50,  "toKwh": null, "pricePerKwh": 0.00 }
  ],
  "fixedChargeRs": 0.00
}
```
- `toKwh: null` means "and above".
- Bands are sorted by `fromKwh`.

---

## 2. POST `/api/bill/calculate`

Work out a monthly bill **without solar**.

**Request**
```json
{ "monthlyKwh": 250 }
```
| Field | Rule |
|---|---|
| `monthlyKwh` | Required, 0 to 5000 |

**Response 200**
```json
{
  "monthlyKwh": 250,
  "lines": [
    { "fromKwh": 0,  "toKwh": 25,  "kwh": 25,  "pricePerKwh": 0.00, "amountRs": 0.00 },
    { "fromKwh": 25, "toKwh": 50,  "kwh": 25,  "pricePerKwh": 0.00, "amountRs": 0.00 },
    { "fromKwh": 50, "toKwh": null,"kwh": 200, "pricePerKwh": 0.00, "amountRs": 0.00 }
  ],
  "fixedChargeRs": 0.00,
  "totalRs": 0.00,
  "source": "CEB domestic tariff 2026"
}
```
- One line per band the usage reaches.
- `totalRs` = sum of `amountRs` + `fixedChargeRs` (+ any VAT/levies, if the tariff includes them; **Data Lead to confirm**).

---

## 3. POST `/api/solar/estimate` ⭐ main feature

Compare **without solar vs with solar**.

**Request**
```json
{ "monthlyKwh": 250, "systemSizeKwp": 3 }
```
| Field | Rule |
|---|---|
| `monthlyKwh` | Required, 0 to 5000 |
| `systemSizeKwp` | Optional; if missing, use the scheme's default size. Must be > 0 and ≤ scheme maximum. |

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
  "source": "CEB 2026 solar scheme",
  "assumptions": [
    "Generation of X kWh per kWp per month (source: ...)",
    "Exported kWh credited at Rs Y (scheme rule)"
  ]
}
```
- `withoutSolar.monthlyBillRs` must equal `totalRs` from `/api/bill/calculate` for the same kWh. **QA test case.**
- `assumptions` is shown on screen under the result, so users and judges can see where the numbers come from.
- If savings are 0 or negative, `paybackYears` is `null`, and the UI shows "Solar doesn't pay back at this usage".

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

**Response 201**
```json
{ "id": 1, "month": "2026-10", "kwh": 250, "billRs": 0.00 }
```
- If that month already exists, **overwrite it** and return 200 (one reading per month).
- `billRs` is calculated with the same logic as `/api/bill/calculate`.

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

## Shared C# records (suggested)

The backend and the frontend use the same shapes. Put them in one file the Integrator owns, e.g. `backend/PowerCheck.Api/Contracts/`, and copy them to the frontend, or move them into a shared project later.

```csharp
public record TariffBandDto(decimal FromKwh, decimal? ToKwh, decimal PricePerKwh);
public record TariffsResponse(string Source, string SourceUrl, List<TariffBandDto> Bands, decimal FixedChargeRs);

public record BillRequest(decimal MonthlyKwh);
public record BillLineDto(decimal FromKwh, decimal? ToKwh, decimal Kwh, decimal PricePerKwh, decimal AmountRs);
public record BillResponse(decimal MonthlyKwh, List<BillLineDto> Lines, decimal FixedChargeRs, decimal TotalRs, string Source);

public record SolarRequest(decimal MonthlyKwh, decimal? SystemSizeKwp);
public record WithoutSolarDto(decimal MonthlyBillRs);
public record WithSolarDto(decimal MonthlyGenerationKwh, decimal KwhFromGrid, decimal MonthlyBillRs);
public record SolarResponse(decimal MonthlyKwh, decimal SystemSizeKwp, WithoutSolarDto WithoutSolar, WithSolarDto WithSolar,
    decimal MonthlySavingsRs, decimal YearlySavingsRs, decimal InstallCostRs, decimal? PaybackYears,
    string Source, List<string> Assumptions);

public record ReadingRequest(string Month, decimal Kwh);
public record ReadingDto(int Id, string Month, decimal Kwh, decimal BillRs);
public record ReadingsResponse(List<ReadingDto> Readings, string? Trend);
```

---

## Open questions (answer before marking AGREED)

| # | Question | Who |
|---|---|---|
| 1 | Does the CEB domestic bill have a fixed/minimum charge, VAT or other levies? | Data Lead |
| 2 | Is the tariff "by band" (each band at its own price) or "by bracket" (whole usage at one price)? | Data Lead |
| 3 | 2026 solar scheme: system sizes, install cost, how exported kWh are credited | Data Lead |
| 4 | Generation per kWp per month in Mauritius: which official source? | Data Lead |
| 5 | Readings without login: one shared list for the demo, OK? | Team |
| 6 | Keep `/api` prefix? | Backend Lead |
