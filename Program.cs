using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LearningAppNetCoreApi;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Google.Cloud.SecretManager.V1;
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
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionStringBuilder.ConnectionString)
               .UseSnakeCaseNamingConvention());
}
else
{
    // For local development, use your existing appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString)
               .UseSnakeCaseNamingConvention());
}

// --- Add Application Services ---
builder.Services.AddScoped<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

// --- Run the Application ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var url = $"http://0.0.0.0:{port}";
app.Run(url);