using BuildingBlocks.Core;
using BuildingBlocks.Core.Exceptions.Handler;
using BuildingBlocks.Core.Exceptions.Handler.BuildingBlocks.Core.Exceptions.Handler;
using CoreJudge.API.Extentions;
using CoreJudge.Application;
using CoreJudge.Infrastructure;
using BuildingBlocks.Identity;
using BuildingBlocks.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
