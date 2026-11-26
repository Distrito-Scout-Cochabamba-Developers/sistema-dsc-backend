using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFormationCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "formation_condition",
                table: "training_module_enrollment",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "formation_condition",
                table: "person",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "formation_condition",
                table: "training_module_enrollment");

            migrationBuilder.DropColumn(
                name: "formation_condition",
                table: "person");
        }
    }
}
