namespace PowerCheck.Web.Api;

// The 5 endpoints from docs/api-contract.md.
// Pages use this. They don't know if the data is fake or real.
public interface IPowerCheckApi
{
    Task<TariffsResponse> GetTariffsAsync(string? tariffCode = null);   // GET  /api/tariffs?tariffCode=110
    Task<BillResponse> CalculateBillAsync(BillRequest request);         // POST /api/bill/calculate
    Task<SolarResponse> EstimateSolarAsync(SolarRequest request);       // POST /api/solar/estimate
    Task<ReadingDto> SaveReadingAsync(ReadingRequest request);          // POST /api/readings
    Task<ReadingsResponse> GetReadingsAsync();                          // GET  /api/readings

    // Cyclone mode (surprise card "Made for Mauritius")
    Task<AppliancesResponse> GetAppliancesAsync();                      // GET  /api/cyclone/appliances
    Task<BackupResponse> CalculateBackupAsync(BackupRequest request);   // POST /api/cyclone/backup                          // GET  /api/readings
}