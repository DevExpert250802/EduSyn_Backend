using edusync_backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using edusync_backend.Services;
using edusync_backend.Models;
using edusync_backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
             "https://yellow-bay-02e117200.2.azurestaticapps.net"

            )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register DbContext
builder.Services.AddDbContext<EduSyncDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<UserRepository>();

// Register Services
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// Register Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            RequireAudience = true,
            ValidateWithLKG = true,
            ValidateTokenReplay = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Admin policy - only Admin role can access
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    // Instructor policy - only Instructor role can access
    options.AddPolicy("RequireInstructorRole", policy =>
        policy.RequireRole("Instructor"));

    // Student policy - only Student role can access
    options.AddPolicy("RequireStudentRole", policy =>
        policy.RequireRole("Student"));

    // Combined policies
    options.AddPolicy("RequireAdminOrInstructorRole", policy =>
        policy.RequireRole("Admin", "Instructor"));

    options.AddPolicy("RequireAdminOrStudentRole", policy =>
        policy.RequireRole("Admin", "Student"));

    // Policy for all roles
    options.AddPolicy("RequireAdminOrInstructorOrStudentRole", policy =>
        policy.RequireRole("Admin", "Instructor", "Student"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 500,
                Message = "An error occurred while processing your request.",
                DetailedMessage = app.Environment.IsDevelopment() ? ex.Message : null
            });
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
