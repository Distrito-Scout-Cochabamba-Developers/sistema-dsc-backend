using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class TrainingModuleSessionStatusConfiguration : IEntityTypeConfiguration<TrainingModuleSessionStatus>
{
    public void Configure(EntityTypeBuilder<TrainingModuleSessionStatus> builder)
    {
        builder.ToTable("training_module_session_status");

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

public class TrainingModuleSessionConfiguration : IEntityTypeConfiguration<TrainingModuleSession>
{
    public void Configure(EntityTypeBuilder<TrainingModuleSession> builder)
    {
        builder.ToTable("training_module_session");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleId)
            .HasColumnName("training_module_id")
            .IsRequired();

        builder.Property(x => x.TrainingModuleSessionStatusId)
            .HasColumnName("training_module_session_status_id")
            .IsRequired();

        builder.Property(x => x.QrToken)
            .HasColumnName("qr_token")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(x => x.IsRegistrationOpen)
            .HasColumnName("is_registration_open")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => x.QrToken).IsUnique();

        builder.HasOne(x => x.TrainingModule)
            .WithMany(tm => tm.Sessions)
            .HasForeignKey(x => x.TrainingModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany(s => s.Sessions)
            .HasForeignKey(x => x.TrainingModuleSessionStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LessonPlan)
            .WithOne(lp => lp.Session)
            .HasForeignKey<TrainingModuleLessonPlan>(lp => lp.TrainingModuleSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingModuleSessionStatusHistoryConfiguration : IEntityTypeConfiguration<TrainingModuleSessionStatusHistory>
{
    public void Configure(EntityTypeBuilder<TrainingModuleSessionStatusHistory> builder)
    {
        builder.ToTable("training_module_session_status_history");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleSessionId)
            .HasColumnName("training_module_session_id")
            .IsRequired();

        builder.Property(x => x.StatusId)
            .HasColumnName("status_id")
            .IsRequired();

        builder.Property(x => x.ChangedByUserId)
            .HasColumnName("changed_by_user_id");

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(255);

        builder.Property(x => x.ChangedAt)
            .HasColumnName("changed_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(x => x.Session)
            .WithMany(s => s.StatusHistories)
            .HasForeignKey(x => x.TrainingModuleSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Status)
            .WithMany(st => st.StatusHistories)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany(u => u.StatusChanges)
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TrainingModuleScheduleConfiguration : IEntityTypeConfiguration<TrainingModuleSchedule>
{
    public void Configure(EntityTypeBuilder<TrainingModuleSchedule> builder)
    {
        builder.ToTable("training_module_schedule");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleSessionId)
            .HasColumnName("training_module_session_id")
            .IsRequired();

        builder.Property(x => x.FacilityId)
            .HasColumnName("facility_id");

        builder.Property(x => x.PartNumber)
            .HasColumnName("part_number")
            .HasDefaultValue((short)1)
            .IsRequired();

        builder.Property(x => x.PartTitle)
            .HasColumnName("part_title")
            .HasMaxLength(150);

        builder.Property(x => x.SessionDate)
            .HasColumnName("session_date")
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.HasOne(x => x.Session)
            .WithMany(s => s.Schedules)
            .HasForeignKey(x => x.TrainingModuleSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Facility)
            .WithMany(f => f.Schedules)
            .HasForeignKey(x => x.FacilityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TrainingModuleInstructorConfiguration : IEntityTypeConfiguration<TrainingModuleInstructor>
{
    public void Configure(EntityTypeBuilder<TrainingModuleInstructor> builder)
    {
        builder.ToTable("training_module_instructor");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleSessionId)
            .HasColumnName("training_module_session_id")
            .IsRequired();

        builder.Property(x => x.PersonId)
            .HasColumnName("person_id")
            .IsRequired();

        builder.Property(x => x.InvitedAt)
            .HasColumnName("invited_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.IsConfirmed)
            .HasColumnName("is_confirmed")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.HasIndex(x => new { x.TrainingModuleSessionId, x.PersonId }).IsUnique();

        builder.HasOne(x => x.Session)
            .WithMany(s => s.Instructors)
            .HasForeignKey(x => x.TrainingModuleSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Person)
            .WithMany(p => p.InstructorAssignments)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingModuleLessonPlanConfiguration : IEntityTypeConfiguration<TrainingModuleLessonPlan>
{
    public void Configure(EntityTypeBuilder<TrainingModuleLessonPlan> builder)
    {
        builder.ToTable("training_module_lesson_plan");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleSessionId)
            .HasColumnName("training_module_session_id")
            .IsRequired();

        builder.Property(x => x.DerivedFromTemplateId)
            .HasColumnName("derived_from_template_id");

        builder.Property(x => x.AuthorPersonId)
            .HasColumnName("author_person_id");

        builder.Property(x => x.GeneralObjective)
            .HasColumnName("general_objective");

        builder.Property(x => x.SpecificObjectives)
            .HasColumnName("specific_objectives");

        builder.Property(x => x.BibliographyReferences)
            .HasColumnName("bibliography_references");

        builder.Property(x => x.SlideFileId)
            .HasColumnName("slide_file_id");

        builder.Property(x => x.DocumentFileId)
            .HasColumnName("document_file_id");

        builder.Property(x => x.IsSubmitted)
            .HasColumnName("is_submitted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.HasIndex(x => x.TrainingModuleSessionId).IsUnique();

        builder.HasOne(x => x.DerivedFromTemplate)
            .WithMany(t => t.DerivedLessonPlans)
            .HasForeignKey(x => x.DerivedFromTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AuthorPerson)
            .WithMany(p => p.LessonPlans)
            .HasForeignKey(x => x.AuthorPersonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SlideFile)
            .WithMany()
            .HasForeignKey(x => x.SlideFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.DocumentFile)
            .WithMany()
            .HasForeignKey(x => x.DocumentFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TrainingModuleLessonPlanActivityConfiguration : IEntityTypeConfiguration<TrainingModuleLessonPlanActivity>
{
    public void Configure(EntityTypeBuilder<TrainingModuleLessonPlanActivity> builder)
    {
        builder.ToTable("training_module_lesson_plan_activity");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LessonPlanId)
            .HasColumnName("lesson_plan_id")
            .IsRequired();

        builder.Property(x => x.StepNumber)
            .HasColumnName("step_number")
            .HasDefaultValue((short)1)
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .HasColumnName("duration_minutes")
            .IsRequired();

        builder.Property(x => x.ActivityName)
            .HasColumnName("activity_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Materials)
            .HasColumnName("materials");

        builder.HasOne(x => x.LessonPlan)
            .WithMany(lp => lp.Activities)
            .HasForeignKey(x => x.LessonPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
