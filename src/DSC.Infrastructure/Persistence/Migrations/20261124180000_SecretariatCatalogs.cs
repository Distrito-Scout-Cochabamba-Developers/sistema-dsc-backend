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
        -- Verificaciones previas del índice. Nunca imprimen Carnet de Identidad ni datos de personas.
        DO $$
        DECLARE
            existing_def text;
            duplicate_groups bigint;
        BEGIN
            SELECT pg_get_indexdef(i.indexrelid) INTO existing_def
            FROM pg_index i JOIN pg_class c ON c.oid = i.indexrelid
            WHERE c.relname = 'ux_person_ci_extension' AND c.relnamespace = 'public'::regnamespace
              AND i.indisunique AND i.indisvalid;
            IF existing_def IS NULL AND EXISTS (SELECT 1 FROM pg_class WHERE relname = 'ux_person_ci_extension' AND relnamespace = 'public'::regnamespace) THEN
                RAISE EXCEPTION 'Ya existe un objeto llamado ux_person_ci_extension que no es un índice único válido. Elimínelo o renómbrelo y reintente.';
            END IF;
            IF existing_def IS NOT NULL AND existing_def NOT LIKE '%(id_card, COALESCE(id_card_extension, ''''::text))%' THEN
                RAISE EXCEPTION 'El índice ux_person_ci_extension existe con una definición distinta a la esperada. Revíselo manualmente antes de migrar.';
            END IF;
            IF existing_def IS NULL THEN
                SELECT count(*) INTO duplicate_groups FROM (
                    SELECT 1 FROM person GROUP BY id_card, COALESCE(id_card_extension, '') HAVING count(*) > 1
                ) d;
                IF duplicate_groups > 0 THEN
                    RAISE EXCEPTION 'Hay % grupo(s) de personas con Carnet de Identidad duplicados (mismo Carnet de Identidad y complemento). Fusione o corrija los duplicados antes de migrar; consulte docs/database-migrations.md.', duplicate_groups;
                END IF;
            END IF;
        END $$;
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
        -- Respaldo atómico del bloqueo por Carnet de Identidad: NULL y vacío cuentan como el mismo complemento.
        CREATE UNIQUE INDEX IF NOT EXISTS ux_person_ci_extension ON person (id_card, COALESCE(id_card_extension, ''));
        """);

    // Solo se revierte el índice; no se eliminan catálogos que pueden estar referenciados o preexistían.
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP INDEX IF EXISTS ux_person_ci_extension;");
}
