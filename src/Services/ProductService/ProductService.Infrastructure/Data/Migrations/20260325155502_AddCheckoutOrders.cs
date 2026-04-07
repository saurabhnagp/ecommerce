using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.ProductService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_method = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    coupon_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    bill_first_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bill_last_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bill_company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bill_country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bill_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    bill_apartment = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bill_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bill_state = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bill_zip = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    bill_phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    bill_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ship_first_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ship_last_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ship_company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ship_country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ship_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ship_apartment = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ship_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ship_state = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ship_zip = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ship_phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ship_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_order_items_customer_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "customer_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_order_items_order_id",
                table: "customer_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_order_items_product_id",
                table: "customer_order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_orders_order_number",
                table: "customer_orders",
                column: "order_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_order_items");

            migrationBuilder.DropTable(
                name: "customer_orders");
        }
    }
}
