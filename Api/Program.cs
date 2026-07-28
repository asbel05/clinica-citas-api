using Application.Interfaces;
using Application.Services;
using Application.Validators.Citas;
using Application.Validators.Doctores;
using Application.Validators.Pacientes;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataDirectory);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=Data/citasmedicas.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddValidatorsFromAssemblyContaining<CreateDoctorValidator>();

builder.Services.AddScoped<IDoctorRepository, DoctorRepositoryImpl>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepositoryImpl>();
builder.Services.AddScoped<ICitaRepository, CitaRepositoryImpl>();

builder.Services.AddScoped<IDoctorService, DoctorServiceImpl>();
builder.Services.AddScoped<IPacienteService, PacienteServiceImpl>();
builder.Services.AddScoped<ICitaService, CitaServiceImpl>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
