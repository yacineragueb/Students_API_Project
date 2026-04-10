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
        public ActionResult<IEnumerable<StudentDTO>> GetPassedStudents()
        {
            List<StudentDTO> PassedStudents = StudentAPIBusinessLayer.Student.GetPassedStudents();

            if (PassedStudents.Count == 0) return NotFound("There is no student passed");

            return Ok(PassedStudents);
        }


        [HttpGet("average-grade", Name = "GetAverageStudentsGrade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAverageStudentsGrade()
        {
            if (StudentAPIBusinessLayer.Student.GetAllStudents().Count == 0)
            {
                return NotFound("No student found.");
            }

            double Average = StudentAPIBusinessLayer.Student.GetAverageGrade();
            return Ok(Average);
        }


        [HttpGet("{ID}/student", Name = "GetStudentByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<StudentDTO> GetStudentByID(int ID)
        {
            if(ID < 1)
            {
                return BadRequest($"You enter an invalid ID: {ID}");
            }
            
            StudentAPIBusinessLayer.Student? student = StudentAPIBusinessLayer.Student.GetStudentByID(ID);

            if(student == null)
            {
                return NotFound("Student with ID = " + ID + " Not found!");
            }

            return Ok(student);
        }


        [HttpPost(Name = "AddNewStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<StudentDTO> AddNewStudent(StudentDTO student)
        {
            if(student == null || string.IsNullOrEmpty(student.Name) || student.Age < 0 || student.Grade < 0)
            {
                return BadRequest("Invalid Student Data.");
            }

            StudentAPIBusinessLayer.Student NewStudent = new StudentAPIBusinessLayer.Student(student);

            NewStudent.Save();

            return CreatedAtRoute("GetStudentByID", new { ID = student.ID }, NewStudent);
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

            StudentAPIBusinessLayer.Student? student = StudentAPIBusinessLayer.Student.GetStudentByID(ID);

            if (student == null)
            {
                return NotFound("Student With ID = " + ID + " Not Found!");
            }

            if(student.Delete())
            {
                return Ok("Student deleted successfully.");
            } else
            {
                return BadRequest("Failed to delete student!");
            }

        }


        [HttpPut("{ID}/student", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<StudentDTO> UpdateStudent(int ID, StudentDTO updatedStudent)
        {
            if(ID < 0 || updatedStudent == null || updatedStudent.Age < 0 || updatedStudent.Grade < 0 || string.IsNullOrEmpty(updatedStudent.Name))
            {
                return BadRequest("Invalid ID!");
            }

            StudentAPIBusinessLayer.Student? student = StudentAPIBusinessLayer.Student.GetStudentByID(ID);

            if(student == null)
            {
                return NotFound("Student not found!");
            }

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

            student.Save();

            return Ok(student);
        }
    }
}
