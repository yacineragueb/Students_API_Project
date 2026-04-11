using Microsoft.AspNetCore.Mvc;
using StudentAPIDataAccessLayer;
using StudentAPIBusinessLayer;

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
            List<StudentDTO> Students = Student.GetAllStudents();
            if (Students.Count == 0) return NotFound("There are no students!");

            return Ok(Students);
        }


        [HttpGet("passed", Name = "GetPassedStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDTO>> GetPassedStudents()
        {
            List<StudentDTO> PassedStudents = Student.GetPassedStudents();

            if (PassedStudents.Count == 0) return NotFound("There is no student passed");

            return Ok(PassedStudents);
        }


        [HttpGet("average-grade", Name = "GetAverageStudentsGrade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAverageStudentsGrade()
        {
            if (Student.GetAllStudents().Count == 0)
            {
                return NotFound("No student found.");
            }

            double Average = Student.GetAverageGrade();
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
            
            Student? student = Student.Find(ID);

            if(student == null)
            {
                return NotFound("Student with ID = " + ID + " Not found!");
            }

            StudentDTO studentDTO = student.StudentDTO;

            return Ok(studentDTO);
        }


        [HttpPost(Name = "AddNewStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<StudentDTO> AddNewStudent(StudentDTO student)
        {
            if(student == null || string.IsNullOrEmpty(student.Name) || student.Age < 0 || student.Grade < 0)
            {
                return BadRequest("Invalid Student Data.");
            }

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


        [HttpDelete("{ID}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult DeleteStudent(int ID)
        {
            if(ID < 0)
            {
                return BadRequest("Invalid ID!");
            }

            Student? student = Student.Find(ID);

            if (student == null)
            {
                return NotFound("Student With ID = " + ID + " Not Found!");
            }

            if(student.Delete())
            {
                return Ok("Student deleted successfully.");
            } else
            {
                return StatusCode
                    (
                        StatusCodes.Status500InternalServerError,
                        new { message = "An internal server error occurred. Please try again later." }
                    );
            }
        }


        [HttpPut("{ID}/student", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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


        [HttpPost("upload-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            string uploadedDirectory = @"D:\MyUpload";

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadedDirectory, fileName);

            if(!Directory.Exists(uploadedDirectory))
            {
                Directory.CreateDirectory(uploadedDirectory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Ok(new {filePath});
        }


        [HttpGet("get-image/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetImage(string fileName)
        {
            string uploadedDirectory = @"D:\MyUpload";
            string filePath = Path.Combine(uploadedDirectory, fileName);

            if(!System.IO.File.Exists(filePath))
            {
                return NotFound("Image not found!");
            }

            FileStream imageStream = System.IO.File.OpenRead(filePath);
            string mimeType = GetMimeType(filePath);

            return File(imageStream, mimeType);

        }

        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

    }
}
