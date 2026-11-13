# Data sources

Every figure in `data/seed/` and `tests/test-cases/` comes from an official source or is calculated from one. No figure is invented or rounded.

## Sources

| Data | Source |
|---|---|
| 2026 tariffs and 2026 minimum charges | Central Electricity Board, "Tariffs & Rates", https://ceb.mu/tariffs |
| 2023 tariffs | Central Electricity Board, "Approved Tariff by the Utility Regulatory Authority", https://ceb.mu/docs/files/files/tariffs/CEBTARIFFS.pdf |
| 2023 minimum charges | Government Gazette of Mauritius No. 154 of 17 December 2022, General Notice No. 1804 of 2022, Appendix I, pages 5005 to 5006. Copy read: https://ceb.mu/docs/files/files/tariffs/Tariff_Schedule_Feb2023_URA.pdf |
| Domestic consumers and domestic sales | Statistics Mauritius, Economic and Social Indicators Issue No 1929, Energy and Water Statistics Year 2025, Table 11, page 18, https://statsmauritius.govmu.org/Documents/Statistics/ESI/2026/EI1929/Energy_Water_Yr25_050626.pdf |
| Solar irradiance | NASA POWER daily API, latitude -20.25, longitude 57.55, parameter ALLSKY_SFC_SW_DWN |
| 2026 household solar scheme terms | Central Electricity Board, Household Rooftop Solar Photovoltaic (PV) Scheme information leaflet, revised July 2026, https://ceb.mu/docs/files/files/application%20schemes/Information%20Leaflet%20on%20Household%20Rooftop%20Solar%20Photovoltaic%20Scheme.pdf |
| Solar system prices and panel degradation | Central Electricity Board, "Solar PV Comparison" page, "Estimated price guide (with 5kWh battery)", https://ceb.mu/pv_calculator |
| Solar yield per kWp | PVGIS 5.3, European Commission Joint Research Centre, https://re.jrc.ec.europa.eu/api/v5_3/PVcalc?lat=-20.25&lon=57.55&peakpower=1&loss=14&optimalangles=1&outputformat=json |
| Meter rental rates | General Notice No. 1804 of 2022, Appendix XI, page 5019. General Notice No. 473 of 2026, paragraph 13, keeps them unchanged |
| VAT on electricity | Value Added Tax Act 1998, Fifth Schedule, item 7(1), consolidated text of 30 June 2017, https://mauritiuslii.org/akn/mu/act/1998/2/eng@2017-06-30 |
| Test cases and chart | Calculated from `tariff_tiers.csv` and `minimum_charges.csv` |

## Files

