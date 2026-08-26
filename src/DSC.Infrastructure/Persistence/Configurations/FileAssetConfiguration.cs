using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class FileAssetConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("file");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder
            .Property(x => x.OriginalName)
            .HasColumnName("original_name")
            .HasMaxLength(255)
            .IsRequired();

        builder
            .Property(x => x.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.MimeType).HasColumnName("mime_type").HasMaxLength(120).IsRequired();

        builder.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();

        builder.Property(x => x.Sha256Hash).HasColumnName("sha256_hash").HasMaxLength(64);

        builder.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id");

        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder
            .HasOne(x => x.UploadedByUser)
            .WithMany(u => u.UploadedFiles)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
