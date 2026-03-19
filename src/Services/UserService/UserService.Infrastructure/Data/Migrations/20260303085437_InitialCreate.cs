using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmCart.UserService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cognito_sub = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone_verified = table.Column<bool>(type: "boolean", nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    auth_provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "email"),
                    google_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    facebook_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "customer"),
                    permissions = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preferences = table.Column<string>(type: "jsonb", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    total_orders = table.Column<int>(type: "integer", nullable: false),
                    total_spent = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    loyalty_points = table.Column<int>(type: "integer", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_verification_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_verification_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_reset_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    password_reset_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    lockout_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "shipping"),
                    label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address_line2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    landmark = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "India"),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "IN"),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    is_default_shipping = table.Column<bool>(type: "boolean", nullable: false),
                    is_default_billing = table.Column<bool>(type: "boolean", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    gstin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_order_updates = table.Column<bool>(type: "boolean", nullable: false),
                    email_shipping_updates = table.Column<bool>(type: "boolean", nullable: false),
                    email_promotions = table.Column<bool>(type: "boolean", nullable: false),
                    email_newsletters = table.Column<bool>(type: "boolean", nullable: false),
                    email_price_drops = table.Column<bool>(type: "boolean", nullable: false),
                    email_back_in_stock = table.Column<bool>(type: "boolean", nullable: false),
                    sms_order_updates = table.Column<bool>(type: "boolean", nullable: false),
                    sms_delivery_updates = table.Column<bool>(type: "boolean", nullable: false),
                    sms_promotions = table.Column<bool>(type: "boolean", nullable: false),
                    sms_otp = table.Column<bool>(type: "boolean", nullable: false),
                    push_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    push_order_updates = table.Column<bool>(type: "boolean", nullable: false),
                    push_promotions = table.Column<bool>(type: "boolean", nullable: false),
                    push_chat_messages = table.Column<bool>(type: "boolean", nullable: false),
                    whatsapp_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    whatsapp_order_updates = table.Column<bool>(type: "boolean", nullable: false),
                    promotion_frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    device_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activity_description = table.Column<string>(type: "text", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_activity_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_activity_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cognito_session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    device_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    browser = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    os = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_active_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_addresses_user_id",
                table: "addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_user_id",
                table: "notification_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id_expires_at",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_activity_logs_user_id",
                table: "user_activity_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_cognito_sub",
                table: "users",
                column: "cognito_sub",
                unique: true,
                filter: "cognito_sub IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_activity_logs");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
