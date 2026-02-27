using BuildingBlocks.Logging;
using BuildingBlocks.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

// builder.Services.AddTelemetryConsumer<ForwarderTelemetry>();

builder.Services.AddLoggingConfigs(builder.Configuration)
    .AddIdentity(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//adds additional entries to the log detailing the incoming and outgoing request headers.   
app.UseHttpLogging();
// Enable endpoint routing, required for the reverse proxy
app.UseRouting();
// Register the reverse proxy routes
app.MapReverseProxy();

app.Run();

