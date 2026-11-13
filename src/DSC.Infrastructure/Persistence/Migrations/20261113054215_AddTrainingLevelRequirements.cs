using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingLevelRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "training_level_requirement",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_level_id = table.Column<short>(type: "smallint", nullable: false),
                    requirement_type = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    total_modules_count = table.Column<int>(type: "integer", nullable: false),
                    min_mandatory_modules_count = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_level_requirement", x => x.id);
                    table.CheckConstraint(
                        "ck_training_level_requirement_counts",
                        "total_modules_count >= 0 AND min_mandatory_modules_count >= 0 AND min_mandatory_modules_count <= total_modules_count"
                    );
                    table.ForeignKey(
                        name: "FK_training_level_requirement_training_level_training_level_id",
                        column: x => x.training_level_id,
                        principalTable: "training_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_level_mandatory_module",
                columns: table => new
                {
                    training_level_requirement_id = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    training_module_id = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_training_level_mandatory_module",
                        x => new { x.training_level_requirement_id, x.training_module_id }
                    );
                    table.ForeignKey(
                        name: "FK_training_level_mandatory_module_training_level_requirement_~",
                        column: x => x.training_level_requirement_id,
                        principalTable: "training_level_requirement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_training_level_mandatory_module_training_module_training_mo~",
                        column: x => x.training_module_id,
                        principalTable: "training_module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_level_mandatory_module_training_module_id",
                table: "training_level_mandatory_module",
                column: "training_module_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_level_requirement_training_level_id_requirement_ty~",
                table: "training_level_requirement",
                columns: new[] { "training_level_id", "requirement_type" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "training_level_mandatory_module");

            migrationBuilder.DropTable(name: "training_level_requirement");
        }
    }
}
