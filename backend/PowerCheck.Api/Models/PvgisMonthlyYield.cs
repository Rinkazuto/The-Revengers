namespace PowerCheck.Api.Models;

// Average solar output in Mauritius for each month, for a 1 kWp system.
// Source: data/seed/pvgis_monthly_yield_1kwp.csv (PVGIS, European Commission JRC)
public class PvgisMonthlyYield
{
    public int Id { get; set; }
    public int Month { get; set; }                   // 1 = January ... 12 = December
    public decimal EmKwhPerKwp { get; set; }         // kWh produced per kWp in that month
    public decimal HiMKwhPerM2 { get; set; }         // sun reaching the panel, kWh per m2 in that month
    public decimal SdMKwh { get; set; }              // year to year variation, kWh
}
