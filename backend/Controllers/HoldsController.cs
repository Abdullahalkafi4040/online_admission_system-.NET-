using Microsoft.AspNetCore.Mvc;
using SchoolBackend.Services;

namespace SchoolBackend.Controllers
{
    [ApiController]
    [Route("")]
    public class HoldsController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;

        public HoldsController(IAdmissionService admissionService)
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

        [HttpGet("holds/{holdId}")]
        public async Task<IActionResult> GetHold(string holdId) 
        {
            var hold = await _admissionService.GetHoldByIdAsync(holdId);
            if (hold == null) return NotFound(new { message = "Hold not found" });
            return Ok(hold);
        }

        [HttpGet("me/holds")]
        public async Task<IActionResult> GetMyHolds()
        {
            var studentNid = GetStudentNid();
            if (string.IsNullOrEmpty(studentNid))
                return BadRequest(new { message = "X-Student-Id header is missing." });

            var holds = await _admissionService.GetStudentHoldsAsync(studentNid);
            return Ok(holds);
        }

        [HttpPost("holds/{holdId}/confirm")]
        public async Task<IActionResult> ConfirmHold(string holdId) 
        {
            var studentNid = GetStudentNid();
            if (string.IsNullOrEmpty(studentNid))
                return BadRequest(new { message = "X-Student-Id header is missing." });

            var success = await _admissionService.ConfirmHoldAsync(holdId, studentNid);
            if (!success) return BadRequest(new { message = "Cannot confirm hold or invalid hold/student." });

            return Ok(new { holdId = holdId, status = "CONFIRMED" });
        }

        [HttpDelete("holds/{holdId}")]
        public async Task<IActionResult> CancelHold(string holdId) 
        {
            var studentNid = GetStudentNid();
            if (string.IsNullOrEmpty(studentNid))
                return BadRequest(new { message = "X-Student-Id header is missing." });

            var success = await _admissionService.CancelHoldAsync(holdId, studentNid);
            if (!success) return NotFound(new { message = "Hold not found or unauthorized." });

            return NoContent();
        }
    }
}