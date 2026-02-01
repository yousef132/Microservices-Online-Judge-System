using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Users.API;
using Users.API.Clients;
using Users.API.Delegates;
using Users.API.Extentions;
using Users.API.Middlewares;
using Users.API.Options;
using Users.API.Repository;
using Users.API.Repository.Implementations;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add1 services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer()
    .RegisterServices(builder.Configuration)
    .AddIdentity(builder.Configuration)
    .AddSwaggerDocumentation();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();
await app.Services.ApplyMigrationsWithRetryAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapControllers();

app.Run();

