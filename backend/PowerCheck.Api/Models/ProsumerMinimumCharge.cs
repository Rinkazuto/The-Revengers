namespace PowerCheck.Api.Models;

// The minimum monthly bill on a prosumer tariff, chosen by the home's TOTAL use that month.
// 150A: up to 75 kWh Rs 50.60, up to 300 kWh Rs 211.60, above 300 kWh Rs 424.35. 150B: Rs 424.35.
// Source: data/seed/prosumer_minimum_charges.csv (General Notice 473 of 2026, Appendix VIII)
public class ProsumerMinimumCharge
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = "";
    public DateTime EffectiveFrom { get; set; }
    public decimal? MaxTotalKwh { get; set; }   // this minimum applies when total use is up to this; null = no upper limit
    public decimal MinimumRs { get; set; }
}
