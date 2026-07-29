using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class TrainingModuleConfiguration : IEntityTypeConfiguration<TrainingModule>
{
    public void Configure(EntityTypeBuilder<TrainingModule> builder)
    {
        builder.ToTable("TrainingModules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SessionToken)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.SessionToken)
            .IsUnique();

        builder.Property(x => x.EventDate)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}
