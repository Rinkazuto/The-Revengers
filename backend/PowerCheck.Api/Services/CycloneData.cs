using System.Globalization;
using Microsoft.VisualBasic.FileIO;

// Appliance watts and battery figures for the cyclone battery planner.
// Read straight from the Data Lead's sourced files, so the CSV is the one source of truth:
//   data/seed/cyclone_appliances.csv        (one row per appliance, with its source)
//   data/seed/cyclone_reference_values.csv  (battery and inverter figures)
// Change a number there and the app changes too. No code edits needed.

public record CycloneValues(List<ApplianceDto> Appliances, decimal UsableBatteryPercent, string UsableBatterySource);

public static class CycloneData
{
    public const decimal MaxBatteryKwh = 50m;

    private static CycloneValues? _cache;
    private static readonly object Lock = new();

    // Load the files once, then reuse them for every request
    public static CycloneValues Get(IWebHostEnvironment env)
    {
        lock (Lock)
        {
            return _cache ??= Load(Path.Combine(env.ContentRootPath, "..", "..", "data", "seed"));
        }
    }

    private static CycloneValues Load(string seedFolder)
    {
        // 1. Appliances: id, name, watts, and a source line built from the file's columns
        var appliances = ReadCsv(Path.Combine(seedFolder, "cyclone_appliances.csv"))
            .Select(r => new ApplianceDto(
                r["appliance_id"],
                r["name"],
                Math.Round(decimal.Parse(r["watts"], CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero),
                SourceLine(r)))
            .ToList();

        // 2. Battery: share of the battery's energy that reaches the appliances (inverter efficiency)
        var reference = ReadCsv(Path.Combine(seedFolder, "cyclone_reference_values.csv"))
            .ToDictionary(r => r["parameter"], r => r);

        var efficiency = reference["inverter_european_weighted_efficiency"];
        var battery = reference["battery_usable_kwh"];

        var usablePercent = decimal.Parse(efficiency["value"], CultureInfo.InvariantCulture);
        var usableSource =
            $"{battery["source"]}; {efficiency["source"]}: \"European weighted efficiency {usablePercent:0.##}%\". " +
            "New battery; older batteries hold less";

        return new CycloneValues(appliances, usablePercent, usableSource);
    }

    // "Samsung RB29FERNDSA…; Courts Mammouth, Mauritius; Courts Mammouth product page… (Average: energy label…)"
    private static string SourceLine(Dictionary<string, string> r)
    {
        var parts = new[] { "example_model", "seller", "source" }
            .Select(col => r.GetValueOrDefault(col, ""))
            .Where(v => !string.IsNullOrWhiteSpace(v));

        var basis = r.GetValueOrDefault("watts_basis", "");
        var line = string.Join("; ", parts);
        return string.IsNullOrWhiteSpace(basis) ? line : $"{line} ({basis})";
    }

    // Reads a CSV with a header row; handles quoted values that contain commas or quotes
    private static List<Dictionary<string, string>> ReadCsv(string path)
    {
        using var parser = new TextFieldParser(path) { HasFieldsEnclosedInQuotes = true };
        parser.SetDelimiters(",");

        var headers = parser.ReadFields() ?? Array.Empty<string>();
        var rows = new List<Dictionary<string, string>>();

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields is null || fields.All(string.IsNullOrWhiteSpace)) continue;

            var row = new Dictionary<string, string>();
            for (var i = 0; i < headers.Length; i++)
                row[headers[i]] = i < fields.Length ? fields[i] : "";
            rows.Add(row);
        }

        return rows;
    }
}