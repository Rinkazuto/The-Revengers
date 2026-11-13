// Endpoint for the solar estimate (the main feature).

public static class SolarEndpoints
{
    public static void MapSolarEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // POST /api/solar/estimate -> send kWh (+ optional size), get without vs with solar
        api.MapPost("/solar/estimate", (SolarRequest request) =>
        {
            // 1. Check the request
            var errors = new Dictionary<string, string[]>();

            if (request.MonthlyKwh < 0 || request.MonthlyKwh > 5000)
                errors["monthlyKwh"] = new[] { "monthlyKwh must be between 0 and 5000." };

            if (request.SystemSizeKwp is <= 0 || request.SystemSizeKwp > FakeSolarData.MaxSystemKwp)
                errors["systemSizeKwp"] = new[] { $"systemSizeKwp must be more than 0 and at most {FakeSolarData.MaxSystemKwp}." };

            if (!FakeTariffData.IsKnownTariff(request.TariffCode))
                errors["tariffCode"] = new[] { $"Unknown tariffCode '{request.TariffCode}'." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Use the default size if the user didn't choose one
            var size = request.SystemSizeKwp ?? FakeSolarData.DefaultSystemKwp;

            // 3. Do the maths and send it back
            var result = SolarCalculator.Calculate(request.MonthlyKwh, size);
            return Results.Ok(result);
        });
    }
}