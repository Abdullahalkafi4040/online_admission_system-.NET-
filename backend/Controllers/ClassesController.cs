using Microsoft.AspNetCore.Mvc;
using SchoolBackend.Services;

namespace SchoolBackend.Controllers
{
    [ApiController]
    [Route("classes")]
    public class ClassesController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;

        public ClassesController(IAdmissionService admissionService)
        {
            _admissionService = admissionService;
        }

        private string? GetStudentNid()
        {
            if (Request.Headers.TryGetValue("X-Student-Id", out var nid))
            {
                return nid.ToString();
            }
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || dto.TotalSeats <= 0)
                return BadRequest(new { message = "Invalid class name or total seats." });

            var schoolClass = await _admissionService.CreateClassAsync(dto.Name, dto.TotalSeats);
            return StatusCode(201, new { id = schoolClass.Id, name = schoolClass.Name, totalSeats = schoolClass.TotalSeats });
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _admissionService.GetAllClassesAsync();
            return Ok(classes);
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassById(string classId) 
        {
            var c = await _admissionService.GetClassByIdAsync(classId);
            if (c == null) return NotFound(new { message = "Class not found" });
            return Ok(c);
        }

        [HttpPost("{classId}/holds")]
        public async Task<IActionResult> HoldSeat(string classId) 
        {
            var studentNid = GetStudentNid();
            if (string.IsNullOrEmpty(studentNid))
                return BadRequest(new { message = "X-Student-Id header is missing." });

            try
            {
                var hold = await _admissionService.HoldSeatAsync(classId, studentNid);
                return StatusCode(201, new { holdId = hold.HoldId, classId = hold.SchoolClassId, studentId = hold.StudentNid, status = hold.Status, expiresAt = hold.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ssZ") });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CreateClassDto
    {
        public string Name { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
    }
}