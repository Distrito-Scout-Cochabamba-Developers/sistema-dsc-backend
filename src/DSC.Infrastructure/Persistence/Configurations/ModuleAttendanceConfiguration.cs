using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class ModuleAttendanceConfiguration : IEntityTypeConfiguration<ModuleAttendance>
{
    public void Configure(EntityTypeBuilder<ModuleAttendance> builder)
    {
        builder.ToTable("ModuleAttendances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TrainingModuleId)
            .IsRequired();

        builder.Property(x => x.ScoutLeaderId)
            .IsRequired();

        builder.Property(x => x.RegisteredAt)
            .IsRequired();

        builder.HasOne(x => x.TrainingModule)
            .WithMany()
            .HasForeignKey(x => x.TrainingModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ScoutLeader)
            .WithMany()
            .HasForeignKey(x => x.ScoutLeaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TrainingModuleId, x.ScoutLeaderId })
            .IsUnique();
    }
}
