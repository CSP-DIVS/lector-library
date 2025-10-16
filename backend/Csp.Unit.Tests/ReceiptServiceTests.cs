using System;
using System.Threading.Tasks;
using Csp.Api.DTOs;
using Csp.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Csp.Unit.Tests
{
    public class ReceiptServiceTests
    {
        private static IReceiptService CreateService()
        {
            // Only GenerateReceiptPdfAsync is exercised in unit tests.
            // We provide a dummy configuration for constructor (connection string unused in this test).
            var inMemorySettings = new System.Collections.Generic.Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=dummy;Uid=dummy;Pwd=dummy;" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            return new ReceiptService(configuration);
        }

        [Fact]
        public async Task GenerateReceiptPdfAsync_ReturnsBytes_ForValidData()
        {
            // Arrange
            var service = CreateService();
            var dto = new PaymentReceiptDto
            {
                PaymentId = 123,
                MemberId = 3,
                MemberName = "John Doe",
                MemberEmail = "john@example.com",
                BookTitle = "The Pragmatic Programmer",
                BookAuthor = "Andrew Hunt / David Thomas",
                Amount = 12.50m,
                PaymentDate = DateTime.UtcNow.AddMinutes(-10),
                PaymentMethod = "Cash",
                RecordedByName = "Admin",
                TransactionId = "TX-UNIT-000123",
                LibraryName = "Lector Library",
                LibraryAddress = "SLIIT Campus, Malabe, Sri Lanka"
            };

            // Act
            var pdf = await service.GenerateReceiptPdfAsync(dto);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 500, $"Expected PDF length > 500, got {pdf.Length}");
            
            // Verify PDF file signature (starts with %PDF)
            var header = System.Text.Encoding.ASCII.GetString(pdf, 0, Math.Min(5, pdf.Length));
            Assert.StartsWith("%PDF", header);
            
            // Verify PDF footer (ends with %%EOF)
            var footer = System.Text.Encoding.ASCII.GetString(pdf, Math.Max(0, pdf.Length - 10), Math.Min(10, pdf.Length));
            Assert.Contains("%%EOF", footer);
        }
    }
}
