namespace PowerCheck.Api.Models;

// CEB's indicative price range for one solar system size, with a 5 kWh battery.
// Source: data/seed/solar_system_prices_2026.csv
public class SolarSystemPrice
{
    public int Id { get; set; }
    public double SystemKwp { get; set; }      // double so SQLite can sort sizes; 1.5, 2.0 ... 15.0 are stored exactly
    public double BatteryKwh { get; set; }
    public decimal PriceMinRs { get; set; }
    public decimal PriceMaxRs { get; set; }
}
