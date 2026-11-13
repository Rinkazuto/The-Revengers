namespace PowerCheck.Api.Models;

// The lowest monthly bill for a tariff. The bill is the band total or this amount, whichever is higher.
// Source: data/seed/minimum_charges.csv
public class MinimumCharge
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = "";
    public DateTime EffectiveFrom { get; set; }
    public decimal MinimumRs { get; set; }
}
