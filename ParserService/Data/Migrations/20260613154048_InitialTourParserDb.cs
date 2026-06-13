using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParserService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialTourParserDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Priority = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FrontendId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperatorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_countries_operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperatorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_currencies_operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "placements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AdultsCount = table.Column<int>(type: "integer", nullable: false),
                    ChildrenCount = table.Column<int>(type: "integer", nullable: false),
                    OperatorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_placements_operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FrontendId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tours_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FrontendId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TourId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_regions_tours_TourId",
                        column: x => x.TourId,
                        principalTable: "tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "directions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartureRegionId = table.Column<int>(type: "integer", nullable: false),
                    ArrivalRegionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_directions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_directions_regions_ArrivalRegionId",
                        column: x => x.ArrivalRegionId,
                        principalTable: "regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_directions_regions_DepartureRegionId",
                        column: x => x.DepartureRegionId,
                        principalTable: "regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FrontendId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hotels_regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartureCountryId = table.Column<int>(type: "integer", nullable: false),
                    DepartureRegionId = table.Column<int>(type: "integer", nullable: false),
                    ArrivalCountryId = table.Column<int>(type: "integer", nullable: false),
                    ArrivalRegionId = table.Column<int>(type: "integer", nullable: false),
                    HotelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_price_types_countries_ArrivalCountryId",
                        column: x => x.ArrivalCountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_price_types_countries_DepartureCountryId",
                        column: x => x.DepartureCountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_price_types_hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_price_types_regions_ArrivalRegionId",
                        column: x => x.ArrivalRegionId,
                        principalTable: "regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_price_types_regions_DepartureRegionId",
                        column: x => x.DepartureRegionId,
                        principalTable: "regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PriceTypeId = table.Column<int>(type: "integer", nullable: false),
                    PlacementId = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nights = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prices_currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prices_placements_PlacementId",
                        column: x => x.PlacementId,
                        principalTable: "placements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prices_price_types_PriceTypeId",
                        column: x => x.PriceTypeId,
                        principalTable: "price_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_countries_OperatorId",
                table: "countries",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_currencies_OperatorId",
                table: "currencies",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_directions_ArrivalRegionId",
                table: "directions",
                column: "ArrivalRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_directions_DepartureRegionId",
                table: "directions",
                column: "DepartureRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_hotels_RegionId",
                table: "hotels",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_placements_OperatorId",
                table: "placements",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_price_types_ArrivalCountryId",
                table: "price_types",
                column: "ArrivalCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_price_types_ArrivalRegionId",
                table: "price_types",
                column: "ArrivalRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_price_types_DepartureCountryId",
                table: "price_types",
                column: "DepartureCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_price_types_DepartureRegionId",
                table: "price_types",
                column: "DepartureRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_price_types_HotelId",
                table: "price_types",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_prices_CurrencyId",
                table: "prices",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_prices_PlacementId",
                table: "prices",
                column: "PlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_prices_PriceTypeId_Date_Nights",
                table: "prices",
                columns: new[] { "PriceTypeId", "Date", "Nights" });

            migrationBuilder.CreateIndex(
                name: "IX_prices_UpdatedAt",
                table: "prices",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_regions_TourId",
                table: "regions",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_tours_CountryId",
                table: "tours",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "directions");

            migrationBuilder.DropTable(
                name: "prices");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "placements");

            migrationBuilder.DropTable(
                name: "price_types");

            migrationBuilder.DropTable(
                name: "hotels");

            migrationBuilder.DropTable(
                name: "regions");

            migrationBuilder.DropTable(
                name: "tours");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "operators");
        }
    }
}
