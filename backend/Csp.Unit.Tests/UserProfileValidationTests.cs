using System.Security.Claims;
using Csp.Api.Controllers;
using Csp.Api.DTOs;
using Csp.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Csp.Unit.Tests
{
    public class UserProfileValidationTests
    {
        private static UsersController CreateController(Mock<IUserService> userServiceMock, int actorUserId = 10)
        {
            var controller = new UsersController(userServiceMock.Object);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()),
                new Claim(ClaimTypes.Role, "Member")
            }, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
            return controller;
        }

        private static IUserService CreateService()
        {
            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:DefaultConnection", "Server=invalid;Database=invalid;Uid=x;Pwd=y;"},
                    {"Jwt:Secret", "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"}
                })
                .Build();
            var jwt = new JwtTokenService(cfg);
            return new UserService(cfg, jwt);
        }

        [Fact]
        public async Task ChangePassword_ReturnsFalse_When_NewPassword_Shorter_Than_8()
        {
            // This short-circuits before any DB access
            var service = CreateService();
            var ok = await service.ChangePasswordAsync(1, new ChangePasswordRequest
            {
                CurrentPassword = "anything",
                NewPassword = "short" // 5 chars
            });
            Assert.False(ok);
        }

        [Fact]
        public async Task ChangeMyPassword_Controller_Returns_BadRequest_When_Service_False()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.ChangePasswordAsync(10, It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(false);
            var controller = CreateController(mock);

            var result = await controller.ChangeMyPassword(new ChangePasswordRequest { CurrentPassword = "x", NewPassword = "y" });
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ChangeMyPassword_Controller_Returns_Ok_When_Service_True()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.ChangePasswordAsync(10, It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(true);
            var controller = CreateController(mock);

            var result = await controller.ChangeMyPassword(new ChangePasswordRequest { CurrentPassword = "currentpass", NewPassword = "newpassword1" });
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateMyProfile_Controller_Returns_BadRequest_When_Service_False()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateMyProfileAsync(10, It.IsAny<UpdateProfileRequest>()))
                .ReturnsAsync(false);
            var controller = CreateController(mock);

            var result = await controller.UpdateMyProfile(new UpdateProfileRequest { Username = "x", Email = "not-an-email" });
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateMyProfile_Controller_Returns_Ok_When_Service_True()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateMyProfileAsync(10, It.IsAny<UpdateProfileRequest>()))
                .ReturnsAsync(true);
            var controller = CreateController(mock);

            var result = await controller.UpdateMyProfile(new UpdateProfileRequest { Username = "john", Email = "john@example.com" });
            Assert.IsType<OkResult>(result);
        }

        // The following tests assert desired validation that is NOT yet implemented.
        // They are expected to FAIL until backend adds email format and password complexity checks.

        [Fact]
        public async Task UpdateMyProfile_Controller_Should_Return_BadRequest_For_Invalid_Email_Format()
        {
            var mock = new Mock<IUserService>();
            // Simulate service would succeed, expecting controller validation to block
            mock.Setup(s => s.UpdateMyProfileAsync(10, It.IsAny<UpdateProfileRequest>()))
                .ReturnsAsync(true);
            var controller = CreateController(mock);

            var result = await controller.UpdateMyProfile(new UpdateProfileRequest { Username = "userx", Email = "invalid-email-format" });
            // EXPECTED: BadRequest once validation is implemented. Currently will be Ok → test fails.
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ChangeMyPassword_Controller_Should_Return_BadRequest_For_Weak_NewPassword_NoComplexity()
        {
            var mock = new Mock<IUserService>();
            // Service says true, expecting controller/service validation to reject
            mock.Setup(s => s.ChangePasswordAsync(10, It.IsAny<ChangePasswordRequest>()))
                .ReturnsAsync(true);
            var controller = CreateController(mock);

            // 8 lowercase letters: length ok, no digits/upper/special
            var result = await controller.ChangeMyPassword(new ChangePasswordRequest { CurrentPassword = "currentok1", NewPassword = "abcdefgh" });
            // EXPECTED: BadRequest once complexity rules exist. Currently Ok → test fails.
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
