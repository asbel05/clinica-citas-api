using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");

        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(patient => patient.DocumentoIdentidad)
            .IsRequired()
            .HasMaxLength(8);

        builder.HasIndex(patient => patient.DocumentoIdentidad)
            .IsUnique();

        builder.Property(patient => patient.Activo)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
