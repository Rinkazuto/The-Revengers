namespace PowerCheck.Web.Api;

// Copied from backend/PowerCheck.Api/Contracts/CycloneContracts.cs (contract sections 6 and 7).
// When the cyclone contract changes, update this file to match.

public record ApplianceDto(string Id, string Name, decimal Watts, string Source);
public record AppliancesResponse(List<ApplianceDto> Appliances);

public record BackupApplianceRequest(string Id, decimal HoursPerDay);
public record BackupRequest(List<BackupApplianceRequest>? Appliances, decimal? BatteryKwh);
public record BackupResponse(decimal KwhPerDay, decimal BatteryKwh, decimal HoursCovered, string Message, List<string> Assumptions);