| File | Source | Date retrieved | Columns | Rows | Checks done |
|---|---|---|---|---|---|
| `data/seed/tariff_tiers.csv` | 2026 rows: CEB "Tariffs & Rates" page. 2023 rows: CEBTARIFFS.pdf. | 13 November 2026 | `tariff_code`, `effective_from`, `tier`, `from_kwh`, `to_kwh`, `rate_rs_per_kwh` | 60 (12 tiers x 5 groups: 110 from 2023-02-01, then 110, 110A, 120, 140 from 2026-05-01) | All 48 rates dated 2026-05-01 match https://ceb.mu/tariffs, read on 13 November 2026. All 12 rates dated 2023-02-01 match CEBTARIFFS.pdf (column "Effective 01 February 2023") and General Notice No. 1804 of 2022, read on 13 November 2026. The 12 band sizes match the General Notice (25, 25, 25, 25, 100, 50, 50, 200, 500, 500, 500, then all additional units). Each of the 5 groups starts at 1 kWh, has no gap between bands and ends with an open top band. The 120 and 140 rows are text copies of the 110 rows for 2026-05-01. |
| `data/seed/minimum_charges.csv` | 2026 rows: CEB "Tariffs & Rates" page. 2023 rows: General Notice No. 1804 of 2022. | 13 November 2026 | `tariff_code`, `effective_from`, `minimum_rs` | 7 | The 4 minimum charges dated 2026-05-01 (110A Rs 31.00, 110 Rs 50.60, 120 Rs 211.60, 140 Rs 424.35) match https://ceb.mu/tariffs. The 3 minimum charges dated 2023-02-01 (110 Rs 44.00, 120 Rs 184.00, 140 Rs 369.00) match General Notice No. 1804 of 2022, page 5006. Both read on 13 November 2026. |
| `data/seed/domestic_electricity_stats.csv` | Statistics Mauritius, Energy and Water Statistics Year 2025, Table 11, page 18 | 13 November 2026 | `year`, `domestic_consumers`, `domestic_sales_mwh`, `avg_kwh_per_home_per_month_derived` | 2 (2024, 2025) | All 4 published figures match the "Domestic" row of Table 11, read on 13 November 2026: 474,800 consumers and 1,085,024 MWh for 2024, 484,680 consumers and 1,172,239 MWh for 2025. The derived column was recalculated from those figures for both years. |
| `data/seed/solar_scheme_2026.csv` | 14 rows: CEB solar scheme leaflet, revised July 2026. 1 row (`annual_panel_degradation`): CEB "Solar PV Comparison" page. | 13 November 2026 | `parameter`, `value`, `unit`, `applies_to`, `source`, `source_page` | 15 | Each of the 14 leaflet values was matched to its wording on the page given in `source_page`. 14 of 14 found. `contract_term_years` matches page 8: "a contractual term of 20 years". The degradation figure matches the page text "an estimated 0.5% annual panel degradation". |
| `data/seed/solar_system_prices_2026.csv` | CEB "Solar PV Comparison" page, "Estimated price guide (with 5kWh battery)" | 13 November 2026 | `system_kwp`, `battery_kwh`, `price_min_rs`, `price_max_rs` | 12 (1.5 to 15.0 kWp) | All 12 rows match the page text. CEB labels these "Approx. Price". They exclude every government incentive. |
| `data/sources/raw/pvgis_pvcalc_-20.25_57.55_1kwp_loss14_optimal.json` | PVGIS 5.3 API, query in the Sources table | 13 November 2026 | PVGIS JSON: `inputs`, `outputs.monthly.fixed`, `outputs.totals.fixed`, `meta` | 12 monthly results plus 1 yearly total | Stored unchanged as downloaded (5,209 bytes). Inputs confirmed in the file: latitude -20.25, longitude 57.55, elevation 444.0 m, 1 kWp crystalline silicon, 14% system loss, slope 17 degrees and azimuth -173 degrees (both optimal), free-standing mounting, PVGIS-SARAH3 data 2005 to 2023. Yearly total E_y 1364.48 kWh per kWp, monthly mean E_m 113.71. |
| `data/seed/pvgis_monthly_yield_1kwp.csv` | The raw PVGIS file above | 13 November 2026 | `month`, `e_m_kwh_per_kwp`, `h_i_m_kwh_per_m2`, `sd_m_kwh` | 12 | Every value is the same text as in the raw file. The 12 monthly E_m values average 113.7075, which PVGIS reports as 113.71. |
| `data/seed/meter_rentals.csv` | General Notice No. 1804 of 2022, Appendix XI, "Meter rental rates applicable per month or part thereof" | 13 November 2026 | `consumer_type`, `meter_type`, `rent_rs_per_month`, `effective_from` | 2 (domestic single-phase Rs 20.00, domestic three-phase Rs 120.00) | Both rates read from the scanned Gazette page 5019. General Notice No. 473 of 2026, paragraph 13, says meter rental "shall remain unchanged from those previously approved". The 10 commercial, industrial and irrigation rates on the same page are left out. |
| `data/seed/chart_bill_vs_kwh.csv` | Calculated from `tariff_tiers.csv` and `minimum_charges.csv` (tariff 110, 2023-02-01 and 2026-05-01) | 13 November 2026 (calculated) | `kwh`, `bill_2023_rs`, `bill_2026_rs`, `increase_rs` | 101 (0 to 1000 kWh in steps of 10) | All 101 rows recalculated with exact decimal arithmetic. 0 mismatches. |
| `tests/test-cases/bill_test_cases.csv` | Calculated from `tariff_tiers.csv` and `minimum_charges.csv` | 13 November 2026 (calculated) | `tariff_code`, `effective_from`, `kwh`, `expected_rs`, `minimum_applied`, `working` | 40 (12 each for 110 from 2023, 110 from 2026 and 110A from 2026, then 2 each for 120 and 140 from 2026) | All 40 expected bills, all 40 `minimum_applied` flags and all 40 `working` strings recalculated with exact decimal arithmetic. 0 mismatches. |
| `data/sources/raw/POWER_Point_Daily_20230101_20251231_020d25S_057d55E_LST.csv` | NASA POWER daily API, latitude -20.25, longitude 57.55, parameter ALLSKY_SFC_SW_DWN | 13 November 2026 | 9 header lines, then `YEAR`, `MO`, `DY`, `ALLSKY_SFC_SW_DWN` (kWh per square metre per day) | 1,096 (1 January 2023 to 31 December 2025) | Stored unchanged, byte for byte identical to the downloaded file. 0 missing values (NASA marks missing data as -999). 0 duplicate dates. |
| `data/seed/nasa_power_daily_irradiance_2023_2025.csv` | The raw NASA POWER file above | 13 November 2026 | `date` (YYYY-MM-DD), `allsky_sfc_sw_dwn_kwh_per_m2_per_day` | 1,096 | Every irradiance value is the same text as in the raw file. The dates run from 2023-01-01 to 2025-12-31 with no missing day. The 9 header lines are removed and `YEAR`, `MO`, `DY` are joined into one `date` column. |

