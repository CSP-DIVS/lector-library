using System.Security.Claims;
using Csp.Api.Controllers;
using Csp.Api.DTOs;
using Csp.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Csp.Unit.Tests
{
    public class FinesControllerTests
    {
        private static ClaimsPrincipal BuildAdminPrincipal(int userId = 123)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Administrator")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task AdjustFine_ReturnsOk_WhenServiceSucceeds()
        {
            // Arrange
            var lendingId = 42;
            var adminId = 123;
            var request = new AdjustFineRequest { NewAmount = 2.00m, Reason = "Reduced due to circumstances" };

            var serviceMock = new Mock<ILendingService>();
            serviceMock
                .Setup(s => s.AdjustFineAsync(lendingId, It.IsAny<AdjustFineRequest>(), adminId))
                .ReturnsAsync(new AdjustFineResponse
                {
                    Success = true,
                    Message = "Fine updated successfully",
                    LendingId = lendingId,
                    OriginalAmount = 10.00m,
                    NewAmount = 2.00m,
                    FinePaid = false
                });

            var controller = new FinesController(serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = BuildAdminPrincipal(adminId)
                    }
                }
            };

            // Act
            var result = await controller.AdjustFine(lendingId, request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<AdjustFineResponse>(ok.Value);
            Assert.True(body.Success);
            Assert.Equal("Fine updated successfully", body.Message);
            Assert.Equal(lendingId, body.LendingId);
            Assert.Equal(2.00m, body.NewAmount);
            serviceMock.Verify(s => s.AdjustFineAsync(lendingId, It.IsAny<AdjustFineRequest>(), adminId), Times.Once);
        }

        [Fact]
        public async Task AdjustFine_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var lendingId = 99;
            var adminId = 555;
            var request = new AdjustFineRequest { NewAmount = 0m, Reason = "Waiver" };

            var serviceMock = new Mock<ILendingService>();
            serviceMock
                .Setup(s => s.AdjustFineAsync(lendingId, It.IsAny<AdjustFineRequest>(), adminId))
                .ReturnsAsync(new AdjustFineResponse
                {
                    Success = false,
                    Message = "Lending record not found"
                });

            var controller = new FinesController(serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = BuildAdminPrincipal(adminId) }
                }
            };

            // Act
            var result = await controller.AdjustFine(lendingId, request);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            var body = Assert.IsType<AdjustFineResponse>(bad.Value);
            Assert.False(body.Success);
            Assert.Equal("Lending record not found", body.Message);
        }

        [Fact]
        public void AdjustFine_HasAuthorizePolicy_RequireAdmin()
        {
            // Arrange
            var method = typeof(FinesController).GetMethod("AdjustFine");
            Assert.NotNull(method);

            // Act
            var authorize = method!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                                    .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                                    .FirstOrDefault();

            // Assert
            Assert.NotNull(authorize);
            Assert.Equal("RequireAdmin", authorize!.Policy);
        }
    }
}
