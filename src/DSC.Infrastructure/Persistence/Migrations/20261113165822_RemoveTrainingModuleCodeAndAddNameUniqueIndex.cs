using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrainingModuleCodeAndAddNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_training_module_code", table: "training_module");

            migrationBuilder.DropColumn(name: "code", table: "training_module");

            migrationBuilder.CreateIndex(
                name: "IX_training_module_name",
                table: "training_module",
                column: "name",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_training_module_name", table: "training_module");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "training_module",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_code",
                table: "training_module",
                column: "code",
                unique: true
            );
        }
    }
}
