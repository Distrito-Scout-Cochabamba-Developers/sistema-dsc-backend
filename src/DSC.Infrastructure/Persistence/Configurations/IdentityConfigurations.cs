using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class UserStatusConfiguration : IEntityTypeConfiguration<UserStatus>
{
    public void Configure(EntityTypeBuilder<UserStatus> builder)
    {
        builder.ToTable("user_status");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(30).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PersonId).HasColumnName("person_id").IsRequired();

        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();

        builder.Property(x => x.Password).HasColumnName("password").HasMaxLength(255).IsRequired();

        builder.Property(x => x.UserStatusId).HasColumnName("user_status_id").IsRequired();

        builder
            .Property(x => x.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .HasDefaultValue(false)
            .IsRequired();

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

        builder.HasIndex(x => x.PersonId).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        builder
            .HasOne(x => x.UserStatus)
            .WithMany(us => us.Users)
            .HasForeignKey(x => x.UserStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(40).IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(255);

        builder
            .Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role");

        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(x => x.RoleId).HasColumnName("role_id").IsRequired();

        builder.Property(x => x.ValidFrom).HasColumnName("valid_from").IsRequired();

        builder.Property(x => x.ValidUntil).HasColumnName("valid_until");

        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder
            .HasOne(x => x.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
