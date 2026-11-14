// The solar maths: compares the bill without solar vs with solar.
// Without solar: the home's normal CEB tariff (e.g. 110).
// With solar: the home moves to the prosumer tariff 150A (single-phase) or 150B (three-phase),
// General Notice 473 of 2026, Appendix VIII. All figures come from the database (data/seed).

// CEB's CST charge on energy covered by solar, from the solar scheme leaflet (pending URA approval).
public record CstRates(decimal UpTo500Kwh, decimal From501To1000Kwh, decimal Above1000Kwh);

public static class SolarCalculator
{
    private const int ProjectionYears = 20;   // how many years the graph shows (the 20-year CEB contract)
    private const int MaxPaybackYears = 30;   // after this we say "doesn't pay back"

    public static SolarResponse Calculate(
        decimal monthlyKwh, decimal systemSizeKwp,
        TariffSnapshot tariff, ProsumerTariffSnapshot prosumer,
        SolarSchemeSnapshot scheme, CstRates cst)
    {
        // 1. Bill WITHOUT solar (same maths as /api/bill/calculate, so the numbers always match)
        var billWithout = BillCalculator.Calculate(
            monthlyKwh, tariff.Bands, tariff.MinimumChargeRs, tariff.Source).TotalRs;

        // 2. Cost of the system: middle of CEB's price range, minus the grant, plus the processing fee
        var price = scheme.SystemPrices.Single(p => p.SystemKwp == systemSizeKwp);
        var systemPrice = (price.PriceMinRs + price.PriceMaxRs) / 2;
        var grant = Math.Min(systemPrice * scheme.GrantSharePercent / 100, scheme.GrantCapRs);
        var installCost = BillCalculator.Round(systemPrice - grant + scheme.ProcessingFeeRs);

        // 3. Year 1, month by month (the sun is stronger in summer, weaker in winter)
        var year1 = SimulateYear(monthlyKwh, systemSizeKwp, 1, prosumer, scheme, cst);
        var monthlyBillWith = BillCalculator.Round(year1.BillRs / 12);
        var monthlyExportCredit = BillCalculator.Round(year1.ExportCreditRs / 12);
        var monthlySavings = BillCalculator.Round(billWithout - monthlyBillWith);
        var yearlySavings = BillCalculator.Round(billWithout * 12 - year1.BillRs);

        // Average month, for the "with solar" box on screen
        var avgGeneration = BillCalculator.Round(systemSizeKwp * scheme.AverageKwhPerKwpPerMonth);
        var avgFromGrid = BillCalculator.Round(year1.KwhFromGrid / 12);

        // 4. Year-by-year savings (panels lose a little output every year), for the graph and payback.
        //    Savings never include the export credit, so they are on the safe side.
        var projection = new List<ProjectionYearDto>();
        decimal? paybackYears = null;
        decimal saved = 0;

        for (var year = 1; year <= MaxPaybackYears; year++)
        {
            var yearSim = year == 1 ? year1 : SimulateYear(monthlyKwh, systemSizeKwp, year, prosumer, scheme, cst);
            var savedThisYear = billWithout * 12 - yearSim.BillRs;
            var savedBefore = saved;
            saved += savedThisYear;

            // Payback = the moment total savings reach the install cost
            if (paybackYears is null && savedThisYear > 0 && saved >= installCost)
            {
                var partOfYear = (installCost - savedBefore) / savedThisYear;
                paybackYears = Math.Round(year - 1 + partOfYear, 1, MidpointRounding.AwayFromZero);
            }

            if (year <= ProjectionYears)
            {
                var total = BillCalculator.Round(saved);
                projection.Add(new ProjectionYearDto(year, total, BillCalculator.Round(total - installCost)));
            }
        }

        // 5. Explain every number and its source (shown on screen)
        var energyRule = prosumer.UsesFlatRate(monthlyKwh)
            ? $"Net energy at your use (up to {prosumer.FlatRateMaxTotalKwh:0} kWh total): flat Rs {prosumer.FlatRateRsPerKwh:N2} per kWh."
            : $"Net energy goes through the prosumer bands (Rs {prosumer.Bands.First().PricePerKwh:N2} to Rs {prosumer.Bands.Last().PricePerKwh:N2} per kWh).";

        var assumptions = new List<string>
        {
            $"Solar output: {scheme.AverageKwhPerKwpPerMonth} kWh per kWp per month on average, higher in summer and lower in winter (PVGIS, European Commission JRC).",
            $"System price: Rs {price.PriceMinRs:N0} to Rs {price.PriceMaxRs:N0} for {systemSizeKwp:0.##} kWp with a {price.BatteryKwh:0.##} kWh battery (CEB estimate, indicative only). We use the middle: Rs {systemPrice:N0}.",
            $"Government grant: {scheme.GrantSharePercent:0.##}% of the cost, up to Rs {scheme.GrantCapRs:N0} (eligible households). Processing fee: Rs {scheme.ProcessingFeeRs:N0}.",
            $"With solar you move to prosumer tariff {prosumer.TariffCode} ({prosumer.Phase}-phase), GN 473/2026. {energyRule} Minimum charge at your total use: Rs {prosumer.MinimumChargeFor(monthlyKwh):N2}.",
        };

        var cstRate = CstRateFor(monthlyKwh, cst);
        if (cstRate > 0)
            assumptions.Add($"CST charge: Rs {cstRate:N2} per kWh covered by solar at your use (above 500 kWh). From the CEB solar leaflet, pending URA approval.");

        assumptions.Add($"Extra solar sent to CEB is credited at Rs {scheme.NetMeteringExcessExportRateRs:N2} per kWh. The credit is shown separately and is not counted in your savings.");
        assumptions.Add($"Panels lose {scheme.AnnualPanelDegradationPercent:0.##}% of their output each year (CEB estimate).");
        assumptions.Add($"Bill without solar uses {tariff.Source}.");

        return new SolarResponse(
            monthlyKwh,
            systemSizeKwp,
            new WithoutSolarDto(billWithout),
            new WithSolarDto(avgGeneration, avgFromGrid, monthlyBillWith, monthlyExportCredit),
            monthlySavings,
            yearlySavings,
            installCost,
            paybackYears,
            $"{scheme.Source}; {prosumer.Source}",
            assumptions,
            projection);
    }

