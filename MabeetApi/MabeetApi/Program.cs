using MabeetApi.Data;
using MabeetApi.Entities;
using MabeetApi.Seeding;
using MabeetApi.Services;
using MabeetApi.Services.Admin;
using MabeetApi.Services.Admin.Accommodations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
// Update program
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mabeet API",
        Version = "v1",
        Description = "API for Mabeet Accommodation Booking System"
    });
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ⭐ Add Identity (VERY IMPORTANT)
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Authentication + Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Register your services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminAccommodationService, AdminAccommodationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DataSeeder.SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mabeet API V1");
        c.RoutePrefix = "swagger";
    });
}

// Pipeline
app.UseHttpsRedirection();
app.UseAuthentication();   // لازم يكون قبل UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
