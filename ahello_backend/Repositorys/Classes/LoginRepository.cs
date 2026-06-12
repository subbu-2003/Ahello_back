using ahello_backend.DbContexts;
using ahello_backend.Models.Login;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;

namespace ahello_backend.Repositorys.Classes
{
    public class LoginRepository : ILoginRepository
    {
        private readonly DbContext _db;
        private readonly IEmailRepository _emailRepository;
        private readonly IConfiguration _config;

        public LoginRepository(DbContext db, IEmailRepository emailRepository, IConfiguration config)
        {
            _db = db;
            _emailRepository = emailRepository;
            _config = config;

        }

        public async Task<LoginResponseDto> LoginAsync(string email)
        {
            using var connection = _db.GetConnection();

            // CHECK USER

            var user =
                await connection.QueryFirstOrDefaultAsync<LoginResponseDto>(
                @"
        SELECT
            UserId,
            Email,
            FullName AS UserName,
            ProfileUrl
        FROM users
        WHERE Email = @Email
        LIMIT 1",
                new
                {
                    Email = email.Trim()
                });

            // NEW USER INSERT

            if (user == null)
            {
                var newUserId =
                    await connection.ExecuteScalarAsync<int>(
                    @"
            INSERT INTO users
            (
                Email,
                FullName
            )
            VALUES
            (
                @Email,
                @FullName
            );

            SELECT LAST_INSERT_ID();
            ",
                    new
                    {
                        Email = email.Trim(),
                        FullName = email.Split('@')[0]
                    });

                user = new LoginResponseDto
                {
                    UserId = newUserId,
                    Email = email.Trim(),
                    UserName = email.Split('@')[0],
                    ProfileUrl=null
                };
            }

            return user;
        }
        // Repositorys/Classes/LoginRepository.cs


        public async Task<LoginResponseDto> GoogleLoginAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[]
                {
            _config["Google:ClientId"]
        }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (!payload.EmailVerified)
                return null;

            using var connection = _db.GetConnection();

            var email = payload.Email.Trim();

            var user = await connection.QueryFirstOrDefaultAsync<LoginResponseDto>(
                @"
        SELECT
            UserId,
            Email,
            FullName AS UserName,
            ProfileUrl
        FROM users
        WHERE Email = @Email
        LIMIT 1",
                new { Email = email });

            // Existing user: don't change profile image
            if (user != null)
                return user;

            // New Google user: insert Google profile image
            var fullName = !string.IsNullOrWhiteSpace(payload.Name)
                ? payload.Name
                : email.Split('@')[0];

            var newUserId = await connection.ExecuteScalarAsync<int>(
                @"
        INSERT INTO users
        (
            Email,
            FullName,
            ProfileUrl
        )
        VALUES
        (
            @Email,
            @FullName,
            @ProfileUrl
        );

        SELECT LAST_INSERT_ID();
        ",
                new
                {
                    Email = email,
                    FullName = fullName,
                    ProfileUrl = payload.Picture
                });

            return new LoginResponseDto
            {
                UserId = newUserId,
                Email = email,
                UserName = fullName,
                ProfileUrl = payload.Picture
            };
        }

        public async Task<bool> SendOtpAsync(string email)
        {
            using var connection = _db.GetConnection();

            var otp = new Random()
                .Next(100000, 999999)
                .ToString();

            await connection.ExecuteAsync(
                @"INSERT INTO loginotp
                  (
                      Email,
                      Otp,
                      Expired,
                      CreatedAt
                  )
                  VALUES
                  (
                      @Email,
                      @Otp,
                      DATE_ADD(NOW(), INTERVAL 5 MINUTE),
                      NOW()
                  )",
                new
                {
                    Email = email,
                    Otp = otp
                });

            await _emailRepository
                .SendLoginOtpEmailAsync(
                    email,
                    otp);

            return true;
        }
        public async Task<LoginResponseDto> ValidateOtpAsync(
            string email,
            string otp)
        {
            using var connection = _db.GetConnection();

            // CHECK OTP

            var otpData = await connection.QueryFirstOrDefaultAsync(
                @"
        SELECT *
        FROM loginotp
        WHERE Email = @Email
        AND Otp = @Otp
        AND Expired > NOW()
        ORDER BY Id DESC
        LIMIT 1",
                new
                {
                    Email = email.Trim(),
                    Otp = otp.Trim()
                });

            if (otpData == null)
                return null;

            // GET USER

            var user =
                await connection.QueryFirstOrDefaultAsync<LoginResponseDto>(
                @"
        SELECT
            UserId,
            Email,
            FullName AS UserName,
            ProfileUrl
        FROM users
        WHERE Email = @Email
        LIMIT 1",
                new
                {
                    Email = email.Trim()
                });

            // INSERT NEW USER IF NOT EXISTS

            if (user == null)
            {
                var fullName = email.Split('@')[0];

                var newUserId =
                    await connection.ExecuteScalarAsync<int>(
                    @"
            INSERT INTO users
            (
                Email,
                FullName
            )
            VALUES
            (
                @Email,
                @FullName
            );

            SELECT LAST_INSERT_ID();
            ",
                    new
                    {
                        Email = email.Trim(),
                        FullName = fullName
                    });

                user = new LoginResponseDto
                {
                    UserId = newUserId,
                    Email = email.Trim(),
                    UserName = fullName,
                    ProfileUrl=null
                };
            }

            return user;
        }

        public string GenerateJwtToken(LoginResponseDto user)
        {
            var key =
                _config["Jwt:Key"]
                ?? throw new Exception("JWT key missing");

            var keyBytes =
                Encoding.UTF8.GetBytes(key);

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Email),

                new Claim(
                    "UserId",
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),

                signingCredentials:
                    new SigningCredentials(
                        new SymmetricSecurityKey(keyBytes),
                        SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }

}