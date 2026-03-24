using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.ProductService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCartDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    min_order_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    applied_coupon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.id);
                    table.CheckConstraint("ck_carts_user_or_session", "(user_id IS NOT NULL AND session_id IS NULL) OR (user_id IS NULL AND session_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_carts_coupons_applied_coupon_id",
                        column: x => x.applied_coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_cart_id_product_id",
                table: "cart_items",
                columns: new[] { "cart_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_product_id",
                table: "cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_carts_applied_coupon_id",
                table: "carts",
                column: "applied_coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_carts_session_id_unique",
                table: "carts",
                column: "session_id",
                unique: true,
                filter: "session_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_carts_user_id_unique",
                table: "carts",
                column: "user_id",
                unique: true,
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_coupons_code",
                table: "coupons",
                column: "code",
                unique: true);

            var welcomeId = new Guid("a0000001-0000-4000-8000-000000000001");
            var flatId = new Guid("a0000001-0000-4000-8000-000000000002");
            var validFrom = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var validTo = new DateTime(2035, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "coupons",
                columns: new[] { "id", "code", "discount_type", "discount_value", "min_order_total", "valid_from", "valid_to", "is_active" },
                values: new object[] { welcomeId, "WELCOME10", "Percentage", 10m, null, validFrom, validTo, true });

            migrationBuilder.InsertData(
                table: "coupons",
                columns: new[] { "id", "code", "discount_type", "discount_value", "min_order_total", "valid_from", "valid_to", "is_active" },
                values: new object[] { flatId, "FLAT50", "FixedAmount", 50m, 200m, validFrom, validTo, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "coupons");
        }
    }
}
