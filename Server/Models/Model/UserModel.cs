using Microsoft.AspNetCore.Rewrite;
using Server.Models.DatabaseModel;
using System.Xml.Linq;

namespace Server.Models.ModelDTO
{
    public class UserModel
    {
        public int Iduser { get; set; }

        public string Login { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public int RoleId { get; set; }

        public int PersonId { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime? CreatedAt { get; set; }

        public UserModel(User user)
        {
            if (user != null)
            {
                Iduser = user.Id;
                Login = user.Login;
                PasswordHash = user.PasswordHash;
                RoleId = user.Role.Id;
                PersonId = user.Person.Id;
                IsActive = user.IsActive;
                LastLogin = user.LastLogin;
                CreatedAt = user.CreatedAt;
            }
            else
            {
                Iduser = -1;
                Login = "Unknown";
                PasswordHash = "Unknown";
                RoleId = -1;
                PersonId = -1;
                IsActive = null;
                LastLogin = null;
                CreatedAt = null;
            }
        }
    }
}
