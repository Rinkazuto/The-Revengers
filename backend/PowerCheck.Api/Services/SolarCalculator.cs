// The solar maths: compares the bill without solar vs with solar,
// using the real CEB tariff and the real 2026 household solar scheme (from the database).

// CEB's CST charge on energy offset by solar under net metering, by monthly consumption.
public record CstRates(decimal UpTo500Kwh, decimal From501To1000Kwh, decimal Above1000Kwh);

public static class SolarCalculator
{
    private const int ProjectionYears = 20;   // how many years the graph shows
    private const int MaxPaybackYears = 30;   // after this we say "doesn't pay back"

    public static SolarResponse Calculate(
        decimal monthlyKwh, decimal systemSizeKwp,
        TariffSnapshot tariff, SolarSchemeSnapshot scheme, CstRates cst)
    {
        // 1. Bill WITHOUT solar (same maths as /api/bill/calculate, so the numbers always match)
        var billWithout = Bill(monthlyKwh, tariff);

        // 2. Cost of the system: middle of CEB's price range, minus the grant, plus the processing fee
        var price = scheme.SystemPrices.Single(p => p.SystemKwp == systemSizeKwp);
        var systemPrice = (price.PriceMinRs + price.PriceMaxRs) / 2;
        var grant = Math.Min(systemPrice * scheme.GrantSharePercent / 100, scheme.GrantCapRs);
        var installCost = BillCalculator.Round(systemPrice - grant + scheme.ProcessingFeeRs);

        // 3. Year 1, month by month (the sun is stronger in summer, weaker in winter)
        var year1 = SimulateYear(monthlyKwh, systemSizeKwp, 1, tariff, scheme, cst);
        var monthlyBillWith = BillCalculator.Round(year1.BillWithSolarRs / 12);
        var monthlySavings = BillCalculator.Round(billWithout - monthlyBillWith);
        var yearlySavings = BillCalculator.Round(billWithout * 12 - year1.BillWithSolarRs);

        // Average month, for the "with solar" box on screen
        var avgGeneration = BillCalculator.Round(systemSizeKwp * scheme.AverageKwhPerKwpPerMonth);
        var avgFromGrid = BillCalculator.Round(year1.KwhFromGrid / 12);

        // 4. Year-by-year savings (panels lose a little output every year), for the graph and payback
        var projection = new List<ProjectionYearDto>();
        decimal? paybackYears = null;
        decimal saved = 0;

        for (var year = 1; year <= MaxPaybackYears; year++)
        {
            var yearSim = year == 1 ? year1 : SimulateYear(monthlyKwh, systemSizeKwp, year, tariff, scheme, cst);
            var savedThisYear = billWithout * 12 - yearSim.BillWithSolarRs;
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

        // 5. Explain where every number comes from (shown on screen)
        var cstRate = CstRateFor(monthlyKwh, cst);
        var assumptions = new List<string>
        {
            $"Solar output: {scheme.AverageKwhPerKwpPerMonth} kWh per kWp per month on average, higher in summer and lower in winter (PVGIS, European Commission JRC).",
            $"System price: Rs {price.PriceMinRs:N0} to Rs {price.PriceMaxRs:N0} for {systemSizeKwp:0.##} kWp with a {price.BatteryKwh:0.##} kWh battery (CEB estimate, indicative only). We use the middle: Rs {systemPrice:N0}.",
            $"Government grant: {scheme.GrantSharePercent:0.##}% of the cost, up to Rs {scheme.GrantCapRs:N0} (eligible households). Processing fee: Rs {scheme.ProcessingFeeRs:N0}.",
            $"Net metering: extra solar energy is credited at Rs {scheme.NetMeteringExcessExportRateRs:N2} per kWh. CST charge at your usage: Rs {cstRate:N2} per kWh of energy offset.",
            $"Panels lose {scheme.AnnualPanelDegradationPercent:0.##}% of their output each year (CEB estimate).",
            $"Bills use {tariff.Source}.",
        };

        return new SolarResponse(
            monthlyKwh,
            systemSizeKwp,
            new WithoutSolarDto(billWithout),
            new WithSolarDto(avgGeneration, avgFromGrid, monthlyBillWith),
            monthlySavings,
            yearlySavings,
            installCost,
            paybackYears,
            scheme.Source,
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

    // One year with solar, month by month
    private static (decimal BillWithSolarRs, decimal KwhFromGrid) SimulateYear(
        decimal monthlyKwh, decimal sizeKwp, int year,
        TariffSnapshot tariff, SolarSchemeSnapshot scheme, CstRates cst)
    {
        // Output left after panel ageing, e.g. 0.5% less each year
        var ageFactor = (decimal)Math.Pow(1 - (double)scheme.AnnualPanelDegradationPercent / 100, year - 1);
        var cstRate = CstRateFor(monthlyKwh, cst);

        decimal yearBill = 0, yearFromGrid = 0;

        foreach (var month in scheme.MonthlyKwhPerKwp)
        {
            var generation = sizeKwp * month.KwhPerKwp * ageFactor;

            var fromGrid = Math.Max(0, monthlyKwh - generation);      // still bought from CEB
            var exported = Math.Max(0, generation - monthlyKwh);      // extra sent to CEB
            var offset = Math.Min(monthlyKwh, generation);            // usage covered by solar

            var bill = Bill(fromGrid, tariff)
                       + offset * cstRate
                       - exported * scheme.NetMeteringExcessExportRateRs;

            yearBill += BillCalculator.Round(Math.Max(0, bill));      // a bill is never below zero
            yearFromGrid += fromGrid;
        }

        return (yearBill, yearFromGrid);
    }

    private static decimal Bill(decimal kwh, TariffSnapshot tariff) =>
        BillCalculator.Calculate(kwh, tariff.Bands, tariff.MinimumChargeRs, tariff.Source).TotalRs;

    private static decimal CstRateFor(decimal monthlyKwh, CstRates cst) =>
        monthlyKwh <= 500 ? cst.UpTo500Kwh
        : monthlyKwh <= 1000 ? cst.From501To1000Kwh
        : cst.Above1000Kwh;
}