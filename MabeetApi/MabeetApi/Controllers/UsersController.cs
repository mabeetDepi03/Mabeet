using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MabeetApi.Entities;
using MabeetApi.Data.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MabeetApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IConfiguration _configuration;

		public UsersController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_configuration = configuration;
		}

		// ================== Register ==================
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(new { Message = "بيانات غير صالحة", Errors = ModelState });

			var userExists = await _userManager.FindByEmailAsync(model.Email);
			if (userExists != null)
				return BadRequest(new { Message = "البريد الإلكتروني مسجل بالفعل!" });

			// تحديد الصلاحية بناءً على اختيار المستخدم
			UserRole userRoleEnum;
			if (model.UserType == "Owner") userRoleEnum = UserRole.Owner;
			else if (model.UserType == "Admin") userRoleEnum = UserRole.Admin; // حماية إضافية يمكن إزالتها
			else userRoleEnum = UserRole.Client;

			AppUser newUser = new AppUser()
			{
				UserName = model.Email,
				Email = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName,
				NationalID = model.NationalID,
				PhoneNumber = model.PhoneNumber,
				Type = userRoleEnum,
				SecurityStamp = Guid.NewGuid().ToString()
			};

			var result = await _userManager.CreateAsync(newUser, model.Password);

			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				return StatusCode(500, new { Message = "فشل إنشاء المستخدم", Errors = errors });
			}

			// إضافة الصلاحية (Role) في جدول الأدوار
			await _userManager.AddToRoleAsync(newUser, userRoleEnum.ToString());

			return Ok(new { Message = "تم إنشاء الحساب بنجاح!" });
		}

		// ================== Login ==================
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(new { Message = "بيانات الدخول غير مكتملة" });

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
			{
				var userRoles = await _userManager.GetRolesAsync(user);

				// نأخذ أول دور للمستخدم (في هذا النظام المستخدم له دور واحد)
				var role = userRoles.FirstOrDefault() ?? "Client";

				var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.NameIdentifier, user.Id),
					new Claim(ClaimTypes.Role, role),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				};

				var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

				var token = new JwtSecurityToken(
					issuer: _configuration["JWT:ValidIssuer"],
					audience: _configuration["JWT:ValidAudience"],
					expires: DateTime.Now.AddDays(7),
					claims: authClaims,
					signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
				);

				return Ok(new
				{
					token = new JwtSecurityTokenHandler().WriteToken(token),
					expiration = token.ValidTo,
					userRole = role, // إرسال الدور للواجهة الأمامية للتوجيه
					Message = "تم تسجيل الدخول بنجاح"
				});
			}
			return Unauthorized(new { Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
		}
	}
}