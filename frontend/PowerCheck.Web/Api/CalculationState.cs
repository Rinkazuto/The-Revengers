namespace PowerCheck.Web.Api;

// Remembers the last bill the user calculated, so the Solar screen
// ("Based on __ kWh / month") and "Save this reading" can use the same kWh and tariff.
// Registered as Scoped: each phone gets its own copy while the page is open.
public class CalculationState
{
    public decimal? MonthlyKwh { get; set; }
    public string? TariffCode { get; set; }
    public BillResponse? LastBill { get; set; }
}