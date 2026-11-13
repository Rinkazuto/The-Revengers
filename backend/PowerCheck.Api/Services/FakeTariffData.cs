// TEMPORARY fake tariff numbers so we can build and test now.
// NOT REAL. Replace with the Data Lead's database (real CEB 2026 numbers).

public static class FakeTariffData
{
    public const string Source = "FAKE DATA - replace with CEB domestic tariff 2026";
    public const string SourceUrl = "https://ceb.mu/customer-corner/electricity-tariffs-and-applicable-rates";
    public const decimal FixedChargeRs = 0m;

    public static readonly List<TariffBandDto> Bands = new()
    {
        new(0,   25,   3.00m),
        new(25,  50,   4.00m),
        new(50,  75,   5.00m),
        new(75,  100,  6.00m),
        new(100, null, 7.00m),   // null = "100 kWh and above"
    };
}