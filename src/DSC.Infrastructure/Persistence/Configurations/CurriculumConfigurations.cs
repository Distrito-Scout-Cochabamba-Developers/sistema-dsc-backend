using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class ModuleTypeConfiguration : IEntityTypeConfiguration<ModuleType>
{
    public void Configure(EntityTypeBuilder<ModuleType> builder)
    {
        builder.ToTable("module_type");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class TrainingStageConfiguration : IEntityTypeConfiguration<TrainingStage>
{
    public void Configure(EntityTypeBuilder<TrainingStage> builder)
    {
        builder.ToTable("training_stage");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class TrainingLevelConfiguration : IEntityTypeConfiguration<TrainingLevel>
{
    public void Configure(EntityTypeBuilder<TrainingLevel> builder)
    {
        builder.ToTable("training_level");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.Property(x => x.ModuleTypeId).HasColumnName("module_type_id");

        builder.Property(x => x.TrainingStageId).HasColumnName("training_stage_id");

        builder
            .Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasOne(x => x.ModuleType)
            .WithMany(mt => mt.TrainingLevels)
            .HasForeignKey(x => x.ModuleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.TrainingStage)
            .WithMany(ts => ts.TrainingLevels)
            .HasForeignKey(x => x.TrainingStageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingModuleConfiguration : IEntityTypeConfiguration<TrainingModule>
{
    public void Configure(EntityTypeBuilder<TrainingModule> builder)
    {
        builder.ToTable("training_module");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description");

        builder.Property(x => x.ModuleTypeId).HasColumnName("module_type_id").IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasOne(x => x.ModuleType)
            .WithMany(mt => mt.TrainingModules)
            .HasForeignKey(x => x.ModuleTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingModuleLessonPlanTemplateConfiguration
    : IEntityTypeConfiguration<TrainingModuleLessonPlanTemplate>
{
    public void Configure(EntityTypeBuilder<TrainingModuleLessonPlanTemplate> builder)
    {
        builder.ToTable("training_module_lesson_plan_template");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleId).HasColumnName("training_module_id").IsRequired();

        builder.Property(x => x.AuthorPersonId).HasColumnName("author_person_id");

        builder
            .Property(x => x.VersionCode)
            .HasColumnName("version_code")
            .HasMaxLength(20)
            .HasDefaultValue("1.0")
            .IsRequired();

        builder.Property(x => x.GeneralObjective).HasColumnName("general_objective");

        builder.Property(x => x.SpecificObjectives).HasColumnName("specific_objectives");

        builder.Property(x => x.BibliographyReferences).HasColumnName("bibliography_references");

        builder.Property(x => x.SlideFileId).HasColumnName("slide_file_id");

        builder.Property(x => x.DocumentFileId).HasColumnName("document_file_id");

        builder
            .Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder
            .HasOne(x => x.TrainingModule)
            .WithMany(tm => tm.LessonPlanTemplates)
            .HasForeignKey(x => x.TrainingModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.AuthorPerson)
            .WithMany(p => p.LessonPlanTemplates)
            .HasForeignKey(x => x.AuthorPersonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.SlideFile)
            .WithMany()
            .HasForeignKey(x => x.SlideFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.DocumentFile)
            .WithMany()
            .HasForeignKey(x => x.DocumentFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TrainingModuleLessonPlanTemplateActivityConfiguration
    : IEntityTypeConfiguration<TrainingModuleLessonPlanTemplateActivity>
{
    public void Configure(EntityTypeBuilder<TrainingModuleLessonPlanTemplateActivity> builder)
    {
        builder.ToTable("training_module_lesson_plan_template_activity");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TemplateId).HasColumnName("template_id").IsRequired();

        builder
            .Property(x => x.StepNumber)
            .HasColumnName("step_number")
            .HasDefaultValue((short)1)
            .IsRequired();

        builder.Property(x => x.DurationMinutes).HasColumnName("duration_minutes").IsRequired();

        builder
            .Property(x => x.ActivityName)
            .HasColumnName("activity_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.RecommendedMaterials).HasColumnName("recommended_materials");

        builder
            .HasOne(x => x.Template)
            .WithMany(t => t.Activities)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
