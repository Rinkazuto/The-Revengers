namespace PowerCheck.Api.Models;

// One rule of the CEB 2026 household solar scheme, looked up by name,
// e.g. "gross_metering_export_rate" = 4.83 Rs per kWh produced.
// Source: data/seed/solar_scheme_2026.csv
public class SolarSchemeParameter
{
    public int Id { get; set; }
    public string Parameter { get; set; } = "";   // unique name used by the code
    public decimal Value { get; set; }
    public string Unit { get; set; } = "";
    public string AppliesTo { get; set; } = "";
    public string Source { get; set; } = "";      // document the value came from
    public int? SourcePage { get; set; }          // page in that document; null for a web page
}