    // The default size: the smallest CEB size that covers the usage, or the biggest one
    public static decimal DefaultSize(decimal monthlyKwh, SolarSchemeSnapshot scheme) =>
        scheme.SystemPrices
            .Select(p => p.SystemKwp)
            .OrderBy(s => s)
            .FirstOrDefault(s => s * scheme.AverageKwhPerKwpPerMonth >= monthlyKwh,
                            scheme.SystemPrices.Max(p => p.SystemKwp));

    // One year with solar on the prosumer tariff, month by month
    private static (decimal BillRs, decimal KwhFromGrid, decimal ExportCreditRs) SimulateYear(
        decimal monthlyKwh, decimal sizeKwp, int year,
        ProsumerTariffSnapshot prosumer, SolarSchemeSnapshot scheme, CstRates cst)
    {
        // Output left after panel ageing, e.g. 0.5% less each year
        var ageFactor = (decimal)Math.Pow(1 - (double)scheme.AnnualPanelDegradationPercent / 100, year - 1);

        var totalUse = monthlyKwh;                          // C = P + I − E (CEB leaflet p.3)
        var minimum = prosumer.MinimumChargeFor(totalUse);  // picked by TOTAL use
        var cstRate = CstRateFor(totalUse, cst);            // 0 at 500 kWh or less

        decimal yearBill = 0, yearFromGrid = 0, yearCredit = 0;

        foreach (var month in scheme.MonthlyKwhPerKwp)
        {
            var production = sizeKwp * month.KwhPerKwp * ageFactor;

            var netEnergy = Math.Max(0, totalUse - production);     // still bought from CEB
            var exported = Math.Max(0, production - totalUse);      // extra sent to CEB
            var coveredBySolar = Math.Min(totalUse, production);    // used for CST only

            // Energy charge: flat rate (150A, total use up to 300 kWh) or the prosumer bands from 0 kWh
            var energyCharge = prosumer.UsesFlatRate(totalUse)
                ? netEnergy * prosumer.FlatRateRsPerKwh!.Value
                : BillCalculator.Calculate(netEnergy, prosumer.Bands, 0, prosumer.Source).TotalRs;

            var bill = energyCharge + coveredBySolar * cstRate;

            yearBill += BillCalculator.Round(Math.Max(bill, minimum));            // never below the minimum
            yearFromGrid += netEnergy;
            yearCredit += BillCalculator.Round(exported * scheme.NetMeteringExcessExportRateRs);  // shown separately
        }

        return (yearBill, yearFromGrid, yearCredit);
    }

    private static decimal CstRateFor(decimal monthlyKwh, CstRates cst) =>
        monthlyKwh <= 500 ? cst.UpTo500Kwh
        : monthlyKwh <= 1000 ? cst.From501To1000Kwh
        : cst.Above1000Kwh;
}