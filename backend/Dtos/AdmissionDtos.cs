namespace SchoolBackend.Dtos;


public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
}


public class ClassResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int TotalHold { get; set; }
    public int ConfirmedSeats { get; set; }
    public int AvailableSeats { get; set; }
}


public class SeatHoldDto
{
    public Guid HoldId { get; set; }
    public Guid ClassId { get; set; }
    public string? ClassName { get; set; }
    public string StudentNid { get; set; } = string.Empty;
    public DateTime HoldExpiresAt { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsExpired { get; set; }
}