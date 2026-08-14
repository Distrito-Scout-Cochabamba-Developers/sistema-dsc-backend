using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branch");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class ScoutGroupConfiguration : IEntityTypeConfiguration<ScoutGroup>
{
    public void Configure(EntityTypeBuilder<ScoutGroup> builder)
    {
        builder.ToTable("scout_group");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.FoundationDate)
            .HasColumnName("foundation_date");

        builder.Property(x => x.FacilityId)
            .HasColumnName("facility_id");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(x => x.Facility)
            .WithMany(f => f.ScoutGroups)
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ScoutGroupContactMechanismConfiguration : IEntityTypeConfiguration<ScoutGroupContactMechanism>
{
    public void Configure(EntityTypeBuilder<ScoutGroupContactMechanism> builder)
    {
        builder.ToTable("scout_group_contact_mechanism");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ScoutGroupId)
            .HasColumnName("scout_group_id")
            .IsRequired();

        builder.Property(x => x.ContactMechanismTypeId)
            .HasColumnName("contact_mechanism_type_id")
            .IsRequired();

        builder.Property(x => x.ContactPurposeTypeId)
            .HasColumnName("contact_purpose_type_id")
            .IsRequired();

        builder.Property(x => x.ContactValue)
            .HasColumnName("contact_value")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne(x => x.ScoutGroup)
            .WithMany(sg => sg.ContactMechanisms)
            .HasForeignKey(x => x.ScoutGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactMechanismType)
            .WithMany(cmt => cmt.ScoutGroupContactMechanisms)
            .HasForeignKey(x => x.ContactMechanismTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactPurposeType)
            .WithMany(cpt => cpt.ScoutGroupContactMechanisms)
            .HasForeignKey(x => x.ContactPurposeTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ScoutPositionConfiguration : IEntityTypeConfiguration<ScoutPosition>
{
    public void Configure(EntityTypeBuilder<ScoutPosition> builder)
    {
        builder.ToTable("scout_position");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class ScoutAffiliationConfiguration : IEntityTypeConfiguration<ScoutAffiliation>
{
    public void Configure(EntityTypeBuilder<ScoutAffiliation> builder)
    {
        builder.ToTable("scout_affiliation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PersonId)
            .HasColumnName("person_id")
            .IsRequired();

        builder.Property(x => x.ScoutGroupId)
            .HasColumnName("scout_group_id")
            .IsRequired();

        builder.Property(x => x.BranchId)
            .HasColumnName("branch_id");

        builder.Property(x => x.ScoutPositionId)
            .HasColumnName("scout_position_id");

        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(x => x.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(x => x.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(x => x.Person)
            .WithMany(p => p.Affiliations)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ScoutGroup)
            .WithMany(sg => sg.Affiliations)
            .HasForeignKey(x => x.ScoutGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany(b => b.Affiliations)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ScoutPosition)
            .WithMany(sp => sp.Affiliations)
            .HasForeignKey(x => x.ScoutPositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
