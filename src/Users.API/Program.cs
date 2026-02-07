using Users.API.Common.Extentions;
using Users.API.Common.Middlewares;

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

