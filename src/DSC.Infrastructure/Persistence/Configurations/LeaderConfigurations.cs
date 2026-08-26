using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class ScoutLeaderConfiguration : IEntityTypeConfiguration<ScoutLeader>
{
    public void Configure(EntityTypeBuilder<ScoutLeader> builder)
    {
        builder.ToTable("scout_leader");

        builder.HasKey(x => x.PersonId);
        builder.Property(x => x.PersonId).HasColumnName("person_id");

        builder
            .Property(x => x.IsSpeakerEligible)
            .HasColumnName("is_speaker_eligible")
            .HasDefaultValue(false)
            .IsRequired();
    }
}

public class LeaderTrainingStatusConfiguration : IEntityTypeConfiguration<LeaderTrainingStatus>
{
    public void Configure(EntityTypeBuilder<LeaderTrainingStatus> builder)
    {
        builder.ToTable("leader_training_status");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class ScoutLeaderTrainingLevelConfiguration
    : IEntityTypeConfiguration<ScoutLeaderTrainingLevel>
{
    public void Configure(EntityTypeBuilder<ScoutLeaderTrainingLevel> builder)
    {
        builder.ToTable("scout_leader_training_level");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PersonId).HasColumnName("person_id").IsRequired();

        builder.Property(x => x.TrainingLevelId).HasColumnName("training_level_id").IsRequired();

        builder.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();

        builder.Property(x => x.StatusDate).HasColumnName("status_date").IsRequired();

        builder.Property(x => x.Notes).HasColumnName("notes");

        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder
            .Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder
            .HasIndex(x => new
            {
                x.PersonId,
                x.TrainingLevelId,
                x.StatusId,
            })
            .IsUnique();

        builder
            .HasOne(x => x.ScoutLeader)
            .WithMany(sl => sl.TrainingLevels)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.TrainingLevel)
            .WithMany(tl => tl.LeaderLevels)
            .HasForeignKey(x => x.TrainingLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Status)
            .WithMany(s => s.LeaderTrainingLevels)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
