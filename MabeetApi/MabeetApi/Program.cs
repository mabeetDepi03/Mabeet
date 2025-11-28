using MabeetApi.Data;
using MabeetApi.Interfaces;
using MabeetApi.Services;
using MabeetApi.Services.Admin;
using MabeetApi.Services.Admin.Accommodations;
using MabeetApi.Services.Property;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ⭐ إضافة Swagger/OpenAPI
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

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add other services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminAccommodationService, AdminAccommodationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAccommodationService, AccommodationService>();

// أضف باقي الـ services هنا...

var app = builder.Build();

// ⭐ تمكين Swagger في development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mabeet API V1");
        c.RoutePrefix = "swagger"; // يجعل Swagger UI available على /swagger
    });
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();