using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.ProductService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusAndShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "carrier",
                table: "customer_orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "shipped_at",
                table: "customer_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "customer_orders",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Placed");

            migrationBuilder.AddColumn<string>(
                name: "tracking_number",
                table: "customer_orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "carrier",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "shipped_at",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "status",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "tracking_number",
                table: "customer_orders");
        }
    }
}
