using System.Globalization;
using System.Text;
using PowerCheck.Api.Models;

namespace PowerCheck.Api.Data;

// Loads the verified CSV files in data/seed into the database on first run.
// Every value is parsed from the CSV text unchanged: no figure is typed into the code.
public static class Seeder
{
    // seedFolder is the repo's data/seed folder. Each table is filled only while it is empty,
    // so running the app again never adds duplicate rows.
    public static void Run(AppDbContext db, string seedFolder)
    {
        if (!Directory.Exists(seedFolder))
            throw new DirectoryNotFoundException($"Seed folder not found: {Path.GetFullPath(seedFolder)}");

        if (!db.TariffTiers.Any())
            db.TariffTiers.AddRange(Read(seedFolder, "tariff_tiers.csv", r => new TariffTier
            {
                TariffCode = r["tariff_code"],
                EffectiveFrom = Date(r["effective_from"]),
                Tier = int.Parse(r["tier"], CultureInfo.InvariantCulture),
                // The CSV counts bands from the first kWh (1 to 25, 26 to 50). The backend counts from
                // the previous band's end (0 to 25, 25 to 50), so the start moves down by 1. Same kWh in each band.
                FromKwh = int.Parse(r["from_kwh"], CultureInfo.InvariantCulture) - 1,
                ToKwh = r["to_kwh"] == "" ? null : int.Parse(r["to_kwh"], CultureInfo.InvariantCulture),
                RateRsPerKwh = Dec(r["rate_rs_per_kwh"]),
            }));

        if (!db.MinimumCharges.Any())
            db.MinimumCharges.AddRange(Read(seedFolder, "minimum_charges.csv", r => new MinimumCharge
            {
                TariffCode = r["tariff_code"],
                EffectiveFrom = Date(r["effective_from"]),
                MinimumRs = Dec(r["minimum_rs"]),
            }));

        if (!db.MeterRentals.Any())
            db.MeterRentals.AddRange(Read(seedFolder, "meter_rentals.csv", r => new MeterRental
            {
                ConsumerType = r["consumer_type"],
                MeterType = r["meter_type"],
                RentRsPerMonth = Dec(r["rent_rs_per_month"]),
                EffectiveFrom = Date(r["effective_from"]),
            }));

        // Adds any parameter missing from the database, so a new CSV row reaches old app.db files too.
        var knownParameters = db.SolarSchemeParameters.Select(p => p.Parameter).ToHashSet();
        db.SolarSchemeParameters.AddRange(Read(seedFolder, "solar_scheme_2026.csv", r => new SolarSchemeParameter
            {
                Parameter = r["parameter"],
                Value = Dec(r["value"]),
                Unit = r["unit"],
                AppliesTo = r["applies_to"],
                Source = r["source"],
                SourcePage = r["source_page"] == "" ? null : int.Parse(r["source_page"], CultureInfo.InvariantCulture),
            }).Where(p => !knownParameters.Contains(p.Parameter)));

        if (!db.SolarSystemPrices.Any())
            db.SolarSystemPrices.AddRange(Read(seedFolder, "solar_system_prices_2026.csv", r => new SolarSystemPrice
            {
                SystemKwp = double.Parse(r["system_kwp"], CultureInfo.InvariantCulture),
                BatteryKwh = double.Parse(r["battery_kwh"], CultureInfo.InvariantCulture),
                PriceMinRs = Dec(r["price_min_rs"]),
                PriceMaxRs = Dec(r["price_max_rs"]),
            }));

        if (!db.PvgisMonthlyYields.Any())
            db.PvgisMonthlyYields.AddRange(Read(seedFolder, "pvgis_monthly_yield_1kwp.csv", r => new PvgisMonthlyYield
            {
                Month = int.Parse(r["month"], CultureInfo.InvariantCulture),
                EmKwhPerKwp = Dec(r["e_m_kwh_per_kwp"]),
                HiMKwhPerM2 = Dec(r["h_i_m_kwh_per_m2"]),
                SdMKwh = Dec(r["sd_m_kwh"]),
            }));

        db.SaveChanges();
    }

    // Invariant culture: "3.63" always means three point six three, whatever the laptop's language.
    private static decimal Dec(string s) => decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);

    private static DateTime Date(string s) =>
        DateTime.SpecifyKind(DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc);

    // Reads a CSV file and turns each row into an object, looking columns up by their header name.
    private static List<T> Read<T>(string folder, string file, Func<Dictionary<string, string>, T> map)
    {
        var lines = File.ReadAllLines(Path.Combine(folder, file))   // handles CRLF and LF line endings
            .Where(l => l.Length > 0)
            .ToList();
        var header = SplitCsvLine(lines[0]);
        var rows = new List<T>();
        foreach (var line in lines.Skip(1))
        {
            var cells = SplitCsvLine(line);
            if (cells.Count != header.Count)
                throw new FormatException($"{file}: expected {header.Count} columns, found {cells.Count} in: {line}");
            var row = header.Zip(cells).ToDictionary(p => p.First, p => p.Second);
            rows.Add(map(row));
        }
        return rows;
    }

    // Splits one CSV line on commas, keeping commas inside "quoted text" (used in solar_scheme_2026.csv).
    private static List<string> SplitCsvLine(string line)
    {
        var cells = new List<string>();
        var cell = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"' && inQuotes && i + 1 < line.Length && line[i + 1] == '"') { cell.Append('"'); i++; }
            else if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes) { cells.Add(cell.ToString()); cell.Clear(); }
            else cell.Append(c);
        }
        cells.Add(cell.ToString());
        return cells;
    }
}
