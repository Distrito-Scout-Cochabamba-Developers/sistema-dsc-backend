using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DSC.Infrastructure.Persistence.Migrations;

/// <summary>Agrega los códigos requeridos sin alterar los identificadores existentes.</summary>
[DbContext(typeof(ScoutDbContext))]
[Migration("20261124180000_SecretariatCatalogs")]
public sealed class SecretariatCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        -- Ids explícitos (max+1) y secuencia sincronizada: una instalación con filas insertadas a mano no colisiona.
        INSERT INTO person_status (id, code, name) OVERRIDING SYSTEM VALUE
            SELECT COALESCE((SELECT MAX(id) FROM person_status), 0) + 1, 'ACTIVE', 'Activa'
            WHERE NOT EXISTS (SELECT 1 FROM person_status WHERE code = 'ACTIVE');
        INSERT INTO contact_purpose_type (id, code, name) OVERRIDING SYSTEM VALUE
            SELECT COALESCE((SELECT MAX(id) FROM contact_purpose_type), 0) + 1, 'PERSONAL', 'Personal'
            WHERE NOT EXISTS (SELECT 1 FROM contact_purpose_type WHERE code = 'PERSONAL');
        INSERT INTO contact_mechanism_type (id, code, name) OVERRIDING SYSTEM VALUE
            SELECT COALESCE((SELECT MAX(id) FROM contact_mechanism_type), 0) + 1, 'EMAIL', 'Correo'
            WHERE NOT EXISTS (SELECT 1 FROM contact_mechanism_type WHERE code = 'EMAIL');
        INSERT INTO contact_mechanism_type (id, code, name) OVERRIDING SYSTEM VALUE
            SELECT COALESCE((SELECT MAX(id) FROM contact_mechanism_type), 0) + 1, 'CELLPHONE', 'Celular'
            WHERE NOT EXISTS (SELECT 1 FROM contact_mechanism_type WHERE code = 'CELLPHONE');
        DO $$ BEGIN PERFORM setval(pg_get_serial_sequence('person_status', 'id'), (SELECT MAX(id) FROM person_status)); END $$;
        DO $$ BEGIN PERFORM setval(pg_get_serial_sequence('contact_purpose_type', 'id'), (SELECT MAX(id) FROM contact_purpose_type)); END $$;
        DO $$ BEGIN PERFORM setval(pg_get_serial_sequence('contact_mechanism_type', 'id'), (SELECT MAX(id) FROM contact_mechanism_type)); END $$;
        -- Respaldo atómico del bloqueo por CI: NULL y vacío cuentan como el mismo complemento.
        CREATE UNIQUE INDEX IF NOT EXISTS ux_person_ci_extension ON person (id_card, COALESCE(id_card_extension, ''));
        """);

    // Solo se revierte el índice; no se eliminan catálogos que pueden estar referenciados o preexistían.
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP INDEX IF EXISTS ux_person_ci_extension;");
}
