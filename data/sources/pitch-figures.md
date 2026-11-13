# Pitch figures

Every number for the slides and the judges' questions, with its source. Figures marked "calculated" come from the seed files in `data/seed/` and can be rechecked with the app. Full source details are in `data/sources/README.md`.

Checked against the seed data on 13 November 2026.

## 1. The problem: the 1 May 2026 rise

| Figure | Value | Source |
|---|---|---|
| Tariff rise | 15% from 1 May 2026 | Cabinet decisions, 10 April 2026; General Notice No. 473 of 2026, paragraphs 16 and 20 |
| Reason given | "increased electricity generation costs" | General Notice No. 473 of 2026, paragraph 16 |
| Actual rise per band, tariff 110 | 14.87% to 15.07% (each rate rounded to the cent) | Calculated from `tariff_tiers.csv` |
| Minimum charges, tariffs 110, 120, 140 | Up exactly 15.00%: Rs 44.00 → 50.60, Rs 184.00 → 211.60, Rs 369.00 → 424.35 | `minimum_charges.csv` |
| Consumers excluded from the rise | 128,800 of 541,127 (23.80%) | Cabinet decisions, 10 April 2026 |
| Who is excluded | Social Register households, tariff 110A, tariffs 215 and 315, SMEs | Cabinet decisions, 10 April 2026; General Notice No. 473 of 2026, paragraph 15 |
| CEB heavy fuel oil spending | Rs 6.87 billion (Jan to Sep 2026) vs Rs 5.88 billion (same months of 2025) | Press report: L'Express, 1 October 2026, via allAfrica. Not an official CEB figure |
| Fuel oil burned by power stations | 288,589 tonnes (2024) → 337,179 tonnes (2025) | Statistics Mauritius, Energy and Water Statistics 2025, Table 10 |

## 2. What it costs a household (tariff 110)

| Monthly use | Before (2023 rates) | After (1 May 2026) | Extra per month | Extra per year |
|---|---|---|---|---|
| 100 kWh | Rs 443.25 | Rs 509.75 | Rs 66.50 | Rs 798.00 |
| **200 kWh** | **Rs 1,058.25** | **Rs 1,216.75** | **Rs 158.50** | **Rs 1,902.00** |
| 300 kWh | Rs 1,804.25 | Rs 2,074.75 | Rs 270.50 | Rs 3,246.00 |
| 500 kWh | Rs 3,896.25 | Rs 4,480.75 | Rs 584.50 | Rs 7,014.00 |

Calculated from `tariff_tiers.csv` and `minimum_charges.csv`. Consumption charge only: meter rent (Rs 20.00 single-phase) is a separate line, and VAT is 0% (VAT Act, Fifth Schedule, item 7(1)). CEB rounds the final bill to the nearest rupee (General Notice No. 473 of 2026, paragraph 18).

## 3. Mauritian households

| Figure | Value | Source |
|---|---|---|
| Domestic consumers, 2025 | 484,680 (up 9,880 from 474,800 in 2024) | Statistics Mauritius, Table 11 |
| Domestic electricity sold, 2025 | 1,172,239 MWh (up 8.04% on 2024) | Statistics Mauritius, Table 11 |
| Average home | 201.5 kWh a month (201.5486 unrounded) | Calculated: 1,172,239 × 1000 ÷ 484,680 ÷ 12 |
| Homes' share of all electricity sold | 38.19% | Calculated from Statistics Mauritius, Table 11 |

The 200 kWh example in section 2 is close to the average Mauritian home.

## 4. The social tariff (110A)

| Figure | Value | Source |
|---|---|---|
| 110A rates | Not raised on 1 May 2026; the same as in 2023 | General Notice No. 473 of 2026, paragraph 15(a); General Notice No. 1804 of 2022, page 5005 |
| Who qualifies (low use) | Tariff 110 or 120 homes with 6+ consecutive readings, a yearly average up to 85 kWh a month and no month above 120 kWh | General Notice No. 1804 of 2022, page 5005 |
| Saving at 80 kWh | Rs 384.35 on 110 vs Rs 239.75 on 110A: Rs 144.60 a month | Calculated from `tariff_tiers.csv` |
| Minimum charge | Rs 31.00 on 110A vs Rs 50.60 on 110 | `minimum_charges.csv` |

## 5. Solar: the 2026 household scheme

| Figure | Value | Source |
|---|---|---|
| Solar output in Mauritius | 113.71 kWh per kWp per month; 1,364.48 kWh per kWp per year | PVGIS 5.3, European Commission Joint Research Centre |
| Best and worst months | December 137.08 kWh per kWp; June 88.09 kWh per kWp | PVGIS 5.3, monthly values |
| Size for a 200 kWh home | About 1.76 kWp (200 ÷ 113.71) | Calculated |
| Government grant | 25% of the cost, up to Rs 75,000 per application | CEB Household Rooftop Solar PV Scheme leaflet, July 2026, pages 1 and 2 |
| Gross metering | Every kWh produced credited at Rs 4.83 | CEB leaflet, page 3 |
| Net metering | Excess kWh credited at Rs 3.00 | CEB leaflet, page 4 |
| Contract length | 20 years | CEB leaflet, page 8 |
| Processing fee | Rs 2,000, non-refundable | CEB leaflet, page 5 |
| Battery | Required above 3.5 kWac | CEB leaflet, pages 1 and 5 |
| Price of a 3 kWp system with a 5 kWh battery | Rs 240,000 to Rs 270,000 (CEB estimate, before the grant) | CEB solar price guide, ceb.mu/pv_calculator |
| Grant on a Rs 240,000 system | Rs 60,000 (25%) | Calculated |
| Price range of all sizes | Rs 165,000 (1.5 kWp) to Rs 850,000 (15 kWp) | CEB solar price guide |

## 6. Our data, for the "real dataset" question

| Figure | Value |
|---|---|
| Official sources used | 11: CEB tariffs page, CEB 2023 tariff PDF, General Notice 1804 of 2022, General Notice 473 of 2026, Cabinet decisions of 10 April 2026, Statistics Mauritius 2025, CEB solar leaflet, CEB solar price guide, PVGIS, NASA POWER, VAT Act |
| Rows of real data in the database | 60 tariff bands, 7 minimum charges, 2 meter rents, 15 solar scheme values, 12 system prices, 12 PVGIS months |
| Expected bills used to test the app | 40, each recalculated by hand with 0 mismatches |

## Ready answers for judges

- **"Is this real data?"** Yes. Every number comes from CEB, the Government Gazette, Statistics Mauritius or PVGIS. Each file's source and checks are in `data/sources/README.md`.
- **"How do you know the bills are right?"** 40 expected bills, each with its working written out, all match the app's calculation. 200 kWh gives Rs 1,216.75.
- **"What happens when CEB changes the tariff again?"** Add the new rates to `tariff_tiers.csv` with the new start date. The app picks the newest rates automatically, and no code changes.
- **"Why 113.71 kWh?"** PVGIS, the European Commission's solar model, using 2005 to 2023 satellite data for Mauritius with standard panel losses.
- **"Why is the bill not rounded?"** The app shows the exact consumption charge. CEB rounds the final bill to the nearest rupee.
