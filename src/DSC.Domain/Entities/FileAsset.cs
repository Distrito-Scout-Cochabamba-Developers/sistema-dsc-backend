namespace DSC.Domain.Entities;

/// <summary>
/// Representa los metadatos y la referencia física/URI de un archivo almacenado externamente.
/// </summary>
public class FileAsset
{
    public long Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Sha256Hash { get; set; }
    public long? UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User? UploadedByUser { get; set; }
}
