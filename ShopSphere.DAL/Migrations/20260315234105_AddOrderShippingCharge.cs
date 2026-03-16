using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSphere.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCharge",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCharge",
                table: "Orders");
        }
    }
}
