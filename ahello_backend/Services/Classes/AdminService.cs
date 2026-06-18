using ahello_backend.Models.Admin;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace ahello_backend.Services.Classes
{
    public class AdminService :IAdminService
    {
        private readonly IAdminRepository _repo;
        private readonly IConfiguration _config;


        public AdminService(IAdminRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<string?> LoginAsync(AdminLoginDto dto)
        {
            var admin = await _repo.GetByUsernameAsync(dto.Username);
            if (admin == null)
                return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);
            if (!isValid)
                return null;

            return GenerateJwtToken(admin);
        }

        private string GenerateJwtToken(AdminUser admin)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, admin.AdminId.ToString()),
            new Claim("username", admin.Username),
            new Claim("role", "Admin")
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_config["Jwt:ExpiryMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<int> CreateAdminAsync(AdminCreateDto dto)
        {
            if (await _repo.UsernameExistsAsync(dto.Username))
                throw new Exception("Username already exists");

            var admin = new AdminUser
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                IsActive = true
            };

            return await _repo.CreateAsync(admin);
        }
    }
}
