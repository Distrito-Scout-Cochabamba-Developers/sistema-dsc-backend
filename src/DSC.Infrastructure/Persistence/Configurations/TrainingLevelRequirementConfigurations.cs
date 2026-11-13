using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class TrainingLevelRequirementConfiguration
    : IEntityTypeConfiguration<TrainingLevelRequirement>
{
    public void Configure(EntityTypeBuilder<TrainingLevelRequirement> builder)
    {
        builder.ToTable(
            "training_level_requirement",
            table =>
                table.HasCheckConstraint(
                    "ck_training_level_requirement_counts",
                    "total_modules_count >= 0 AND min_mandatory_modules_count >= 0 AND min_mandatory_modules_count <= total_modules_count"
                )
        );
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TrainingLevelId).HasColumnName("training_level_id");
        builder
            .Property(x => x.RequirementType)
            .HasColumnName("requirement_type")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.TotalModulesCount).HasColumnName("total_modules_count");
        builder
            .Property(x => x.MinMandatoryModulesCount)
            .HasColumnName("min_mandatory_modules_count");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(x => new { x.TrainingLevelId, x.RequirementType }).IsUnique();
        builder
            .HasOne(x => x.TrainingLevel)
            .WithMany(x => x.Requirements)
            .HasForeignKey(x => x.TrainingLevelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .Navigation(x => x.MandatoryModules)
            .HasField("_mandatoryModules")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class TrainingLevelMandatoryModuleConfiguration
    : IEntityTypeConfiguration<TrainingLevelMandatoryModule>
{
    public void Configure(EntityTypeBuilder<TrainingLevelMandatoryModule> builder)
    {
        builder.ToTable("training_level_mandatory_module");
        builder.HasKey(x => new { x.TrainingLevelRequirementId, x.TrainingModuleId });
        builder
            .Property(x => x.TrainingLevelRequirementId)
            .HasColumnName("training_level_requirement_id");
        builder.Property(x => x.TrainingModuleId).HasColumnName("training_module_id");
        builder
            .HasOne(x => x.TrainingLevelRequirement)
            .WithMany(x => x.MandatoryModules)
            .HasForeignKey(x => x.TrainingLevelRequirementId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.TrainingModule)
            .WithMany(x => x.MandatoryForLevelRequirements)
            .HasForeignKey(x => x.TrainingModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
