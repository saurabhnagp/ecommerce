using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.UserService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTwitterIdAndSocialIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "twitter_id",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_facebook_id",
                table: "users",
                column: "facebook_id",
                unique: true,
                filter: "facebook_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_google_id",
                table: "users",
                column: "google_id",
                unique: true,
                filter: "google_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_twitter_id",
                table: "users",
                column: "twitter_id",
                unique: true,
                filter: "twitter_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_facebook_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_google_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_twitter_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "twitter_id",
                table: "users");
        }
    }
}
