using StudentApi.Models;

namespace StudentApi.SimulateData
{
    public static class SimulateDataStudents
    {
        public static readonly List<Student> Students = new List<Student>()
        {
            new Student { Id = 1, Name = "Lina", Age = 14, Grade = 49 },
            new Student { Id = 2, Name = "Omar", Age = 15, Grade = 60 },
            new Student { Id = 3, Name = "Noura", Age = 13, Grade = 100 },
            new Student { Id = 4, Name = "Youssef", Age = 16, Grade = 90 },
            new Student { Id = 5, Name = "Salma", Age = 15, Grade = 35 }
        };
    }
}
