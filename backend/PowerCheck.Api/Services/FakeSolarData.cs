// TEMPORARY fake solar numbers so we can build and test now.
// NOT REAL. Replace with the Data Lead's database (CEB 2026 solar scheme leaflet).

public static class FakeSolarData
{
    public const string Source = "FAKE DATA - replace with CEB Solar PV Scheme (Households) 2026";

    public const decimal DefaultSystemKwp = 3m;        // size used if the user doesn't choose one
    public const decimal MaxSystemKwp = 10m;           // biggest size allowed by the scheme
    public const decimal KwhPerKwpPerMonth = 120m;     // how much 1 kWp of panels produces in a month
    public const decimal InstallCostPerKwpRs = 50000m; // price to install 1 kWp
    public const decimal ExportCreditPerKwhRs = 3.00m; // what the CEB credits for each extra kWh sent to the grid
    public const int ProjectionYears = 20;             // how many years the graph shows
}