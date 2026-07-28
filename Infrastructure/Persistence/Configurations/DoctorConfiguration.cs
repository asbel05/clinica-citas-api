using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctores");

        builder.HasKey(doctor => doctor.Id);

        builder.Property(doctor => doctor.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(doctor => doctor.Especialidad)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(doctor => doctor.Activo)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
