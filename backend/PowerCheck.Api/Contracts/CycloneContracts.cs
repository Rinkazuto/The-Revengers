// Shapes for the cyclone battery feature (wildcard "Made for Mauritius").
// Contract section 6. Owned by the Integrator. Change only through a PR.

public record ApplianceDto(string Id, string Name, decimal Watts, string Source);
public record AppliancesResponse(List<ApplianceDto> Appliances);

public record BackupApplianceRequest(string Id, decimal HoursPerDay);
public record BackupRequest(List<BackupApplianceRequest>? Appliances, decimal? BatteryKwh);
public record BackupResponse(decimal KwhPerDay, decimal BatteryKwh, decimal HoursCovered, string Message, List<string> Assumptions);