
using Moq;
using Csp.Api.Services;
using Csp.Api.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace Csp.Api.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConfig = new Mock<IConfiguration>();
            // Mock configuration settings if needed, for example:
            _mockConfig.Setup(c => c.GetSection("Jwt:Key").Value).Returns("TestKey123456789012345678901234567890");

            _userService = new UserService(_mockUserRepo.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task RegisterUserAsync_WhenEmailAlreadyExists_ShouldReturnFailureResult()
        {
            // Arrange
            var existingUser = new User { Id = 1, Email = "test@example.com" };
            var newUserDto = new RegisterDto { Email = "test@example.com", Password = "password" };
            _mockUserRepo.Setup(repo => repo.GetUserByEmailAsync(newUserDto.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _userService.RegisterUserAsync(newUserDto, "Member");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("This email address is already in use.", result.ErrorMessage);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenUserNotFound_ShouldReturnFailureResult()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto { OldPassword = "old", NewPassword = "new" };
            var userId = 99; // Non-existent user
            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCurrentPasswordIsIncorrect_ShouldReturnFailureResult()
        {
            // Arrange
            var userId = 1;
            var storedHash = UserService.HashPassword("correct-old-password");
            var user = new User { Id = userId, PasswordHash = storedHash };
            var changePasswordDto = new ChangePasswordDto { OldPassword = "incorrect-old-password", NewPassword = "new-password" };

            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Incorrect current password.", result.ErrorMessage);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_WhenUserNotFound_ShouldReturnFailure()
        {
            // Arrange
            var userId = 99; // Non-existent user
            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.ToggleUserStatusAsync(userId, 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User not found", result.ErrorMessage);
        }
    }
}
