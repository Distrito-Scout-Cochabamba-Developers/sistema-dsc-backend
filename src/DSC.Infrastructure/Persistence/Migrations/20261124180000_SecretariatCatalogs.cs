using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DSC.Infrastructure.Persistence.Migrations;

/// <summary>Agrega los códigos requeridos sin alterar los identificadores existentes.</summary>
[DbContext(typeof(ScoutDbContext))]
[Migration("20261124180000_SecretariatCatalogs")]
public sealed class SecretariatCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        INSERT INTO person_status (code, name) VALUES ('ACTIVE', 'Activa') ON CONFLICT (code) DO NOTHING;
        INSERT INTO contact_purpose_type (code, name) VALUES ('PERSONAL', 'Personal') ON CONFLICT (code) DO NOTHING;
        INSERT INTO contact_mechanism_type (code, name) VALUES ('EMAIL', 'Correo'), ('CELLPHONE', 'Celular') ON CONFLICT (code) DO NOTHING;
        -- Respaldo atómico del bloqueo por CI: NULL y vacío cuentan como el mismo complemento.
        CREATE UNIQUE INDEX IF NOT EXISTS ux_person_ci_extension ON person (id_card, COALESCE(id_card_extension, ''));
        """);

    // Solo se revierte el índice; no se eliminan catálogos que pueden estar referenciados o preexistían.
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DROP INDEX IF EXISTS ux_person_ci_extension;");
}
