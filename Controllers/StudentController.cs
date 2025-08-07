using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Models.Models.RequestModels;
using SchoolProject.Models.Models.ResponseModels;
using SchoolProject.Models.Models.School;
using SchoolProject.Service.Repository.Interfaces;
using Serilog;
namespace SchoolProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;

        public StudentController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        [HttpGet("students")]
        public IActionResult GetAllStudents([FromQuery] StudentFilterModel filterParams)
        {
            try
            {

                Log.Information("Fetching students from DB...");
                var students = _studentRepository.GetFilteredStudents((int)filterParams.page, (int)filterParams.pageSize, filterParams.searchText);

                if (!students.Any())
                {
                    return NotFound(new { message = "No students found!" });
                } 

                return Ok(students);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching students");
                return StatusCode(500, new { message = "Something went wrong while fetching students", error = ex.Message });
            }
        }

        [HttpGet("standard/{standard}")]
        public IActionResult GetStudentsByStandard([FromRoute] int standard)
        {
            try
            {
                Log.Information("Fetching students for standard {Standard}", standard);
                var students = _studentRepository.GetStudentsByStandard(standard);

                if (students == null || students.Count == 0)
                {
                    Log.Information("No students found in standard {Standard}", standard);
                    return NotFound(new { message = $"No students found in standard {standard}" });
                }

                //Log.Information("Fetched {Count} students in standard {Standard}", students.Count, standard);
                return Ok(students);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching students by standard {Standard}", standard);
                return StatusCode(500, new { message = "Error fetching students by standard", error = ex.Message });
            }
        }

        [HttpGet("{studentSid}")]
        public IActionResult GetStudentById([FromRoute] string studentSid)
        {
            try
            {
                Log.Information("Fetching student with SID: {StudentSid}", studentSid);
                var student = _studentRepository.GetStudentBySid(studentSid);

                if (student == null)
                {
                    //Log.Warning("Student with SID {StudentSid} not found", studentSid);
                    return NotFound(new { message = $"Student with SID '{studentSid}' not found." });
                }

                //Log.Information("Student with SID {StudentSid} fetched successfully", studentSid);
                return Ok(student);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching student with SID {StudentSid}", studentSid);
                return StatusCode(500, new { message = "Error fetching student", error = ex.Message });
            }
        }

        [HttpDelete("{studentSid}")]
        public async Task<IActionResult> deleteStudent([FromRoute] string studentSid)
        {
            try
            {
                Log.Information("Attempting to delete student with SID: {StudentSid}", studentSid);
                bool isDeleted = await _studentRepository.deleteStudent(studentSid);

                if (isDeleted)
                {
                    //Log.Information("Student with SID {StudentSid} marked as deleted", studentSid);
                    return Ok(new { message = "Student deleted successfully" });
                }

                //Log.Warning("Student with SID {StudentSid} not found for deletion", studentSid);
                return NotFound(new { message = $"Student with SID '{studentSid}' not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting student with SID {StudentSid}", studentSid);
                return StatusCode(500, new { message = "Error deleting student", error = ex.Message });
            }
        }

        [HttpPost("upsert/{studentSid?}")]
        public async Task<IActionResult> upsertStudent([FromBody] StudentUpsertRequest student, [FromRoute] string? studentSid = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Log.Information("Starting upsert for student SID: {StudentSid}", studentSid ?? "New Student");

                var upsertResponse = await _studentRepository.upsert(student, studentSid);

                if (upsertResponse.StatusCode == 500)
                {
                    //Log.Error("Upsert operation failed: {Message}", upsertResponse.Message);
                    return StatusCode(500, new { message = "Upsert operation failed", details = upsertResponse.Message });
                }

                //Log.Information("Upsert operation completed successfully: {Message}", upsertResponse.Message);
                return StatusCode(upsertResponse.StatusCode, upsertResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during upsert operation for student SID {StudentSid}", studentSid);
                return StatusCode(500, new { message = "Error during upsert operation", error = ex.Message });
            }
        }

        [HttpGet("test-exception")]
        public IActionResult ThrowException()
        {
            throw new Exception("This is a test exception for Serilog middleware!");
        }
    }

}
