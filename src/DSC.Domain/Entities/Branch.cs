namespace DSC.Domain.Entities;

/// <summary>
/// Representa una rama del movimiento scout (Lobatos, Exploradores, Pioneros, Rovers).
/// </summary>
public class Branch
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Colecciones de navegación
    public ICollection<ScoutAffiliation> Affiliations { get; set; } = [];
}
