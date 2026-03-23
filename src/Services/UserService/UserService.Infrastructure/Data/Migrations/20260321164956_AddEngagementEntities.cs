using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.UserService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEngagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "newsletter_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    subscribed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unsubscribed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletter_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "testimonials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    comment = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testimonials", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contact_messages_created_at",
                table: "contact_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_newsletter_subscriptions_email",
                table: "newsletter_subscriptions",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_is_active",
                table: "testimonials",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_sort_order",
                table: "testimonials",
                column: "sort_order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_messages");

            migrationBuilder.DropTable(
                name: "newsletter_subscriptions");

            migrationBuilder.DropTable(
                name: "testimonials");
        }
    }
}
