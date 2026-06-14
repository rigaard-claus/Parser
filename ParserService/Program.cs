using dotenv.net;
using Microsoft.EntityFrameworkCore;
using NATS.Net;
using ParserService.Data;
using ParserService.ParserCore;
using ParserService.ParserCore.Engine.Parsers.Dertour;
using ParserService.ParserCore.Interfaces;
using Scalar.AspNetCore;

DotEnv.Load(new DotEnvOptions().WithProbeForEnv());

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDatabaseContext(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(typeof(ParserService.Mappings.MappingProfile).Assembly);

var natsUrl = builder.Configuration.GetConnectionString("NatsConnection") ?? "nats://localhost:4222";

var natsClient = new NatsClient(natsUrl);
builder.Services.AddSingleton(natsClient);

builder.Services.AddScoped<ITourOperatorParser, DertourParser>();
builder.Services.AddScoped<ParserFactory>();

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

app.MapGet("/", context => {
    context.Response.Redirect("/scalar/v1");
    return Task.CompletedTask;
});

app.Run();
