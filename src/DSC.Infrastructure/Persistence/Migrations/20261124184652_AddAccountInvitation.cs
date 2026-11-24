using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_invitation",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_failure_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_invitation", x => x.id);
                    table.CheckConstraint("ck_account_invitation_status", "status IN ('PENDING','SENT','CLOSED')");
                    table.ForeignKey(
                        name: "FK_account_invitation_training_module_enrollment_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "training_module_enrollment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_invitation_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_invitation_enrollment_id",
                table: "account_invitation",
                column: "enrollment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_invitation_status",
                table: "account_invitation",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_account_invitation_user_id",
                table: "account_invitation",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_invitation");
        }
    }
}
