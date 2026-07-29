using Api.Errors;
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
using Microsoft.OpenApi;

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
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Citas Médicas API",
        Version = "v1",
        Description = "API para la gestión de doctores, pacientes y citas médicas."
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Citas Médicas API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
