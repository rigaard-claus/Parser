using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParserService.Data.Migrations.TourParser
{
    /// <inheritdoc />
    public partial class AddAiResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiResponse",
                table: "ai_request_logs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiResponse",
                table: "ai_request_logs");
        }
    }
}
