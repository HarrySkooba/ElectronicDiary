﻿using System;
using System.Collections.Generic;

namespace Server.DatabaseModel;

public partial class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? PhotoUrl { get; set; }

    public int Schoolid { get; set; }

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual School School { get; set; } = null!;

    public virtual ICollection<School> Schools { get; set; } = new List<School>();

    public virtual ICollection<SubjectTeacher> SubjectTeachers { get; set; } = new List<SubjectTeacher>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
