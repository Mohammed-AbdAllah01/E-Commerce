using DataBase.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project.DTOs;
using Project.Enums;
using Project.Services.Interfaces;
using Project.Tables;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {


        private readonly UserManager<Person> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;
        private readonly string _profileImagePath;
        private readonly IWebHostEnvironment _env;


        public AuthController(UserManager<Person> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService, ITokenService tokenService , IConfiguration configuration , AppDbContext db , IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _env = env;
            _tokenService = tokenService;
            _configuration = configuration;
            _db = db;
            _profileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Profile_Image");

            if (!Directory.Exists(_profileImagePath))
                {
                    Directory.CreateDirectory(_profileImagePath);
                }

        }




        [HttpPost("/CustomerReg")]
        public async Task<IActionResult> RegistNewCusomer(dtoNewCustomer cus)
        {
            if (ModelState.IsValid)
            {

                if (cus != null)
                {
                    var existingUser = await _userManager.FindByEmailAsync(cus.Email);
                    if (existingUser?.Email != null)
                        return BadRequest(new { message = "Email already exists" });
                    if (existingUser?.UserName != null)
                        return BadRequest(new { message = "UserName already exists" });
                    if (!Enum.TryParse<GenderType>(cus.Gender, true, out var newGender))
                        return BadRequest(new { message = "Invalid status value." });
                    if (cus.UserName == null || cus.Email == null || cus.Password == null ||
                            cus.Phone == null || cus.State == null || cus.Governorate == null ||
                            cus.Location == null || cus.Gender == null || cus.BirthDate == null)
                        return BadRequest(new { message = "All fields are required." });
                    Customer customer = new()
                    {
                        BirthDate = cus.BirthDate,
                        Governorate = cus.Governorate,
                        Location = cus.Location,
                        State = cus.State,
                        Email = cus.Email,                        
                        UserName = cus.UserName,
                        Gender = newGender,
                        Type = Enums.PersonType.Customer,
                        Status = Enums.AccStatus.Active,
                        PhoneNumber = cus.Phone,


                        // IMG = cus.IMG,
                    };
                    IdentityResult result = await _userManager.CreateAsync(customer, cus.Password);
                    if (result.Succeeded)
                    {
                        // توليد رمز التحقق
                        var verificationCode = GenerateVerificationCode();
                        customer.VerificationCode = verificationCode;
                        customer.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);

                        // إرسال الكود بالإيميل
                        await _emailService.SendEmailAsync(customer.Email, "Email Verification Code",
                            $"Your verification code is {verificationCode}. It will expire in 5 minutes.");

                        await _userManager.UpdateAsync(customer);

                        return Ok(new { message = "Registered successfully. Please verify your email using the verification code sent." });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("Error", error.Description);
                        }
                    }
                }
            }
            return BadRequest(ModelState);
        }


        [HttpPost("/AdminReg")]
        public async Task<IActionResult> RegistNewAdmin(dtoNewAdmin ad)
        {
            if (ModelState.IsValid)
            {
                if (ad != null)
                {
                    // Check if the email or username already exists
                    var existingUserByEmail = await _userManager.FindByEmailAsync(ad.Email);
                    if (existingUserByEmail?.Email != null)
                        return BadRequest(new { message = "Email already exists" });

                    var existingUserByUsername = await _userManager.FindByNameAsync(ad.UserName);
                    if (existingUserByUsername?.UserName != null)
                        return BadRequest(new { message = "UserName already exists" });

                    // Check if NationalId already exists
                    var existingAdmin = await _db.Admins
                        .FirstOrDefaultAsync(u => u.NationalId == ad.NationalId);
                    var existingDeliveryRep = await _db.DeliveryReps
                        .FirstOrDefaultAsync(u => u.NationalId == ad.NationalId);
                    var existingMerchant = await _db.Merchants
                        .FirstOrDefaultAsync(u => u.NationalId == ad.NationalId);
                    if (existingAdmin != null || existingMerchant != null || existingDeliveryRep != null)
                        return BadRequest(new { message = "National ID already exists" });

                    if (!Enum.TryParse<GenderType>(ad.Gender, true, out var newGender))
                        return BadRequest(new { message = "Invalid status value." });

                    if (ad.UserName == null || ad.Email == null || ad.Password == null ||
                            ad.Phone == null || ad.State == null || ad.Governorate == null ||
                            ad.Location == null || ad.Gender == null || ad.BirthDate == null 
                            )
                        return BadRequest(new { message = "All fields are required." });


                    Admin admin = new()
                    {
                        BirthDate = ad.BirthDate,
                        Governorate = ad.Governorate,
                        Location = ad.Location,
                        State = ad.State,
                        Email = ad.Email,                        
                        UserName = ad.UserName,
                        Gender = newGender,
                        NationalId = ad.NationalId,
                        Type = Enums.PersonType.Admin,
                        Status = Enums.AccStatus.Active,
                        PhoneNumber = ad.Phone,
                        // IMG = cus.IMG,
                    };

                    IdentityResult result = await _userManager.CreateAsync(admin, ad.Password);
                    if (result.Succeeded)
                    {
                        // توليد رمز التحقق
                        var verificationCode = GenerateVerificationCode();
                        admin.VerificationCode = verificationCode;
                        admin.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);

                        // إرسال الكود بالإيميل
                        await _emailService.SendEmailAsync(admin.Email, "Email Verification Code",
                            $"Your verification code is {verificationCode}. It will expire in 5 minutes.");

                        await _userManager.UpdateAsync(admin);

                        return Ok(new { message = "Registered successfully. Please verify your email using the verification code sent." });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("Error", error.Description);
                        }
                    }
                }
            }
            return BadRequest(ModelState);
        }

        

        [HttpPost("/MerchantReg")]
        public async Task<IActionResult> RegistNewMerchant(dtoNewMerchant Mer)
        {
            if (ModelState.IsValid)
            {
                if (Mer != null)
                {
                    // Check if the email or username already exists
                    var existingUserByEmail = await _userManager.FindByEmailAsync(Mer.Email);
                    if (existingUserByEmail?.Email != null)
                        return BadRequest(new { message = "Email already exists" });

                    var existingUserByUsername = await _userManager.FindByNameAsync(Mer.UserName);
                    if (existingUserByUsername?.UserName != null)
                        return BadRequest(new { message = "UserName already exists" });

                    var existingAdmin = await _db.Admins
    .FirstOrDefaultAsync(u => u.NationalId == Mer.NationalId);
                    var existingDeliveryRep = await _db.DeliveryReps
                        .FirstOrDefaultAsync(u => u.NationalId == Mer.NationalId);
                    var existingMerchant = await _db.Merchants
                        .FirstOrDefaultAsync(u => u.NationalId == Mer.NationalId);
                    if (existingAdmin != null || existingMerchant != null || existingDeliveryRep != null)
                        return BadRequest(new { message = "National ID already exists" });
                    if (!Enum.TryParse<GenderType>(Mer.Gender, true, out var newGender))
                        return BadRequest(new { message = "Invalid status value." });

                    Merchant merchant = new()
                    {
                        Governorate = Mer.Governorate,
                        Location = Mer.Location,
                        State = Mer.State,
                        Email = Mer.Email,                        
                        UserName = Mer.UserName,
                        Gender = newGender,
                        Type = Enums.PersonType.Merchant,
                        NationalId = Mer.NationalId,
                        PhoneNumber = Mer.Phone,
                        Status = Enums.AccStatus.Inactive,


                    }; 

                    IdentityResult result = await _userManager.CreateAsync(merchant, Mer.Password);
                    if (result.Succeeded)
                    {
                        // توليد رمز التحقق
                        var verificationCode = GenerateVerificationCode();
                        merchant.VerificationCode = verificationCode;
                        merchant.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);

                        // إرسال الكود بالإيميل
                        await _emailService.SendEmailAsync(merchant.Email, "Email Verification Code",
                            $"Your verification code is {verificationCode}. It will expire in 5 minutes.");

                        await _userManager.UpdateAsync(merchant);

                        return Ok(new { message = "Registered successfully. Please verify your email using the verification code sent Then Please Waiting your Account will Check first by Admin." });
                        
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("Error", error.Description);
                        }
                    }                  
                   
                }
            }
            return BadRequest(ModelState);
        }



        [HttpPost("verify-Code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            // التأكد من أن الرمز لم ينتهِ بعد
            if (user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "The verification code has expired. Please request a new one." });
            }

            // التحقق من الرمز المدخل
            if (user.VerificationCode != model.VerificationCode)
            {
                return BadRequest(new { message = "Invalid verification code." });
            }

            // إذا كان الرمز صحيحًا، تأكيد البريد الإلكتروني
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "✅ Email verified successfully!" });
        }

        [HttpPost("resend-verification-code")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationCodeDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            if (user.VerificationCodeExpiry > DateTime.UtcNow)
            {
                return BadRequest(new { message = "You can only request a new verification code after the previous one expires." });
            }

            // توليد رمز تحقق جديد باستخدام RNGCryptoServiceProvider
            var verificationCode = GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5); // تعيين صلاحية جديدة لمدة 5 دقائق

            // إرسال الرمز عبر البريد الإلكتروني
            await _emailService.SendEmailAsync(user.Email, "New Verification Code",
                $"Your new verification code is {verificationCode}. It will expire in 5 minutes.");

            // تحديث المستخدم في قاعدة البيانات
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "A new verification code has been sent to your email." });
        }


        [HttpPost("/Login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            if (ModelState.IsValid)
            {
                if (userLogin.Email == null || userLogin.Password == null)
                {
                    return BadRequest(new { message = "Invalid Login Attempt" });
                }

                var user = await _userManager.FindByEmailAsync(userLogin.Email);
                if (user != null )
                {
                    if(user.Status != AccStatus.Active)
                    {
                        return Unauthorized(new { message = "Your account is not Active now. Please contact support." });
                    }
                    if (await _userManager.CheckPasswordAsync(user, userLogin.Password))
                    {
                        if (!user.EmailConfirmed)
                            return Unauthorized(new { message = "Please confirm your email first." });

                        // Create a list of claims
                        var claims = new List<Claim>
                        {
                            new Claim("Name", user.UserName),
                            new Claim("ID", user.Id),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        // Add the user type as a claim
                        claims.Add(new Claim("Role", user.Type.ToString()));  // This is where you're adding the 'PersonType'

                        // Add roles as claims
                        var roles = await _userManager.GetRolesAsync(user);
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));  // Add roles to the claims
                        }

                        // Add the PersonType as a role claim if necessary
                        claims.Add(new Claim(ClaimTypes.Role, user.Type.ToString())); // This ensures that user type is added as role

                        // Generate JWT token
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            claims: claims,
                            issuer: _configuration["JWT:Issuer"],
                            audience: _configuration["JWT:Audience"],
                            signingCredentials: creds,
                            expires: DateTime.UtcNow.AddDays(1)
                        );

                        // Return the token
                        var _toke = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };
                        return Ok(_toke);
                    }
                    else
                    {
                        return Unauthorized(new { message = "Invalid Password" });
                    }
                }
                else
                {
                    return NotFound(new { message = "User Not Found" });
                }
            }
            return BadRequest(ModelState);
        }






        //  لو هنستخدم كود مش لينك

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null )
                return BadRequest(new { message = "Invalid request." });

            // توليد كود جديد
            var verificationCode = GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userManager.UpdateAsync(user);

            // إرسال الكود
            await _emailService.SendEmailAsync(user.Email, "Password Reset Code",
                $"Use this code to reset your password: {verificationCode}. It expires in 5 minutes.");

            return Ok(new { message = "Verification code sent to your email." });
        }






        // هنا بيتاكد من الكود
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });


            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;


            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);


            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "✅ Password reset successful." });
        }



        // هنا بحدث الصورة
        [HttpPut("/ProfilePicture")]
        public async Task<IActionResult> UpdatePicture(string userId, IFormFile image)
        {
            string imageUrl = null;
            if (image != null && image.Length > 0)
            {
                imageUrl = SaveImage( userId, image);
            }            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User ID is not exist." });
            }
            user.IMG = imageUrl;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                Console.WriteLine(imageUrl +"     " +user.IMG);
                return Ok(new { message = "Image Add Succesfully" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }


        // هنا بجيب الصورة
        [HttpGet("/ProfileInfo")]
        public async Task<IActionResult> ProfileInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var Profile = new dtoProfile
            {
                Name = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Governorate = user.Governorate,
                State = user.State,
                Location = user.Location,
                Image = $"//aston.runasp.net//Profile_Image//{user.IMG ?? "unknownUser.jpg"}"
            };
            if (Profile.Image == null )
            {
                return BadRequest(new { message = "User has no image." });
            }
            return Ok(Profile);
        }


        private string GenerateVerificationCode()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] number = new byte[4];
            rng.GetBytes(number);
            int code = Math.Abs(BitConverter.ToInt32(number, 0)) % 1000000;
            return code.ToString("D6");
        }


        private string SaveImage(string Id ,IFormFile imageFile)
        {
            string uniqueFileName = Id + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(_profileImagePath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            return  uniqueFileName;
        }


    }
}
