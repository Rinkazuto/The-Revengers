# Q&A prep: ReVolt pitch

The pitch deck is limited to 6 slides (plus the title), so the backup answers live here and in the speaker notes of the last slide. Rehearse these before stage day.

**Rule:** decide now who answers each topic, so nobody hesitates or talks over each other. If you can't answer: "Good question, we haven't solved that yet. Here's how we'd approach it."

| Topic | Who answers | Answer |
|---|---|---|
| What's real and what's mocked? | [Name] | The Solar check runs on real data: CEB 2026 system prices, the 25% grant (max Rs 75,000), the Rs 2,000 fee, monthly PVGIS sun data for Mauritius and 0.5% panel ageing a year. [Add the Bill calculator and My readings once the developers confirm they use the final data too.] |
| How do you work out the solar savings? | [Name] | We take the household's kWh, pick the right CEB system size (or use the one they choose), estimate monthly solar output from PVGIS, and compare the bill with and without solar. The cost is CEB's price guide minus the 25% grant, plus the Rs 2,000 fee. Every assumption is listed on screen under the result. |
| Why is your payback longer than CEB's calculator? | [Name] | Two reasons. (1) A solar home moves to CEB tariff 150A, which keeps a minimum charge (Rs 211.60 a month for 76–300 kWh); CEB's calculator ignores it and even showed a negative bill (Rs −388.40) for 200 kWh on 3 kWp. (2) CEB assumes about 400 kWh a month from 3 kWp; we use PVGIS data for Mauritius, about 341 kWh. We'd rather be conservative than promise too much. (CEB's prices leave out the grant; ours include it.) |
| Can solar make my bill zero? | [Name] | No. On tariff 150A there's always a minimum charge based on how much you use in total: Rs 50.60 (up to 75 kWh), Rs 211.60 (76–300 kWh) or Rs 424.35 (over 300 kWh). For a 200 kWh home, the app recommends 2 kWp and the bill drops from Rs 1,216.75 to Rs 211.60, saving Rs 1,005.15 a month. Source: GN 473/2026, Appendix VIII. |
| Isn't 13 years a long payback? | [Name] | It's inside the 20-year CEB solar contract, so the family keeps saving for about 7 more years after paying it back, and they save Rs 1,005.15 every month from day one (Rs 12,061.80 in the first year). It's the honest figure: it includes CEB's minimum charge on tariff 150A, which CEB's own calculator leaves out. The app also picks the smallest system that covers the home's usage, 2 kWp here, which pays back faster than a bigger one (3 kWp would take 16 years for the same saving). |
| Cyclone mode: will my solar keep the lights on in a cyclone? | [Name] | Only with a battery. Grid-tied rooftop solar switches off when the CEB grid goes down (CEB SSDG Grid Code, April 2026, s. 2.5). Cyclone mode shows how long a battery keeps the essentials on: with the standard 5 kWh battery, a fridge, Wi-Fi and 6 hours of LED light last **up to about 4 days** (98.3 hours). It assumes a new, fully charged battery and no solar charging during the storm. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less. |
| Where do the cyclone numbers come from? | [Name] | Fridge 311 kWh a year from the Samsung energy label (Courts Mammouth); LED 9 W (US FTC); Wi-Fi router 12 W (TP-Link label); TV, laptop and fan from Virginia Tech's appliance guide; phone charger 35 W from a team member's Apple charger label; battery 5 kWh (CEB price guide) with 97.8% reaching the appliances (Huawei SUN2000 inverter efficiency). Full list in `data/seed/cyclone_appliances.csv`. |
| Is your cyclone screen an official warning? | [Name] | No. It uses the free OpenWeather wind forecast only to give battery advice ("charge your battery now"). For official cyclone warnings, the screen tells people to check the Mauritius Meteorological Services (MMS). If OpenWeather doesn't answer, it shows the last real forecast, labelled "offline". |
| How would it scale? | [Name] | [Confirm the database with the developers first, then: how the data and API would move to the cloud.] |
| Is the data safe and private? | [Name] | No login and no personal details. We store only the month and kWh of each reading. |
| How did you build it in 24 hours? | [Name] | [Who did what, how you split the work, and what you chose to cut.] |
| Doesn't this already exist? | [Name] | The closest tool is CEB's online solar calculator. It's good if you already have a quote: you type in the system size and price, and it shows your bill, savings and payback. ReVolt is for the step before that: it explains your bill band by band, picks a system size and price for you, includes the grant, and tracks your usage month after month. |
| Does solar keep the lights on during power cuts? | [Name] | Only with a battery. A grid-connected system without a battery switches off during a cut, for safety (CEB SSDG Grid Code, April 2026, s. 2.5). The CEB price guide quotes systems with a 5 kWh battery, and the 2026 scheme only requires a battery above 3.5 kW, so many small systems have none. That's exactly why we built Cyclone mode. |
| Where do your numbers come from? | [Name] | CEB tariffs 2026 (ceb.mu/tariffs), the CEB Household Rooftop Solar PV Scheme leaflet (July 2026), Statistics Mauritius Energy and Water Statistics 2025, PVGIS and NASA POWER. Full list in `data/sources/README.md`. |

## Team roles

| Role | Name |
|---|---|
| Backend Lead | |
| Data Lead | |
| Frontend Lead | |
| Design / QA / Pitch | Laura Isabelle |
| Integrator | |
