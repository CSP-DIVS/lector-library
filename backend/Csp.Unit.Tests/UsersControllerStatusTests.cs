using System.Security.Claims;
using Csp.Api.Controllers;
using Csp.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Csp.Unit.Tests
{
    public class UsersControllerStatusTests
    {
        private static UsersController CreateController(Mock<IUserService> userServiceMock, int actorUserId = 7, string role = "Administrator")
        {
            var controller = new UsersController(userServiceMock.Object);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()),
                new Claim(ClaimTypes.Role, role)
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

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_When_ServiceReturnsFalse()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateUserStatusAsync(15, false, 7)).ReturnsAsync(false);

            var controller = CreateController(mock, actorUserId: 7);
            var result = await controller.UpdateStatus(15, false);

            Assert.IsType<BadRequestResult>(result);
            mock.Verify(s => s.UpdateUserStatusAsync(15, false, 7), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_When_ServiceReturnsTrue()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateUserStatusAsync(15, true, 7)).ReturnsAsync(true);

            var controller = CreateController(mock, actorUserId: 7);
            var result = await controller.UpdateStatus(15, true);

            Assert.IsType<OkResult>(result);
            mock.Verify(s => s.UpdateUserStatusAsync(15, true, 7), Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_Uses_ActorId_From_Claims()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateUserStatusAsync(99, true, 123)).ReturnsAsync(true);

            var controller = CreateController(mock, actorUserId: 123);
            var result = await controller.UpdateStatus(99, true);

            Assert.IsType<OkResult>(result);
            mock.Verify(s => s.UpdateUserStatusAsync(99, true, 123), Times.Once);
        }
    }
}
