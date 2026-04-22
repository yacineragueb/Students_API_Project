using Microsoft.AspNetCore.Mvc;
using StudentAPIDataAccessLayer;
using StudentAPIBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace StudentApi.Controllers
{
    [Authorize]
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet(Name = "GetAllStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<StudentDTO>> GetAllStudents()
        {
            List<StudentDTO> Students = Student.GetAllStudents();
            if (Students.Count == 0) return NotFound("There are no students!");

            return Ok(Students);
        }


        [AllowAnonymous]
        [HttpGet("passed", Name = "GetPassedStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDTO>> GetPassedStudents()
        {
            List<StudentDTO> PassedStudents = Student.GetPassedStudents();

            if (PassedStudents.Count == 0) return NotFound("There is no student passed");

            return Ok(PassedStudents);
        }


        [AllowAnonymous]
        [HttpGet("average-grade", Name = "GetAverageStudentsGrade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAverageStudentsGrade()
        {
            double Average = Student.GetAverageGrade();
            return Ok(Average);
        }


        [HttpGet("{ID}/student", Name = "GetStudentByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StudentDTO>> GetStudentByID(int ID, [FromServices] IAuthorizationService authorizationService)
        {
            if(ID < 1)
            {
                return BadRequest($"You enter an invalid ID: {ID}");
            }

            AuthorizationResult authResult = await authorizationService.AuthorizeAsync(User, ID, "StudentOwnerOrAdmin");
            
            if(!authResult.Succeeded)
            {
                return Forbid();
            }

            Student? student = Student.Find(ID);

            if (student == null)
            {
                return NotFound("Student with ID = " + ID + " Not found!");
            }

            StudentDTO studentDTO = student.StudentDTO;

            return Ok(studentDTO);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost(Name = "AddNewStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<StudentDTO> AddNewStudent(StudentCreateDTO student)
        {
            if(student == null || string.IsNullOrEmpty(student.Name) || student.Age < 0 || student.Grade < 0 || string.IsNullOrEmpty(student.Email) || string.IsNullOrEmpty(student.Password) || string.IsNullOrEmpty(student.Role))
            {
                return BadRequest("Invalid Student Data.");
            }

            student.Password = BCrypt.Net.BCrypt.HashPassword(student.Password);

            Student NewStudent = new Student(student, Student.enMode.AddNew);

            if(NewStudent.Save())
            {
                student.ID = NewStudent.ID;

                return CreatedAtRoute("GetStudentByID", new { ID = student.ID }, student);

            } else
            {
                return StatusCode
                    (
                        StatusCodes.Status500InternalServerError,
                        new { message = "An internal server error occurred. Please try again later." }
                    );
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{ID}/student", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<StudentDTO> UpdateStudent(int ID, StudentDTO updatedStudent)
        {
            if(ID < 0 || updatedStudent == null || updatedStudent.Age < 0 || updatedStudent.Grade < 0 || string.IsNullOrEmpty(updatedStudent.Name))
            {
                return BadRequest("Invalid ID!");
            }

            Student? student = Student.Find(ID);

            if(student == null)
            {
                return NotFound("Student not found!");
            }

            student.Name = updatedStudent.Name;
            student.Age = updatedStudent.Age;
            student.Grade = updatedStudent.Grade;

            if (student.Save())
            {
                return Ok(student.StudentDTO);
            }
            else
            {
                return StatusCode
                    (
                        StatusCodes.Status500InternalServerError,
                        new { message = "An internal server error occurred. Please try again later." }
                    );
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{ID}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult DeleteStudent(int ID)
        {
            if (ID < 0)
            {
                return BadRequest("Invalid ID!");
            }

            Student? student = Student.Find(ID);

            if (student == null)
            {
                return NotFound("Student With ID = " + ID + " Not Found!");
            }

            if (student.Delete())
            {
                return Ok("Student deleted successfully.");
            }
            else
            {
                return StatusCode
                    (
                        StatusCodes.Status500InternalServerError,
                        new { message = "An internal server error occurred. Please try again later." }
                    );
            }
        }

    }
}
