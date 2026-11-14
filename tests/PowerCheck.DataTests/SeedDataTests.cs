using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace PowerCheck.DataTests;

// Card #55: fails if a CSV in data/seed gets a band gap, a duplicate row or an unexpected row count.
// These tests read the CSV files only. They never start the app or touch the database.
public class SeedDataTests
{
    // ---------- Helpers ----------

    // Walks up from the test's build folder until it finds data/seed, so it works on any laptop and in CI.
    private static readonly string SeedFolder = FindSeedFolder();

    private static string FindSeedFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "data", "seed")))
            dir = dir.Parent;
        return dir is null
            ? throw new DirectoryNotFoundException("data/seed not found above " + AppContext.BaseDirectory)
            : Path.Combine(dir.FullName, "data", "seed");
    }

    // Reads a CSV (header row, quoted values allowed) into one dictionary per row.
    private static List<Dictionary<string, string>> Read(string file)
    {
        using var parser = new TextFieldParser(Path.Combine(SeedFolder, file)) { HasFieldsEnclosedInQuotes = true };
        parser.SetDelimiters(",");
        var header = parser.ReadFields() ?? throw new InvalidDataException(file + " has no header");
        var rows = new List<Dictionary<string, string>>();
        while (!parser.EndOfData)
        {
            var cells = parser.ReadFields();
            if (cells is null || cells.All(string.IsNullOrEmpty)) continue;
            Assert.True(cells.Length == header.Length, $"{file}: row {rows.Count + 1} has {cells.Length} cells, header has {header.Length}");
            rows.Add(header.Zip(cells).ToDictionary(p => p.First, p => p.Second));
        }
        return rows;
    }

    private static decimal Dec(string s) => decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);

    // ---------- Row counts ----------

    // Every seed file and the number of data rows it must have. A new or lost row fails here,
    // so any change to the data is a deliberate decision (update this number and data/sources/README.md).
    [Theory]
    [InlineData("tariff_tiers.csv", 60)]
    [InlineData("minimum_charges.csv", 7)]
    [InlineData("meter_rentals.csv", 2)]
    [InlineData("domestic_electricity_stats.csv", 2)]
    [InlineData("chart_bill_vs_kwh.csv", 101)]
    [InlineData("nasa_power_daily_irradiance_2023_2025.csv", 1096)]
    [InlineData("pvgis_monthly_yield_1kwp.csv", 12)]
    [InlineData("solar_scheme_2026.csv", 15)]
    [InlineData("solar_system_prices_2026.csv", 12)]
    [InlineData("prosumer_tariffs.csv", 2)]
    [InlineData("prosumer_tariff_bands.csv", 10)]
    [InlineData("prosumer_minimum_charges.csv", 4)]
    [InlineData("cyclone_appliances.csv", 7)]
    [InlineData("cyclone_reference_values.csv", 10)]
    public void Row_count_is_as_expected(string file, int expected) =>
        Assert.Equal(expected, Read(file).Count);

    // ---------- Duplicates ----------

    [Theory]
    [InlineData("tariff_tiers.csv", "tariff_code,effective_from,tier")]
    [InlineData("minimum_charges.csv", "tariff_code,effective_from")]
    [InlineData("meter_rentals.csv", "consumer_type,meter_type,effective_from")]
    [InlineData("domestic_electricity_stats.csv", "year")]
    [InlineData("chart_bill_vs_kwh.csv", "kwh")]
    [InlineData("nasa_power_daily_irradiance_2023_2025.csv", "date")]
    [InlineData("pvgis_monthly_yield_1kwp.csv", "month")]
    [InlineData("solar_scheme_2026.csv", "parameter")]
    [InlineData("solar_system_prices_2026.csv", "system_kwp")]
    [InlineData("prosumer_tariffs.csv", "tariff_code,effective_from")]
    [InlineData("prosumer_tariff_bands.csv", "tariff_code,effective_from,tier")]
    [InlineData("prosumer_minimum_charges.csv", "tariff_code,effective_from,max_total_kwh")]
    [InlineData("cyclone_appliances.csv", "appliance_id")]
    [InlineData("cyclone_reference_values.csv", "parameter")]
    public void No_duplicate_rows(string file, string keyColumns)
    {
        var columns = keyColumns.Split(',');
        var duplicates = Read(file)
            .GroupBy(r => string.Join("|", columns.Select(c => r[c])))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        Assert.True(duplicates.Count == 0, $"{file}: duplicate rows for {string.Join(", ", duplicates)}");
    }

    // ---------- Band gaps ----------

    // In each tariff group the bands must start at 1 kWh, follow on with no gap or overlap,
    // and end with one open top band (to_kwh empty). Rates must be above zero.
    [Theory]
    [InlineData("tariff_tiers.csv")]
    [InlineData("prosumer_tariff_bands.csv")]
    public void Bands_have_no_gaps(string file)
    {
        foreach (var group in Read(file).GroupBy(r => (Code: r["tariff_code"], Date: r["effective_from"])))
        {
            var bands = group.OrderBy(r => int.Parse(r["tier"], CultureInfo.InvariantCulture)).ToList();
            var name = $"{file} {group.Key.Code} from {group.Key.Date}";

            Assert.True(bands[0]["from_kwh"] == "1", $"{name}: first band starts at {bands[0]["from_kwh"]}, expected 1");
            for (var i = 0; i < bands.Count; i++)
            {
                Assert.True(int.Parse(bands[i]["tier"]) == i + 1, $"{name}: tiers are not 1, 2, 3 ... (found {bands[i]["tier"]} at position {i + 1})");
                Assert.True(Dec(bands[i]["rate_rs_per_kwh"]) > 0, $"{name}: tier {i + 1} has rate {bands[i]["rate_rs_per_kwh"]}");
                var isLast = i == bands.Count - 1;
                if (isLast)
                {
                    Assert.True(bands[i]["to_kwh"] == "", $"{name}: top band must be open (to_kwh empty)");
                }
                else
                {
                    var to = int.Parse(bands[i]["to_kwh"], CultureInfo.InvariantCulture);
                    var nextFrom = int.Parse(bands[i + 1]["from_kwh"], CultureInfo.InvariantCulture);
                    Assert.True(nextFrom == to + 1, $"{name}: gap or overlap between tier {i + 1} (to {to}) and tier {i + 2} (from {nextFrom})");
                }
            }
        }
    }

    // Prosumer minimum charges must rise with total use and end with one open range.
    [Fact]
    public void Prosumer_minimum_ranges_have_no_gaps()
    {
        foreach (var group in Read("prosumer_minimum_charges.csv").GroupBy(r => r["tariff_code"]))
        {
            var rows = group.ToList();
            Assert.True(rows.Count(r => r["max_total_kwh"] == "") == 1, $"{group.Key}: needs exactly one open range");
            Assert.True(rows[^1]["max_total_kwh"] == "", $"{group.Key}: the open range must be last");
            var limits = rows.Where(r => r["max_total_kwh"] != "").Select(r => Dec(r["max_total_kwh"])).ToList();
            Assert.True(limits.SequenceEqual(limits.OrderBy(x => x)), $"{group.Key}: ranges are not in rising order");
        }
    }

    // ---------- Links between files ----------

    // Every tariff group with bands needs a minimum charge for the same tariff and date.
    [Fact]
    public void Every_tariff_has_a_minimum_charge()
    {
        var minimums = Read("minimum_charges.csv").Select(r => (r["tariff_code"], r["effective_from"])).ToHashSet();
        var missing = Read("tariff_tiers.csv").Select(r => (r["tariff_code"], r["effective_from"])).Distinct()
            .Where(k => !minimums.Contains(k)).ToList();
        Assert.True(missing.Count == 0, "No minimum charge for: " + string.Join(", ", missing));
    }

    // PVGIS must have one value for each month, 1 to 12.
    [Fact]
    public void Pvgis_has_all_12_months() =>
        Assert.Equal(Enumerable.Range(1, 12),
            Read("pvgis_monthly_yield_1kwp.csv").Select(r => int.Parse(r["month"], CultureInfo.InvariantCulture)).OrderBy(m => m));

    // The parameters the app looks up by name (TariffData, CycloneData) must exist,
    // or the solar and cyclone endpoints would crash with KeyNotFoundException.
    [Theory]
    [InlineData("solar_scheme_2026.csv", "grant_share,grant_cap,gross_metering_export_rate,net_metering_excess_export_rate,cst_rate_up_to_500_kwh,cst_rate_501_to_1000_kwh,cst_rate_above_1000_kwh,processing_fee,annual_panel_degradation,battery_required_above,network_survey_required_above,max_installation_capacity,contract_term_years")]
    [InlineData("cyclone_reference_values.csv", "battery_usable_kwh,inverter_european_weighted_efficiency")]
    public void Required_parameters_exist(string file, string names)
    {
        var present = Read(file).Select(r => r["parameter"]).ToHashSet();
        var missing = names.Split(',').Where(n => !present.Contains(n)).ToList();
        Assert.True(missing.Count == 0, $"{file}: missing {string.Join(", ", missing)}");
    }

    // ---------- Expected bills still match the tariff data ----------

    // Recomputes all 40 expected bills in tests/test-cases/bill_test_cases.csv from tariff_tiers.csv
    // and minimum_charges.csv, so a changed rate can't slip through without the test cases noticing.
    [Fact]
    public void Bill_test_cases_match_the_tariff_data()
    {
        var tiers = Read("tariff_tiers.csv");
        var minimums = Read("minimum_charges.csv");
        var casesFile = Path.Combine(SeedFolder, "..", "..", "tests", "test-cases", "bill_test_cases.csv");
        using var parser = new TextFieldParser(casesFile) { HasFieldsEnclosedInQuotes = true };
        parser.SetDelimiters(",");
        var header = parser.ReadFields()!;
        var count = 0;
        while (!parser.EndOfData)
        {
            var c = header.Zip(parser.ReadFields()!).ToDictionary(p => p.First, p => p.Second);
            var kwh = Dec(c["kwh"]);
            var bandTotal = tiers
                .Where(t => t["tariff_code"] == c["tariff_code"] && t["effective_from"] == c["effective_from"])
                .Where(t => kwh >= Dec(t["from_kwh"]))
                .Sum(t => ((t["to_kwh"] == "" ? kwh : Math.Min(kwh, Dec(t["to_kwh"]))) - Dec(t["from_kwh"]) + 1) * Dec(t["rate_rs_per_kwh"]));
            var minimum = Dec(minimums.Single(m => m["tariff_code"] == c["tariff_code"] && m["effective_from"] == c["effective_from"])["minimum_rs"]);
            Assert.True(Math.Max(bandTotal, minimum) == Dec(c["expected_rs"]),
                $"{c["tariff_code"]} {c["effective_from"]} {c["kwh"]} kWh: tariff data gives {Math.Max(bandTotal, minimum)}, test case says {c["expected_rs"]}");
            count++;
        }
        Assert.Equal(40, count);
    }
}