## Notes

**Average household consumption is derived.** Statistics Mauritius publishes the number of domestic consumers and the domestic sales in MWh. The average is calculated as:

```
sales (MWh) x 1000 / consumers / 12 = kWh per home per month
```

For 2025: 1,172,239 x 1000 / 484,680 / 12 = 201.5486 unrounded. The CSV column shows 201.5. For 2024 the same formula gives 190.4353 to 4 decimal places. The CSV column shows 190.4.

**Tariffs 120 and 140 share the tariff 110 rates from 1 May 2026.** The CEB page prints one column headed "Tariff 110/120/140 (Rs)" for the 12 consumption tiers. The three tariffs differ only in their minimum monthly charge (Rs 50.60, Rs 211.60, Rs 424.35). The file has no 2023 rows for tariffs 120 and 140. General Notice No. 1804 of 2022 gives tariffs 110, 120 and 140 one shared rate table from 1 February 2023 as well.

**Tariff 110A.** General Notice No. 1804 of 2022 lists tariff 110A from 1 February 2023 with the same 12 rates and the same Rs 31.00 minimum charge that the CEB page shows from 1 May 2026. The file holds tariff 110A rows dated 2026-05-01 only.

**Band labels.** CEBTARIFFS.pdf labels the first two bands "0 to 24" and "25 to 50". General Notice No. 1804 of 2022 says "Initial 25 kilowatt hours" then "Next 25 kilowatt hours". The CSV follows the General Notice: 1 to 25, then 26 to 50.

**Rounding of bills.** General Notice No. 1804 of 2022, paragraph 14, says "All Bills shall be rounded to the nearest rupee." General Notice No. 473 of 2026 (Government Gazette of 30 April 2026, page 2213), paragraph 18, says "All bills shall be rounded to the nearest rupee." Paragraph 20 of the same notice sets the start date of 1 May 2026. Copy read on 13 November 2026: https://ceb.mu/docs/files/files/publications/regulations/electricity_tariff_2026.pdf. The expected bills in the test cases and the chart are the unrounded totals, in rupees and cents.

**Web addresses.** https://ceb.mu/customer-corner/customer-information showed the same 2026 tariff table earlier on 13 November 2026. Later that day it returned "404 File or directory not found", and the table was read at https://ceb.mu/tariffs. Two other addresses returned 404 on 13 November 2026: https://ceb.mu/files/files/tariffs/CEBTARIFFS.pdf and https://uramauritius.mu/wp-content/uploads/2022/12/154_Main_171222_URA.pdf. The same CEB file was read at the `/docs/files/files/tariffs/` address in the Sources table. The Gazette was read from the scanned copy on the CEB site, also in the Sources table.

**Solar prices are CEB estimates.** The CEB page calls the price guide "purely indicative" and says final quotations need a site survey. The app should show the price range and label it as a CEB estimate.

**Two different solar incentives.** The leaflet describes a Government grant of 25% up to Rs 75,000, with terms set by the Development Bank of Mauritius. The CEB comparison page mentions "the 15% cash back scheme administered by the Mauritius Revenue Authority (MRA)". `solar_scheme_2026.csv` holds the leaflet grant only.

**PVGIS yield and the NASA figure differ.** PVGIS gives 113.71 kWh per kWp per month for 1 kWp. Its output divided by the sun reaching the panel (1364.48 / 1746.29) is 0.7814, which is the performance ratio PVGIS models. The NASA irradiance times an assumed ratio of 0.75 gives 128.4661 kWh. PVGIS placed the point at 444 m elevation, inland. PVGIS models the panels as free-standing, which cools them better than panels fixed flat to a roof.

**VAT on domestic bills is 0%.** Section 11(2) of the Value Added Tax Act makes supplies in the Fifth Schedule zero-rated. Fifth Schedule item 7(1) lists "Electricity supplied by the Central Electricity Board and the renting out of a meter". The text read is the consolidated version of 30 June 2017. The MRA says the Finance Act 2026 amended the Fifth Schedule, and the amended item was not read.

**How a bill is calculated.** Each band of units is charged at its own rate, and the band charges are added. If the total is below the minimum charge for that tariff, the bill is the minimum charge.

**Line endings.** All CSV files use Windows line endings (CRLF), as in the original files.
