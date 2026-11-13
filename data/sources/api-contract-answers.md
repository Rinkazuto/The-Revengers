# Data Lead answers to the API contract open questions

Prepared 13 Nov 2026 by the Data Lead. Every figure has a source. Items marked ESTIMATE, ASSUMPTION, UNCONFIRMED or TEAM DECISION are not official figures.

## Q1. Fixed charge, minimum charge, VAT, levies
- There is **no fixed charge added on top**. CEB uses a **minimum monthly charge**: the bill is the tier total, or the minimum charge if the tier total is lower.
  - 2026 minimums: tariff 110 Rs 50.60, 110A Rs 31.00, 120 Rs 211.60, 140 Rs 424.35 (data/seed/minimum_charges.csv, source https://ceb.mu/tariffs).
  - Example: 5 kWh on tariff 110 = 5 x 3.63 = Rs 18.15 by tiers, so the bill is Rs 50.60.
- **Contract change requested:** rename `fixedChargeRs` to `minimumChargeRs`, and compute `totalRs = max(sum of amountRs, minimumChargeRs)`. Do not add the two.
- Rounding: General Notice No. 473 of 2026, paragraph 18, says "All bills shall be rounded to the nearest rupee." TEAM DECISION: show the exact amount in rupees and cents, the rounded bill, or both. The test cases hold the exact amount.
- VAT: 0%. The Value Added Tax Act, Fifth Schedule, item 7(1), lists "Electricity supplied by the Central Electricity Board and the renting out of a meter". Section 11(2) makes Fifth Schedule supplies zero-rated. Text read: consolidated version of 30 June 2017. The Finance Act 2026 amended the Fifth Schedule, and the amended item was not read. The app adds no VAT.
- Meter rental is a separate monthly bill line: Rs 20.00 for a domestic single-phase meter, Rs 120.00 for a domestic three-phase meter (data/seed/meter_rentals.csv, General Notice No. 1804 of 2022, Appendix XI, page 5019). General Notice No. 473 of 2026, paragraph 13, keeps those rates unchanged. TEAM DECISION: show it as its own line, or leave it out and say so on screen.
- MBC TV licence fee: UNCONFIRMED. No source for the amount has been read. Leave it out of the app.
- The app shows the **consumption charge** and says so on screen.

## Q2. By band or by bracket
- **By band.** General Notice No. 1804 of 2022 charges "Initial 25 kilowatt hours", then "Next 25 kilowatt hours", each at its own rate. 200 kWh on tariff 110 (2026) = 25x3.63 + 25x5.04 + 25x5.45 + 25x6.27 + 100x7.07 = **Rs 1,216.75**.
- 40 expected bills are in tests/test-cases/bill_test_cases.csv for QA: 12 each for tariff 110 (2023), 110 (2026) and 110A (2026), and 2 each for 120 and 140 (2026).
- Band boundaries: AGREED with the Backend Lead on 13 Nov 2026. data/seed/tariff_tiers.csv keeps the Gazette wording, from_kwh 1 to 25, 26 to 50 and so on. The seeder stores FromKwh = from_kwh minus 1, so the database holds 0 to 25, 25 to 50, matching the contract. Each band covers the same kWh either way: kWh in a band = min(usage, ToKwh) minus FromKwh. All 40 test cases give the same bills with the database values.

## Q3. 2026 solar scheme
Source: CEB Household Rooftop Solar Photovoltaic (PV) Scheme information leaflet, revised July 2026 (link in data/sources/README.md). The figures are in data/seed/solar_scheme_2026.csv and data/seed/solar_system_prices_2026.csv.

| Item | Value | Status |
|---|---|---|
| Who can join | CEB domestic customers, households only | Confirmed, leaflet page 5 |
| Maximum installation | 50 kW | Confirmed, leaflet page 5 |
| Maximum export | 5 kWac per electrical phase. A three-phase supply is needed above 5 kWac of export | Confirmed, leaflet page 1 |
| Network survey | Needed above 10 kWac | Confirmed, leaflet pages 1 and 5 |
| Battery | Required above 3.5 kWac, for at least 3 evening hours | Confirmed, leaflet pages 1 and 5 |
| Grant | 25% of the cost, maximum Rs 75,000 per application | Confirmed, leaflet pages 1 and 2 |
| Gross metering: export rate | Rs 4.83 per kWh produced | Confirmed, leaflet page 3 |
| Net metering: excess export credit | Rs 3.00 per kWh | Confirmed, leaflet page 4 |
| Net metering: CST on energy offset | Rs 0.00 up to 500 kWh per month, Rs 0.82 for 501 to 1000, Rs 1.63 above 1000 | Confirmed, leaflet page 3 |
| Processing fee | Rs 2,000, non-refundable | Confirmed, leaflet page 5 |
| Contract length | 20 years | Confirmed, leaflet page 8: "a contractual term of 20 years" |
| System price | 12 sizes from 1.5 kWp (Rs 165,000 to 195,000) to 15.0 kWp (Rs 760,000 to 850,000), with a 5 kWh battery | CEB ESTIMATE, "Approx. Price" on https://ceb.mu/pv_calculator. Excludes all incentives |
| Panel degradation | 0.5% per year | CEB ESTIMATE, https://ceb.mu/pv_calculator |

The CEB comparison page also mentions "the 15% cash back scheme administered by the Mauritius Revenue Authority (MRA)". The leaflet grant is 25% up to Rs 75,000 through the Development Bank of Mauritius. TEAM DECISION: the app uses the leaflet grant and says so in `assumptions`.

Suggested calculation for the MVP (net metering, monthly):
- generation = systemSizeKwp x 113.71 kWh (PVGIS, see Q4)
- kwhFromGrid = max(0, monthlyKwh - generation)
- withSolar.monthlyBillRs = bill(kwhFromGrid) - max(0, generation - monthlyKwh) x 3.00, never below 0
- withoutSolar.monthlyBillRs = bill(monthlyKwh), equal to /api/bill/calculate
- installCostRs = CEB guide price for systemSizeKwp - min(25% of that price, 75,000). The guide gives a minimum and a maximum, so return both or pick one and state which in `assumptions`.
- The Rs 2,000 processing fee is a one-off cost. Add it to installCostRs and state it in `assumptions`.
- paybackYears = installCostRs / yearlySavingsRs, null if savings are 0 or less
- For usage up to 500 kWh the CST is Rs 0.00, so the MVP can ignore it. State this in `assumptions`.
- Default systemSizeKwp: TEAM DECISION. Suggestion: the smallest size in solar_system_prices_2026.csv whose generation covers monthlyKwh, with 15.0 kWp as the largest priced size.

## Q4. Generation per kWp in Mauritius
Recommended source: PVGIS, the photovoltaic calculator of the European Commission Joint Research Centre. It models the panel losses directly, so no performance ratio has to be assumed.
- Query read on 13 Nov 2026: https://re.jrc.ec.europa.eu/api/v5_3/PVcalc?lat=-20.25&lon=57.55&peakpower=1&loss=14&optimalangles=1&outputformat=json
- Inputs: 1 kWp crystalline silicon, 14% system loss (the PVGIS default), slope 17 degrees and azimuth -173 degrees (both chosen by PVGIS as optimal, roughly facing north), PVGIS-SARAH3 satellite data 2005 to 2023.
- Output: **113.71 kWh per kWp per month = 1,364.48 kWh per kWp per year**. PVGIS reports total losses of 21.86%.
- Monthly kWh per kWp: Jan 132.38, Feb 116.8, Mar 121.97, Apr 109.32, May 100.04, Jun 88.09, Jul 89.15, Aug 99.41, Sep 113.97, Oct 127.88, Nov 128.4, Dec 137.08.
- STATUS: confirmed. The raw JSON is saved as data/sources/raw/pvgis_pvcalc_-20.25_57.55_1kwp_loss14_optimal.json. The 12 monthly values are in data/seed/pvgis_monthly_yield_1kwp.csv.
- The ratio of PVGIS output to sun on the panel is 0.7814 (1364.48 / 1746.29).
- PVGIS placed the point at 444 m elevation, which is inland. Coastal homes probably receive more sun.

Earlier method, kept for comparison:
- NASA POWER daily irradiance, 2023 to 2025, latitude -20.25, longitude 57.55 (data/seed/nasa_power_daily_irradiance_2023_2025.csv, 1,096 days):
  - mean 5.6314 kWh per m2 per day (unrounded 5.631391514598540)
- With a performance ratio of 0.75 (ASSUMPTION, no official source found yet): 4.2235 kWh per kWp per day = **128.4661 kWh per kWp per month (x 365 / 12) = 1,541.59 kWh per kWp per year**.
- The CEB Renewable Energy Roadmap 2030 (review 2022) was checked for a yield figure on 13 Nov 2026. Its text gives no kWh per kW per year figure, so it cannot confirm the 0.75 ratio.
- Monthly means (kWh per m2 per day, to 4 decimal places) for a seasonal chart: Jan 6.6329, Feb 6.6577, Mar 5.9326, Apr 4.9192, May 4.2262, Jun 3.8396, Jul 4.2061, Aug 4.7964, Sep 5.7980, Oct 6.7386, Nov 6.8772, Dec 7.0053.
