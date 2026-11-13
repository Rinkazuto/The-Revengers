// Shapes of the data sent to and from the API (from the API contract).
// Owned by the Integrator. Change only through a PR.

public record TariffBandDto(decimal FromKwh, decimal? ToKwh, decimal PricePerKwh);
public record TariffsResponse(string Source, string SourceUrl, List<TariffBandDto> Bands, decimal FixedChargeRs);

public record BillRequest(decimal MonthlyKwh);
public record BillLineDto(decimal FromKwh, decimal? ToKwh, decimal Kwh, decimal PricePerKwh, decimal AmountRs);
public record BillResponse(decimal MonthlyKwh, List<BillLineDto> Lines, decimal FixedChargeRs, decimal TotalRs, string Source);

public record SolarRequest(decimal MonthlyKwh, decimal? SystemSizeKwp);
public record WithoutSolarDto(decimal MonthlyBillRs);
public record WithSolarDto(decimal MonthlyGenerationKwh, decimal KwhFromGrid, decimal MonthlyBillRs);
public record SolarResponse(decimal MonthlyKwh, decimal SystemSizeKwp, WithoutSolarDto WithoutSolar, WithSolarDto WithSolar,
    decimal MonthlySavingsRs, decimal YearlySavingsRs, decimal InstallCostRs, decimal? PaybackYears,
    string Source, List<string> Assumptions);

public record ReadingRequest(string Month, decimal Kwh);
public record ReadingDto(int Id, string Month, decimal Kwh, decimal BillRs);
public record ReadingsResponse(List<ReadingDto> Readings, string? Trend);