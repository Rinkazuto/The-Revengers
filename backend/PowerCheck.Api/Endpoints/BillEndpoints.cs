// Endpoints for tariffs and the bill calculator.

public static class BillEndpoints
{
    public static void MapBillEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");   // every address here starts with /api

        // GET /api/tariffs?tariffCode=110 -> the CEB price bands
        api.MapGet("/tariffs", (string? tariffCode) =>
        {
            if (!FakeTariffData.IsKnownTariff(tariffCode))
                return Results.NotFound($"Unknown tariffCode '{tariffCode}'");

            var response = new TariffsResponse(
                FakeTariffData.DefaultTariffCode,
                FakeTariffData.Source,
                FakeTariffData.SourceUrl,
                FakeTariffData.Bands,
                FakeTariffData.MinimumChargeRs);

            return Results.Ok(response);
        });

        // POST /api/bill/calculate -> send kWh, get the bill
        api.MapPost("/bill/calculate", (BillRequest request) =>
        {
            // 1. Check the request
            var errors = new Dictionary<string, string[]>();

            if (request.MonthlyKwh < 0 || request.MonthlyKwh > 5000)
                errors["monthlyKwh"] = new[] { "monthlyKwh must be between 0 and 5000." };

            if (!FakeTariffData.IsKnownTariff(request.TariffCode))
                errors["tariffCode"] = new[] { $"Unknown tariffCode '{request.TariffCode}'." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Do the maths
            var bill = BillCalculator.Calculate(
                request.MonthlyKwh,
                FakeTariffData.Bands,
                FakeTariffData.MinimumChargeRs,
                FakeTariffData.Source);

            // 3. Send it back
            return Results.Ok(bill);
        });
    }
}