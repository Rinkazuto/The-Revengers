namespace PowerCheck.Web.Api;

// FAKE data for building the screens before the real API is ready.
// The numbers are made up. Never use them in the demo.
public class MockPowerCheckApi : IPowerCheckApi
{
    private static readonly List<TariffBandDto> Bands =
    [
        new(0, 25, 3.60m),
        new(25, 50, 5.00m),
        new(50, 100, 5.50m),
        new(100, null, 7.00m),   // null = "and above"
    ];
    private const decimal MinimumChargeRs = 50.00m;

    // 6 months, for the "Last 6 months" chart on the Readings screen
    private readonly List<ReadingDto> _readings =
    [
        new(1, "2026-05", 210, Bill(210).TotalRs),
        new(2, "2026-06", 195, Bill(195).TotalRs),
        new(3, "2026-07", 220, Bill(220).TotalRs),
        new(4, "2026-08", 230, Bill(230).TotalRs),
        new(5, "2026-09", 240, Bill(240).TotalRs),
        new(6, "2026-10", 250, Bill(250).TotalRs),
    ];

    // One shared fake for everyone, so protect the list when two phones save at once
    private readonly Lock _lock = new();

    public async Task<TariffsResponse> GetTariffsAsync(string? tariffCode = null)
    {
        await Task.Delay(400);   // pretend the network takes a moment, so loading states show
        return new TariffsResponse(tariffCode ?? "110", "FAKE data", "", Bands, MinimumChargeRs);
    }

    public async Task<BillResponse> CalculateBillAsync(BillRequest request)
    {
        await Task.Delay(500);
        return Bill(request.MonthlyKwh);
    }

    public async Task<SolarResponse> EstimateSolarAsync(SolarRequest request)
    {
        await Task.Delay(600);
        var size = request.SystemSizeKwp ?? 3m;
        var generation = Math.Round(size * 110m, 0);
        var fromGrid = Math.Max(0, request.MonthlyKwh - generation);
        var withoutSolar = Bill(request.MonthlyKwh).TotalRs;
        var withSolar = Bill(fromGrid).TotalRs;
        var monthlySavings = withoutSolar - withSolar;
        var yearlySavings = monthlySavings * 12;
        var installCost = 250_000m;
        decimal? payback = yearlySavings > 0 ? Math.Round(installCost / yearlySavings, 1) : null;
        var projection = Enumerable.Range(1, 20)
            .Select(year => new ProjectionYearDto(year, yearlySavings * year, yearlySavings * year - installCost))
            .ToList();

        return new SolarResponse(
            request.MonthlyKwh, size,
            new WithoutSolarDto(withoutSolar),
                        new WithSolarDto(generation, fromGrid, withSolar, Math.Max(0, generation - request.MonthlyKwh) * 3.00m),
            monthlySavings, yearlySavings, installCost, payback,
            "FAKE data",
            ["FAKE: 110 kWh per kWp per month", "FAKE: install cost Rs 250,000"],
            projection);
    }

    public async Task<ReadingDto> SaveReadingAsync(ReadingRequest request)
    {
        await Task.Delay(400);
        lock (_lock)
        {
            _readings.RemoveAll(r => r.Month == request.Month);   // one reading per month: overwrite
            var nextId = _readings.Count == 0 ? 1 : _readings.Max(r => r.Id) + 1;
            var saved = new ReadingDto(nextId, request.Month, request.Kwh, Bill(request.Kwh).TotalRs);
            _readings.Add(saved);
            return saved;
        }
    }

    public async Task<ReadingsResponse> GetReadingsAsync()
    {
        await Task.Delay(400);
        List<ReadingDto> sorted;
        lock (_lock)
        {
            sorted = _readings.OrderBy(r => r.Month).ToList();   // oldest → newest
        }
        string? trend = null;
        if (sorted.Count >= 2)
        {
            var before = sorted[^2].Kwh;
            var last = sorted[^1].Kwh;
            trend = last > before ? "up" : last < before ? "down" : "flat";
        }
        return new ReadingsResponse(sorted, trend);
    }

    // ---------- Cyclone (FAKE: same appliances as the seed file, but don't use in the demo) ----------
    private static readonly List<ApplianceDto> FakeAppliances =
    [
        new("fridge", "Fridge", 35.5m, "FAKE data"),
        new("tv", "TV (flat screen)", 120m, "FAKE data"),
        new("laptop", "Laptop", 50m, "FAKE data"),
        new("ceiling_fan", "Ceiling fan", 175m, "FAKE data"),
        new("wifi_router", "Wi-Fi router", 12m, "FAKE data"),
        new("phone_charger", "USB-C charger (phone or laptop)", 35m, "FAKE data"),
        new("led_light", "LED light (one bulb)", 9m, "FAKE data"),
    ];

    public async Task<AppliancesResponse> GetAppliancesAsync()
    {
        await Task.Delay(400);
        return new AppliancesResponse(FakeAppliances);
    }

    public async Task<BackupResponse> CalculateBackupAsync(BackupRequest request)
    {
        await Task.Delay(400);
        var chosen = request.Appliances ?? [];
        if (chosen.Count == 0 || chosen.Sum(a => a.HoursPerDay) == 0)
            throw new ApiException("Choose at least one appliance with more than 0 hours a day.");

        var batteryKwh = request.BatteryKwh ?? 5m;
        var usableKwh = batteryKwh * 0.8m;   // FAKE: 80% usable, like the backend
        var kwhPerDay = chosen.Sum(a => FakeAppliances.First(f => f.Id == a.Id).Watts * a.HoursPerDay / 1000);
        var hours = Math.Round(usableKwh / kwhPerDay * 24, 1);

        return new BackupResponse(
            Math.Round(kwhPerDay, 2), batteryKwh, hours,
            $"FAKE: your {batteryKwh:0.##} kWh battery keeps these running for about {hours:0.#} hours.",
            ["FAKE data: usable battery 80%"]);
    }

    // Splits the kWh across the bands, like the real backend does.
    private static BillResponse Bill(decimal kwh)
    {
        var lines = new List<BillLineDto>();
        foreach (var band in Bands)
        {
            if (kwh <= band.FromKwh) break;
            var top = band.ToKwh ?? kwh;
            var kwhInBand = Math.Min(kwh, top) - band.FromKwh;
            lines.Add(new BillLineDto(band.FromKwh, band.ToKwh, kwhInBand, band.PricePerKwh,
                Math.Round(kwhInBand * band.PricePerKwh, 2)));
        }
        var total = Math.Max(lines.Sum(l => l.AmountRs), MinimumChargeRs);
        return new BillResponse(kwh, lines, MinimumChargeRs, total, "FAKE data");
    }
}