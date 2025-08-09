using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using LearningAppNetCoreApi;
using LearningAppNetCoreApi.Middleware;
using LearningAppNetCoreApi.Services;
using LearningAppNetCoreApi.Services.Jobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// --- Initialize Firebase Admin SDK ---
if (builder.Environment.IsProduction())
{
    var projectId = "tuttilearni-54f1f";
    var secretId = "firebase-service-account";
    var secretVersionId = "latest";
    var secretManager = await SecretManagerServiceClient.CreateAsync();
    var secretVersionName = new SecretVersionName(projectId, secretId, secretVersionId);
    var result = await secretManager.AccessSecretVersionAsync(secretVersionName);
    var jsonCredentials = result.Payload.Data.ToStringUtf8();
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(jsonCredentials)
    });
}
else
{
    var firebaseCredential = GoogleCredential.FromFile("firebase-service-account.json");
    FirebaseApp.Create(new AppOptions()
    {
        Credential = firebaseCredential,
    });
}

// --- Configure Services ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectId"]}";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{builder.Configuration["Firebase:ProjectId"]}",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Firebase:ProjectId"],
            ValidateLifetime = true
        };
    });

// --- Configure Database Context using the official NpgsqlConnectionStringBuilder ---
if (builder.Environment.IsProduction())
{
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = $"/cloudsql/{Environment.GetEnvironmentVariable("INSTANCE_CONNECTION_NAME")}",
        Username = Environment.GetEnvironmentVariable("DB_USER"),
        Password = Environment.GetEnvironmentVariable("DB_PASS"),
        Database = Environment.GetEnvironmentVariable("DB_NAME"),
        SslMode = SslMode.Disable, // Required for Cloud SQL Auth Proxy
        Pooling = true
    };
    var prodDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStringBuilder.ConnectionString);
    prodDataSourceBuilder.EnableDynamicJson();
    var prodDataSource = prodDataSourceBuilder.Build();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(prodDataSource)
               .UseSnakeCaseNamingConvention());
}
else
{
    // For local development, use your existing appsettings.json
    var localDataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
    localDataSourceBuilder.EnableDynamicJson();
    var localDataSource = localDataSourceBuilder.Build();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(localDataSource)
               .UseSnakeCaseNamingConvention());
}

// --- Add Application Services ---
builder.Services.AddScoped<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IJobsService, JobsService>();
builder.Services.AddScoped<IWaitlistService, WaitlistService>(); // TODO: Remove after app is not on waitlist anymore

// --- Add Jobs ---
builder.Services.AddTransient<SendLearningRemindersJob>();
builder.Services.AddTransient<SubscriptionValidationJob>();
builder.Services.AddTransient<ResetMonthlyUsageJob>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders(); // Clear existing providers
    builder.Logging.AddJsonConsole(options =>
    {
        // These options format the JSON nicely for Google Cloud Logging
        options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
        {
            Indented = false
        };
        options.IncludeScopes = true; // This is the crucial part that includes your UID
    });
}

var webAppOrigin = "AllowWebApp";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: webAppOrigin,
        policy =>
        {
            // This only allows requests from your live website.
            policy.WithOrigins("https://tuttilearni.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// --- Apply Database Migrations on Startup ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// --- Configure the HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors(webAppOrigin);

app.UseAuthentication();
app.UseAuthorization();

if (builder.Environment.IsProduction())
{
    app.UseMiddleware<StructuredLoggingMiddleware>();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

// --- Run the Application ---
if (builder.Environment.IsProduction())
{
    // In production, listen on the port Cloud Run provides
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    var url = $"http://0.0.0.0:{port}";
    app.Run(url);
}
else
{
    // In development, just run the app. It will use the URLs from launchSettings.json
    app.Run();
}