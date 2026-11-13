using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerCheck.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProsumerTariffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProsumerMinimumCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TariffCode = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxTotalKwh = table.Column<decimal>(type: "TEXT", nullable: true),
                    MinimumRs = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsumerMinimumCharges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProsumerTariffBands",
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
                    table.PrimaryKey("PK_ProsumerTariffBands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProsumerTariffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TariffCode = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Phase = table.Column<string>(type: "TEXT", nullable: false),
                    FlatRateMaxTotalKwh = table.Column<decimal>(type: "TEXT", nullable: true),
                    FlatRateRsPerKwh = table.Column<decimal>(type: "TEXT", nullable: true),
                    SourcePage = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsumerTariffs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProsumerMinimumCharges_TariffCode_EffectiveFrom_MaxTotalKwh",
                table: "ProsumerMinimumCharges",
                columns: new[] { "TariffCode", "EffectiveFrom", "MaxTotalKwh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsumerTariffBands_TariffCode_EffectiveFrom_Tier",
                table: "ProsumerTariffBands",
                columns: new[] { "TariffCode", "EffectiveFrom", "Tier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsumerTariffs_TariffCode_EffectiveFrom",
                table: "ProsumerTariffs",
                columns: new[] { "TariffCode", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProsumerMinimumCharges");

            migrationBuilder.DropTable(
                name: "ProsumerTariffBands");

            migrationBuilder.DropTable(
                name: "ProsumerTariffs");
        }
    }
}
