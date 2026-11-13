// The solar maths: compares the bill without solar vs with solar.

public static class SolarCalculator
{
    public static SolarResponse Calculate(decimal monthlyKwh, decimal systemSizeKwp)
    {
        // 1. Bill WITHOUT solar (same maths as /api/bill/calculate, so the numbers always match)
        var billWithout = BillCalculator.Calculate(
            monthlyKwh, FakeTariffData.Bands, FakeTariffData.MinimumChargeRs, FakeTariffData.Source).TotalRs;

        // 2. How much the panels produce in a month
        var generation = Math.Round(systemSizeKwp * FakeSolarData.KwhPerKwpPerMonth, 2, MidpointRounding.AwayFromZero);

        // 3. What you still need from the CEB grid, and the extra you send back
        var kwhFromGrid = Math.Max(0, monthlyKwh - generation);
        var kwhExported = Math.Max(0, generation - monthlyKwh);

        // 4. Bill WITH solar = bill for grid kWh, minus credit for exported kWh (never below 0)
        var gridBill = BillCalculator.Calculate(
            kwhFromGrid, FakeTariffData.Bands, FakeTariffData.MinimumChargeRs, FakeTariffData.Source).TotalRs;
        var billWith = BillCalculator.Round(Math.Max(0, gridBill - kwhExported * FakeSolarData.ExportCreditPerKwhRs));

        // 5. Savings, cost and payback
        var monthlySavings = BillCalculator.Round(billWithout - billWith);
        var yearlySavings = BillCalculator.Round(monthlySavings * 12);
        var installCost = BillCalculator.Round(systemSizeKwp * FakeSolarData.InstallCostPerKwpRs);

        decimal? paybackYears = yearlySavings > 0
            ? Math.Round(installCost / yearlySavings, 1, MidpointRounding.AwayFromZero)
            : null;   // solar never pays back at this usage

        // 6. Year-by-year numbers for the graph
        var projection = new List<ProjectionYearDto>();
        for (var year = 1; year <= FakeSolarData.ProjectionYears; year++)
        {
            var saved = BillCalculator.Round(yearlySavings * year);
            projection.Add(new ProjectionYearDto(year, saved, saved - installCost));
        }

        // 7. Explain where the numbers come from (shown on screen)
        var assumptions = new List<string>
        {
            $"Solar output: {FakeSolarData.KwhPerKwpPerMonth} kWh per kWp per month (PVGIS, European Commission JRC).",
            $"Install cost of Rs {FakeSolarData.InstallCostPerKwpRs:N0} per kWp",
            $"Exported kWh credited at Rs {FakeSolarData.ExportCreditPerKwhRs} each",
        };

        return new SolarResponse(
            monthlyKwh,
            systemSizeKwp,
            new WithoutSolarDto(billWithout),
            new WithSolarDto(generation, kwhFromGrid, billWith),
            monthlySavings,
            yearlySavings,
            installCost,
            paybackYears,
            FakeSolarData.Source,
            assumptions,
            projection);
    }
}