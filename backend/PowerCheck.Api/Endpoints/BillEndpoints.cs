// Endpoints for tariffs and the bill calculator.

public static class BillEndpoints
{
    public static void MapBillEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");   // every address here starts with /api

        // GET /api/tariffs -> the CEB price bands
        api.MapGet("/tariffs", () =>
        {
            var response = new TariffsResponse(
                FakeTariffData.Source,
                FakeTariffData.SourceUrl,
                FakeTariffData.Bands,
                FakeTariffData.FixedChargeRs);

            return Results.Ok(response);
        });

        // POST /api/bill/calculate -> send kWh, get the bill
        api.MapPost("/bill/calculate", (BillRequest request) =>
        {
            // 1. Check the request
            if (request.MonthlyKwh < 0 || request.MonthlyKwh > 5000)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["monthlyKwh"] = new[] { "monthlyKwh must be between 0 and 5000." }
                });
            }

            // 2. Do the maths
            var bill = BillCalculator.Calculate(
                request.MonthlyKwh,
                FakeTariffData.Bands,
                FakeTariffData.FixedChargeRs,
                FakeTariffData.Source);

            // 3. Send it back
            return Results.Ok(bill);
        });
    }
}