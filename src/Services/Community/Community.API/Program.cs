using Amazon.S3;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Exceptions.Handler;
using BuildingBlocks.Identity;
using BuildingBlocks.Logging;
using Community.API.BackgroundJobs;
using Community.API.Common.Behaviors;
using Community.API.Common.Extensions;
using Community.API.Common.Helpers;
using Community.API.Database;
using Community.API.Persistence;
using Community.API.Services.Auth;
using Community.API.Services.S3;
using Community.API.Services.Slugs;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────
// Database
// ────────────────────────────────────────────
builder.Services.AddSingleton<MongoDbContext>();


builder.Services.Configure<S3MinioOptions>(
    builder.Configuration.GetSection("Minio"));

// ────────────────────────────────────────────
// Repositories
// ────────────────────────────────────────────
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
builder.Services.AddScoped<IRecommendationCacheRepository, RecommendationCacheRepository>();

// ────────────────────────────────────────────
// Services
// ────────────────────────────────────────────
builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<IOptions<S3MinioOptions>>().Value;
    var config = new AmazonS3Config
    {
        ServiceURL = options.Endpoint, // backend -> s3 storage, then use minio
        ForcePathStyle = true,
        UseHttp = true
    };
    return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
});
builder.Services.AddScoped<ISlugGenerator, SlugGenerator>();

// ────────────────────────────────────────────
// MediatR + FluentValidation Pipeline
// ────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ────────────────────────────────────────────
// Middleware
// ────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ────────────────────────────────────────────
// Background Jobs
// ────────────────────────────────────────────
builder.Services.AddHostedService<HotScoreRecalculator>();
builder.Services.AddHttpClient();

// ────────────────────────────────────────────
// Authentication / Authorization
// ────────────────────────────────────────────

builder.Services.AddSwaggerDocumentation()
    .AddIdentity(builder.Configuration)
    .AddLoggingConfigs(builder.Configuration);

// ────────────────────────────────────────────
// API / Swagger
// ────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
              )
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ────────────────────────────────────────────
// DB Initialization
// ────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await MongoDbInitializer.InitializeAsync(dbContext.Database);
}


// ────────────────────────────────────────────
// Middleware Pipeline
// ────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowReactApp");

// ────────────────────────────────────────────
// Map all IEndpoint implementations via reflection
// ────────────────────────────────────────────
app.MapEndpoints();

app.Run();
