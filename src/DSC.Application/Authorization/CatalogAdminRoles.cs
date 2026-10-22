namespace DSC.Application.Authorization;

/// <summary>
/// Roles que pueden administrar el catálogo curricular (`role.code` / claim JWT `roles`).
/// </summary>
public static class CatalogAdminRoles
{
    public const string Admin = "ADMIN";
    public const string DistrictSecretary = "DISTRICT_SECRETARY";

    /// <summary>
    /// Valor de <c>[Authorize(Roles = ...)]</c>: cualquiera de los dos roles basta.
    /// </summary>
    public const string PolicyRoles = $"{Admin},{DistrictSecretary}";
}
