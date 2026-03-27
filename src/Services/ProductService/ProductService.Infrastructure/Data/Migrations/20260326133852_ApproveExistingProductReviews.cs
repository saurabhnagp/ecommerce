using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.ProductService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApproveExistingProductReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Storefront list only returns approved rows; older inserts may have stayed false.
            migrationBuilder.Sql(
                "UPDATE product_reviews SET is_approved = true WHERE is_approved = false;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
