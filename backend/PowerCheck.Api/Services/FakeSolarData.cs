// TEMPORARY fake solar numbers so we can build and test now.
// Generation (113.71) is REAL (PVGIS). The other numbers are still NOT REAL.

public static class FakeSolarData
{
    public const string Source = "FAKE DATA - replace with CEB Solar PV Scheme (Households) 2026";

    public const decimal DefaultSystemKwp = 3m;        // size used if the user doesn't choose one
    public const decimal MaxSystemKwp = 10m;           // biggest size allowed by the scheme
    public const decimal KwhPerKwpPerMonth = 113.71m;  // REAL: PVGIS 5.3, European Commission JRC
    public const decimal InstallCostPerKwpRs = 50000m; // price to install 1 kWp
    public const decimal ExportCreditPerKwhRs = 3.00m; // what the CEB credits for each extra kWh sent to the grid
    public const int ProjectionYears = 20;             // how many years the graph shows
}