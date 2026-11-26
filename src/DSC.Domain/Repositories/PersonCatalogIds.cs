namespace DSC.Domain.Repositories;

/// <summary>Ids de catálogo de módulos resueltos por código (nunca fijos): persona activa, celular y propósito personal.</summary>
public sealed record PersonCatalogIds(short ActiveStatusId, short CellphoneTypeId, short PersonalPurposeId);
