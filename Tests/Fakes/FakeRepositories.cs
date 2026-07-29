using System.Reflection;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Tests.Fakes;

public sealed class FakeDoctorRepository : IDoctorRepository
{
    private int _nextId = 1;

    public List<Doctor> Doctors { get; } = [];

    public List<Cita> Appointments { get; } = [];

    public Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        SetId(doctor, _nextId++);
        Doctors.Add(doctor);
        return Task.CompletedTask;
    }

    public Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Doctors.FirstOrDefault(doctor => doctor.Id == id));
    }

    public Task<IReadOnlyList<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Doctor> result = Doctors
            .Where(doctor => doctor.Activo)
            .OrderBy(doctor => doctor.Nombre)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Cita>> GetFutureScheduledAppointmentsAsync(
        int doctorId,
        DateTime from,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Cita> result = Appointments
            .Where(cita => cita.DoctorId == doctorId
                && cita.Estado == EstadoCita.Programada
                && cita.FechaHoraInicio >= from)
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToList();

        return Task.FromResult(result);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    internal void AddExisting(Doctor doctor)
    {
        SetId(doctor, _nextId++);
        Doctors.Add(doctor);
    }

    internal static void SetId<T>(T entity, int id)
    {
        typeof(T)
            .GetProperty(nameof(Doctor.Id), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(entity, id);
    }
}

public sealed class FakePacienteRepository : IPacienteRepository
{
    private int _nextId = 1;

    public List<Paciente> Patients { get; } = [];

    public Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        FakeDoctorRepository.SetId(paciente, _nextId++);
        Patients.Add(paciente);
        return Task.CompletedTask;
    }

    public Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Patients.FirstOrDefault(patient => patient.Id == id));
    }

    public Task<IReadOnlyList<Paciente>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Paciente> result = Patients
            .Where(patient => patient.Activo)
            .OrderBy(patient => patient.Nombre)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<bool> ExistsByDocumentAsync(
        string documentoIdentidad,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Patients.Any(patient =>
            patient.DocumentoIdentidad == documentoIdentidad));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    internal void AddExisting(Paciente paciente)
    {
        FakeDoctorRepository.SetId(paciente, _nextId++);
        Patients.Add(paciente);
    }
}

public sealed class FakeCitaRepository : ICitaRepository
{
    private int _nextId = 1;

    public List<Cita> Appointments { get; } = [];

    public Task AddAsync(Cita cita, CancellationToken cancellationToken = default)
    {
        FakeDoctorRepository.SetId(cita, _nextId++);
        Appointments.Add(cita);
        return Task.CompletedTask;
    }

    public Task<Cita?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Appointments.FirstOrDefault(cita => cita.Id == id));
    }

    public Task<IReadOnlyList<Cita>> GetAsync(
        int? doctorId = null,
        int? pacienteId = null,
        EstadoCita? estado = null,
        CancellationToken cancellationToken = default)
    {
        var query = Appointments.AsEnumerable();

        if (doctorId.HasValue)
        {
            query = query.Where(cita => cita.DoctorId == doctorId.Value);
        }

        if (pacienteId.HasValue)
        {
            query = query.Where(cita => cita.PacienteId == pacienteId.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(cita => cita.Estado == estado.Value);
        }

        IReadOnlyList<Cita> result = query
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Cita>> GetScheduledByDoctorAsync(
        int doctorId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetOverlapping(cita => cita.DoctorId == doctorId, from, to));
    }

    public Task<IReadOnlyList<Cita>> GetScheduledByPatientAsync(
        int pacienteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetOverlapping(cita => cita.PacienteId == pacienteId, from, to));
    }

    public Task<int> CountScheduledByPatientAsync(
        int pacienteId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Appointments.Count(cita =>
            cita.PacienteId == pacienteId && cita.Estado == EstadoCita.Programada));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    internal void AddExisting(Cita cita)
    {
        FakeDoctorRepository.SetId(cita, _nextId++);
        Appointments.Add(cita);
    }

    private IReadOnlyList<Cita> GetOverlapping(
        Func<Cita, bool> ownerFilter,
        DateTime from,
        DateTime to)
    {
        return Appointments
            .Where(cita => ownerFilter(cita)
                && cita.Estado == EstadoCita.Programada
                && cita.FechaHoraInicio < to
                && cita.FechaHoraFin > from)
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToList();
    }
}
