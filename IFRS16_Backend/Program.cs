using Microsoft.AspNetCore.Authentication.JwtBearer;
using IFRS16_Backend.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using IFRS16_Backend.Services.LeaseData;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.ROUSchedule;
using IFRS16_Backend.Services.LeaseLiability;
using IFRS16_Backend.Services.LeaseDataWorkflow;
using IFRS16_Backend.Services.JournalEntries;
using IFRS16_Backend.Services.LeaseLiabilityAggregation;
using IFRS16_Backend.Services.Currencies;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Services.Report;
using IFRS16_Backend.Services.ExchangeRate;
using IFRS16_Backend.Services.UserCrud;
using IFRS16_Backend.Services.RemeasurementFCL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using IFRS16_Backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// JWT configuration
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Require authentication by default for all endpoints. Use [AllowAnonymous] on login/register endpoints.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ILeaseDataService, LeaseDataService>();
builder.Services.AddScoped<IInitialRecognitionService, InitialRecognitionService>();
builder.Services.AddScoped<IROUScheduleService, ROUScheduleService>();
builder.Services.AddScoped<ILeaseLiabilityService, LeaseLiabilityService>();
builder.Services.AddScoped<ILeaseDataWorkflowService, LeaseDataWorkflowService>();
builder.Services.AddScoped<IJournalEntriesService, JournalEntriesService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<ICurrenciesService, CurrenciesService>();
builder.Services.AddScoped<GetCurrecyRates>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRemeasureFCLService, RemeasureFCLService>();





// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IFRS16 API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Don't enter 'Bearer', only enter token in the text input below.\r\n\r\nExample: '12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(300) // Timeout in seconds (5 minutes)

    ));

// Allow all origins (for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseCors("AllowAllOrigins");
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<SingleSessionMiddleware>();
app.MapControllers();

app.Run();
