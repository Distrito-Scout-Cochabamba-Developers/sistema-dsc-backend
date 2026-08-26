using System;
using DSC.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DSC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'gender_type') THEN
                        CREATE TYPE gender_type AS ENUM ('female', 'male', 'other');
                    END IF;
                END
                $$;
            "
            );

            migrationBuilder.CreateTable(
                name: "branch",
                columns: table => new
                {
                    id = table
                        .Column<short>(type: "smallint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "contact_mechanism_type",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_mechanism_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "contact_purpose_type",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_purpose_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "enrollment_source",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_source", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "evaluator_type",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluator_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "facility_type",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "leader_training_status",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leader_training_status", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "module_type",
                columns: table => new
                {
                    id = table
                        .Column<short>(type: "smallint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    code = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "person_status",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_status", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table
                        .Column<short>(type: "smallint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    code = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "scout_position",
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
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scout_position", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_session_status",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_session_status", x => x.id);
                }
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
                    name = table.Column<string>(
                        type: "character varying(80)",
                        maxLength: 80,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_stage", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "user_status",
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
                    name = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_status", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "evaluation_criterion",
                columns: table => new
                {
                    id = table
                        .Column<short>(type: "smallint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    code = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    evaluator_type_id = table.Column<short>(type: "smallint", nullable: true),
                    max_score = table.Column<decimal>(
                        type: "numeric(5,2)",
                        precision: 5,
                        scale: 2,
                        nullable: false,
                        defaultValue: 100m
                    ),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluation_criterion", x => x.id);
                    table.ForeignKey(
                        name: "FK_evaluation_criterion_evaluator_type_evaluator_type_id",
                        column: x => x.evaluator_type_id,
                        principalTable: "evaluator_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "facility",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    facility_type_id = table.Column<short>(type: "smallint", nullable: false),
                    parent_facility_id = table.Column<long>(type: "bigint", nullable: true),
                    code = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: true
                    ),
                    name = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    physical_address = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    capacity = table.Column<short>(type: "smallint", nullable: true),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility", x => x.id);
                    table.ForeignKey(
                        name: "FK_facility_facility_parent_facility_id",
                        column: x => x.parent_facility_id,
                        principalTable: "facility",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_facility_facility_type_facility_type_id",
                        column: x => x.facility_type_id,
                        principalTable: "facility_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    code = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    description = table.Column<string>(type: "text", nullable: true),
                    module_type_id = table.Column<short>(type: "smallint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_module_type_module_type_id",
                        column: x => x.module_type_id,
                        principalTable: "module_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_level",
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
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    module_type_id = table.Column<short>(type: "smallint", nullable: true),
                    training_stage_id = table.Column<short>(type: "smallint", nullable: true),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_level", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_level_module_type_module_type_id",
                        column: x => x.module_type_id,
                        principalTable: "module_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_level_training_stage_training_stage_id",
                        column: x => x.training_stage_id,
                        principalTable: "training_stage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "scout_group",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    name = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    foundation_date = table.Column<DateOnly>(type: "date", nullable: true),
                    facility_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scout_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_scout_group_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facility",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_session",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_id = table.Column<long>(type: "bigint", nullable: false),
                    training_module_session_status_id = table.Column<short>(
                        type: "smallint",
                        nullable: false
                    ),
                    qr_token = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "gen_random_uuid()"
                    ),
                    is_registration_open = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_session_training_module_session_status_trai~",
                        column: x => x.training_module_session_status_id,
                        principalTable: "training_module_session_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_session_training_module_training_module_id",
                        column: x => x.training_module_id,
                        principalTable: "training_module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "scout_group_contact_mechanism",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    scout_group_id = table.Column<long>(type: "bigint", nullable: false),
                    contact_mechanism_type_id = table.Column<short>(
                        type: "smallint",
                        nullable: false
                    ),
                    contact_purpose_type_id = table.Column<short>(
                        type: "smallint",
                        nullable: false
                    ),
                    contact_value = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    is_primary = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scout_group_contact_mechanism", x => x.id);
                    table.ForeignKey(
                        name: "FK_scout_group_contact_mechanism_contact_mechanism_type_contac~",
                        column: x => x.contact_mechanism_type_id,
                        principalTable: "contact_mechanism_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_group_contact_mechanism_contact_purpose_type_contact_~",
                        column: x => x.contact_purpose_type_id,
                        principalTable: "contact_purpose_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_group_contact_mechanism_scout_group_scout_group_id",
                        column: x => x.scout_group_id,
                        principalTable: "scout_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_schedule",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_session_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    facility_id = table.Column<long>(type: "bigint", nullable: true),
                    part_number = table.Column<short>(
                        type: "smallint",
                        nullable: false,
                        defaultValue: (short)1
                    ),
                    part_title = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: true
                    ),
                    session_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(
                        type: "time without time zone",
                        nullable: false
                    ),
                    end_time = table.Column<TimeOnly>(
                        type: "time without time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_schedule", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_schedule_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facility",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_schedule_training_module_session_training_m~",
                        column: x => x.training_module_session_id,
                        principalTable: "training_module_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "file",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    original_name = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    storage_path = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    mime_type = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256_hash = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    uploaded_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "person",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    id_card = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    id_card_extension = table.Column<string>(
                        type: "character varying(5)",
                        maxLength: 5,
                        nullable: true
                    ),
                    first_name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    middle_name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: true
                    ),
                    first_last_name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    second_last_name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: true
                    ),
                    gender = table.Column<GenderType>(type: "gender_type", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    photo_file_id = table.Column<long>(type: "bigint", nullable: true),
                    person_status_id = table.Column<short>(type: "smallint", nullable: false),
                    is_profile_complete = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_person_file_photo_file_id",
                        column: x => x.photo_file_id,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_person_person_status_person_status_id",
                        column: x => x.person_status_id,
                        principalTable: "person_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ModuleAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingModuleId = table.Column<long>(type: "bigint", nullable: false),
                    ScoutLeaderId = table.Column<long>(type: "bigint", nullable: false),
                    RegisteredAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleAttendances_person_ScoutLeaderId",
                        column: x => x.ScoutLeaderId,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ModuleAttendances_training_module_TrainingModuleId",
                        column: x => x.TrainingModuleId,
                        principalTable: "training_module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "person_contact_mechanism",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    contact_mechanism_type_id = table.Column<short>(
                        type: "smallint",
                        nullable: false
                    ),
                    contact_purpose_type_id = table.Column<short>(
                        type: "smallint",
                        nullable: false
                    ),
                    contact_value = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    is_primary = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person_contact_mechanism", x => x.id);
                    table.ForeignKey(
                        name: "FK_person_contact_mechanism_contact_mechanism_type_contact_mec~",
                        column: x => x.contact_mechanism_type_id,
                        principalTable: "contact_mechanism_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_person_contact_mechanism_contact_purpose_type_contact_purpo~",
                        column: x => x.contact_purpose_type_id,
                        principalTable: "contact_purpose_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_person_contact_mechanism_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "scout_affiliation",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    scout_group_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<short>(type: "smallint", nullable: true),
                    scout_position_id = table.Column<short>(type: "smallint", nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    is_primary = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scout_affiliation", x => x.id);
                    table.ForeignKey(
                        name: "FK_scout_affiliation_branch_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_affiliation_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_affiliation_scout_group_scout_group_id",
                        column: x => x.scout_group_id,
                        principalTable: "scout_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_affiliation_scout_position_scout_position_id",
                        column: x => x.scout_position_id,
                        principalTable: "scout_position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "scout_leader",
                columns: table => new
                {
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    is_speaker_eligible = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scout_leader", x => x.person_id);
                    table.ForeignKey(
                        name: "FK_scout_leader_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_instructor",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_session_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    invited_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    is_confirmed = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    confirmed_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_instructor", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_instructor_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_instructor_training_module_session_training~",
                        column: x => x.training_module_session_id,
                        principalTable: "training_module_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_lesson_plan_template",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_id = table.Column<long>(type: "bigint", nullable: false),
                    author_person_id = table.Column<long>(type: "bigint", nullable: true),
                    version_code = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "1.0"
                    ),
                    general_objective = table.Column<string>(type: "text", nullable: true),
                    specific_objectives = table.Column<string>(type: "text", nullable: true),
                    bibliography_references = table.Column<string>(type: "text", nullable: true),
                    slide_file_id = table.Column<long>(type: "bigint", nullable: true),
                    document_file_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_lesson_plan_template", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_template_file_document_file_id",
                        column: x => x.document_file_id,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_template_file_slide_file_id",
                        column: x => x.slide_file_id,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_template_person_author_person_id",
                        column: x => x.author_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_template_training_module_traini~",
                        column: x => x.training_module_id,
                        principalTable: "training_module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    username = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    email = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    password = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    user_status_id = table.Column<short>(type: "smallint", nullable: false),
                    is_email_verified = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_user_user_status_user_status_id",
                        column: x => x.user_status_id,
                        principalTable: "user_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "scout_leader_training_level",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    training_level_id = table.Column<short>(type: "smallint", nullable: false),
                    status_id = table.Column<short>(type: "smallint", nullable: false),
                    status_date = table.Column<DateOnly>(type: "date", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scout_leader_training_level", x => x.id);
                    table.ForeignKey(
                        name: "FK_scout_leader_training_level_leader_training_status_status_id",
                        column: x => x.status_id,
                        principalTable: "leader_training_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_leader_training_level_scout_leader_person_id",
                        column: x => x.person_id,
                        principalTable: "scout_leader",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_scout_leader_training_level_training_level_training_level_id",
                        column: x => x.training_level_id,
                        principalTable: "training_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_lesson_plan",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_session_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    derived_from_template_id = table.Column<long>(type: "bigint", nullable: true),
                    author_person_id = table.Column<long>(type: "bigint", nullable: true),
                    general_objective = table.Column<string>(type: "text", nullable: true),
                    specific_objectives = table.Column<string>(type: "text", nullable: true),
                    bibliography_references = table.Column<string>(type: "text", nullable: true),
                    slide_file_id = table.Column<long>(type: "bigint", nullable: true),
                    document_file_id = table.Column<long>(type: "bigint", nullable: true),
                    is_submitted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    submitted_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_lesson_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_file_document_file_id",
                        column: x => x.document_file_id,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_file_slide_file_id",
                        column: x => x.slide_file_id,
                        principalTable: "file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_person_author_person_id",
                        column: x => x.author_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_training_module_lesson_plan_tem~",
                        column: x => x.derived_from_template_id,
                        principalTable: "training_module_lesson_plan_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_training_module_session_trainin~",
                        column: x => x.training_module_session_id,
                        principalTable: "training_module_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_lesson_plan_template_activity",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    template_id = table.Column<long>(type: "bigint", nullable: false),
                    step_number = table.Column<short>(
                        type: "smallint",
                        nullable: false,
                        defaultValue: (short)1
                    ),
                    duration_minutes = table.Column<short>(type: "smallint", nullable: false),
                    activity_name = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    recommended_materials = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_lesson_plan_template_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_template_activity_training_modu~",
                        column: x => x.template_id,
                        principalTable: "training_module_lesson_plan_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_enrollment",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_session_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    person_id = table.Column<long>(type: "bigint", nullable: false),
                    enrollment_source_id = table.Column<short>(type: "smallint", nullable: false),
                    attended = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    attendance_registered_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    registered_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_enrollment", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_enrollment_enrollment_source_enrollment_sou~",
                        column: x => x.enrollment_source_id,
                        principalTable: "enrollment_source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_enrollment_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_enrollment_training_module_session_training~",
                        column: x => x.training_module_session_id,
                        principalTable: "training_module_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_enrollment_user_registered_by_user_id",
                        column: x => x.registered_by_user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_session_status_history",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_session_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    status_id = table.Column<short>(type: "smallint", nullable: false),
                    changed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    reason = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    changed_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_session_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_session_status_history_training_module_sess~",
                        column: x => x.status_id,
                        principalTable: "training_module_session_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_session_status_history_training_module_ses~1",
                        column: x => x.training_module_session_id,
                        principalTable: "training_module_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_session_status_history_user_changed_by_user~",
                        column: x => x.changed_by_user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<short>(type: "smallint", nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_role_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_user_role_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_lesson_plan_activity",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    lesson_plan_id = table.Column<long>(type: "bigint", nullable: false),
                    step_number = table.Column<short>(
                        type: "smallint",
                        nullable: false,
                        defaultValue: (short)1
                    ),
                    duration_minutes = table.Column<short>(type: "smallint", nullable: false),
                    activity_name = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    materials = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_lesson_plan_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_lesson_plan_activity_training_module_lesson~",
                        column: x => x.lesson_plan_id,
                        principalTable: "training_module_lesson_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_evaluation",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_instructor_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    evaluator_type_id = table.Column<short>(type: "smallint", nullable: false),
                    training_module_enrollment_id = table.Column<long>(
                        type: "bigint",
                        nullable: true
                    ),
                    evaluator_person_id = table.Column<long>(type: "bigint", nullable: true),
                    general_feedback = table.Column<string>(type: "text", nullable: true),
                    average_score = table.Column<decimal>(
                        type: "numeric(5,2)",
                        precision: 5,
                        scale: 2,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_evaluation", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_evaluation_evaluator_type_evaluator_type_id",
                        column: x => x.evaluator_type_id,
                        principalTable: "evaluator_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_evaluation_person_evaluator_person_id",
                        column: x => x.evaluator_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_evaluation_training_module_enrollment_train~",
                        column: x => x.training_module_enrollment_id,
                        principalTable: "training_module_enrollment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_training_module_evaluation_training_module_instructor_train~",
                        column: x => x.training_module_instructor_id,
                        principalTable: "training_module_instructor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "training_module_evaluation_detail",
                columns: table => new
                {
                    id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    training_module_evaluation_id = table.Column<long>(
                        type: "bigint",
                        nullable: false
                    ),
                    criterion_id = table.Column<short>(type: "smallint", nullable: false),
                    score = table.Column<decimal>(
                        type: "numeric(5,2)",
                        precision: 5,
                        scale: 2,
                        nullable: false
                    ),
                    comment = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_module_evaluation_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_module_evaluation_detail_evaluation_criterion_crit~",
                        column: x => x.criterion_id,
                        principalTable: "evaluation_criterion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_training_module_evaluation_detail_training_module_evaluatio~",
                        column: x => x.training_module_evaluation_id,
                        principalTable: "training_module_evaluation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_branch_name",
                table: "branch",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_contact_mechanism_type_code",
                table: "contact_mechanism_type",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_contact_purpose_type_code",
                table: "contact_purpose_type",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_source_code",
                table: "enrollment_source",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_evaluation_criterion_code",
                table: "evaluation_criterion",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_evaluation_criterion_evaluator_type_id",
                table: "evaluation_criterion",
                column: "evaluator_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_evaluator_type_code",
                table: "evaluator_type",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_facility_code",
                table: "facility",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_facility_facility_type_id",
                table: "facility",
                column: "facility_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_facility_parent_facility_id",
                table: "facility",
                column: "parent_facility_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_facility_type_code",
                table: "facility_type",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_file_uploaded_by_user_id",
                table: "file",
                column: "uploaded_by_user_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_leader_training_status_code",
                table: "leader_training_status",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_module_type_code",
                table: "module_type",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ModuleAttendances_ScoutLeaderId",
                table: "ModuleAttendances",
                column: "ScoutLeaderId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ModuleAttendances_TrainingModuleId_ScoutLeaderId",
                table: "ModuleAttendances",
                columns: new[] { "TrainingModuleId", "ScoutLeaderId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_id_card_id_card_extension",
                table: "person",
                columns: new[] { "id_card", "id_card_extension" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_person_status_id",
                table: "person",
                column: "person_status_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_photo_file_id",
                table: "person",
                column: "photo_file_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_contact_mechanism_contact_mechanism_type_id",
                table: "person_contact_mechanism",
                column: "contact_mechanism_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_contact_mechanism_contact_purpose_type_id",
                table: "person_contact_mechanism",
                column: "contact_purpose_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_contact_mechanism_person_id",
                table: "person_contact_mechanism",
                column: "person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_person_status_code",
                table: "person_status",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_role_code",
                table: "role",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_affiliation_branch_id",
                table: "scout_affiliation",
                column: "branch_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_affiliation_person_id",
                table: "scout_affiliation",
                column: "person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_affiliation_scout_group_id",
                table: "scout_affiliation",
                column: "scout_group_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_affiliation_scout_position_id",
                table: "scout_affiliation",
                column: "scout_position_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_group_facility_id",
                table: "scout_group",
                column: "facility_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_group_contact_mechanism_contact_mechanism_type_id",
                table: "scout_group_contact_mechanism",
                column: "contact_mechanism_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_group_contact_mechanism_contact_purpose_type_id",
                table: "scout_group_contact_mechanism",
                column: "contact_purpose_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_group_contact_mechanism_scout_group_id",
                table: "scout_group_contact_mechanism",
                column: "scout_group_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_leader_training_level_person_id_training_level_id_sta~",
                table: "scout_leader_training_level",
                columns: new[] { "person_id", "training_level_id", "status_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_leader_training_level_status_id",
                table: "scout_leader_training_level",
                column: "status_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_leader_training_level_training_level_id",
                table: "scout_leader_training_level",
                column: "training_level_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_scout_position_code",
                table: "scout_position",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_level_code",
                table: "training_level",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_level_module_type_id",
                table: "training_level",
                column: "module_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_level_training_stage_id",
                table: "training_level",
                column: "training_stage_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_code",
                table: "training_module",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_module_type_id",
                table: "training_module",
                column: "module_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_enrollment_enrollment_source_id",
                table: "training_module_enrollment",
                column: "enrollment_source_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_enrollment_person_id",
                table: "training_module_enrollment",
                column: "person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_enrollment_registered_by_user_id",
                table: "training_module_enrollment",
                column: "registered_by_user_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_enrollment_training_module_session_id_perso~",
                table: "training_module_enrollment",
                columns: new[] { "training_module_session_id", "person_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_evaluation_evaluator_person_id",
                table: "training_module_evaluation",
                column: "evaluator_person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_evaluation_evaluator_type_id",
                table: "training_module_evaluation",
                column: "evaluator_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_evaluation_training_module_enrollment_id",
                table: "training_module_evaluation",
                column: "training_module_enrollment_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_evaluation_training_module_instructor_id_tr~",
                table: "training_module_evaluation",
                columns: new[] { "training_module_instructor_id", "training_module_enrollment_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_evaluation_detail_criterion_id",
                table: "training_module_evaluation_detail",
                column: "criterion_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_evaluation_detail_training_module_evaluatio~",
                table: "training_module_evaluation_detail",
                columns: new[] { "training_module_evaluation_id", "criterion_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_instructor_person_id",
                table: "training_module_instructor",
                column: "person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_instructor_training_module_session_id_perso~",
                table: "training_module_instructor",
                columns: new[] { "training_module_session_id", "person_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_author_person_id",
                table: "training_module_lesson_plan",
                column: "author_person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_derived_from_template_id",
                table: "training_module_lesson_plan",
                column: "derived_from_template_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_document_file_id",
                table: "training_module_lesson_plan",
                column: "document_file_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_slide_file_id",
                table: "training_module_lesson_plan",
                column: "slide_file_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_training_module_session_id",
                table: "training_module_lesson_plan",
                column: "training_module_session_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_activity_lesson_plan_id",
                table: "training_module_lesson_plan_activity",
                column: "lesson_plan_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_template_author_person_id",
                table: "training_module_lesson_plan_template",
                column: "author_person_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_template_document_file_id",
                table: "training_module_lesson_plan_template",
                column: "document_file_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_template_slide_file_id",
                table: "training_module_lesson_plan_template",
                column: "slide_file_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_template_training_module_id",
                table: "training_module_lesson_plan_template",
                column: "training_module_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_lesson_plan_template_activity_template_id",
                table: "training_module_lesson_plan_template_activity",
                column: "template_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_schedule_facility_id",
                table: "training_module_schedule",
                column: "facility_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_schedule_training_module_session_id",
                table: "training_module_schedule",
                column: "training_module_session_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_qr_token",
                table: "training_module_session",
                column: "qr_token",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_training_module_id",
                table: "training_module_session",
                column: "training_module_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_training_module_session_status_id",
                table: "training_module_session",
                column: "training_module_session_status_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_status_code",
                table: "training_module_session_status",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_status_history_changed_by_user_id",
                table: "training_module_session_status_history",
                column: "changed_by_user_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_status_history_status_id",
                table: "training_module_session_status_history",
                column: "status_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_module_session_status_history_training_module_sess~",
                table: "training_module_session_status_history",
                column: "training_module_session_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_training_stage_code",
                table: "training_stage",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_email",
                table: "user",
                column: "email",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_person_id",
                table: "user",
                column: "person_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_user_status_id",
                table: "user",
                column: "user_status_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_username",
                table: "user",
                column: "username",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_role_role_id",
                table: "user_role",
                column: "role_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_user_status_code",
                table: "user_status",
                column: "code",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_file_user_uploaded_by_user_id",
                table: "file",
                column: "uploaded_by_user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_file_user_uploaded_by_user_id",
                table: "file"
            );

            migrationBuilder.DropTable(name: "ModuleAttendances");

            migrationBuilder.DropTable(name: "person_contact_mechanism");

            migrationBuilder.DropTable(name: "scout_affiliation");

            migrationBuilder.DropTable(name: "scout_group_contact_mechanism");

            migrationBuilder.DropTable(name: "scout_leader_training_level");

            migrationBuilder.DropTable(name: "training_module_evaluation_detail");

            migrationBuilder.DropTable(name: "training_module_lesson_plan_activity");

            migrationBuilder.DropTable(name: "training_module_lesson_plan_template_activity");

            migrationBuilder.DropTable(name: "training_module_schedule");

            migrationBuilder.DropTable(name: "training_module_session_status_history");

            migrationBuilder.DropTable(name: "user_role");

            migrationBuilder.DropTable(name: "branch");

            migrationBuilder.DropTable(name: "scout_position");

            migrationBuilder.DropTable(name: "contact_mechanism_type");

            migrationBuilder.DropTable(name: "contact_purpose_type");

            migrationBuilder.DropTable(name: "scout_group");

            migrationBuilder.DropTable(name: "leader_training_status");

            migrationBuilder.DropTable(name: "scout_leader");

            migrationBuilder.DropTable(name: "training_level");

            migrationBuilder.DropTable(name: "evaluation_criterion");

            migrationBuilder.DropTable(name: "training_module_evaluation");

            migrationBuilder.DropTable(name: "training_module_lesson_plan");

            migrationBuilder.DropTable(name: "role");

            migrationBuilder.DropTable(name: "facility");

            migrationBuilder.DropTable(name: "training_stage");

            migrationBuilder.DropTable(name: "evaluator_type");

            migrationBuilder.DropTable(name: "training_module_enrollment");

            migrationBuilder.DropTable(name: "training_module_instructor");

            migrationBuilder.DropTable(name: "training_module_lesson_plan_template");

            migrationBuilder.DropTable(name: "facility_type");

            migrationBuilder.DropTable(name: "enrollment_source");

            migrationBuilder.DropTable(name: "training_module_session");

            migrationBuilder.DropTable(name: "training_module_session_status");

            migrationBuilder.DropTable(name: "training_module");

            migrationBuilder.DropTable(name: "module_type");

            migrationBuilder.DropTable(name: "user");

            migrationBuilder.DropTable(name: "person");

            migrationBuilder.DropTable(name: "user_status");

            migrationBuilder.DropTable(name: "file");

            migrationBuilder.DropTable(name: "person_status");
        }
    }
}
