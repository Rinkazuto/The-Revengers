namespace PowerCheck.Api.Models;

// A residential prosumer tariff: the tariff a home moves to once it has solar panels.
// 150A = single-phase supply, 150B = three-phase supply.
// Source: data/seed/prosumer_tariffs.csv (General Notice 473 of 2026, Appendix VIII)
public class ProsumerTariff
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = "";       // "150A" or "150B"
    public DateTime EffectiveFrom { get; set; }
    public string Phase { get; set; } = "";            // "single" or "three"
    public decimal? FlatRateMaxTotalKwh { get; set; }  // 150A: flat rate applies when TOTAL use is up to this (300); null for 150B
    public decimal? FlatRateRsPerKwh { get; set; }     // 150A: Rs 8.58 per kWh of NET energy; null for 150B
    public int SourcePage { get; set; }                // Gazette page
}
