using MabeetApi.Data;
using MabeetApi.Entities;
using MabeetApi.Interfaces;
using MabeetApi.Seeding;
using MabeetApi.Services;
using MabeetApi.Services.Admin;
using MabeetApi.Services.Admin.Accommodations;
using MabeetApi.Services.Property;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration; // Ensure this is not used if not needed, typically not required for standard setup.

// Update program
var builder = WebApplication.CreateBuilder(args);

// ⭐ 1. تحديد سياسة CORS (مهم جداً لعنوان العميل)
var MyAllowedOrigins = "_myAllowedOrigins";

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: MyAllowedOrigins,
		policy =>
		{
			// هذا هو المنشأ الذي يحاول الوصول إلى الـ API (http://127.0.0.1:5500)
			policy.WithOrigins("http://127.0.0.1:5500")
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});
// ⭐ نهاية إضافة CORS Services

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Swagger
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
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.Password.RequiredLength = 8;
	options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
// JWT Authentication

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ClockSkew = TimeSpan.Zero
	};
});

// Authorization Policies

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("OwnerOnly", policy => policy.RequireRole("Owner"));
	options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
});


// Register your services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminAccommodationService, AdminAccommodationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAccommodationService, AccommodationService>();

// Build App
var app = builder.Build();
// Seed Data
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
		logger.LogError(ex, "❌ Error during data seeding");
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

// ⭐ 2. تطبيق سياسة CORS في مسار الطلب (يجب أن يكون في وقت مبكر)
app.UseCors(MyAllowedOrigins);
// ⭐ نهاية تطبيق CORS

// Pipeline
app.UseHttpsRedirection();
app.UseAuthentication();    // لازم يكون قبل UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();