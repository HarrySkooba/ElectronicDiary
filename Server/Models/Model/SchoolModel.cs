using System;
using System.Collections.Generic;
using Server.Models.DatabaseModel;

namespace Server.Models.ModelDTO
{
    public class SchoolModel
    {
        public int Idschool { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string? Phone { get; set; }

        public int? DirectorId { get; set; }

        public string? Website { get; set; }

        public SchoolModel(School school) 
        { 
            if (school != null)
            {
                Idschool = school.Id;
                Name = school.Name;
                Address = school.Address;
                Phone = school.Phone;
                DirectorId = school.DirectorId;
                Website = school.Website;
            }
            else
            {
                Idschool = -1;
                Name = "Unknowk";
                Address = "Unknowk";
                Phone = "Unknowk";
                DirectorId = -1;   
                Website = "Unknowk";
            }
        }
    }
}
