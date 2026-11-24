using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountActivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve pre-existing catalog entries and IDs; Down must not remove referenced statuses.
            migrationBuilder.Sql(
                """
                INSERT INTO user_status (code, name)
                VALUES ('PENDING_VERIFICATION', 'Pendiente de verificación'), ('ACTIVE', 'Activo')
                ON CONFLICT (code) DO NOTHING;
                """
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "activation_expires_at",
                table: "user",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "activation_token_hash",
                table: "user",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_activation_token_hash",
                table: "user",
                column: "activation_token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_activation_token_hash",
                table: "user");

            migrationBuilder.DropColumn(
                name: "activation_expires_at",
                table: "user");

            migrationBuilder.DropColumn(
                name: "activation_token_hash",
                table: "user");
        }
    }
}
