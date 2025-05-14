using Server.Models.DTO;

namespace Server
{
    public interface IAuthService
    {
        Task<UserResponseDto> Register(UserRegisterDto request);
        Task<UserResponseDto> Login(UserLoginDto request);
    }
}
