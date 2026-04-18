using BuildingBlocks.Core;
using BuildingBlocks.Core.Exceptions.Handler.BuildingBlocks.Core.Exceptions.Handler;
using BuildingBlocks.Identity;
using BuildingBlocks.Logging;
using CoreJudge.API.Extentions;
using CoreJudge.Application;
using CoreJudge.Domain.Premitives;
using CoreJudge.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
    });// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen().AddSwaggerDocumentation();
builder.Services.AddHealthChecks();
builder.Services.AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddIdentity(builder.Configuration)
    .AddLoggingConfigs(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var app = builder.Build();
await app.Services.ApplyMigrationsWithRetryAsync();
await app.Services.CleanScriptFile(Helper.ScriptFilePath);
await app.Services.SeedDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseExceptionHandler();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("healthy"));
app.Run();

public partial class Program { }
