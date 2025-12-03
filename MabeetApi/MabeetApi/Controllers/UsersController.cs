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
				return BadRequest(new { Message = "بيانات التسجيل غير صالحة.", Errors = ModelState });

			try
			{
				var existingUser = await _userManager.FindByEmailAsync(model.Email);
				if (existingUser != null)
					return BadRequest(new { Message = "البريد الإلكتروني مستخدم بالفعل." });

				var user = new AppUser
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					UserName = model.Email,
					NationalID = model.NationalID,
					PhoneNumber = model.PhoneNumber,
					IsActive = true,
					Type = model.UserType == "Owner" ? UserRole.Owner : UserRole.Client,
					CreatedAt = DateTime.UtcNow
				};

				var result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					// إضافة الدور لجدول الأدوار أيضاً للتوافق
					string roleName = user.Type.ToString();
					if (!await _roleManager.RoleExistsAsync(roleName))
						await _roleManager.CreateAsync(new IdentityRole(roleName));

					await _userManager.AddToRoleAsync(user, roleName);

					return Ok(new { Message = "تم التسجيل بنجاح" });
				}
				else
				{
					var identityErrors = result.Errors.Select(e => e.Description).ToList();
					return BadRequest(new { Message = "فشل إنشاء المستخدم.", Errors = identityErrors });
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "حدث خطأ داخلي.", ErrorDetails = ex.Message });
			}
		}

		// ================== Login ==================
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginViewModel model)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
				return Unauthorized(new { Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة." });

			if (!user.IsActive)
			{
				return Unauthorized(new { Message = "عذراً، تم تعطيل هذا الحساب. يرجى التواصل مع الإدارة." });
			}

			// 🛑🛑 التعديل الجذري هنا 🛑🛑
			// بدلاً من البحث في جدول الأدوار (الذي قد يكون قديماً)، نعتمد على حقل Type الموجود في المستخدم نفسه
			// هذا يضمن أنه إذا قمتِ بتغيير النوع من لوحة التحكم، سينعكس هنا فوراً
			string role = user.Type.ToString();

			var token = GenerateJwtToken(user, role);

			return Ok(new
			{
				user.Id,
				user.UserName,
				user.Email,
				role, // سيرسل الآن "Admin" لأننا قرأناه من الـ Type المحدث
				token = new JwtSecurityTokenHandler().WriteToken(token)
			});
		}

		// ================== Get User By ID ==================
		[Authorize]
		[HttpGet("{id}")]
		public async Task<IActionResult> GetUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound("User not found");

			// قراءة الدور من النوع أيضاً
			return Ok(new { user.Id, user.Email, user.FirstName, user.LastName, Role = user.Type.ToString() });
		}

		// ================== JWT Helper ==================
		private SecurityToken GenerateJwtToken(AppUser user, string role)
		{
			var authClaims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.UserName ?? ""),
				new Claim(ClaimTypes.Email, user.Email ?? ""),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Role, role),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			};

			var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

			return new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"],
				audience: _configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddDays(7),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
			);
		}
	}
}