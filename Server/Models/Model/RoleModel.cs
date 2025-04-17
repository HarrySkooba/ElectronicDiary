using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class RoleModel
    {
        public int Idrole { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public RoleModel(Role role) 
        {
            if (role != null)
            {
                Idrole = role.Id;
                Name = role.Name;
                Description = role.Description;
            }
            else
            {
                Idrole = -1;
                Name = "Unknown";
                Description = "Unknown";
            }
        }
    }
}
