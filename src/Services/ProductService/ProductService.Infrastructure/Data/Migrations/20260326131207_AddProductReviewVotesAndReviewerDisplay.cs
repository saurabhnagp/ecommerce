using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.ProductService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductReviewVotesAndReviewerDisplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "not_helpful_count",
                table: "product_reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "reviewer_display_name",
                table: "product_reviews",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewer_photo_url",
                table: "product_reviews",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_review_votes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_up = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_review_votes", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_review_votes_product_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "product_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_review_votes_review_id_user_id",
                table: "product_review_votes",
                columns: new[] { "review_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_review_votes_user_id",
                table: "product_review_votes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_review_votes");

            migrationBuilder.DropColumn(
                name: "not_helpful_count",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "reviewer_display_name",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "reviewer_photo_url",
                table: "product_reviews");
        }
    }
}
