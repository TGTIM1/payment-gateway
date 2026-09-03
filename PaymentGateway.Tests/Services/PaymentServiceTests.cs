using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentGateway.Models;   
using PaymentGateway.Models.Enums;  
using PaymentGateway.Services;

namespace PaymentGateway.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<ILogger<PaymentService>> _loggerMock = new();

    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldCreatePayment_WhenDataIsValid()
    {
        // Arrange
        using var dbContext = GetDbContext();
        var service = new PaymentService(dbContext, _loggerMock.Object);

        var dto = new PaymentRequest(100, "USD", "test-key-1");

        // Act
        var result = await service.CreatePaymentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(100);
        result.Currency.Should().Be("USD");
        result.Status.Should().Be(PaymentStatus.Pending);

        var paymentInDb = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == result.Id);
        paymentInDb.Should().NotBeNull();
        paymentInDb!.IdempotencyKey.Should().Be("test-key-1");
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldReturnExistingPayment_WhenIdempotencyKeyAlreadyExists()
    {
        // Arrange
        using var dbContext = GetDbContext();
        var service = new PaymentService(dbContext, _loggerMock.Object);

        var existingDto = new PaymentRequest(100, "USD", "duplicate-key-123");
        
        
        await service.CreatePaymentAsync(existingDto);

        var duplicateDto = new PaymentRequest(200, "EUR", "duplicate-key-123");

        // Act
        var result = await service.CreatePaymentAsync(duplicateDto);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(100);
        result.Currency.Should().Be("USD");
    }
}