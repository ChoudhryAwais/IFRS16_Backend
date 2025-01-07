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
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ILeaseDataService, LeaseDataService>();
builder.Services.AddScoped<IInitialRecognitionService, InitialRecognitionService>();
builder.Services.AddScoped<IROUScheduleService, ROUScheduleService>();
builder.Services.AddScoped<ILeaseLiabilityService, LeaseLiabilityService>();
builder.Services.AddScoped<ILeaseDataWorkflowService, LeaseDataWorkflowService>();





// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowAllOrigins");

app.Run();
