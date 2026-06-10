using ahello_backend.DbContexts;
using ahello_backend.Models.Login;
using ahello_backend.Repositorys.Classes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Classes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}



// =====================================================
// ADD SERVICE
// =====================================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient();


// =====================================================
// EMAIL CONFIG
// =====================================================

builder.Services.AddSingleton(new EmailCon());



// =====================================================
// MYSQL DATABASE
// =====================================================

builder.Services.AddScoped<IDbConnection>(sp =>
    new MySqlConnection(
        builder.Configuration.GetConnectionString("MySqlConnection")
    ));

builder.Services.AddScoped<DbContext>();
builder.Services.AddScoped<VideoValidationService>();

builder.Services.AddScoped<FileUploadService>();

// =====================================================
// UPLOAD SIZE LIMITS  ← ADD THIS BLOCK
// =====================================================

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // 50 MB (covers 10MB video + other form fields)
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52_428_800; // 50 MB — must match above
});

// =====================================================
// CORS POLICY
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy
            .WithOrigins(
                "http://localhost:5173",
                "https://ahllo.com",
                "https://www.ahllo.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});


// =====================================================
// SCRUTOR AUTO DI
// =====================================================
builder.Services.AddScoped<DbContextConnection>();

builder.Services.Scan(scan =>
    scan.FromAssemblyOf<Program>()
        .AddClasses(classes => classes.Where(t =>
            t.Namespace != null &&
            (
                t.Namespace.StartsWith("ahello_backend.Repositorys") ||
                t.Namespace.StartsWith("ahello_backend.Services")
            ) &&
            !typeof(IHostedService).IsAssignableFrom(t)
        ))
        .AsImplementedInterfaces()
        .WithScopedLifetime());


// =====================================================
// CUSTOM SERVICES
// =====================================================

//builder.Services.AddScoped<BlobStorageService>();

//builder.Services.AddScoped<FirebaseService>();

//builder.Services.AddScoped<RazorpayService>();

//builder.Services.AddScoped<InvoicePdfService>();

builder.Services.AddHostedService<
    MeetingReminderBackgroundService>();
builder.Services.AddHostedService<NoShowDetectorService>();

// =====================================================
// BUILD APP
// =====================================================

var app = builder.Build();


// =====================================================
// FIREBASE
// =====================================================

//FirebaseApp.Create(new AppOptions()
//{
//    Credential = GoogleCredential.FromFile(
//        "firebase-adminsdk.json"
//    )
//});


// =====================================================
// HTTP PIPELINE
// =====================================================

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("CorsPolicy");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();


// =====================================================
// CUSTOM MIDDLEWARE
// =====================================================

// app.UseMiddleware<YourMiddleware>();


app.MapControllers();

app.Run();