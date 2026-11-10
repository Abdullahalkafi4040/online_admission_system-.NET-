using SchoolBackend.Models;

namespace SchoolBackend.Services
{
    public interface IAdmissionService
    {
        Task CheckAndExpireHoldsAsync();
        
        
        Task<SchoolClass> CreateClassAsync(string name, int totalSeats);
        Task<List<object>> GetAllClassesAsync();
        Task<object?> GetClassByIdAsync(string classId);
        
        
        Task<SeatHold> HoldSeatAsync(string classId, string studentNid);
        Task<object?> GetHoldByIdAsync(string holdId);
        Task<List<object>> GetStudentHoldsAsync(string studentNid);
        Task<bool> ConfirmHoldAsync(string holdId, string studentNid);
        Task<bool> CancelHoldAsync(string holdId, string studentNid);
    }
}