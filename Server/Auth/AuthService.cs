using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Models.DTO;

namespace Server
{
    public class AuthService : IAuthService
    {
        private readonly ElectronicDiaryContext _context;
        private readonly IConfiguration _config;

        public AuthService(ElectronicDiaryContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<UserResponseDto> Register(UserRegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Login == request.Login))
                throw new Exception("Пользователь с таким логином уже существует");

            CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                Login = request.Login,
                PasswordHash = hash,
                RoleId = request.RoleId,
                PersonId = request.PersonId,
                Salt = salt
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user)
            .Reference(u => u.Role)
            .LoadAsync();

            return new UserResponseDto
            {
                Login = user.Login,
                RoleName = user.Role.Name,
                Token = CreateToken(user)
            };
        }

        public async Task<UserResponseDto> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.Salt))
                throw new Exception("Неверный логин или пароль");

            await _context.Entry(user)
      .Reference(u => u.Role)
      .LoadAsync();

            return new UserResponseDto
            {
                Login = user.Login,
                RoleName = user.Role.Name,
                Token = CreateToken(user)
            };
        }


        private string CreateToken(User user)
        {
            var claims = new[]
            {
               new Claim(ClaimTypes.Name, user.Login),
               new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(hash);
        }
    }
}
