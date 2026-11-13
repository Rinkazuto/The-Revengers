using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerCheck.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeterRentals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsumerType = table.Column<string>(type: "TEXT", nullable: false),
                    MeterType = table.Column<string>(type: "TEXT", nullable: false),
                    RentRsPerMonth = table.Column<decimal>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterRentals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MinimumCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TariffCode = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MinimumRs = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinimumCharges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PvgisMonthlyYields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    EmKwhPerKwp = table.Column<decimal>(type: "TEXT", nullable: false),
                    HiMKwhPerM2 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SdMKwh = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvgisMonthlyYields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Month = table.Column<string>(type: "TEXT", nullable: false),
                    Kwh = table.Column<decimal>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolarSchemeParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Parameter = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    AppliesTo = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    SourcePage = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarSchemeParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolarSystemPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SystemKwp = table.Column<double>(type: "REAL", nullable: false),
                    BatteryKwh = table.Column<double>(type: "REAL", nullable: false),
                    PriceMinRs = table.Column<decimal>(type: "TEXT", nullable: false),
                    PriceMaxRs = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarSystemPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TariffTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TariffCode = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tier = table.Column<int>(type: "INTEGER", nullable: false),
                    FromKwh = table.Column<int>(type: "INTEGER", nullable: false),
                    ToKwh = table.Column<int>(type: "INTEGER", nullable: true),
                    RateRsPerKwh = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffTiers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeterRentals_ConsumerType_MeterType_EffectiveFrom",
                table: "MeterRentals",
                columns: new[] { "ConsumerType", "MeterType", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinimumCharges_TariffCode_EffectiveFrom",
                table: "MinimumCharges",
                columns: new[] { "TariffCode", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PvgisMonthlyYields_Month",
                table: "PvgisMonthlyYields",
                column: "Month",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readings_Month",
                table: "Readings",
                column: "Month",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolarSchemeParameters_Parameter",
                table: "SolarSchemeParameters",
                column: "Parameter",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolarSystemPrices_SystemKwp",
                table: "SolarSystemPrices",
                column: "SystemKwp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TariffTiers_TariffCode_EffectiveFrom_Tier",
                table: "TariffTiers",
                columns: new[] { "TariffCode", "EffectiveFrom", "Tier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeterRentals");

            migrationBuilder.DropTable(
                name: "MinimumCharges");

            migrationBuilder.DropTable(
                name: "PvgisMonthlyYields");

            migrationBuilder.DropTable(
                name: "Readings");

            migrationBuilder.DropTable(
                name: "SolarSchemeParameters");

            migrationBuilder.DropTable(
                name: "SolarSystemPrices");

            migrationBuilder.DropTable(
                name: "TariffTiers");
        }
    }
}
