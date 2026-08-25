using DSC.Domain.Entities;
using DSC.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DSC.Infrastructure.Persistence.Configurations;

public class PersonStatusConfiguration : IEntityTypeConfiguration<PersonStatus>
{
    public void Configure(EntityTypeBuilder<PersonStatus> builder)
    {
        builder.ToTable("person_status");

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

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("person");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.IdCard)
            .HasColumnName("id_card")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IdCardExtension)
            .HasColumnName("id_card_extension")
            .HasMaxLength(5);

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(120);

        builder.Property(x => x.FirstLastName)
            .HasColumnName("first_last_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.SecondLastName)
            .HasColumnName("second_last_name")
            .HasMaxLength(120);

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasColumnType("gender_type");

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(x => x.PhotoFileId)
            .HasColumnName("photo_file_id");

        builder.Property(x => x.PersonStatusId)
            .HasColumnName("person_status_id")
            .IsRequired();

        builder.Property(x => x.IsProfileComplete)
            .HasColumnName("is_profile_complete")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(x => new { x.IdCard, x.IdCardExtension }).IsUnique();

        builder.HasOne(x => x.PhotoFile)
            .WithMany()
            .HasForeignKey(x => x.PhotoFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.PersonStatus)
            .WithMany(ps => ps.Persons)
            .HasForeignKey(x => x.PersonStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithOne(u => u.Person)
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ScoutLeader)
            .WithOne(sl => sl.Person)
            .HasForeignKey<ScoutLeader>(sl => sl.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PersonContactMechanismConfiguration : IEntityTypeConfiguration<PersonContactMechanism>
{
    public void Configure(EntityTypeBuilder<PersonContactMechanism> builder)
    {
        builder.ToTable("person_contact_mechanism");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PersonId)
            .HasColumnName("person_id")
            .IsRequired();

        builder.Property(x => x.ContactMechanismTypeId)
            .HasColumnName("contact_mechanism_type_id")
            .IsRequired();

        builder.Property(x => x.ContactPurposeTypeId)
            .HasColumnName("contact_purpose_type_id")
            .IsRequired();

        builder.Property(x => x.ContactValue)
            .HasColumnName("contact_value")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(x => x.ValidUntil)
            .HasColumnName("valid_until");

        builder.HasOne(x => x.Person)
            .WithMany(p => p.ContactMechanisms)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactMechanismType)
            .WithMany(cmt => cmt.PersonContactMechanisms)
            .HasForeignKey(x => x.ContactMechanismTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactPurposeType)
            .WithMany(cpt => cpt.PersonContactMechanisms)
            .HasForeignKey(x => x.ContactPurposeTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
