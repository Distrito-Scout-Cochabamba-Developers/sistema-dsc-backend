using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrainingStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_training_level_training_stage_training_stage_id",
                table: "training_level"
            );

            migrationBuilder.DropTable(name: "training_stage");

            migrationBuilder.DropIndex(
                name: "IX_training_level_training_stage_id",
                table: "training_level"
            );

            migrationBuilder.DropColumn(name: "training_stage_id", table: "training_level");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "training_stage_id",
                table: "training_level",
                type: "smallint",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "training_stage",
                columns: table => new
                {
                    id = table
                        .Column<short>(type: "smallint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    code = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    name = table.Column<string>(
                        type: "character varying(80)",
                        maxLength: 80,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_stage", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_level_training_stage_id",
                table: "training_level",
                column: "training_stage_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_stage_code",
                table: "training_stage",
                column: "code",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_training_level_training_stage_training_stage_id",
                table: "training_level",
                column: "training_stage_id",
                principalTable: "training_stage",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
