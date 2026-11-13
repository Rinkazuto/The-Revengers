namespace PowerCheck.Api.Models;

// One CEB price band, e.g. tariff 110 from 1 May 2026, tier 1: 1 to 25 kWh at Rs 3.63 per kWh.
// Source: data/seed/tariff_tiers.csv
public class TariffTier
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = "";   // "110", "110A", "120" or "140"
    public DateTime EffectiveFrom { get; set; }    // first day the rate applies
    public int Tier { get; set; }                  // 1 to 12, cheapest band first
    public int FromKwh { get; set; }               // where the band starts: 0, 25, 50 ... (CSV from_kwh minus 1)
    public int? ToKwh { get; set; }                // where the band ends: 25, 50, 75 ...; null for the top band
                                                   // kWh charged in the band = min(usage, ToKwh) - FromKwh
    public decimal RateRsPerKwh { get; set; }      // decimal keeps 3.63 exact
}
