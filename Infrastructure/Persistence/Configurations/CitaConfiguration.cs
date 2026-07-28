using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("Citas");

        builder.HasKey(appointment => appointment.Id);

        builder.Property(appointment => appointment.DoctorId)
            .IsRequired();

        builder.Property(appointment => appointment.PacienteId)
            .IsRequired();

        builder.Property(appointment => appointment.FechaHoraInicio)
            .IsRequired();

        builder.Property(appointment => appointment.FechaHoraFin)
            .IsRequired();

        builder.Property(appointment => appointment.Estado)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(EstadoCita.Programada)
            .HasSentinel((EstadoCita)0);

        builder.Property(appointment => appointment.MotivoCancelacion)
            .HasMaxLength(500);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(appointment => appointment.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Paciente>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(appointment => new
        {
            appointment.DoctorId,
            appointment.FechaHoraInicio,
            appointment.FechaHoraFin
        });

        builder.HasIndex(appointment => new
        {
            appointment.PacienteId,
            appointment.FechaHoraInicio,
            appointment.FechaHoraFin
        });
    }
}
