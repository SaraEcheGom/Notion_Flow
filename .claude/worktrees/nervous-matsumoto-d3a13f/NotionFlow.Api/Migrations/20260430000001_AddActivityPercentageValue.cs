using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotionFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityPercentageValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PercentageValue",
                table: "Activities",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentageValue",
                table: "Activities");
        }
    }
}
