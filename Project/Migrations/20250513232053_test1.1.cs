using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class test11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAddedCart",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "IsAddedFav",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "IsBrowsing",
                table: "Histories");

            migrationBuilder.AddColumn<int>(
                name: "event_type",
                table: "Histories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "event_type",
                table: "Histories");

            migrationBuilder.AddColumn<bool>(
                name: "IsAddedCart",
                table: "Histories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddedFav",
                table: "Histories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBrowsing",
                table: "Histories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
