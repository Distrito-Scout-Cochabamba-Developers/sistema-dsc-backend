using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoverEnrollmentModality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "rover_without_account",
                table: "training_module_enrollment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rover_without_account",
                table: "training_module_enrollment");
        }
    }
}
