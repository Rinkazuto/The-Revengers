using Microsoft.EntityFrameworkCore;
using PowerCheck.Api.Data;
using PowerCheck.Api.Models;

// Reads the real CEB tariff and 2026 solar scheme data from the SQLite database.
// It returns the same shapes as FakeTariffData and FakeSolarData, so the endpoints
// can swap one for the other without changing the bill or solar maths.
// Every figure comes from data/seed (see data/sources/README.md). None is typed here.
public class TariffData(AppDbContext db)
{
    public const string DefaultTariffCode = "110";

    // The tariff codes that have bands in the database: 110, 110A, 120, 140.
    public List<string> TariffCodes() =>
        db.TariffTiers.AsNoTracking()
            .Select(t => t.TariffCode).Distinct().OrderBy(c => c).ToList();

    public bool IsKnownTariff(string? tariffCode) =>
        TariffCodes().Contains(tariffCode ?? DefaultTariffCode);

    // The bands and minimum charge in force on a date (default: today).
    // "Newest effective_from wins": the latest start date on or before that date is used.
    // Returns null when the tariff had no rates on that date, e.g. tariff 120 before 1 May 2026.
    public TariffSnapshot? GetTariff(string? tariffCode = null, DateTime? onDate = null)
    {
        var code = tariffCode ?? DefaultTariffCode;
        var day = (onDate ?? DateTime.UtcNow).Date;

        // Dates are compared in memory: the tariff tables hold 60 and 7 rows, so this is instant.
        var tiers = db.TariffTiers.AsNoTracking().Where(t => t.TariffCode == code).ToList();
        var effectiveFrom = tiers.Select(t => t.EffectiveFrom.Date).Where(d => d <= day)
            .DefaultIfEmpty(DateTime.MinValue).Max();
        if (effectiveFrom == DateTime.MinValue) return null;

        var bands = tiers.Where(t => t.EffectiveFrom.Date == effectiveFrom)
            .OrderBy(t => t.Tier)
            .Select(t => new TariffBandDto(t.FromKwh, t.ToKwh, t.RateRsPerKwh))
            .ToList();

        var minimum = db.MinimumCharges.AsNoTracking().Where(m => m.TariffCode == code).ToList()
            .SingleOrDefault(m => m.EffectiveFrom.Date == effectiveFrom);
        if (minimum is null) return null;

        return new TariffSnapshot(
            code,
            effectiveFrom,
            bands,
            minimum.MinimumRs,
            $"CEB domestic tariff {code}, in force from {effectiveFrom:d MMMM yyyy}",
            SourceUrlFor(effectiveFrom));
    }

    // The 2026 rates are on the CEB tariffs page. The 2023 rates are in General Notice 1804 of 2022.
    private static string SourceUrlFor(DateTime effectiveFrom) =>
        effectiveFrom.Year >= 2026
            ? "https://ceb.mu/tariffs"
            : "https://ceb.mu/docs/files/files/tariffs/Tariff_Schedule_Feb2023_URA.pdf";

    // The 2026 household solar scheme values, PVGIS output and CEB system prices.
    public SolarSchemeSnapshot GetSolarScheme()
    {
        var p = db.SolarSchemeParameters.AsNoTracking().ToDictionary(x => x.Parameter, x => x.Value);
        var months = db.PvgisMonthlyYields.AsNoTracking().OrderBy(y => y.Month)
            .Select(y => new MonthlyYieldDto(y.Month, y.EmKwhPerKwp)).ToList();
        var prices = db.SolarSystemPrices.AsNoTracking().OrderBy(x => x.SystemKwp)
            .Select(x => new SystemPriceDto((decimal)x.SystemKwp, (decimal)x.BatteryKwh, x.PriceMinRs, x.PriceMaxRs))
            .ToList();

        return new SolarSchemeSnapshot(
            // PVGIS reports 113.71 as its average monthly output; the 12 monthly values average 113.7075.
            AverageKwhPerKwpPerMonth: Math.Round(months.Average(m => m.KwhPerKwp), 2, MidpointRounding.AwayFromZero),
            MonthlyKwhPerKwp: months,
            GrossMeteringExportRateRs: p["gross_metering_export_rate"],
            NetMeteringExcessExportRateRs: p["net_metering_excess_export_rate"],
            GrantSharePercent: p["grant_share"],
            GrantCapRs: p["grant_cap"],
            ProcessingFeeRs: p["processing_fee"],
            AnnualPanelDegradationPercent: p["annual_panel_degradation"],
            BatteryRequiredAboveKwac: p["battery_required_above"],
            NetworkSurveyRequiredAboveKwac: p["network_survey_required_above"],
            MaxInstallationKw: p["max_installation_capacity"],
            ContractTermYears: (int)p["contract_term_years"],
            SystemPrices: prices,
            Source: "CEB Household Rooftop Solar PV Scheme leaflet (July 2026); CEB solar price guide; PVGIS 5.3, European Commission JRC");
    }
}

// The tariff in force on one date: bands cheapest first, 0-based (0-25, 25-50 ...).
public record TariffSnapshot(
    string TariffCode, DateTime EffectiveFrom, List<TariffBandDto> Bands,
    decimal MinimumChargeRs, string Source, string SourceUrl);

public record MonthlyYieldDto(int Month, decimal KwhPerKwp);

// CEB's indicative price range for one system size, with a 5 kWh battery.
public record SystemPriceDto(decimal SystemKwp, decimal BatteryKwh, decimal PriceMinRs, decimal PriceMaxRs);

public record SolarSchemeSnapshot(
    decimal AverageKwhPerKwpPerMonth,
    List<MonthlyYieldDto> MonthlyKwhPerKwp,
    decimal GrossMeteringExportRateRs,
    decimal NetMeteringExcessExportRateRs,
    decimal GrantSharePercent,
    decimal GrantCapRs,
    decimal ProcessingFeeRs,
    decimal AnnualPanelDegradationPercent,
    decimal BatteryRequiredAboveKwac,
    decimal NetworkSurveyRequiredAboveKwac,
    decimal MaxInstallationKw,
    int ContractTermYears,             // years the CEB credits solar output (leaflet page 8)
    List<SystemPriceDto> SystemPrices,
    string Source);
