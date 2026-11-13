namespace PowerCheck.Api.Models;

// One price band for NET energy on a prosumer tariff, e.g. 150A tier 1: 0 to 500 kWh at Rs 12.03.
// On 150A these bands apply only when TOTAL use is above 300 kWh; on 150B they always apply.
// Source: data/seed/prosumer_tariff_bands.csv (General Notice 473 of 2026, Appendix VIII)
public class ProsumerTariffBand
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = "";
    public DateTime EffectiveFrom { get; set; }
    public int Tier { get; set; }
    public int FromKwh { get; set; }            // 0-based like TariffTier: 0, 500, 1000 ... (CSV from_kwh minus 1)
    public int? ToKwh { get; set; }             // 500, 1000 ...; null for the top band
    public decimal RateRsPerKwh { get; set; }
}
