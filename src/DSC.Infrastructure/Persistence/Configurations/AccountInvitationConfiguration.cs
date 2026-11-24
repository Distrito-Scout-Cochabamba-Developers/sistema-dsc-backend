using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class AccountInvitationConfiguration : IEntityTypeConfiguration<AccountInvitation>
{
    public void Configure(EntityTypeBuilder<AccountInvitation> builder)
    {
        builder.ToTable("account_invitation", t =>
            t.HasCheckConstraint("ck_account_invitation_status", "status IN ('PENDING','SENT','CLOSED')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.EnrollmentId).HasColumnName("enrollment_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(16).IsRequired();
        builder.Property(x => x.Attempts).HasColumnName("attempts").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.LastFailureCode).HasColumnName("last_failure_code").HasMaxLength(64);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.SentAt).HasColumnName("sent_at");
        builder.HasIndex(x => x.EnrollmentId).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Enrollment).WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
