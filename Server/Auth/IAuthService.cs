using Server.Models.DTO;

namespace Server
{
    public interface IAuthService
    {
        Task<UserResponseDto> Login(UserLoginDto request);
    }
}
