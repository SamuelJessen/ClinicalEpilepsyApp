using ClinicalEpilepsyApp.Infrastructure.Context;
using ClinicalEpilepsyApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ClinicalEpilepsyAppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicalEpilepsyDbContext"), options => options.EnableRetryOnFailure()));
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<EcgAlarmRepository>();
builder.Services.AddScoped<EcgProcessedMeasurementRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
