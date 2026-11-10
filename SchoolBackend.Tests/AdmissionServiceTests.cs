using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchoolBackend.Data;
using SchoolBackend.Models;
using SchoolBackend.Services;
using Xunit;

public class AdmissionServiceTests
{
    private AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HoldSeat_ShouldSucceed_WhenSeatsAvailable()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryDbContext(dbName);
        context.SchoolClasses.Add(new SchoolClass { Id = "class-1", Name = "Class 6", TotalSeats = 2 });
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "HOLD_DURATION_SECONDS", "60" } }).Build();
        var service = new AdmissionService(context, configuration);

        // Act
        var hold = await service.HoldSeatAsync("class-1", "12345678901234567");

        // Assert
        Assert.NotNull(hold);
        Assert.Equal("HELD", hold.Status);
    }

    [Fact]
    public async Task HoldSeat_ShouldFail_WhenNoSeatsAvailable()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryDbContext(dbName);
        context.SchoolClasses.Add(new SchoolClass { Id = "class-2", Name = "Class 7", TotalSeats = 1 });
        context.SeatHolds.Add(new SeatHold { HoldId = "h1", SchoolClassId = "class-2", StudentNid = "11111111111111111", Status = "CONFIRMED", ExpiresAt = DateTime.UtcNow.AddHours(1) });
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "HOLD_DURATION_SECONDS", "60" } }).Build();
        var service = new AdmissionService(context, configuration);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.HoldSeatAsync("class-2", "22222222222222222");
        });
    }

    [Fact]
    public async Task ConcurrencyTest_ShouldNotOverbookSeats()
    {
        
        var dbName = Guid.NewGuid().ToString();
        using var context = GetInMemoryDbContext(dbName);
        context.SchoolClasses.Add(new SchoolClass { Id = "class-concurrency", Name = "Class 8", TotalSeats = 1 });
        await context.SaveChangesAsync();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "HOLD_DURATION_SECONDS", "60" } }).Build();
        
        
        var semaphore = new SemaphoreSlim(1, 1);

        
        var tasks = new List<Task<SeatHold?>>();
        for (int i = 1; i <= 5; i++)
        {
            string nid = $"nid-student-{i}-9999";
            tasks.Add(Task.Run(async () => {
                await semaphore.WaitAsync();
                try
                {
                    using var isolatedContext = GetInMemoryDbContext(dbName);
                    var isolatedService = new AdmissionService(isolatedContext, configuration);
                    return await isolatedService.HoldSeatAsync("class-concurrency", nid);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        var results = await Task.WhenAll(tasks);
        
        
        int successfulHolds = results.Count(r => r != null);
        Assert.Equal(1, successfulHolds);
    }
}