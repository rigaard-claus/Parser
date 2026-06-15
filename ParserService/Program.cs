using dotenv.net;
using Microsoft.EntityFrameworkCore;
using NATS.Client.Core;
using NATS.Client.Serializers.Json;
using ParserService.Application.Handlers.Operators;
using ParserService.Application.Mapping;
using ParserService.Application.Messaging;
using ParserService.Application.Services;
using ParserService.Data;
using ParserService.ParserCore;
using ParserService.ParserCore.Engine.Parsers.Dertour;
using ParserService.ParserCore.Interfaces;
using Scalar.AspNetCore;
using System.Text.Json;

var hostName = Environment.MachineName;
var processId = Environment.ProcessId;
var stage = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

DotEnv.Load(new DotEnvOptions().WithProbeForEnv());

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDatabaseContext(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(typeof(ParserService.Mappings.MappingProfile).Assembly);
var natsUrl = builder.Configuration.GetConnectionString("NatsConnection") ?? "nats://localhost:4222";
builder.Services.AddSingleton(sp =>
{
    var natsOpts = new NatsOpts
    {
        Name = $"[{stage}][{hostName}][PID:{processId}] ParserService",
        Url = natsUrl,
        SerializerRegistry = new NatsJsonContextOptionsSerializerRegistry(new JsonSerializerOptions()),
        ConnectTimeout = TimeSpan.FromSeconds(30)
    };
    return new NatsConnection(natsOpts);
});

builder.Services.AddScoped<ITourOperatorParser, DertourParser>();
builder.Services.AddScoped<ParserFactory>();

builder.Services.AddScoped<OperatorConfigurationService>();
builder.Services.AddScoped<GetOperatorsHandler>();
builder.Services.AddHostedService<NatsSubscriptionWorker>();
builder.Services.AddSingleton<INatsBus, NatsBus>();

WebApplication app = null;
try
{
    app = builder.Build();
}
catch (Exception ex)
{
    Console.WriteLine($"Error building the application: {ex.Message}");
    throw;
}

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

app.MapOperators();

app.Run();
