using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class test12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavProducts",
                table: "FavProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavMerchants",
                table: "FavMerchants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Carts",
                table: "Carts");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FavProducts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FavMerchants",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Carts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavProducts",
                table: "FavProducts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavMerchants",
                table: "FavMerchants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carts",
                table: "Carts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FavProducts_productId",
                table: "FavProducts",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_FavMerchants_merchantId",
                table: "FavMerchants",
                column: "merchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_productId",
                table: "Carts",
                column: "productId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavProducts",
                table: "FavProducts");

            migrationBuilder.DropIndex(
                name: "IX_FavProducts_productId",
                table: "FavProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavMerchants",
                table: "FavMerchants");

            migrationBuilder.DropIndex(
                name: "IX_FavMerchants_merchantId",
                table: "FavMerchants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Carts",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_productId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FavProducts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FavMerchants");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Carts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavProducts",
                table: "FavProducts",
                columns: new[] { "productId", "customerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavMerchants",
                table: "FavMerchants",
                columns: new[] { "merchantId", "customerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carts",
                table: "Carts",
                columns: new[] { "productId", "colorId", "sizeId", "customerId" });
        }
    }
}
