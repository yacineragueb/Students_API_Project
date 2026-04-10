using Microsoft.AspNetCore.Mvc;
using StudentApi.Models;
using StudentApi.SimulateData;
using StudentAPIDataAccessLayer;

namespace StudentApi.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        [HttpGet(Name = "GetAllStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDTO>> GetAllStudents()
        {
            List<StudentDTO> Students = StudentAPIBusinessLayer.Student.GetAllStudents();
            if (Students.Count == 0) return NotFound("There are no students!");

            return Ok(Students);
        }


        [HttpGet("passed", Name = "GetPassedStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Student>> GetPassedStudents()
        {
            List<Student> PassedStudents = SimulateDataStudents.Students.Where(student => student.Grade > 50).ToList();

            if (PassedStudents.Count == 0) return NotFound("There is no student passed");

            return Ok(PassedStudents);
        }


        [HttpGet("average-grade", Name = "GetAverageStudentsGrade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAverageStudentsGrade()
        {
            if (SimulateDataStudents.Students.Count == 0)
            {
                return NotFound("No student found.");
            }

            double Average = SimulateDataStudents.Students.Average(student => student.Grade);
            return Ok(Average);
        }


        [HttpGet("{ID}/student", Name = "GetStudentByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Student> GetStudentByID(int ID)
        {
            if(ID < 1)
            {
                return BadRequest($"You enter an invalid ID: {ID}");
            }
            
            Student? student = SimulateDataStudents.Students.FirstOrDefault(student => student.Id == ID);

            if(student == null)
            {
                return NotFound("Student with ID = " + ID + " Not found!");
            }

            return Ok(student);
        }


        [HttpPost(Name = "AddNewStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Student> AddNewStudent(Student student)
        {
            if(student == null || string.IsNullOrEmpty(student.Name) || student.Age < 0 || student.Grade < 0)
            {
                return BadRequest("Invalid Student Data.");
            }

            student.Id = SimulateDataStudents.Students.Count > 0 ? SimulateDataStudents.Students.Max(student => student.Id) + 1 : 1;
            SimulateDataStudents.Students.Add(student);

            return CreatedAtRoute("GetStudentByID", new { ID = student.Id }, student);
        }


        [HttpDelete("{ID}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteStudent(int ID)
        {
            if(ID < 0)
            {
                return BadRequest("Invalid ID!");
            }

            Student? student = SimulateDataStudents.Students.FirstOrDefault(s => s.Id == ID);

            if(student == null)
            {
                return NotFound("Student With ID = " + ID + " Not Found!");
            }

            SimulateDataStudents.Students.Remove(student);
            return Ok("Student deleted successfully.");
        }


        [HttpPut("{ID}/student", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<Student> UpdateStudent(int ID, Student updatedStudent)
        {
            if(ID < 0 || updatedStudent == null || updatedStudent.Age < 0 || updatedStudent.Grade < 0 || string.IsNullOrEmpty(updatedStudent.Name))
            {
                return BadRequest("Invalid ID!");
            }

            Student? student = SimulateDataStudents.Students.FirstOrDefault(s => s.Id == ID);

            if(student == null)
            {
                return NotFound("Student not found!");
            }

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

            return Ok(student);
        }
    }
}
