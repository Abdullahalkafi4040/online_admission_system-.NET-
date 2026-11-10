using Microsoft.EntityFrameworkCore;
using SchoolBackend.Data;
using SchoolBackend.Models;

namespace SchoolBackend.Services
{
    public class AdmissionService : IAdmissionService
    {
        private readonly AppDbContext _context;
        private readonly int _holdDurationSeconds;

        public AdmissionService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _holdDurationSeconds = configuration.GetValue<int>("HOLD_DURATION_SECONDS", 60);
        }

        public async Task CheckAndExpireHoldsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredHolds = await _context.SeatHolds
                .Where(h => h.Status == "HELD" && h.ExpiresAt <= now)
                .ToListAsync();

            if (expiredHolds.Any())
            {
                foreach (var hold in expiredHolds)
                {
                    hold.Status = "EXPIRED";
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<SchoolClass> CreateClassAsync(string name, int totalSeats)
        {
            var schoolClass = new SchoolClass
            {
                Name = name,
                TotalSeats = totalSeats
            };

            _context.SchoolClasses.Add(schoolClass);
            await _context.SaveChangesAsync();
            return schoolClass;
        }

        public async Task<List<object>> GetAllClassesAsync()
        {
            await CheckAndExpireHoldsAsync();
            var classes = await _context.SchoolClasses.Include(c => c.SeatHolds).ToListAsync();

            var now = DateTime.UtcNow;

            return classes.Select(c => {
                int heldSeats = c.SeatHolds.Count(h => h.Status == "HELD" && h.ExpiresAt.ToUniversalTime() > now);
                int confirmedSeats = c.SeatHolds.Count(h => h.Status == "CONFIRMED");
                int availableSeats = c.TotalSeats - (heldSeats + confirmedSeats);

                return (object)new {
                    id = c.Id,
                    name = c.Name,
                    totalSeats = c.TotalSeats,
                    heldSeats = heldSeats,
                    confirmedSeats = confirmedSeats,
                    availableSeats = Math.Max(0, availableSeats)
                };
            }).ToList();
        }

        public async Task<object?> GetClassByIdAsync(string classId)
        {
            await CheckAndExpireHoldsAsync();
            var c = await _context.SchoolClasses.Include(x => x.SeatHolds).FirstOrDefaultAsync(x => x.Id == classId);
            if (c == null) return null;

            var now = DateTime.UtcNow;
            int held = c.SeatHolds.Count(h => h.Status == "HELD" && h.ExpiresAt.ToUniversalTime() > now);
            int confirmed = c.SeatHolds.Count(h => h.Status == "CONFIRMED");
            int available = Math.Max(0, c.TotalSeats - (held + confirmed));

            return new {
                id = c.Id,
                name = c.Name,
                totalSeats = c.TotalSeats,
                heldSeats = held,
                confirmedSeats = confirmed,
                availableSeats = available
            };
        }

        public async Task<SeatHold> HoldSeatAsync(string classId, string studentNid)
        {
            await CheckAndExpireHoldsAsync();

            bool isInMemory = _context.Database.IsInMemory();
            var transaction = isInMemory ? null : await _context.Database.BeginTransactionAsync();

            try
            {
                var schoolClass = await _context.SchoolClasses
                    .Include(c => c.SeatHolds)
                    .FirstOrDefaultAsync(c => c.Id == classId);

                if (schoolClass == null) throw new Exception("Class not found");

                var now = DateTime.UtcNow;

                var existingActiveHold = schoolClass.SeatHolds
                    .FirstOrDefault(h => h.StudentNid == studentNid && 
                                         ((h.Status == "HELD" && h.ExpiresAt.ToUniversalTime() > now) || h.Status == "CONFIRMED"));

                if (existingActiveHold != null)
                {
                    throw new InvalidOperationException("Student already has an active hold or confirmation in this class.");
                }

                int heldSeats = schoolClass.SeatHolds
                    .Count(h => h.Status == "HELD" && h.ExpiresAt.ToUniversalTime() > now);
                
                int confirmedSeats = schoolClass.SeatHolds
                    .Count(h => h.Status == "CONFIRMED");

                int availableSeats = schoolClass.TotalSeats - (heldSeats + confirmedSeats);

                if (availableSeats <= 0)
                {
                    throw new InvalidOperationException("No seats available.");
                }

                var newHold = new SeatHold
                {
                    SchoolClassId = classId,
                    StudentNid = studentNid,
                    Status = "HELD",
                    ExpiresAt = now.AddSeconds(_holdDurationSeconds)
                };

                _context.SeatHolds.Add(newHold);
                await _context.SaveChangesAsync();

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return newHold;
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }

        public async Task<object?> GetHoldByIdAsync(string holdId)
        {
            await CheckAndExpireHoldsAsync();
            var hold = await _context.SeatHolds.FindAsync(holdId);
            if (hold == null) return null;

            return new {
                holdId = hold.HoldId,
                classId = hold.SchoolClassId,
                status = hold.Status,
                expiresAt = hold.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        public async Task<List<object>> GetStudentHoldsAsync(string studentNid)
        {
            await CheckAndExpireHoldsAsync();
            var now = DateTime.UtcNow;

            return await _context.SeatHolds
                .Include(h => h.SchoolClass)
                .Where(h => h.StudentNid == studentNid && !(h.Status == "HELD" && h.ExpiresAt <= now))
                .Select(h => (object)new {
                    holdId = h.HoldId,
                    classId = h.SchoolClassId,
                    className = h.SchoolClass != null ? h.SchoolClass.Name : "",
                    status = h.Status,
                    expiresAt = h.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToListAsync();
        }

        public async Task<bool> ConfirmHoldAsync(string holdId, string studentNid)
        {
            await CheckAndExpireHoldsAsync();
            var hold = await _context.SeatHolds.FindAsync(holdId);

            if (hold == null || hold.StudentNid != studentNid || hold.Status != "HELD")
                return false;

            hold.Status = "CONFIRMED";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelHoldAsync(string holdId, string studentNid)
        {
            var hold = await _context.SeatHolds.FindAsync(holdId);

            if (hold == null || hold.StudentNid != studentNid)
                return false;

            hold.Status = "CANCELLED";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}