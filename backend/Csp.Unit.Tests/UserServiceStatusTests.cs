using System.Collections.Generic;
using Csp.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Csp.Unit.Tests
{
    public class UserServiceStatusTests
    {
        private static IUserService CreateService()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=invalid;Database=invalid;Uid=x;Pwd=y;"},
                {"Jwt:Secret", "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            var jwt = new JwtTokenService(configuration);
            return new UserService(configuration, jwt);
        }

        [Fact]
        public async Task UpdateUserStatus_ReturnsFalse_WhenActorTriesToChangeOwnStatus()
        {
            var service = CreateService();
            var actorId = 42;
            var resultDeactivate = await service.UpdateUserStatusAsync(actorId, false, actorId);
            var resultReactivate = await service.UpdateUserStatusAsync(actorId, true, actorId);

            Assert.False(resultDeactivate);
            Assert.False(resultReactivate);
        }
    }
}


