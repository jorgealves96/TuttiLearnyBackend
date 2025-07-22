using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using LearningAppNetCoreApi;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Google.Cloud.SecretManager.V1; // Add this using statement

var builder = WebApplication.CreateBuilder(args);

// --- Initialize Firebase Admin SDK ---
// This logic now handles both production and development environments.
if (builder.Environment.IsProduction())
{
    // In production (Cloud Run), fetch the credentials from Secret Manager.
    var projectId = "tuttilearni-54f1f"; // Your GCP Project ID
    var secretId = "firebase-service-account";
    var secretVersionId = "latest"; // Use the latest version of the secret

    // Create the Secret Manager client. It will automatically use the
    // service account credentials of the running Cloud Run instance.
    var secretManager = await SecretManagerServiceClient.CreateAsync();
    var secretVersionName = new SecretVersionName(projectId, secretId, secretVersionId);

    // Access the secret payload
    var result = await secretManager.AccessSecretVersionAsync(secretVersionName);
    var jsonCredentials = result.Payload.Data.ToStringUtf8();

    // Initialize Firebase from the secret's JSON content
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(jsonCredentials)
    });
}
else
{
    // For local development, use the file from your project directory.
    var firebaseCredential = GoogleCredential.FromFile("firebase-service-account.json");
    FirebaseApp.Create(new AppOptions()
    {
        Credential = firebaseCredential,
    });
}

// --- Configure JWT Authentication ---
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

// --- Configure Database Context ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// --- Add Services to the Container ---
builder.Services.AddScoped<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Configure Port for Cloud Run ---
// This ensures the app listens on the port provided by the environment.
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

//
var app = builder.Build();

// --- Configure the HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // This must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();