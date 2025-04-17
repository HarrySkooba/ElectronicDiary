using System;
using System.Collections.Generic;
using Server.Models.DatabaseModel;

namespace Server.Models.ModelDTO
{
    public class SubjectModel
    {
        public int Idsubject { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public SubjectModel(Subject subject)
        {
            if (subject != null)
            {
                Idsubject = subject.Id;
                Name = subject.Name;
                Description = subject.Description;
            }
            else
            {
                Idsubject = -1;
                Name = "Unknown";
                Description = "Unknown";
            }
        } 
    }
}


