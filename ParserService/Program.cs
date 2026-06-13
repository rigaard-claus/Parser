using dotenv.net;
using Microsoft.EntityFrameworkCore;
using ParserService.Data;
using Scalar.AspNetCore;

DotEnv.Load(new DotEnvOptions().WithProbeForEnv());

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDatabaseContext(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
