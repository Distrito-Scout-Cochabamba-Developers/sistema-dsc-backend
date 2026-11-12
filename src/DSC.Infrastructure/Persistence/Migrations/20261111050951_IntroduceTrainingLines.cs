using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations;

public partial class IntroduceTrainingLines : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            "FK_training_level_module_type_module_type_id",
            "training_level"
        );
        migrationBuilder.DropForeignKey(
            "FK_training_module_module_type_module_type_id",
            "training_module"
        );
        migrationBuilder.DropPrimaryKey("PK_module_type", "module_type");
        migrationBuilder.RenameTable("module_type", newName: "training_line");
        migrationBuilder.AddPrimaryKey("PK_training_line", "training_line", "id");
        migrationBuilder.RenameIndex(
            name: "IX_module_type_code",
            table: "training_line",
            newName: "IX_training_line_code"
        );
        migrationBuilder.RenameColumn("module_type_id", "training_level", "training_line_id");
        migrationBuilder.RenameIndex(
            name: "IX_training_level_module_type_id",
            table: "training_level",
            newName: "IX_training_level_training_line_id"
        );

        // No inventar una línea para niveles existentes sin clasificación.
        migrationBuilder.AlterColumn<short>(
            "training_line_id",
            "training_level",
            type: "smallint",
            nullable: false,
            oldClrType: typeof(short),
            oldType: "smallint",
            oldNullable: true
        );

        migrationBuilder.CreateTable(
            name: "training_module_line",
            columns: table => new
            {
                training_module_id = table.Column<long>(type: "bigint", nullable: false),
                training_line_id = table.Column<short>(type: "smallint", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_training_module_line",
                    x => new { x.training_module_id, x.training_line_id }
                );
                table.ForeignKey(
                    "FK_training_module_line_training_line_training_line_id",
                    x => x.training_line_id,
                    "training_line",
                    "id",
                    onDelete: ReferentialAction.Restrict
                );
                table.ForeignKey(
                    "FK_training_module_line_training_module_training_module_id",
                    x => x.training_module_id,
                    "training_module",
                    "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        // Transferencia de datos dentro de la transacción de EF, antes de retirar la FK antigua.
        migrationBuilder.Sql(
            """
            INSERT INTO training_module_line (training_module_id, training_line_id)
            SELECT id, module_type_id FROM training_module;
            """
        );
        migrationBuilder.DropIndex("IX_training_module_module_type_id", "training_module");
        migrationBuilder.DropColumn("module_type_id", "training_module");
        migrationBuilder.CreateIndex(
            "IX_training_module_line_training_line_id",
            "training_module_line",
            "training_line_id"
        );
        migrationBuilder.AddForeignKey(
            "FK_training_level_training_line_training_line_id",
            "training_level",
            "training_line_id",
            "training_line",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // El modelo anterior solo admite una línea. Rechazar un rollback con pérdida de pertenencias.
        migrationBuilder.Sql(
            """
            DO $$ BEGIN
                IF EXISTS (
                    SELECT m.id FROM training_module m
                    LEFT JOIN training_module_line l ON l.training_module_id = m.id
                    GROUP BY m.id HAVING COUNT(l.training_line_id) <> 1
                ) THEN
                    RAISE EXCEPTION 'No se puede revertir: cada módulo debe tener exactamente una línea.';
                END IF;
            END $$;
            """
        );
        migrationBuilder.AddColumn<short>(
            "module_type_id",
            "training_module",
            type: "smallint",
            nullable: true
        );
        migrationBuilder.Sql(
            """
            UPDATE training_module m SET module_type_id = l.training_line_id
            FROM training_module_line l WHERE l.training_module_id = m.id;
            """
        );
        migrationBuilder.AlterColumn<short>(
            "module_type_id",
            "training_module",
            type: "smallint",
            nullable: false,
            oldClrType: typeof(short),
            oldType: "smallint",
            oldNullable: true
        );
        migrationBuilder.DropTable("training_module_line");
        migrationBuilder.DropForeignKey(
            "FK_training_level_training_line_training_line_id",
            "training_level"
        );
        migrationBuilder.AlterColumn<short>(
            "training_line_id",
            "training_level",
            type: "smallint",
            nullable: true,
            oldClrType: typeof(short),
            oldType: "smallint"
        );
        migrationBuilder.RenameColumn("training_line_id", "training_level", "module_type_id");
        migrationBuilder.RenameIndex(
            name: "IX_training_level_training_line_id",
            table: "training_level",
            newName: "IX_training_level_module_type_id"
        );
        migrationBuilder.DropPrimaryKey("PK_training_line", "training_line");
        migrationBuilder.RenameTable("training_line", newName: "module_type");
        migrationBuilder.RenameIndex(
            name: "IX_training_line_code",
            table: "module_type",
            newName: "IX_module_type_code"
        );
        migrationBuilder.AddPrimaryKey("PK_module_type", "module_type", "id");
        migrationBuilder.CreateIndex(
            "IX_training_module_module_type_id",
            "training_module",
            "module_type_id"
        );
        migrationBuilder.AddForeignKey(
            "FK_training_level_module_type_module_type_id",
            "training_level",
            "module_type_id",
            "module_type",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict
        );
        migrationBuilder.AddForeignKey(
            "FK_training_module_module_type_module_type_id",
            "training_module",
            "module_type_id",
            "module_type",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict
        );
    }
}
