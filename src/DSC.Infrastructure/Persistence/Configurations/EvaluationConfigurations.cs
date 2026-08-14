using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class EnrollmentSourceConfiguration : IEntityTypeConfiguration<EnrollmentSource>
{
    public void Configure(EntityTypeBuilder<EnrollmentSource> builder)
    {
        builder.ToTable("enrollment_source");

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

public class TrainingModuleEnrollmentConfiguration : IEntityTypeConfiguration<TrainingModuleEnrollment>
{
    public void Configure(EntityTypeBuilder<TrainingModuleEnrollment> builder)
    {
        builder.ToTable("training_module_enrollment");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleSessionId)
            .HasColumnName("training_module_session_id")
            .IsRequired();

        builder.Property(x => x.PersonId)
            .HasColumnName("person_id")
            .IsRequired();

        builder.Property(x => x.EnrollmentSourceId)
            .HasColumnName("enrollment_source_id")
            .IsRequired();

        builder.Property(x => x.Attended)
            .HasColumnName("attended")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.AttendanceRegisteredAt)
            .HasColumnName("attendance_registered_at");

        builder.Property(x => x.RegisteredByUserId)
            .HasColumnName("registered_by_user_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => new { x.TrainingModuleSessionId, x.PersonId }).IsUnique();

        builder.HasOne(x => x.Session)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(x => x.TrainingModuleSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Person)
            .WithMany(p => p.Enrollments)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EnrollmentSource)
            .WithMany(es => es.Enrollments)
            .HasForeignKey(x => x.EnrollmentSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RegisteredByUser)
            .WithMany(u => u.RegisteredEnrollments)
            .HasForeignKey(x => x.RegisteredByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EvaluatorTypeConfiguration : IEntityTypeConfiguration<EvaluatorType>
{
    public void Configure(EntityTypeBuilder<EvaluatorType> builder)
    {
        builder.ToTable("evaluator_type");

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

public class EvaluationCriterionConfiguration : IEntityTypeConfiguration<EvaluationCriterion>
{
    public void Configure(EntityTypeBuilder<EvaluationCriterion> builder)
    {
        builder.ToTable("evaluation_criterion");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.EvaluatorTypeId)
            .HasColumnName("evaluator_type_id");

        builder.Property(x => x.MaxScore)
            .HasColumnName("max_score")
            .HasPrecision(5, 2)
            .HasDefaultValue(100m)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.EvaluatorType)
            .WithMany(et => et.Criteria)
            .HasForeignKey(x => x.EvaluatorTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrainingModuleEvaluationConfiguration : IEntityTypeConfiguration<TrainingModuleEvaluation>
{
    public void Configure(EntityTypeBuilder<TrainingModuleEvaluation> builder)
    {
        builder.ToTable("training_module_evaluation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleInstructorId)
            .HasColumnName("training_module_instructor_id")
            .IsRequired();

        builder.Property(x => x.EvaluatorTypeId)
            .HasColumnName("evaluator_type_id")
            .IsRequired();

        builder.Property(x => x.TrainingModuleEnrollmentId)
            .HasColumnName("training_module_enrollment_id");

        builder.Property(x => x.EvaluatorPersonId)
            .HasColumnName("evaluator_person_id");

        builder.Property(x => x.GeneralFeedback)
            .HasColumnName("general_feedback");

        builder.Property(x => x.AverageScore)
            .HasColumnName("average_score")
            .HasPrecision(5, 2);

        builder.HasIndex(x => new { x.TrainingModuleInstructorId, x.TrainingModuleEnrollmentId }).IsUnique();

        builder.HasOne(x => x.Instructor)
            .WithMany(i => i.Evaluations)
            .HasForeignKey(x => x.TrainingModuleInstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EvaluatorType)
            .WithMany(et => et.Evaluations)
            .HasForeignKey(x => x.EvaluatorTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Enrollment)
            .WithMany(e => e.GivenEvaluations)
            .HasForeignKey(x => x.TrainingModuleEnrollmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.EvaluatorPerson)
            .WithMany(p => p.GivenEvaluations)
            .HasForeignKey(x => x.EvaluatorPersonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TrainingModuleEvaluationDetailConfiguration : IEntityTypeConfiguration<TrainingModuleEvaluationDetail>
{
    public void Configure(EntityTypeBuilder<TrainingModuleEvaluationDetail> builder)
    {
        builder.ToTable("training_module_evaluation_detail");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TrainingModuleEvaluationId)
            .HasColumnName("training_module_evaluation_id")
            .IsRequired();

        builder.Property(x => x.CriterionId)
            .HasColumnName("criterion_id")
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnName("score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.TrainingModuleEvaluationId, x.CriterionId }).IsUnique();

        builder.HasOne(x => x.Evaluation)
            .WithMany(e => e.EvaluationDetails)
            .HasForeignKey(x => x.TrainingModuleEvaluationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Criterion)
            .WithMany(c => c.EvaluationDetails)
            .HasForeignKey(x => x.CriterionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
