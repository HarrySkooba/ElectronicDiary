using Server.Models.DTO;

namespace Server.Profile
{
    public interface IProfileService
    {
        Task<ProfileResponseDTO> GetProfileInfo();
        Task<SchoolResponseDTO> GetSchoolInfo();
        Task<ClassResponseDTO> GetClassInfo();
    }
}
