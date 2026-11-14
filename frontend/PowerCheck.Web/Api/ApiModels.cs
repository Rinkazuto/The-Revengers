namespace PowerCheck.Web.Api;

// Copied from backend/PowerCheck.Api/Contracts/ApiContracts.cs (submain, PR #53).
// When the API contract changes, update this file to match.

public record TariffBandDto(decimal FromKwh, decimal? ToKwh, decimal PricePerKwh);
public record TariffsResponse(string TariffCode, string Source, string SourceUrl, List<TariffBandDto> Bands, decimal MinimumChargeRs);

public record BillRequest(decimal MonthlyKwh, string? TariffCode = null);
public record BillLineDto(decimal FromKwh, decimal? ToKwh, decimal Kwh, decimal PricePerKwh, decimal AmountRs);
public record BillResponse(decimal MonthlyKwh, List<BillLineDto> Lines, decimal MinimumChargeRs, decimal TotalRs, string Source);

public record SolarRequest(decimal MonthlyKwh, decimal? SystemSizeKwp, string? TariffCode = null, string? Phase = null);
public record WithoutSolarDto(decimal MonthlyBillRs);
public record WithSolarDto(decimal MonthlyGenerationKwh, decimal KwhFromGrid, decimal MonthlyBillRs, decimal ExportCreditRs);
public record ProjectionYearDto(int Year, decimal CumulativeSavingsRs, decimal NetGainRs);
public record SolarResponse(decimal MonthlyKwh, decimal SystemSizeKwp, WithoutSolarDto WithoutSolar, WithSolarDto WithSolar,
    decimal MonthlySavingsRs, decimal YearlySavingsRs, decimal InstallCostRs, decimal? PaybackYears,
    string Source, List<string> Assumptions, List<ProjectionYearDto> Projection);

public record ReadingRequest(string Month, decimal Kwh);
public record ReadingDto(int Id, string Month, decimal Kwh, decimal BillRs);
public record ReadingsResponse(List<ReadingDto> Readings, string? Trend);

// The error the API sends for bad input (400).
public record ProblemResponse(string? Title, string? Detail, Dictionary<string, string[]>? Errors);

// Thrown when a call fails. The message is plain text, ready to show the user.
public class ApiException(string message) : Exception(message);