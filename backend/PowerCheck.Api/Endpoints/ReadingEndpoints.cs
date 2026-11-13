using System.Globalization;

// Endpoints for saving and listing monthly readings.

public static class ReadingEndpoints
{
    public static void MapReadingEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // POST /api/readings -> save one month's usage
        api.MapPost("/readings", (ReadingRequest request) =>
        {
            // 1. Check the request
            var errors = new Dictionary<string, string[]>();

            // Month must look like "2026-10" and must not be in the future
            var isValidMonth = DateTime.TryParseExact(request.Month, "yyyy-MM",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate);

            if (!isValidMonth)
                errors["month"] = new[] { "month must look like YYYY-MM, e.g. 2026-10." };
            else if (monthDate > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1))
                errors["month"] = new[] { "month cannot be in the future." };

            if (request.Kwh < 0 || request.Kwh > 5000)
                errors["kwh"] = new[] { "kwh must be between 0 and 5000." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Save it (new month, or overwrite an existing one)
            var (reading, isNew) = ReadingStore.Save(request.Month, request.Kwh);
            var dto = ToDto(reading);

            // 3. 201 = new month created, 200 = existing month overwritten
            return isNew
                ? Results.Created($"/api/readings/{dto.Id}", dto)
                : Results.Ok(dto);
        });

        // GET /api/readings -> all saved months + trend
        api.MapGet("/readings", () =>
        {
            var readings = ReadingStore.GetAll().Select(ToDto).ToList();
            return Results.Ok(new ReadingsResponse(readings, GetTrend(readings)));
        });
    }

    // Turn a stored reading into what we send back, with its bill worked out
    private static ReadingDto ToDto(ReadingStore.StoredReading r)
    {
        var bill = BillCalculator.Calculate(
            r.Kwh, FakeTariffData.Bands, FakeTariffData.MinimumChargeRs, FakeTariffData.Source);

        return new ReadingDto(r.Id, r.Month, r.Kwh, bill.TotalRs);
    }

    // Compare the last two months: "up", "down", "flat", or null if fewer than 2
    private static string? GetTrend(List<ReadingDto> readings)
    {
        if (readings.Count < 2) return null;

        var previous = readings[^2].Kwh;
        var latest = readings[^1].Kwh;

        if (latest > previous) return "up";
        if (latest < previous) return "down";
        return "flat";
    }
}