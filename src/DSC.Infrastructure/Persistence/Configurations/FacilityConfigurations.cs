using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class FacilityTypeConfiguration : IEntityTypeConfiguration<FacilityType>
{
    public void Configure(EntityTypeBuilder<FacilityType> builder)
    {
        builder.ToTable("facility_type");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("facility");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FacilityTypeId).HasColumnName("facility_type_id").IsRequired();

        builder.Property(x => x.ParentFacilityId).HasColumnName("parent_facility_id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30);

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description");

        builder
            .Property(x => x.PhysicalAddress)
            .HasColumnName("physical_address")
            .HasMaxLength(255);

        builder.Property(x => x.Capacity).HasColumnName("capacity");

        builder
            .Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasOne(x => x.FacilityType)
            .WithMany(ft => ft.Facilities)
            .HasForeignKey(x => x.FacilityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ParentFacility)
            .WithMany(pf => pf.ChildFacilities)
            .HasForeignKey(x => x.ParentFacilityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
