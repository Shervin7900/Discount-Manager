using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscountManager.Modules.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPriceToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SalesPrice",
                schema: "catalog",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesPrice",
                schema: "catalog",
                table: "Products");
        }
    }
}
