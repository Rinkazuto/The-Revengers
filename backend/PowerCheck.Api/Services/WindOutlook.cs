using System.Globalization;
using System.Text.Json;

// Wind outlook for the next 48 hours, turned into BATTERY ADVICE (contract section 8).
// It is not a cyclone warning: official warnings come from the Mauritius Meteorological Services (MMS).
//
// Live:    OpenWeather free "5 day / 3 hour forecast" for the PVGIS point (-20.25, 57.55).
//          API key only in user-secrets ("OpenWeather:ApiKey"), never in a committed file.
// Safe:    3-second timeout; a good answer is reused for 15 minutes.
// Offline: if OpenWeather doesn't answer (no internet, no key, key not active), we use a REAL
//          saved forecast from data/sources/raw/ and say so (isOffline = true).

public static class WindOutlook
{
    private const string ForecastFile = "openweather_forecast_-20.25_57.55.json";
    private const string Url = "https://api.openweathermap.org/data/2.5/forecast?lat=-20.25&lon=57.55&units=metric&appid=";
    private const int Entries48Hours = 16;                         // 16 × 3 hours = 48 hours
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MauritiusOffset = TimeSpan.FromHours(4);   // UTC+4

    private static OutlookResponse? _cached;
    private static DateTime _cachedAtUtc;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    // Returns the outlook, or null if there is no live forecast AND no saved forecast
    public static async Task<OutlookResponse?> GetAsync(
        IHttpClientFactory httpFactory, IConfiguration config, IWebHostEnvironment env, CycloneValues values)
    {
        await Gate.WaitAsync();
        try
        {
            // 1. Reuse a recent live answer
            if (_cached is not null && DateTime.UtcNow - _cachedAtUtc < CacheFor)
                return _cached;

            // 2. Try OpenWeather live
            var live = await TryLiveAsync(httpFactory, config, values);
            if (live is not null)
            {
                _cached = live;
                _cachedAtUtc = DateTime.UtcNow;
                return live;
            }

            // 3. Fall back to the real saved forecast (not cached, so the next request tries live again)
            return TryOffline(env, values);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task<OutlookResponse?> TryLiveAsync(
        IHttpClientFactory httpFactory, IConfiguration config, CycloneValues values)
    {
        var apiKey = config["OpenWeather:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;   // no key on this laptop: use the saved forecast

        try
        {
            using var timeout = new CancellationTokenSource(Timeout);
            var http = httpFactory.CreateClient();
            using var response = await http.GetAsync(Url + Uri.EscapeDataString(apiKey), timeout.Token);
            if (!response.IsSuccessStatusCode)
                return null;   // e.g. 401 key not active yet

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);
            return Build(json, values, isOffline: false, fetchedAtUtc: DateTime.UtcNow);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;   // no internet, too slow, or an unexpected answer
        }
    }

    private static OutlookResponse? TryOffline(IWebHostEnvironment env, CycloneValues values)
    {
        var path = Path.Combine(env.ContentRootPath, "..", "..", "data", "sources", "raw", ForecastFile);
        if (!File.Exists(path))
            return null;

        using var json = JsonDocument.Parse(File.ReadAllText(path));

        // The saved file has no download time, so we use the time of its first forecast entry
        var firstEntryUtc = DateTimeOffset.FromUnixTimeSeconds(
            json.RootElement.GetProperty("list")[0].GetProperty("dt").GetInt64()).UtcDateTime;

        return Build(json, values, isOffline: true, fetchedAtUtc: firstEntryUtc);
    }

    // Turn OpenWeather's forecast into a level + battery advice
    private static OutlookResponse Build(JsonDocument json, CycloneValues values, bool isOffline, DateTime fetchedAtUtc)
    {
        // The next 48 hours: time, sustained wind and gust (m/s × 3.6 = km/h)
        var entries = json.RootElement.GetProperty("list").EnumerateArray()
            .Take(Entries48Hours)
            .Select(e =>
            {
                var wind = e.GetProperty("wind");
                var speedKmh = wind.GetProperty("speed").GetDecimal() * 3.6m;
                var gustKmh = wind.TryGetProperty("gust", out var g) ? g.GetDecimal() * 3.6m : speedKmh;
                var timeUtc = DateTimeOffset.FromUnixTimeSeconds(e.GetProperty("dt").GetInt64()).UtcDateTime;
                return (TimeUtc: timeUtc, WindKmh: speedKmh, GustKmh: Math.Max(gustKmh, speedKmh));
            })
            .ToList();

        var strongest = entries.MaxBy(e => e.WindKmh);
        var maxWind = Math.Round(strongest.WindKmh, 1, MidpointRounding.AwayFromZero);
        var maxGust = Math.Round(entries.Max(e => e.GustKmh), 1, MidpointRounding.AwayFromZero);

        // Level from SUSTAINED wind (Beaufort scale); gusts only appear in the advice
        var level = maxWind >= values.StormKmh ? "chargeNow"
                  : maxWind >= values.GaleKmh ? "charge"
                  : "ok";

        var when = WhenInMauritius(strongest.TimeUtc);
        const string mms = "For official cyclone warnings, check MMS.";

        var advice = level switch
        {
            "chargeNow" => $"Storm-force winds forecast {when} (wind {maxWind:0} km/h, gusts {maxGust:0} km/h). Charge your battery now. {mms}",
            "charge" => $"Strong winds forecast {when} (wind {maxWind:0} km/h, gusts {maxGust:0} km/h). Charge your battery. {mms}",
            _ => $"No strong winds in the next 48 hours (strongest wind about {maxWind:0} km/h, gusts {maxGust:0} km/h). Keep your battery charged during cyclone season. {mms}",
        };

        var source = "Wind forecast from OpenWeather (5 day / 3 hour forecast). " +
                     $"Levels from the Beaufort scale ({values.BeaufortSource}). {mms}";

        return new OutlookResponse(level, maxWind, maxGust, strongest.TimeUtc, advice, isOffline, fetchedAtUtc, source);
    }

    // e.g. "Tuesday night" or "Wednesday afternoon", in Mauritius time (UTC+4)
    private static string WhenInMauritius(DateTime timeUtc)
    {
        var local = timeUtc + MauritiusOffset;
        var day = local.ToString("dddd", CultureInfo.InvariantCulture);
        var part = local.Hour switch
        {
            < 6 => "early morning",
            < 12 => "morning",
            < 18 => "afternoon",
            _ => "night",
        };
        return $"{day} {part}";
    }
}