using K8Intel.Data;
using K8Intel.Helpers; 
using K8Intel.Interfaces;
using K8Intel.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using K8Intel.Security;
using K8Intel.Middleware;
using Serilog;
using K8Intel.Data.Seeding;
using Hangfire;
using Hangfire.PostgreSql;
using K8Intel.Jobs;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; 

// 1. Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Register services and interfaces
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClusterService, ClusterService>(); 
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IMetricService, MetricService>();
builder.Services.AddScoped<IDataRetentionJob, DataRetentionJob>();
builder.Services.AddScoped<IKubernetesService, KubernetesService>();
builder.Services.AddScoped<IInsightsGeneratorJob, InsightsGeneratorJob>();
   
builder.Services.AddHttpsRedirection(options =>
        {
            options.HttpsPort = 8082; 
        });

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile)); 

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5001")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .WithExposedHeaders("X-Pagination");
                      });
});

// 1. Add Hangfire services.
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

// 2. Add the processing server as a hosted service.
builder.Services.AddHangfireServer();

// 2. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    // For a stateless API, the default should always be the token scheme.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    
    // This correctly tells the app to challenge with the Bearer scheme,
    // which results in a 401 Unauthorized if the token is missing or invalid.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    
    // (Optional) This tells other parts of the system (like Hangfire Dashboard)
    // what scheme to use for signing in.
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options => // Keep the cookie configuration for Hangfire or a future MVC UI
{
    options.Events.OnRedirectToLogin = context =>
    {
        // For an API, never redirect. Just return 401.
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")
            )
        )
    };
})
.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
    ApiKeyAuthenticationOptions.DefaultScheme, options => { });

// 3. Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "K8Intel API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// This should run only if needed for initial setup, but it's okay here.
await DefaultUserSeeder.SeedDefaultAdminUserAsync(app);

// Global Exception handler should be first.
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// CORS must be called before Authentication/Authorization.
app.UseCors(MyAllowSpecificOrigins);

// Authentication must come before Authorization.
app.UseAuthentication();
app.UseAuthorization();

// Add Serilog request logging after auth to log the user.
app.UseSerilogRequestLogging();


// --->>> MAP YOUR ENDPOINTS LAST <<<---

// 1. Map your API controllers.
app.MapControllers();

// 2. Map the Hangfire Dashboard.
// This requires its own specific policy to be correctly secured.
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

RecurringJob.AddOrUpdate<IDataRetentionJob>(
    "daily-data-retention-job",   // A unique ID for the job
    job => job.PurgeOldDataAsync(),
    "0 1 * * *");                 

RecurringJob.AddOrUpdate<IInsightsGeneratorJob>(
    "stability-recommendation-job",
    job => job.GenerateStabilityRecommendationsAsync(),
    "*/15 * * * *"); 

app.Run();

// app.MapHangfireDashboard("/hangfire", new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorizationFilter() }
// })
// .RequireAuthorization(policy => policy.RequireRole("Admin")); // Double-enforce security
