using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingModuleSessionCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "training_module_session",
                type: "integer",
                nullable: true
            );

            migrationBuilder.Sql(
                """
                UPDATE training_module_session AS session
                SET capacity = first_facility.capacity
                FROM (
                    SELECT DISTINCT ON (block.training_module_session_id)
                        block.training_module_session_id,
                        CASE WHEN facility.capacity < 0 THEN 0 ELSE facility.capacity END AS capacity
                    FROM training_module_schedule AS block
                    JOIN facility ON facility.id = block.facility_id
                    ORDER BY block.training_module_session_id, block.part_number,
                        block.session_date, block.start_time, block.id
                ) AS first_facility
                WHERE session.id = first_facility.training_module_session_id;
                """
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_module_session_capacity",
                table: "training_module_session",
                sql: "capacity >= 0"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_training_module_session_capacity",
                table: "training_module_session"
            );

            migrationBuilder.DropColumn(name: "capacity", table: "training_module_session");
        }
    }
}
