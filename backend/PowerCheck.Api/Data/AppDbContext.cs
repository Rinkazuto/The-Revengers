using Microsoft.EntityFrameworkCore;
using PowerCheck.Api.Models;

namespace PowerCheck.Api.Data;

// EF Core's map of the database: one DbSet per table.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TariffTier> TariffTiers => Set<TariffTier>();
    public DbSet<MinimumCharge> MinimumCharges => Set<MinimumCharge>();
    public DbSet<MeterRental> MeterRentals => Set<MeterRental>();
    public DbSet<SolarSchemeParameter> SolarSchemeParameters => Set<SolarSchemeParameter>();
    public DbSet<SolarSystemPrice> SolarSystemPrices => Set<SolarSystemPrice>();
    public DbSet<PvgisMonthlyYield> PvgisMonthlyYields => Set<PvgisMonthlyYield>();
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<ProsumerTariff> ProsumerTariffs => Set<ProsumerTariff>();
    public DbSet<ProsumerTariffBand> ProsumerTariffBands => Set<ProsumerTariffBand>();
    public DbSet<ProsumerMinimumCharge> ProsumerMinimumCharges => Set<ProsumerMinimumCharge>();

    // Unique indexes stop the same row being loaded twice.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TariffTier>()
            .HasIndex(t => new { t.TariffCode, t.EffectiveFrom, t.Tier }).IsUnique();
        modelBuilder.Entity<MinimumCharge>()
            .HasIndex(m => new { m.TariffCode, m.EffectiveFrom }).IsUnique();
        modelBuilder.Entity<MeterRental>()
            .HasIndex(m => new { m.ConsumerType, m.MeterType, m.EffectiveFrom }).IsUnique();
        modelBuilder.Entity<SolarSchemeParameter>()
            .HasIndex(p => p.Parameter).IsUnique();
        modelBuilder.Entity<SolarSystemPrice>()
            .HasIndex(p => p.SystemKwp).IsUnique();
        modelBuilder.Entity<PvgisMonthlyYield>()
            .HasIndex(y => y.Month).IsUnique();
        modelBuilder.Entity<Reading>()
            .HasIndex(r => r.Month).IsUnique();   // one reading per month, as the contract says
        modelBuilder.Entity<ProsumerTariff>()
            .HasIndex(p => new { p.TariffCode, p.EffectiveFrom }).IsUnique();
        modelBuilder.Entity<ProsumerTariffBand>()
            .HasIndex(b => new { b.TariffCode, b.EffectiveFrom, b.Tier }).IsUnique();
        modelBuilder.Entity<ProsumerMinimumCharge>()
            .HasIndex(m => new { m.TariffCode, m.EffectiveFrom, m.MaxTotalKwh }).IsUnique();
    }
}
