using Microsoft.EntityFrameworkCore;
using PowerCheck.Api.Data;

// Endpoint for the solar estimate (the main feature).
// Uses the real CEB tariff and 2026 solar scheme from the database.

public static class SolarEndpoints
{
    public static void MapSolarEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // POST /api/solar/estimate -> send kWh (+ optional size), get without vs with solar
        api.MapPost("/solar/estimate", (SolarRequest request, AppDbContext db) =>
        {
            var data = new TariffData(db);
            var scheme = data.GetSolarScheme();
            var sizes = scheme.SystemPrices.Select(p => p.SystemKwp).OrderBy(s => s).ToList();

            // 1. Check the request
            var errors = new Dictionary<string, string[]>();

            if (request.MonthlyKwh < 0 || request.MonthlyKwh > 5000)
                errors["monthlyKwh"] = new[] { "monthlyKwh must be between 0 and 5000." };

            if (request.SystemSizeKwp is not null && !sizes.Contains(request.SystemSizeKwp.Value))
                errors["systemSizeKwp"] = new[] { $"systemSizeKwp must be one of the CEB sizes: {string.Join(", ", sizes)}." };

            if (!data.IsKnownTariff(request.TariffCode))
                errors["tariffCode"] = new[] { $"Unknown tariffCode '{request.TariffCode}'. Known codes: {string.Join(", ", data.TariffCodes())}." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Get the real tariff in force today
            var tariff = data.GetTariff(request.TariffCode);
            if (tariff is null)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["tariffCode"] = new[] { $"Tariff '{request.TariffCode}' has no prices in force today." }
                });

            // 3. The CST charge rates (net metering), from the scheme table
            var p = db.SolarSchemeParameters.AsNoTracking().ToDictionary(x => x.Parameter, x => x.Value);
            var cst = new CstRates(
                p["cst_rate_up_to_500_kwh"],
                p["cst_rate_501_to_1000_kwh"],
                p["cst_rate_above_1000_kwh"]);

            // 4. Use the smallest CEB size that covers the usage if the user didn't choose one
            var size = request.SystemSizeKwp ?? SolarCalculator.DefaultSize(request.MonthlyKwh, scheme);

            // 5. Do the maths and send it back
            var result = SolarCalculator.Calculate(request.MonthlyKwh, size, tariff, scheme, cst);
            return Results.Ok(result);
        });
    }
}