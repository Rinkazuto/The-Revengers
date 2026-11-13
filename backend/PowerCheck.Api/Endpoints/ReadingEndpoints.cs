using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PowerCheck.Api.Data;
using PowerCheck.Api.Models;

// Endpoints for saving and listing monthly readings.
// Readings are saved in the database (Readings table), so they survive restarts.

public static class ReadingEndpoints
{
    public static void MapReadingEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // POST /api/readings -> save one month's usage
        api.MapPost("/readings", (ReadingRequest request, AppDbContext db) =>
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

            // 2. Save it: overwrite the month if it already exists (one reading per month)
            var reading = db.Readings.FirstOrDefault(r => r.Month == request.Month);
            var isNew = reading is null;

            if (isNew)
            {
                reading = new Reading { Month = request.Month };
                db.Readings.Add(reading);
            }

            reading!.Kwh = request.Kwh;
            reading.UpdatedAtUtc = DateTime.UtcNow;
            db.SaveChanges();

            // 3. 201 = new month created, 200 = existing month overwritten
            var dto = ToDto(reading, new TariffData(db));
            return isNew
                ? Results.Created($"/api/readings/{dto.Id}", dto)
                : Results.Ok(dto);
        });

        // GET /api/readings -> all saved months (oldest first) + trend
        api.MapGet("/readings", (AppDbContext db) =>
        {
            var data = new TariffData(db);

            var readings = db.Readings.AsNoTracking()
                .OrderBy(r => r.Month)              // "YYYY-MM" text sorts in date order
                .ToList()
                .Select(r => ToDto(r, data))
                .ToList();

            return Results.Ok(new ReadingsResponse(readings, GetTrend(readings)));
        });
    }

    // Turn a saved reading into what we send back, with its bill worked out.
    // Uses the CEB tariff that was in force in that month (falls back to today's if none).
    private static ReadingDto ToDto(Reading r, TariffData data)
    {
        var firstDay = DateTime.ParseExact(r.Month, "yyyy-MM", CultureInfo.InvariantCulture);
        var tariff = data.GetTariff(null, firstDay) ?? data.GetTariff();

        var billRs = tariff is null
            ? 0
            : BillCalculator.Calculate(r.Kwh, tariff.Bands, tariff.MinimumChargeRs, tariff.Source).TotalRs;

        return new ReadingDto(r.Id, r.Month, r.Kwh, billRs);
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