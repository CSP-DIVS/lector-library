//unit tests for users controller, not for the service layer. doesnt prove duplicates are handled correctly.
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
    public class UsersControllerTests
    {
        private static UsersController CreateController(Mock<IUserService> userServiceMock, int actorUserId = 1)
        {
            var controller = new UsersController(userServiceMock.Object);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()),
                new Claim(ClaimTypes.Role, "Administrator")
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
        public async Task CreateMember_BadRequest_When_Missing_Fields()
        {
            var mock = new Mock<IUserService>(MockBehavior.Strict);
            var controller = CreateController(mock);

            var req = new CreateMemberRequest { Username = "", Email = "", Password = "" };
            var result = await controller.CreateMember(req);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            var payload = Assert.IsType<LoginResponse>(bad.Value);
            Assert.False(payload.Success);
        }

        [Fact]
        public async Task CreateMember_BadRequest_When_Duplicate()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.CreateMemberAsync(It.IsAny<CreateMemberRequest>(), It.IsAny<int>()))
                .ReturnsAsync(new LoginResponse { Success = false, Message = "Username or email already exists" });
            var controller = CreateController(mock);

            var req = new CreateMemberRequest { Username = "dup", Email = "dup@example.com", Password = "memberpass!1" };
            var result = await controller.CreateMember(req);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            var payload = Assert.IsType<LoginResponse>(bad.Value);
            Assert.False(payload.Success);
            Assert.Equal("Username or email already exists", payload.Message);
        }

        [Fact]
        public async Task CreateMember_Ok_When_Valid()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.CreateMemberAsync(It.IsAny<CreateMemberRequest>(), It.IsAny<int>()))
                .ReturnsAsync(new LoginResponse { Success = true, User = new UserDto { Id = 123, Username = "ok" } });
            var controller = CreateController(mock);

            var req = new CreateMemberRequest { Username = "ok", Email = "ok@example.com", Password = "memberpass!1" };
            var result = await controller.CreateMember(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<LoginResponse>(ok.Value);
            Assert.True(payload.Success);
            Assert.Equal(123, payload.User!.Id);
        }

        [Fact]
        public async Task UpdateMember_BadRequest_When_Missing_Fields()
        {
            var mock = new Mock<IUserService>(MockBehavior.Strict);
            var controller = CreateController(mock);

            var req = new UpdateMemberRequest { Username = "", Email = "" };
            var result = await controller.UpdateMember(5, req);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateMember_BadRequest_When_Duplicate()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateMemberAsync(5, It.IsAny<UpdateMemberRequest>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            var controller = CreateController(mock);

            var req = new UpdateMemberRequest { Username = "dup2", Email = "dup2@example.com" };
            var result = await controller.UpdateMember(5, req);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateMember_Ok_When_Valid()
        {
            var mock = new Mock<IUserService>();
            mock.Setup(s => s.UpdateMemberAsync(5, It.IsAny<UpdateMemberRequest>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            var controller = CreateController(mock);

            var req = new UpdateMemberRequest { Username = "ok2", Email = "ok2@example.com" };
            var result = await controller.UpdateMember(5, req);

            Assert.IsType<OkResult>(result);
        }
    }
}
