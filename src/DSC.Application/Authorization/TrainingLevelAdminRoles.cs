namespace DSC.Application.Authorization;

/// <summary>
/// Roles que pueden administrar requisitos por nivel de formación (`role.code` / claim JWT `roles`).
/// </summary>
public static class TrainingLevelAdminRoles
{
    public const string AdultResourcesCoordinator = "ADULT_RESOURCES_COORDINATOR";
    public const string DistrictDirector = "DISTRICT_DIRECTOR";
    public const string DistrictSecretary = "DISTRICT_SECRETARY";

    /// <summary>
    /// Valor de <c>[Authorize(Roles = ...)]</c>: cualquiera de los tres roles basta.
    /// </summary>
    public const string PolicyRoles =
        $"{AdultResourcesCoordinator},{DistrictDirector},{DistrictSecretary}";
}
