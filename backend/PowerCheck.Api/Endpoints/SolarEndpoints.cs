using Microsoft.EntityFrameworkCore;
using PowerCheck.Api.Data;

// Endpoint for the solar estimate (the main feature).
// Without solar: the home's normal tariff. With solar: prosumer tariff 150A/150B (GN 473/2026).

public static class SolarEndpoints
{
    public static void MapSolarEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // POST /api/solar/estimate -> send kWh (+ optional size, tariff, phase), get without vs with solar
        api.MapPost("/solar/estimate", (SolarRequest request, AppDbContext db) =>
        {
            var data = new TariffData(db);
            var scheme = data.GetSolarScheme();
            var sizes = scheme.SystemPrices.Select(p => p.SystemKwp).OrderBy(s => s).ToList();
            var phase = request.Phase ?? "single";

            // 1. Check the request
            var errors = new Dictionary<string, string[]>();

            if (request.MonthlyKwh < 0 || request.MonthlyKwh > 5000)
                errors["monthlyKwh"] = new[] { "monthlyKwh must be between 0 and 5000." };

            if (request.SystemSizeKwp is not null && !sizes.Contains(request.SystemSizeKwp.Value))
                errors["systemSizeKwp"] = new[] { $"systemSizeKwp must be one of the CEB sizes: {string.Join(", ", sizes)}." };

            if (!data.IsKnownTariff(request.TariffCode))
                errors["tariffCode"] = new[] { $"Unknown tariffCode '{request.TariffCode}'. Known codes: {string.Join(", ", data.TariffCodes())}." };

            if (phase != "single" && phase != "three")
                errors["phase"] = new[] { "phase must be \"single\" (tariff 150A) or \"three\" (tariff 150B)." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Without solar: the home's normal tariff in force today
            var tariff = data.GetTariff(request.TariffCode);
            if (tariff is null)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["tariffCode"] = new[] { $"Tariff '{request.TariffCode}' has no prices in force today." }
                });

            // 3. With solar: the prosumer tariff (150A single-phase, 150B three-phase) in force today
            var prosumer = data.GetProsumerTariff(phase);
            if (prosumer is null)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["phase"] = new[] { $"No prosumer tariff in force today for phase '{phase}'." }
                });

            // 4. The CST charge rates, from the scheme table
            var p = db.SolarSchemeParameters.AsNoTracking().ToDictionary(x => x.Parameter, x => x.Value);
            var cst = new CstRates(
                p["cst_rate_up_to_500_kwh"],
                p["cst_rate_501_to_1000_kwh"],
                p["cst_rate_above_1000_kwh"]);

            // 5. Use the smallest CEB size that covers the usage if the user didn't choose one
            var size = request.SystemSizeKwp ?? SolarCalculator.DefaultSize(request.MonthlyKwh, scheme);

            // 6. Do the maths and send it back
            var result = SolarCalculator.Calculate(request.MonthlyKwh, size, tariff, prosumer, scheme, cst);
            return Results.Ok(result);
        });
    }
}