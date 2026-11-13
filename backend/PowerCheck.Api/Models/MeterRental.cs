namespace PowerCheck.Api.Models;

// Monthly meter rent, a separate line on the CEB bill.
// Source: data/seed/meter_rentals.csv
public class MeterRental
{
    public int Id { get; set; }
    public string ConsumerType { get; set; } = "";   // "domestic"
    public string MeterType { get; set; } = "";      // "single-phase" or "three-phase (direct connection)"
    public decimal RentRsPerMonth { get; set; }
    public DateTime EffectiveFrom { get; set; }
}
