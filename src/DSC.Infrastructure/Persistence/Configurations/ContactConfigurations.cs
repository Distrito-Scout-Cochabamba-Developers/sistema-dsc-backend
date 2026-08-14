using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class ContactMechanismTypeConfiguration : IEntityTypeConfiguration<ContactMechanismType>
{
    public void Configure(EntityTypeBuilder<ContactMechanismType> builder)
    {
        builder.ToTable("contact_mechanism_type");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class ContactPurposeTypeConfiguration : IEntityTypeConfiguration<ContactPurposeType>
{
    public void Configure(EntityTypeBuilder<ContactPurposeType> builder)
    {
        builder.ToTable("contact_purpose_type");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
