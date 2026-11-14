using PowerCheck.Api.Data;

// Cyclone battery planner (wildcard "Made for Mauritius").
// In a cyclone the CEB grid often goes down, and ordinary rooftop solar switches off with it.
// Only a battery keeps the power on. This tells a family how long their battery lasts.
// Appliance watts and battery figures come from data/seed (CycloneData).

public static class CycloneEndpoints
{
    public static void MapCycloneEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/cyclone");

        // GET /api/cyclone/appliances -> appliances with sourced watts
        api.MapGet("/appliances", (IWebHostEnvironment env) =>
            Results.Ok(new AppliancesResponse(CycloneData.Get(env).Appliances)));

        // GET /api/cyclone/outlook -> next 48 hours of wind, turned into battery advice (not a cyclone warning)
        api.MapGet("/outlook", async (IHttpClientFactory httpFactory, IConfiguration config, IWebHostEnvironment env) =>
        {
            var outlook = await WindOutlook.GetAsync(httpFactory, config, env, CycloneData.Get(env));

            return outlook is null
                ? Results.Problem(
                    "The wind forecast is not available right now (no connection to OpenWeather and no saved forecast).",
                    statusCode: StatusCodes.Status503ServiceUnavailable)
                : Results.Ok(outlook);
        });

        // POST /api/cyclone/backup -> send appliances + hours a day, get how long the battery lasts
        api.MapPost("/backup", (BackupRequest request, AppDbContext db, IWebHostEnvironment env) =>
        {
            var values = CycloneData.Get(env);

            // 1. Check the request
            var errors = new Dictionary<string, string[]>();
            var chosen = request.Appliances ?? new List<BackupApplianceRequest>();

            if (chosen.Count == 0)
                errors["appliances"] = new[] { "Choose at least one appliance." };

            var duplicates = chosen.GroupBy(a => a.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
                errors["appliances"] = new[] { $"Each appliance can only be listed once: {string.Join(", ", duplicates)}." };

            var unknown = chosen.Where(a => values.Appliances.All(k => k.Id != a.Id)).Select(a => a.Id).ToList();
            if (unknown.Count > 0)
                errors["appliances"] = new[] { $"Unknown appliance id: {string.Join(", ", unknown)}. Use GET /api/cyclone/appliances for the list." };

            if (chosen.Any(a => a.HoursPerDay < 0 || a.HoursPerDay > 24))
                errors["hoursPerDay"] = new[] { "hoursPerDay must be between 0 and 24." };
            else if (chosen.Count > 0 && chosen.Sum(a => a.HoursPerDay) == 0)
                errors["hoursPerDay"] = new[] { "At least one appliance needs more than 0 hours a day." };

            if (request.BatteryKwh is <= 0 || request.BatteryKwh > CycloneData.MaxBatteryKwh)
                errors["batteryKwh"] = new[] { $"batteryKwh must be more than 0 and at most {CycloneData.MaxBatteryKwh}." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            // 2. Battery size: what the user sent, or the battery in the CEB price list (5 kWh)
            var schemeBattery = new TariffData(db).GetSolarScheme().SystemPrices.First().BatteryKwh;
            var batteryKwh = request.BatteryKwh ?? schemeBattery;
            var usableKwh = batteryKwh * values.UsableBatteryPercent / 100;

            // 3. Energy needed per day: watts × hours ÷ 1000 for each appliance
            var kwhPerDay = chosen.Sum(a =>
            {
                var appliance = values.Appliances.Single(k => k.Id == a.Id);
                return appliance.Watts * a.HoursPerDay / 1000;
            });

            // 4. How long the battery lasts: usable kWh ÷ kWh per day × 24 hours
            var hoursCovered = Math.Round(usableKwh / kwhPerDay * 24, 1, MidpointRounding.AwayFromZero);

            var message = hoursCovered >= 24 * 30
                ? $"Your {batteryKwh:0.##} kWh battery keeps these running for up to more than a month."
                : hoursCovered >= 48
                    ? $"Your {batteryKwh:0.##} kWh battery keeps these running for up to about {hoursCovered / 24:0.#} days."
                    : $"Your {batteryKwh:0.##} kWh battery keeps these running for up to about {hoursCovered:0.#} hours.";

            // 5. Explain every number (shown on screen)
            var assumptions = new List<string>
            {
                $"Usable battery: {values.UsableBatteryPercent:0.##}% of {batteryKwh:0.##} kWh = {usableKwh:0.##} kWh ({values.UsableBatterySource}).",
                $"Battery size: {schemeBattery:0.##} kWh is the battery in CEB's solar price guide, unless you choose another size.",
                "Assumes a new, fully charged battery. Real time is usually shorter: inverters lose more at small loads, and older batteries hold less.",
                "We ignore recharging from the panels during the cyclone (heavy cloud), so this is the safe, lower figure.",
                "Appliance watts are from labels and published tables; your own appliances may use less.",
                "Without a battery, ordinary rooftop solar switches off when the CEB grid goes down.",
                "For official cyclone warnings, check the Mauritius Meteorological Services (MMS).",
            };

            return Results.Ok(new BackupResponse(
                Math.Round(kwhPerDay, 2, MidpointRounding.AwayFromZero),
                batteryKwh,
                hoursCovered,
                message,
                assumptions));
        });
    }
}