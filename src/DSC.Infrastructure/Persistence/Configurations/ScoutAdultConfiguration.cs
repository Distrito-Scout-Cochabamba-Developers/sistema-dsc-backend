using DSC.Domain.Entities;
using DSC.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class ScoutAdultConfiguration : IEntityTypeConfiguration<ScoutAdult>
{
    public void Configure(EntityTypeBuilder<ScoutAdult> builder)
    {
        builder.ToTable("ScoutAdults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.IsPartialRegistration)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Complex conversion for CiNumber Value Object
        builder.Property(x => x.Ci)
            .HasConversion(
                ci => ci.FullValue,
                v => ParseCiNumber(v))
            .HasMaxLength(25)
            .IsRequired();

        builder.HasIndex(x => x.Ci);
    }

    private static CiNumber ParseCiNumber(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new CiNumber("0");

        var parts = raw.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var number = parts[0];
        var extension = parts.Length > 1 ? parts[1] : string.Empty;

        return new CiNumber(number, extension);
    }
}
