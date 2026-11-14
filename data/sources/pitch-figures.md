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

## 5b. What solar does to the bill (tariff 150A)

A home with solar moves to prosumer tariff 150A (single-phase) or 150B (three-phase), General Notice 473 of 2026, Appendix VIII. Figures from `tests/test-cases/solar_expected_results.csv`; all match the app.

| Figure | Value | Source |
|---|---|---|
| Solar bill can't go below | Rs 211.60 a month (150A minimum for 76 to 300 kWh of total use) | General Notice 473 of 2026, Appendix VIII, page 2230 |
| 200 kWh home, 3 kWp | Rs 1,216.75 → Rs 211.60 a month; saving Rs 1,005.15 | Calculated (SC-10) |
| Export credit, 200 kWh home, 3 kWp | Rs 423.37 a month, shown separately, not counted in the saving | CEB leaflet, page 4 (Rs 3.00 per excess kWh) |
| Install cost, 3 kWp | Rs 193,250 (Rs 255,000 middle of CEB range − Rs 63,750 grant + Rs 2,000 fee) | CEB price guide and leaflet |
| Payback, 200 kWh home, 3 kWp | 16.0 years (panels lose 0.5% a year) | Calculated (SC-10) |
| The 300 kWh trap, 1.5 kWp | 300 kWh → Rs 1,110.58; 301 kWh → Rs 1,569.18. One extra kWh adds Rs 458.60, because the flat Rs 8.58 rate stops at 300 kWh of total use | Calculated (SC-14, SC-15) |

## 5c. Cyclone mode (surprise card)

| Figure | Value | Source |
|---|---|---|
| Solar without a battery in a power cut | Must switch off within 0.2 seconds | CEB SSDG Grid Code, April 2026, section 2.5.1 |
| Solar with a battery | Can keep running ("islanding mode") | CEB SSDG Grid Code, April 2026, section 2.5.2 |
| Battery in CEB's price guide | 5 kWh | CEB solar price guide |
| Energy the battery delivers | 4.89 kWh (5 kWh × 97.8% inverter efficiency) | Huawei LUNA2000 and SUN2000-5KTL-L1 specifications |
| Fridge + Wi-Fi + 6 h of one LED light | Up to about 4 days (98.3 hours) | Calculated (CY-03) |
| Fridge alone | Up to about 5.7 days (137.7 hours) | Calculated (CY-01) |
| Strong wind levels | Force 8 "Gale" from 34 knots (62.968 km/h); force 10 "Storm" from 48 knots (88.896 km/h) | Met Office Beaufort scale |

On screen and on slides, say "up to". The team's wording: "Assumes a new, fully charged battery. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less."

## 6. Our data, for the "real dataset" question

| Figure | Value |
|---|---|
| Official sources behind the figures | 10: CEB tariffs page, CEB 2023 tariff PDF, General Notice 1804 of 2022, General Notice 473 of 2026, Cabinet decisions of 10 April 2026, Statistics Mauritius 2025, CEB solar leaflet, CEB solar price guide, CEB SSDG Grid Code, PVGIS, VAT Act |
| Other named, checkable sources (cyclone mode) | Courts Mammouth (fridge label), Virginia Tech BSE-301NP, TP-Link Archer C6 spec, US FTC "Shopping for Light Bulbs", Huawei LUNA2000 and SUN2000 specs, Met Office Beaufort scale, label photo of a team member's Apple 35W charger (Model A2676) |
| NASA POWER | Kept in `data/` as a cross-check of PVGIS only. Not used in any calculation |
| Rows of real data in the database | 60 tariff bands, 7 minimum charges, 2 meter rents, 15 solar scheme values, 12 system prices, 12 PVGIS months, 2 prosumer tariffs with 10 bands and 4 minimum charges |
| Cyclone values read straight from `data/seed` | 7 appliances, 10 reference values (battery, inverter efficiency, Beaufort wind levels) |
| Expected results used to test the app | 40 bills, 7 solar cases, 11 cyclone cases, each worked out separately from the app's code |

## Ready answers for judges

- **"Is this real data?"** Yes. Every number comes from a named, checkable source: the bill and solar figures from CEB, the Government Gazette, Statistics Mauritius and PVGIS; the cyclone figures from appliance labels and maker specifications. Each file's source and checks are in `data/sources/README.md`.
- **"How do you know the bills are right?"** 40 expected bills, each with its working written out, all match the app's calculation. 200 kWh gives Rs 1,216.75.
- **"What happens when CEB changes the tariff again?"** Add the new rates to `tariff_tiers.csv` with the new start date. The app picks the newest rates automatically, and no code changes.
- **"Why 113.71 kWh?"** PVGIS, the European Commission's solar model, using 2005 to 2023 satellite data for Mauritius with standard panel losses.
- **"Why is the bill not rounded?"** The app shows the exact consumption charge. CEB rounds the final bill to the nearest rupee.
- **"Why doesn't solar bring the bill to zero?"** A solar home moves to tariff 150A, which keeps a minimum charge set by total use: Rs 211.60 for 76 to 300 kWh (General Notice 473 of 2026, Appendix VIII). The export credit is shown separately.
- **"Why 4 days on the battery?"** CEB prices every system with a 5 kWh battery. Huawei's inverter delivers 97.8% of it, 4.89 kWh. A fridge, Wi-Fi and one LED light use 1.19 kWh a day. We say "up to", because real batteries and small loads lose more.
- **"Why does solar switch off in a cyclone?"** CEB's grid code, section 2.5.1: solar without a battery must disconnect within 0.2 seconds of a power cut. With a battery it can keep running (section 2.5.2).
