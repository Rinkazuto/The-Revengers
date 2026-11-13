using PowerCheck.Api.Data;

// Endpoints for tariffs and the bill calculator.
// Prices come from the real CEB data in the database (TariffData, by the Data Lead).

public static class BillEndpoints
{
    public static void MapBillEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");   // every address here starts with /api

        // GET /api/tariffs?tariffCode=110 -> the CEB price bands in force today
        api.MapGet("/tariffs", (string? tariffCode, AppDbContext db) =>
        {
            var data = new TariffData(db);

            if (!data.IsKnownTariff(tariffCode))
                return Results.NotFound($"Unknown tariffCode '{tariffCode}'. Known codes: {string.Join(", ", data.TariffCodes())}");

            var tariff = data.GetTariff(tariffCode);
            if (tariff is null)
                return Results.NotFound($"Tariff '{tariffCode}' has no prices in force today.");

            var response = new TariffsResponse(
                tariff.TariffCode,
                tariff.Source,
                tariff.SourceUrl,
                tariff.Bands,
                tariff.MinimumChargeRs);

            return Results.Ok(response);
        });

        // POST /api/bill/calculate -> send kWh, get the bill
        api.MapPost("/bill/calculate", (BillRequest request, AppDbContext db) =>
        {
            var data = new TariffData(db);

            // 1. Check the request
            var errors = new Dictionary<string, string[]>();

            if (request.MonthlyKwh < 0 || request.MonthlyKwh > 5000)
                errors["monthlyKwh"] = new[] { "monthlyKwh must be between 0 and 5000." };

            if (!data.IsKnownTariff(request.TariffCode))
                errors["tariffCode"] = new[] { $"Unknown tariffCode '{request.TariffCode}'. Known codes: {string.Join(", ", data.TariffCodes())}." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Get the real prices in force today
            var tariff = data.GetTariff(request.TariffCode);
            if (tariff is null)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["tariffCode"] = new[] { $"Tariff '{request.TariffCode}' has no prices in force today." }
                });

            // 3. Do the maths and send it back
            var bill = BillCalculator.Calculate(
                request.MonthlyKwh,
                tariff.Bands,
                tariff.MinimumChargeRs,
                tariff.Source);

            return Results.Ok(bill);
        });
    }
}