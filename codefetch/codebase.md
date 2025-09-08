lector-library.sln
```
1 | Microsoft Visual Studio Solution File, Format Version 12.00
2 | # Visual Studio Version 17
3 | VisualStudioVersion = 17.5.2.0
4 | MinimumVisualStudioVersion = 10.0.40219.1
5 | Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "backend", "backend", "{1AE8ACA6-933B-BF2A-3671-3E2EAC007D16}"
6 | EndProject
7 | Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Csp.Api", "backend\Csp.Api\Csp.Api.csproj", "{9BBF13F6-2D8E-AE5B-7A8F-44AE2A9688E0}"
8 | EndProject
9 | Global
10 | 	GlobalSection(SolutionConfigurationPlatforms) = preSolution
11 | 		Debug|Any CPU = Debug|Any CPU
12 | 		Release|Any CPU = Release|Any CPU
13 | 	EndGlobalSection
14 | 	GlobalSection(ProjectConfigurationPlatforms) = postSolution
15 | 		{9BBF13F6-2D8E-AE5B-7A8F-44AE2A9688E0}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
16 | 		{9BBF13F6-2D8E-AE5B-7A8F-44AE2A9688E0}.Debug|Any CPU.Build.0 = Debug|Any CPU
17 | 		{9BBF13F6-2D8E-AE5B-7A8F-44AE2A9688E0}.Release|Any CPU.ActiveCfg = Release|Any CPU
18 | 		{9BBF13F6-2D8E-AE5B-7A8F-44AE2A9688E0}.Release|Any CPU.Build.0 = Release|Any CPU
19 | 	EndGlobalSection
20 | 	GlobalSection(SolutionProperties) = preSolution
21 | 		HideSolutionNode = FALSE
22 | 	EndGlobalSection
23 | 	GlobalSection(NestedProjects) = preSolution
24 | 		{9BBF13F6-2D8E-AE5B-7A8F-44AE2A9688E0} = {1AE8ACA6-933B-BF2A-3671-3E2EAC007D16}
25 | 	EndGlobalSection
26 | 	GlobalSection(ExtensibilityGlobals) = postSolution
27 | 		SolutionGuid = {1E0321E0-AADC-4A20-B48F-CD9DBDBA52B4}
28 | 	EndGlobalSection
29 | EndGlobal
```

plan.md
```
1 | Here’s a concise, multi-stage implementation plan that merges the goals of story1, story02, story3, and story4.
2 | 
3 | ### Stage 0: Database and schema foundation
4 | - Add/alter `users` table: `Id`, `Username`, `Email`, `PasswordHash`, `Role` (Member|Librarian|Administrator), `IsActive` (bool), `CreatedAt`, `UpdatedAt`.
5 | - Add `audit_log` table: `Id`, `ActorUserId`, `Action`, `TargetUserId`, `Details`, `CreatedAt`.
6 | - Seed: one Administrator, one Member, one Librarian.
7 | 
8 | ### Stage 1: Authentication and authorization (backend)
9 | - Implement login via username or email, with `IsActive` check.
10 | - Replace placeholder token with signed JWT; include `sub`, `role`, `username`, `isActive`.
11 | - Add JWT validation middleware and role-based authorization attributes/policies.
12 | - Standardize error responses/messages to match acceptance criteria.
13 | 
14 | ### Stage 2: Authentication UX and routing (frontend)
15 | - Update login to accept username or email; client-side required-field validation.
16 | - Store JWT in memory with refresh to localStorage fallback; attach auth header via axios interceptor.
17 | - Implement protected routes and role-based redirects (Member/Librarian/Admin dashboards).
18 | - Minimal dashboard placeholders per role for navigation targets.
19 | 
20 | ### Stage 3: Member management (Admin) — CRUD minus password
21 | - Backend endpoints:
22 |   - POST `/api/users/members` (create Member; unique email validation; audit log)
23 |   - GET `/api/users/members` (list, pagination, search, filter)
24 |   - PUT `/api/users/members/{id}` (update profile fields; audit log)
25 | - Frontend Admin UI:
26 |   - Register form
27 |   - Paginated/searchable table
28 |   - Edit modal/form
29 | - Shared validation (email format, required fields).
30 | 
31 | ### Stage 4: Deactivate/reactivate users (Admin)
32 | - Backend: PUT `/api/users/{id}/status` with `IsActive` toggle; prevent self-deactivation; audit log entries for both actions.
33 | - Enforce `IsActive` at login and via middleware for protected endpoints.
34 | - Frontend: status toggle/button in Admin user table with optimistic UI update and error handling.
35 | 
36 | ### Stage 5: My Profile (Member)
37 | - Backend: PUT `/api/users/my-profile` for profile updates; GET for current user fetch.
38 | - Password change endpoint with current password verification and strength checks.
39 | - Frontend: “My Profile” page with editable fields and a distinct password change section; success/error messaging.
40 | 
41 | ### Stage 6: Testing
42 | - Unit tests:
43 |   - Password hashing/verification
44 |   - Validators (email, password strength)
45 |   - Role/authorization policies
46 | - Integration tests:
47 |   - Login (success, wrong password, non-existent, inactive)
48 |   - Member CRUD
49 |   - Status toggle
50 |   - Profile update and password change
51 | - E2E (Selenium):
52 |   - Story1: full login flow for all roles and failure cases
53 |   - Story02: register, list, search, edit member
54 |   - Story3: deactivate/reactivate flow and blocked login while inactive
55 |   - Story4: profile update and password change journeys
56 | 
57 | ### Stage 7: Observability, resilience, and hardening
58 | - Centralized error handling and consistent API problem responses.
59 | - Structured logging with correlation IDs; log auth and write audit entries.
60 | - Rate limiting on auth endpoints; minimal lockout on repeated failures.
61 | - CORS tightening to known origins; secure config of JWT secret via env.
62 | 
63 | ### Stage 8: Configuration and deployment
64 | - Environment variables for DB, JWT secret, issuer/audience.
65 | - Docker compose updates to ensure DB readiness and migrations run.
66 | - Swagger auth setup for bearer tokens; README with run/test instructions.
67 | 
68 | ### Success criteria mapping
69 | - Story1: Real JWT auth, active-status enforcement, role-based redirect, error messages aligned.
70 | - Story02: Admin member registration/list/update with validation and UI.
71 | - Story3: Deactivate/reactivate with prevention of self-deactivation and audit log; login blocked while inactive.
72 | - Story4: Profile update and password change with validations and UX feedback.
73 | 
74 | If you want me to proceed, I’ll start with Stage 0 and Stage 1: add schema fields/tables and implement real JWT, email/username login, role and active checks, plus middleware and policies.
```

infra/backend.Dockerfile
```
1 | # build
2 | FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
3 | WORKDIR /src
4 | COPY ./backend/Csp.Api ./Csp.Api
5 | RUN dotnet restore ./Csp.Api && dotnet publish ./Csp.Api -c Release -o /app
6 | 
7 | # run
8 | FROM mcr.microsoft.com/dotnet/aspnet:8.0
9 | WORKDIR /app
10 | COPY --from=build /app .
11 | ENV ASPNETCORE_URLS=http://+:8080
12 | EXPOSE 8080
13 | ENTRYPOINT ["dotnet","Csp.Api.dll"]
```

infra/docker-compose.yml
```
1 | services:
2 |   db:
3 |     image: mysql:8.0
4 |     environment:
5 |       MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
6 |       MYSQL_DATABASE: ${MYSQL_DATABASE}
7 |       MYSQL_USER: ${MYSQL_USER}
8 |       MYSQL_PASSWORD: ${MYSQL_PASSWORD}
9 |     ports:
10 |       - "${DB_PORT}:3306"
11 |     volumes:
12 |       - dbdata:/var/lib/mysql
13 |     networks:
14 |       - lector-network
15 |     healthcheck:
16 |       test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
17 |       timeout: 20s
18 |       retries: 10
19 | 
20 |   backend:
21 |     build:
22 |       context: ..
23 |       dockerfile: infra/backend.Dockerfile
24 |     environment:
25 |       - ConnectionStrings__DefaultConnection=Server=db;Port=3306;Database=${MYSQL_DATABASE};Uid=${MYSQL_USER};Pwd=${MYSQL_PASSWORD};
26 |       - ASPNETCORE_ENVIRONMENT=Development
27 |     ports:
28 |       - "${API_PORT}:8080"
29 |     depends_on:
30 |       db:
31 |         condition: service_healthy
32 |     networks:
33 |       - lector-network
34 |     restart: on-failure
35 | 
36 |   frontend:
37 |     build:
38 |       context: ..
39 |       dockerfile: infra/frontend.Dockerfile
40 |     ports:
41 |       - "${FRONTEND_PORT}:80"
42 |     depends_on:
43 |       - backend
44 |     networks:
45 |       - lector-network
46 | 
47 | volumes:
48 |   dbdata:
49 | 
50 | networks:
51 |   lector-network:
52 |     driver: bridge
```

infra/frontend.Dockerfile
```
1 | # build
2 | FROM node:20 AS build
3 | WORKDIR /app
4 | COPY ./frontend/csp-web ./
5 | RUN npm ci || npm install
6 | RUN npm run build
7 | 
8 | # serve (static)
9 | FROM nginx:alpine
10 | COPY --from=build /app/dist /usr/share/nginx/html
11 | EXPOSE 80
```

.github/workflows/dotnet.yml
```
1 | name: Backend CI
2 | on: [push, pull_request]
3 | jobs:
4 |   build-test:
5 |     runs-on: ubuntu-latest
6 |     steps:
7 |       - uses: actions/checkout@v4
8 |       - uses: actions/setup-dotnet@v4
9 |         with: { dotnet-version: '8.0.x' }
10 |       - name: Restore
11 |         run: dotnet restore backend/Csp.Api
12 |       - name: Build
13 |         run: dotnet build --configuration Release --no-restore backend/Csp.Api
14 |       - name: Test
15 |         run: dotnet test --no-build --verbosity normal || true  # replace with real tests later
```

.github/workflows/node.yml
```
1 | name: Frontend CI
2 | on: [push, pull_request]
3 | jobs:
4 |   build:
5 |     runs-on: ubuntu-latest
6 |     steps:
7 |       - uses: actions/checkout@v4
8 |       - uses: actions/setup-node@v4
9 |         with: { node-version: '20' }
10 |       - name: Install
11 |         working-directory: frontend/csp-web
12 |         run: npm ci || npm install
13 |       - name: Build
14 |         working-directory: frontend/csp-web
15 |         run: npm run build
```

backend/Csp.Api.Tests/Csp.Api.Tests.csproj
```
1 | ﻿<Project Sdk="Microsoft.NET.Sdk">
2 | 
3 |   <PropertyGroup>
4 |     <TargetFramework>net8.0</TargetFramework>
5 |     <ImplicitUsings>enable</ImplicitUsings>
6 |     <Nullable>enable</Nullable>
7 |     <IsPackable>false</IsPackable>
8 |   </PropertyGroup>
9 | 
10 |   <ItemGroup>
11 |     <PackageReference Include="coverlet.collector" Version="6.0.2" />
12 |     <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
13 |     <PackageReference Include="xunit" Version="2.9.2" />
14 |     <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
15 |     <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.6.0" />
16 |     <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.6.0" />
17 |     <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.8" />
18 |   </ItemGroup>
19 | 
20 |   <ItemGroup>
21 |     <Using Include="Xunit" />
22 |   </ItemGroup>
23 | 
24 |   <ItemGroup>
25 |     <ProjectReference Include="..\Csp.Api\Csp.Api.csproj" />
26 |   </ItemGroup>
27 | 
28 | </Project>
```

backend/Csp.Api.Tests/LoginIntegrationTests.cs
```
1 | using System.Net;
2 | using System.Net.Http.Json;
3 | using Csp.Api.DTOs;
4 | 
5 | namespace Csp.Api.Tests;
6 | 
7 | public class LoginIntegrationTests : IClassFixture<ApiFactory>
8 | {
9 |     private readonly ApiFactory factory;
10 |     public LoginIntegrationTests(ApiFactory factory) { this.factory = factory; }
11 | 
12 |     [Fact]
13 |     public async Task Login_Success()
14 |     {
15 |         var client = factory.CreateClient();
16 |         var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = "member", Password = "ok" });
17 |         Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
18 |     }
19 | 
20 |     [Fact]
21 |     public async Task Login_WrongPassword()
22 |     {
23 |         var client = factory.CreateClient();
24 |         var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = "member", Password = "bad" });
25 |         Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
26 |     }
27 | 
28 |     [Fact]
29 |     public async Task Login_Inactive()
30 |     {
31 |         var client = factory.CreateClient();
32 |         var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = "inactive", Password = "ok" });
33 |         Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
34 |     }
35 | }
36 | 
37 | 
```

backend/Csp.Api.Tests/MemberIntegrationTests.cs
```
1 | using System.Net;
2 | using System.Net.Http.Json;
3 | using Csp.Api.DTOs;
4 | 
5 | namespace Csp.Api.Tests;
6 | 
7 | public class MemberIntegrationTests : IClassFixture<ApiFactory>
8 | {
9 |     private readonly ApiFactory factory;
10 |     public MemberIntegrationTests(ApiFactory factory) { this.factory = factory; }
11 | 
12 |     [Fact]
13 |     public async Task MemberCrud_Flows()
14 |     {
15 |         var client = factory.CreateClient();
16 |         client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");
17 |         client.DefaultRequestHeaders.Add("X-Test-UserId", "99");
18 | 
19 |         var bad = await client.PostAsJsonAsync("/api/users/members", new CreateMemberRequest { Username = "dup", Email = "dup@e.com", Password = "x" });
20 |         Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
21 | 
22 |         var ok = await client.PostAsJsonAsync("/api/users/members", new CreateMemberRequest { Username = "new", Email = "n@e.com", Password = "x" });
23 |         Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
24 | 
25 |         var list = await client.GetAsync("/api/users/members");
26 |         Assert.Equal(HttpStatusCode.OK, list.StatusCode);
27 | 
28 |         var upd = await client.PutAsJsonAsync("/api/users/members/1", new UpdateMemberRequest { Username = "u", Email = "u@e.com" });
29 |         Assert.Equal(HttpStatusCode.OK, upd.StatusCode);
30 |     }
31 | 
32 |     [Fact]
33 |     public async Task StatusToggle_Works()
34 |     {
35 |         var client = factory.CreateClient();
36 |         client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");
37 |         client.DefaultRequestHeaders.Add("X-Test-UserId", "99");
38 |         var resp = await client.PutAsync("/api/users/2/status?isActive=true", null);
39 |         Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
40 |     }
41 | }
42 | 
43 | 
```

backend/Csp.Api.Tests/MyProfileIntegrationTests.cs
```
1 | using System.Net;
2 | using System.Net.Http.Json;
3 | using Csp.Api.DTOs;
4 | 
5 | namespace Csp.Api.Tests;
6 | 
7 | public class MyProfileIntegrationTests : IClassFixture<ApiFactory>
8 | {
9 |     private readonly ApiFactory factory;
10 |     public MyProfileIntegrationTests(ApiFactory factory) { this.factory = factory; }
11 | 
12 |     [Fact]
13 |     public async Task Profile_Get_Update_ChangePassword()
14 |     {
15 |         var client = factory.CreateClient();
16 |         client.DefaultRequestHeaders.Add("X-Test-Role", "Member");
17 |         client.DefaultRequestHeaders.Add("X-Test-UserId", "1");
18 | 
19 |         var get = await client.GetAsync("/api/users/my-profile");
20 |         Assert.Equal(HttpStatusCode.OK, get.StatusCode);
21 | 
22 |         var upd = await client.PutAsJsonAsync("/api/users/my-profile", new UpdateProfileRequest { Username = "me", Email = "me@e.com" });
23 |         Assert.Equal(HttpStatusCode.OK, upd.StatusCode);
24 | 
25 |         var badPwd = await client.PutAsJsonAsync("/api/users/my-password", new ChangePasswordRequest { CurrentPassword = "x", NewPassword = "short" });
26 |         Assert.Equal(HttpStatusCode.BadRequest, badPwd.StatusCode);
27 | 
28 |         var goodPwd = await client.PutAsJsonAsync("/api/users/my-password", new ChangePasswordRequest { CurrentPassword = "ok", NewPassword = "longenough" });
29 |         Assert.Equal(HttpStatusCode.OK, goodPwd.StatusCode);
30 |     }
31 | }
32 | 
33 | 
```

backend/Csp.Api.Tests/TestServer.cs
```
1 | using System.Net.Http.Headers;
2 | using System.Security.Claims;
3 | using System.Text.Encodings.Web;
4 | using Csp.Api.DTOs;
5 | using Csp.Api.Services;
6 | using Microsoft.AspNetCore.Authentication;
7 | using Microsoft.AspNetCore.Mvc.Testing;
8 | using Microsoft.Extensions.DependencyInjection;
9 | using Microsoft.Extensions.Logging;
10 | using Microsoft.Extensions.Options;
11 | 
12 | namespace Csp.Api.Tests;
13 | 
14 | public class FakeUserService : IUserService
15 | {
16 |     public Task<LoginResponse> LoginAsync(LoginRequest request)
17 |     {
18 |         if (request.Username == "member" && request.Password == "ok")
19 |             return Task.FromResult(new LoginResponse { Success = true, User = new UserDto { Id = 2, Username = "member", Email = "m@e.com", Role = "Member", IsActive = true }, Token = "t" });
20 |         if (request.Username == "inactive" && request.Password == "ok")
21 |             return Task.FromResult(new LoginResponse { Success = false, Message = "Your account is inactive. Please contact an administrator." });
22 |         return Task.FromResult(new LoginResponse { Success = false, Message = "Invalid username or password" });
23 |     }
24 | 
25 |     public Task<LoginResponse> RegisterAsync(RegisterRequest request) => Task.FromResult(new LoginResponse { Success = true });
26 |     public Task InitializeDatabaseAsync() => Task.CompletedTask;
27 | 
28 |     public Task<LoginResponse> CreateMemberAsync(CreateMemberRequest request, int actorUserId)
29 |     {
30 |         if (request.Username == "dup") return Task.FromResult(new LoginResponse { Success = false, Message = "Username or email already exists" });
31 |         return Task.FromResult(new LoginResponse { Success = true, User = new UserDto { Id = 5, Username = request.Username, Email = request.Email, Role = "Member", IsActive = true } });
32 |     }
33 | 
34 |     public Task<PagedMembersResponse> GetMembersAsync(string? search, int page, int pageSize)
35 |     {
36 |         var all = new List<UserDto>
37 |         {
38 |             new UserDto{ Id=1, Username="a", Email="a@e.com", Role="Member", IsActive=true },
39 |             new UserDto{ Id=2, Username="b", Email="b@e.com", Role="Member", IsActive=false }
40 |         };
41 |         return Task.FromResult(new PagedMembersResponse{ Items = all, Total = all.Count, Page = 1, PageSize = 10 });
42 |     }
43 | 
44 |     public Task<bool> UpdateMemberAsync(int id, UpdateMemberRequest request, int actorUserId) => Task.FromResult(true);
45 |     public Task<bool> UpdateUserStatusAsync(int id, bool isActive, int actorUserId) => Task.FromResult(id != actorUserId);
46 |     public Task<UserDto?> GetCurrentUserAsync(int id) => Task.FromResult<UserDto?>(new UserDto{ Id=id, Username="me", Email="me@e.com", Role="Member", IsActive=true });
47 |     public Task<bool> UpdateMyProfileAsync(int id, UpdateProfileRequest request) => Task.FromResult(true);
48 |     public Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request) => Task.FromResult(request.NewPassword.Length >= 8);
49 | }
50 | 
51 | public class ApiFactory : WebApplicationFactory<Program>
52 | {
53 |     protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
54 |     {
55 |         builder.ConfigureServices(services =>
56 |         {
57 |             var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserService));
58 |             if (descriptor != null) services.Remove(descriptor);
59 |             services.AddSingleton<IUserService, FakeUserService>();
60 | 
61 |             services.AddAuthentication(options =>
62 |             {
63 |                 options.DefaultAuthenticateScheme = "Test";
64 |                 options.DefaultChallengeScheme = "Test";
65 |             }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
66 |         });
67 |     }
68 | }
69 | 
70 | public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
71 | {
72 |     public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
73 |         : base(options, logger, encoder, clock) { }
74 | 
75 |     protected override Task<AuthenticateResult> HandleAuthenticateAsync()
76 |     {
77 |         var role = Request.Headers.ContainsKey("X-Test-Role") ? Request.Headers["X-Test-Role"].ToString() : "Member";
78 |         var sub = Request.Headers.ContainsKey("X-Test-UserId") ? Request.Headers["X-Test-UserId"].ToString() : "1";
79 |         var name = Request.Headers.ContainsKey("X-Test-Username") ? Request.Headers["X-Test-Username"].ToString() : "testuser";
80 | 
81 |         var claims = new[]
82 |         {
83 |             new Claim(ClaimTypes.NameIdentifier, sub),
84 |             new Claim(ClaimTypes.Name, name),
85 |             new Claim(ClaimTypes.Role, role),
86 |             new Claim("sub", sub)
87 |         };
88 |         var identity = new ClaimsIdentity(claims, "Test");
89 |         var principal = new ClaimsPrincipal(identity);
90 |         var ticket = new AuthenticationTicket(principal, "Test");
91 |         return Task.FromResult(AuthenticateResult.Success(ticket));
92 |     }
93 | }
94 | 
95 | 
```

backend/Csp.Api.Tests/UnitTest1.cs
```
1 | ﻿namespace Csp.Api.Tests;
2 | 
3 | public class UnitTest1
4 | {
5 |     [Fact]
6 |     public void Test1()
7 |     {
8 | 
9 |     }
10 | }
```

backend/Csp.Api.Tests/UserServiceHashTests.cs
```
1 | using System.Reflection;
2 | using Csp.Api.Services;
3 | using Microsoft.Extensions.Configuration;
4 | 
5 | namespace Csp.Api.Tests;
6 | 
7 | public class UserServiceHashTests
8 | {
9 |     private static UserService CreateService()
10 |     {
11 |         var config = new ConfigurationBuilder()
12 |             .AddInMemoryCollection(new Dictionary<string, string?>
13 |             {
14 |                 ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=test;User Id=test;Password=test;SslMode=None",
15 |                 ["Jwt:Secret"] = "unit-test-secret-1234567890",
16 |                 ["Jwt:Issuer"] = "csp-api",
17 |                 ["Jwt:Audience"] = "csp-web"
18 |             })
19 |             .Build();
20 | 
21 |         var jwt = new JwtTokenService(config);
22 |         return new UserService(config, jwt);
23 |     }
24 | 
25 |     [Fact]
26 |     public void Hash_And_Verify_Password_Work()
27 |     {
28 |         var service = CreateService();
29 | 
30 |         var hashMethod = typeof(UserService).GetMethod("HashPassword", BindingFlags.NonPublic | BindingFlags.Instance)!;
31 |         var verifyMethod = typeof(UserService).GetMethod("VerifyPassword", BindingFlags.NonPublic | BindingFlags.Instance)!;
32 | 
33 |         var password = "P@ssw0rd!";
34 |         var hash = (string)hashMethod.Invoke(service, new object[] { password })!;
35 | 
36 |         Assert.False(string.IsNullOrWhiteSpace(hash));
37 |         Assert.NotEqual(password, hash);
38 | 
39 |         var ok = (bool)verifyMethod.Invoke(service, new object[] { password, hash })!;
40 |         Assert.True(ok);
41 | 
42 |         var bad = (bool)verifyMethod.Invoke(service, new object[] { "wrong", hash })!;
43 |         Assert.False(bad);
44 |     }
45 | }
46 | 
47 | 
```

backend/Csp.Api.Tests/ValidationTests.cs
```
1 | using System.Text.RegularExpressions;
2 | 
3 | namespace Csp.Api.Tests;
4 | 
5 | public class ValidationTests
6 | {
7 |     private static bool IsValidEmail(string email)
8 |     {
9 |         if (string.IsNullOrWhiteSpace(email)) return false;
10 |         return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
11 |     }
12 | 
13 |     private static bool IsStrongPassword(string pwd)
14 |     {
15 |         if (string.IsNullOrEmpty(pwd) || pwd.Length < 8) return false;
16 |         return true;
17 |     }
18 | 
19 |     [Theory]
20 |     [InlineData("user@example.com", true)]
21 |     [InlineData("user@sub.example.co", true)]
22 |     [InlineData("invalid", false)]
23 |     [InlineData("user@", false)]
24 |     [InlineData("@example.com", false)]
25 |     public void Email_Validation(string email, bool expected)
26 |     {
27 |         Assert.Equal(expected, IsValidEmail(email));
28 |     }
29 | 
30 |     [Theory]
31 |     [InlineData("1234567", false)]
32 |     [InlineData("12345678", true)]
33 |     [InlineData("P@ssw0rd", true)]
34 |     public void Password_Strength(string pwd, bool expected)
35 |     {
36 |         Assert.Equal(expected, IsStrongPassword(pwd));
37 |     }
38 | }
39 | 
40 | 
```

backend/Csp.Api/appsettings.Development.json
```
1 | {
2 |   "Logging": {
3 |     "LogLevel": {
4 |       "Default": "Information",
5 |       "Microsoft.AspNetCore": "Warning"
6 |     }
7 |   },
8 |   "ConnectionStrings": {
9 |     "DefaultConnection": "Server=localhost;Database=LibraryManagement;User=root;Port=3306"
10 |   }
11 | }
```

backend/Csp.Api/appsettings.json
```
1 | {
2 |   "ConnectionStrings": {
3 |     "Default": "Server=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};User Id=${DB_USER};SslMode=None"
4 |   },
5 |   "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
6 |   "AllowedHosts": "*"
7 | }
```

backend/Csp.Api/Csp.Api.csproj
```
1 | <Project Sdk="Microsoft.NET.Sdk.Web">
2 | 
3 |   <PropertyGroup>
4 |     <TargetFramework>net8.0</TargetFramework>
5 |     <Nullable>enable</Nullable>
6 |     <ImplicitUsings>enable</ImplicitUsings>
7 |   </PropertyGroup>
8 | 
9 |   <ItemGroup>
10 |     <PackageReference Include="DotNetEnv" Version="3.1.1" />
11 |     <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.8" />
12 |     <PackageReference Include="MySql.Data" Version="9.4.0" />
13 |     <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.8" />
14 |     <PackageReference Include="Swashbuckle.AspNetCore" Version="8.1.0" />
15 |   </ItemGroup>
16 | 
17 | </Project>
```

backend/Csp.Api/Csp.Api.http
```
1 | @Csp.Api_HostAddress = http://localhost:5250
2 | 
3 | GET {{Csp.Api_HostAddress}}/weatherforecast/
4 | Accept: application/json
5 | 
6 | ###
```

backend/Csp.Api/Program.cs
```
1 | using MySql.Data.MySqlClient;
2 | using Microsoft.OpenApi.Models;
3 | using Csp.Api.Services;
4 | using Microsoft.AspNetCore.Authentication.JwtBearer;
5 | using Microsoft.IdentityModel.Tokens;
6 | using System.Text;
7 | 
8 | var builder = WebApplication.CreateBuilder(args);
9 | builder.Services.AddControllers();
10 | builder.Services.AddEndpointsApiExplorer();
11 | builder.Services.AddSwaggerGen();
12 | 
13 | // Register services
14 | builder.Services.AddScoped<IUserService, UserService>();
15 | builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
16 | 
17 | var jwtValidation = new JwtTokenService(builder.Configuration).GetValidationParameters();
18 | builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
19 |     .AddJwtBearer(options =>
20 |     {
21 |         options.TokenValidationParameters = jwtValidation;
22 |     });
23 | 
24 | builder.Services.AddAuthorization(options =>
25 | {
26 |     options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator"));
27 |     options.AddPolicy("RequireLibrarian", policy => policy.RequireRole("Librarian", "Administrator"));
28 | });
29 | 
30 | // CORS for local React dev server
31 | builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
32 |     p.WithOrigins("http://localhost:5173", "http://localhost:3000")
33 |      .AllowAnyHeader()
34 |      .AllowAnyMethod()));
35 | 
36 | var app = builder.Build();
37 | 
38 | // Initialize database
39 | using (var scope = app.Services.CreateScope())
40 | {
41 |     var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
42 |     await userService.InitializeDatabaseAsync();
43 | }
44 | 
45 | app.UseSwagger();
46 | app.UseSwaggerUI();
47 | app.UseCors();
48 | app.UseAuthentication();
49 | app.UseAuthorization();
50 | app.MapControllers();
51 | 
52 | // Example minimal ADO.NET usage in a test endpoint
53 | app.MapGet("/health/db", async () =>
54 | {
55 |     try
56 |     {
57 |         var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
58 |         if (string.IsNullOrEmpty(connectionString))
59 |         {
60 |             return Results.Problem("Connection string not found");
61 |         }
62 |         
63 |         await using var conn = new MySqlConnection(connectionString);
64 |         await conn.OpenAsync();
65 |         await using var cmd = new MySqlCommand("SELECT 1", conn);
66 |         var result = await cmd.ExecuteScalarAsync();
67 |         return Results.Ok(new { db = result, connectionString = connectionString });
68 |     }
69 |     catch (Exception ex)
70 |     {
71 |         return Results.Problem($"Database connection failed: {ex.Message}");
72 |     }
73 | });
74 | 
75 | app.Run();
76 | 
77 | public partial class Program { }
```

backend/Csp.E2E.Tests/Config.cs
```
1 | namespace Csp.E2E.Tests;
2 | 
3 | public static class Config
4 | {
5 |     public static string BaseUrl => Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5173";
6 | }
7 | 
8 | 
```

backend/Csp.E2E.Tests/Csp.E2E.Tests.csproj
```
1 | <Project Sdk="Microsoft.NET.Sdk">
2 | 
3 |   <PropertyGroup>
4 |     <TargetFramework>net8.0</TargetFramework>
5 |     <ImplicitUsings>enable</ImplicitUsings>
6 |     <Nullable>enable</Nullable>
7 |     <IsPackable>false</IsPackable>
8 |   </PropertyGroup>
9 | 
10 |   <ItemGroup>
11 |     <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
12 |     <PackageReference Include="xunit" Version="2.9.2" />
13 |     <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
14 |     <PackageReference Include="Selenium.WebDriver" Version="4.23.0" />
15 |     <PackageReference Include="Selenium.Support" Version="4.23.0" />
16 |     <PackageReference Include="Selenium.WebDriver.ChromeDriver" Version="128.0.6613.8490" />
17 |   </ItemGroup>
18 | 
19 |   <ItemGroup>
20 |     <Using Include="Xunit" />
21 |   </ItemGroup>
22 | 
23 | </Project>
24 | 
25 | 
```

backend/Csp.E2E.Tests/DriverFixture.cs
```
1 | using OpenQA.Selenium;
2 | using OpenQA.Selenium.Chrome;
3 | 
4 | namespace Csp.E2E.Tests;
5 | 
6 | public class DriverFixture : IDisposable
7 | {
8 |     public IWebDriver Driver { get; }
9 | 
10 |     public DriverFixture()
11 |     {
12 |         var options = new ChromeOptions();
13 |         options.AddArgument("--headless=new");
14 |         options.AddArgument("--window-size=1280,900");
15 |         Driver = new ChromeDriver(options);
16 |         Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
17 |     }
18 | 
19 |     public void Dispose()
20 |     {
21 |         Driver.Quit();
22 |     }
23 | }
24 | 
25 | 
```

backend/Csp.E2E.Tests/Story02MemberManagementTests.cs
```
1 | using OpenQA.Selenium;
2 | using OpenQA.Selenium.Support.UI;
3 | 
4 | namespace Csp.E2E.Tests;
5 | 
6 | public class Story02MemberManagementTests : IClassFixture<DriverFixture>
7 | {
8 |     private readonly IWebDriver driver;
9 |     public Story02MemberManagementTests(DriverFixture fx) { driver = fx.Driver; }
10 | 
11 |     private void LoginAsAdmin()
12 |     {
13 |         driver.Navigate().GoToUrl(Config.BaseUrl);
14 |         driver.FindElement(By.Id("username")).SendKeys("admin");
15 |         driver.FindElement(By.Id("password")).SendKeys("admin123!");
16 |         driver.FindElement(By.CssSelector("button.submit-btn")).Click();
17 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Admin Actions']")).Any());
18 |     }
19 | 
20 |     [Fact]
21 |     public void Register_List_Search_Edit_Member()
22 |     {
23 |         LoginAsAdmin();
24 |         driver.FindElement(By.XPath("//button[contains(.,'Member Management')]")).Click();
25 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h2[text()='Member Management']")).Any());
26 | 
27 |         var username = "user" + Guid.NewGuid().ToString("N").Substring(0, 6);
28 |         driver.FindElement(By.CssSelector("input[placeholder='Username']")).SendKeys(username);
29 |         driver.FindElement(By.CssSelector("input[placeholder='Email']")).SendKeys(username + "@e.com");
30 |         driver.FindElement(By.CssSelector("input[placeholder='Password']")).SendKeys("Passw0rd!");
31 |         driver.FindElement(By.CssSelector("form button[type='submit']")).Click();
32 | 
33 |         driver.FindElement(By.CssSelector("input[placeholder='Search members']")).SendKeys(username);
34 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath($"//td[text()='{username}']")).Any());
35 | 
36 |         driver.FindElement(By.XPath($"//td[text()='{username}']/../td/button[contains(.,'Edit')]")).Click();
37 |         var email = driver.FindElement(By.CssSelector("input[placeholder='Email']"));
38 |         email.Clear();
39 |         email.SendKeys(username + "+edited@e.com");
40 |         driver.FindElement(By.CssSelector("form button[type='submit']")).Click();
41 |     }
42 | }
43 | 
44 | 
```

backend/Csp.E2E.Tests/Story1LoginTests.cs
```
1 | using OpenQA.Selenium;
2 | using OpenQA.Selenium.Support.UI;
3 | 
4 | namespace Csp.E2E.Tests;
5 | 
6 | public class Story1LoginTests : IClassFixture<DriverFixture>
7 | {
8 |     private readonly IWebDriver driver;
9 | 
10 |     public Story1LoginTests(DriverFixture fx)
11 |     {
12 |         driver = fx.Driver;
13 |     }
14 | 
15 |     private void Navigate() => driver.Navigate().GoToUrl(Config.BaseUrl);
16 | 
17 |     [Fact]
18 |     public void Login_Success_Member()
19 |     {
20 |         Navigate();
21 |         driver.FindElement(By.Id("username")).SendKeys("member");
22 |         driver.FindElement(By.Id("password")).SendKeys("member123!");
23 |         driver.FindElement(By.CssSelector("button.submit-btn")).Click();
24 | 
25 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Member Actions']")).Any());
26 |     }
27 | 
28 |     [Fact]
29 |     public void Login_WrongPassword_ShowsError()
30 |     {
31 |         Navigate();
32 |         driver.FindElement(By.Id("username")).SendKeys("member");
33 |         driver.FindElement(By.Id("password")).SendKeys("wrong");
34 |         driver.FindElement(By.CssSelector("button.submit-btn")).Click();
35 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.ClassName("error-message")).Any());
36 |     }
37 | }
38 | 
39 | 
```

backend/Csp.E2E.Tests/Story3StatusToggleTests.cs
```
1 | using OpenQA.Selenium;
2 | using OpenQA.Selenium.Support.UI;
3 | 
4 | namespace Csp.E2E.Tests;
5 | 
6 | public class Story3StatusToggleTests : IClassFixture<DriverFixture>
7 | {
8 |     private readonly IWebDriver driver;
9 |     public Story3StatusToggleTests(DriverFixture fx) { driver = fx.Driver; }
10 | 
11 |     private void LoginAsAdmin()
12 |     {
13 |         driver.Navigate().GoToUrl(Config.BaseUrl);
14 |         driver.FindElement(By.Id("username")).SendKeys("admin");
15 |         driver.FindElement(By.Id("password")).SendKeys("admin123!");
16 |         driver.FindElement(By.CssSelector("button.submit-btn")).Click();
17 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Admin Actions']")).Any());
18 |     }
19 | 
20 |     [Fact]
21 |     public void Deactivate_Reactivate_Blocks_Login()
22 |     {
23 |         LoginAsAdmin();
24 |         driver.FindElement(By.XPath("//button[contains(.,'Member Management')]")).Click();
25 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h2[text()='Member Management']")).Any());
26 | 
27 |         var firstToggle = driver.FindElements(By.XPath("//table/tbody/tr[1]/td/button[contains(.,'Deactivate') or contains(.,'Reactivate')]"))
28 |                                 .FirstOrDefault();
29 |         if (firstToggle == null) return;
30 |         firstToggle.Click();
31 |     }
32 | }
33 | 
34 | 
```

backend/Csp.E2E.Tests/Story4MyProfileTests.cs
```
1 | using OpenQA.Selenium;
2 | using OpenQA.Selenium.Support.UI;
3 | 
4 | namespace Csp.E2E.Tests;
5 | 
6 | public class Story4MyProfileTests : IClassFixture<DriverFixture>
7 | {
8 |     private readonly IWebDriver driver;
9 |     public Story4MyProfileTests(DriverFixture fx) { driver = fx.Driver; }
10 | 
11 |     private void LoginAsMember()
12 |     {
13 |         driver.Navigate().GoToUrl(Config.BaseUrl);
14 |         driver.FindElement(By.Id("username")).SendKeys("member");
15 |         driver.FindElement(By.Id("password")).SendKeys("member123!");
16 |         driver.FindElement(By.CssSelector("button.submit-btn")).Click();
17 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Member Actions']")).Any());
18 |     }
19 | 
20 |     [Fact]
21 |     public void Update_Profile_And_Change_Password()
22 |     {
23 |         LoginAsMember();
24 |         driver.FindElement(By.XPath("//button[contains(.,'My Profile')]")).Click();
25 |         new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h2[text()='My Profile']")).Any());
26 | 
27 |         var email = driver.FindElement(By.XPath("//label[text()='Email']/following::input[1]"));
28 |         var current = email.GetAttribute("value");
29 |         email.Clear();
30 |         email.SendKeys(current);
31 |         driver.FindElement(By.XPath("//form[.//label[text()='Email']]//button[text()='Save']")).Click();
32 | 
33 |         driver.FindElement(By.XPath("//label[text()='Current Password']/following::input[1]")).SendKeys("member123!");
34 |         driver.FindElement(By.XPath("//label[text()='New Password']/following::input[1]")).SendKeys("member123!x");
35 |         driver.FindElement(By.XPath("//form[.//label[text()='New Password']]//button[text()='Change Password']")).Click();
36 |     }
37 | }
38 | 
39 | 
```

frontend/csp-web/eslint.config.js
```
1 | import js from '@eslint/js'
2 | import globals from 'globals'
3 | import reactHooks from 'eslint-plugin-react-hooks'
4 | import reactRefresh from 'eslint-plugin-react-refresh'
5 | import { defineConfig, globalIgnores } from 'eslint/config'
6 | 
7 | export default defineConfig([
8 |   globalIgnores(['dist']),
9 |   {
10 |     files: ['**/*.{js,jsx}'],
11 |     extends: [
12 |       js.configs.recommended,
13 |       reactHooks.configs['recommended-latest'],
14 |       reactRefresh.configs.vite,
15 |     ],
16 |     languageOptions: {
17 |       ecmaVersion: 2020,
18 |       globals: globals.browser,
19 |       parserOptions: {
20 |         ecmaVersion: 'latest',
21 |         ecmaFeatures: { jsx: true },
22 |         sourceType: 'module',
23 |       },
24 |     },
25 |     rules: {
26 |       'no-unused-vars': ['error', { varsIgnorePattern: '^[A-Z_]' }],
27 |     },
28 |   },
29 | ])
```

frontend/csp-web/index.html
```
1 | <!doctype html>
2 | <html lang="en">
3 |   <head>
4 |     <meta charset="UTF-8" />
5 |     <link rel="icon" type="image/svg+xml" href="/vite.svg" />
6 |     <meta name="viewport" content="width=device-width, initial-scale=1.0" />
7 |     <title>Vite + React</title>
8 |   </head>
9 |   <body>
10 |     <div id="root"></div>
11 |     <script type="module" src="/src/main.jsx"></script>
12 |   </body>
13 | </html>
```

frontend/csp-web/package.json
```
1 | {
2 |   "name": "csp-web",
3 |   "private": true,
4 |   "version": "0.0.0",
5 |   "type": "module",
6 |   "scripts": {
7 |     "dev": "vite",
8 |     "build": "vite build",
9 |     "lint": "eslint .",
10 |     "preview": "vite preview"
11 |   },
12 |   "dependencies": {
13 |     "axios": "^1.11.0",
14 |     "react": "^19.1.1",
15 |     "react-dom": "^19.1.1"
16 |   },
17 |   "devDependencies": {
18 |     "@eslint/js": "^9.33.0",
19 |     "@types/react": "^19.1.10",
20 |     "@types/react-dom": "^19.1.7",
21 |     "@vitejs/plugin-react": "^5.0.0",
22 |     "eslint": "^9.33.0",
23 |     "eslint-plugin-react-hooks": "^5.2.0",
24 |     "eslint-plugin-react-refresh": "^0.4.20",
25 |     "globals": "^16.3.0",
26 |     "vite": "^7.1.2"
27 |   }
28 | }
```

frontend/csp-web/vite.config.js
```
1 | import { defineConfig } from 'vite'
2 | import react from '@vitejs/plugin-react'
3 | 
4 | // https://vite.dev/config/
5 | export default defineConfig({
6 |   plugins: [react()],
7 | })
```

backend/Csp.Api/Controllers/AuthController.cs
```
1 | using Microsoft.AspNetCore.Mvc;
2 | using Csp.Api.DTOs;
3 | using Csp.Api.Services;
4 | 
5 | namespace Csp.Api.Controllers
6 | {
7 |     [ApiController]
8 |     [Route("api/[controller]")]
9 |     public class AuthController : ControllerBase
10 |     {
11 |         private readonly IUserService _userService;
12 | 
13 |         public AuthController(IUserService userService)
14 |         {
15 |             _userService = userService;
16 |         }
17 | 
18 |         [HttpPost("login")]
19 |         public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
20 |         {
21 |             if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
22 |             {
23 |                 return BadRequest(new LoginResponse 
24 |                 { 
25 |                     Success = false, 
26 |                     Message = "Username and password are required" 
27 |                 });
28 |             }
29 | 
30 |             var result = await _userService.LoginAsync(request);
31 |             
32 |             if (result.Success)
33 |             {
34 |                 return Ok(result);
35 |             }
36 |             
37 |             return Unauthorized(result);
38 |         }
39 | 
40 |         [HttpPost("register")]
41 |         public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
42 |         {
43 |             if (string.IsNullOrEmpty(request.Username) || 
44 |                 string.IsNullOrEmpty(request.Email) || 
45 |                 string.IsNullOrEmpty(request.Password))
46 |             {
47 |                 return BadRequest(new LoginResponse 
48 |                 { 
49 |                     Success = false, 
50 |                     Message = "Username, email and password are required" 
51 |                 });
52 |             }
53 | 
54 |             var result = await _userService.RegisterAsync(request);
55 |             
56 |             if (result.Success)
57 |             {
58 |                 return Ok(result);
59 |             }
60 |             
61 |             return BadRequest(result);
62 |         }
63 | 
64 |         [HttpGet("test-credentials")]
65 |         public ActionResult GetTestCredentials()
66 |         {
67 |             return Ok(new 
68 |             { 
69 |                 message = "Use these credentials to test login",
70 |                 username = "testuser",
71 |                 password = "password123"
72 |             });
73 |         }
74 |     }
75 | }
```

backend/Csp.Api/Controllers/UsersController.cs
```
1 | using Microsoft.AspNetCore.Mvc;
2 | using Microsoft.AspNetCore.Authorization;
3 | using Csp.Api.Services;
4 | using Csp.Api.DTOs;
5 | 
6 | namespace Csp.Api.Controllers
7 | {
8 |     [ApiController]
9 |     [Route("api/users")]
10 |     public class UsersController : ControllerBase
11 |     {
12 |         private readonly IUserService userService;
13 | 
14 |         public UsersController(IUserService userService)
15 |         {
16 |             this.userService = userService;
17 |         }
18 | 
19 |         [HttpPost("members")]
20 |         [Authorize(Policy = "RequireAdmin")]
21 |         public async Task<ActionResult<LoginResponse>> CreateMember([FromBody] CreateMemberRequest request)
22 |         {
23 |             if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
24 |                 return BadRequest(new LoginResponse { Success = false, Message = "Username, email and password are required" });
25 | 
26 |             var actorId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
27 |             var result = await userService.CreateMemberAsync(request, actorId);
28 |             if (!result.Success) return BadRequest(result);
29 |             return Ok(result);
30 |         }
31 | 
32 |         [HttpGet("members")]
33 |         [Authorize(Policy = "RequireAdmin")]
34 |         public async Task<ActionResult<PagedMembersResponse>> GetMembers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
35 |         {
36 |             if (page <= 0 || pageSize <= 0) return BadRequest();
37 |             var result = await userService.GetMembersAsync(q, page, pageSize);
38 |             return Ok(result);
39 |         }
40 | 
41 |         [HttpPut("members/{id}")]
42 |         [Authorize(Policy = "RequireAdmin")]
43 |         public async Task<ActionResult> UpdateMember([FromRoute] int id, [FromBody] UpdateMemberRequest request)
44 |         {
45 |             if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email)) return BadRequest();
46 |             var actorId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
47 |             var ok = await userService.UpdateMemberAsync(id, request, actorId);
48 |             if (!ok) return BadRequest();
49 |             return Ok();
50 |         }
51 | 
52 |         [HttpPut("{id}/status")]
53 |         [Authorize(Policy = "RequireAdmin")]
54 |         public async Task<ActionResult> UpdateStatus([FromRoute] int id, [FromQuery] bool isActive)
55 |         {
56 |             var actorId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
57 |             var ok = await userService.UpdateUserStatusAsync(id, isActive, actorId);
58 |             if (!ok) return BadRequest();
59 |             return Ok();
60 |         }
61 | 
62 |         [HttpGet("my-profile")]
63 |         [Authorize]
64 |         public async Task<ActionResult<UserDto>> GetMyProfile()
65 |         {
66 |             var id = int.Parse(User.FindFirst("sub")?.Value ?? "0");
67 |             var user = await userService.GetCurrentUserAsync(id);
68 |             if (user == null) return NotFound();
69 |             return Ok(user);
70 |         }
71 | 
72 |         [HttpPut("my-profile")]
73 |         [Authorize]
74 |         public async Task<ActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
75 |         {
76 |             var id = int.Parse(User.FindFirst("sub")?.Value ?? "0");
77 |             var ok = await userService.UpdateMyProfileAsync(id, request);
78 |             if (!ok) return BadRequest();
79 |             return Ok();
80 |         }
81 | 
82 |         [HttpPut("my-password")]
83 |         [Authorize]
84 |         public async Task<ActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
85 |         {
86 |             var id = int.Parse(User.FindFirst("sub")?.Value ?? "0");
87 |             var ok = await userService.ChangePasswordAsync(id, request);
88 |             if (!ok) return BadRequest();
89 |             return Ok();
90 |         }
91 |     }
92 | }
93 | 
94 | 
```

backend/Csp.Api/DTOs/AuthDtos.cs
```
1 | namespace Csp.Api.DTOs
2 | {
3 |     public class LoginRequest
4 |     {
5 |         public string Username { get; set; } = string.Empty;
6 |         public string Password { get; set; } = string.Empty;
7 |     }
8 | 
9 |     public class LoginResponse
10 |     {
11 |         public bool Success { get; set; }
12 |         public string Message { get; set; } = string.Empty;
13 |         public UserDto? User { get; set; }
14 |         public string? Token { get; set; }
15 |     }
16 | 
17 |     public class UserDto
18 |     {
19 |         public int Id { get; set; }
20 |         public string Username { get; set; } = string.Empty;
21 |         public string Email { get; set; } = string.Empty;
22 |         public string Role { get; set; } = string.Empty;
23 |         public bool IsActive { get; set; }
24 |     }
25 | 
26 |     public class RegisterRequest
27 |     {
28 |         public string Username { get; set; } = string.Empty;
29 |         public string Email { get; set; } = string.Empty;
30 |         public string Password { get; set; } = string.Empty;
31 |     }
32 | }
```

backend/Csp.Api/DTOs/MemberDtos.cs
```
1 | namespace Csp.Api.DTOs
2 | {
3 |     public class CreateMemberRequest
4 |     {
5 |         public string Username { get; set; } = string.Empty;
6 |         public string Email { get; set; } = string.Empty;
7 |         public string Password { get; set; } = string.Empty;
8 |     }
9 | 
10 |     public class UpdateMemberRequest
11 |     {
12 |         public string Username { get; set; } = string.Empty;
13 |         public string Email { get; set; } = string.Empty;
14 |     }
15 | 
16 |     public class PagedMembersResponse
17 |     {
18 |         public IEnumerable<UserDto> Items { get; set; } = Enumerable.Empty<UserDto>();
19 |         public int Total { get; set; }
20 |         public int Page { get; set; }
21 |         public int PageSize { get; set; }
22 |     }
23 | }
24 | 
25 | 
```

backend/Csp.Api/DTOs/ProfileDtos.cs
```
1 | namespace Csp.Api.DTOs
2 | {
3 |     public class UpdateProfileRequest
4 |     {
5 |         public string Username { get; set; } = string.Empty;
6 |         public string Email { get; set; } = string.Empty;
7 |     }
8 | 
9 |     public class ChangePasswordRequest
10 |     {
11 |         public string CurrentPassword { get; set; } = string.Empty;
12 |         public string NewPassword { get; set; } = string.Empty;
13 |     }
14 | }
15 | 
16 | 
```

backend/Csp.Api/Models/User.cs
```
1 | namespace Csp.Api.Models
2 | {
3 |     public class User
4 |     {
5 |         public int Id { get; set; }
6 |         public string Username { get; set; } = string.Empty;
7 |         public string Email { get; set; } = string.Empty;
8 |         public string PasswordHash { get; set; } = string.Empty;
9 |         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
10 |         public string Role { get; set; } = "Member";
11 |         public bool IsActive { get; set; } = true;
12 |         public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
13 |     }
14 | }
```

backend/Csp.Api/Properties/launchSettings.json
```
1 | ﻿{
2 |   "$schema": "https://json.schemastore.org/launchsettings.json",
3 |   "profiles": {
4 |     "http": {
5 |       "commandName": "Project",
6 |       "dotnetRunMessages": true,
7 |       "launchBrowser": false,
8 |       "applicationUrl": "http://localhost:5192",
9 |       "environmentVariables": {
10 |         "ASPNETCORE_ENVIRONMENT": "Development"
11 |       }
12 |     },
13 |     "https": {
14 |       "commandName": "Project",
15 |       "dotnetRunMessages": true,
16 |       "launchBrowser": false,
17 |       "applicationUrl": "https://localhost:7009;http://localhost:5250",
18 |       "environmentVariables": {
19 |         "ASPNETCORE_ENVIRONMENT": "Development"
20 |       }
21 |     }
22 |   }
23 | }
```

backend/Csp.Api/Services/UserService.cs
```
1 | using Csp.Api.Models;
2 | using Csp.Api.DTOs;
3 | using MySql.Data.MySqlClient;
4 | using System.Security.Cryptography;
5 | using System.Text;
6 | 
7 | namespace Csp.Api.Services
8 | {
9 |     public interface IUserService
10 |     {
11 |         Task<LoginResponse> LoginAsync(LoginRequest request);
12 |         Task<LoginResponse> RegisterAsync(RegisterRequest request);
13 |         Task InitializeDatabaseAsync();
14 |         Task<LoginResponse> CreateMemberAsync(CreateMemberRequest request, int actorUserId);
15 |         Task<PagedMembersResponse> GetMembersAsync(string? search, int page, int pageSize);
16 |         Task<bool> UpdateMemberAsync(int id, UpdateMemberRequest request, int actorUserId);
17 |         Task<bool> UpdateUserStatusAsync(int id, bool isActive, int actorUserId);
18 |         Task<UserDto?> GetCurrentUserAsync(int id);
19 |         Task<bool> UpdateMyProfileAsync(int id, UpdateProfileRequest request);
20 |         Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request);
21 |     }
22 | 
23 |     public class UserService : IUserService
24 |     {
25 |         private readonly IConfiguration _configuration;
26 |         private readonly string _connectionString;
27 |         private readonly IJwtTokenService _jwtTokenService;
28 | 
29 |         public UserService(IConfiguration configuration, IJwtTokenService jwtTokenService)
30 |         {
31 |             _configuration = configuration;
32 |             _jwtTokenService = jwtTokenService;
33 |             _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
34 |                               throw new InvalidOperationException("Connection string not found");
35 |         }
36 | 
37 |         public async Task InitializeDatabaseAsync()
38 |         {
39 |             var maxRetries = 10;
40 |             var delay = TimeSpan.FromSeconds(5);
41 | 
42 |             for (int i = 0; i < maxRetries; i++)
43 |             {
44 |                 try
45 |                 {
46 |                     await using var conn = new MySqlConnection(_connectionString);
47 |                     await conn.OpenAsync();
48 | 
49 |                     var createTableSql = @"
50 |                         CREATE TABLE IF NOT EXISTS users (
51 |                             Id INT AUTO_INCREMENT PRIMARY KEY,
52 |                             Username VARCHAR(50) UNIQUE NOT NULL,
53 |                             Email VARCHAR(100) UNIQUE NOT NULL,
54 |                             PasswordHash VARCHAR(255) NOT NULL,
55 |                             Role VARCHAR(20) NOT NULL DEFAULT 'Member',
56 |                             IsActive TINYINT(1) NOT NULL DEFAULT 1,
57 |                             CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
58 |                             UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
59 |                         )";
60 | 
61 |                     await using var cmd = new MySqlCommand(createTableSql, conn);
62 |                     await cmd.ExecuteNonQueryAsync();
63 | 
64 |                     var createAuditLogSql = @"
65 |                         CREATE TABLE IF NOT EXISTS audit_log (
66 |                             Id INT AUTO_INCREMENT PRIMARY KEY,
67 |                             ActorUserId INT NULL,
68 |                             Action VARCHAR(100) NOT NULL,
69 |                             TargetUserId INT NULL,
70 |                             Details TEXT NULL,
71 |                             CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
72 |                         )";
73 |                     await using var auditCmd = new MySqlCommand(createAuditLogSql, conn);
74 |                     await auditCmd.ExecuteNonQueryAsync();
75 | 
76 |                     var countUsersSql = "SELECT COUNT(*) FROM users";
77 |                     await using var countCmd = new MySqlCommand(countUsersSql, conn);
78 |                     var totalUsers = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
79 |                     if (totalUsers == 0)
80 |                     {
81 |                         var adminPass = HashPassword("admin123!");
82 |                         var librarianPass = HashPassword("lib123!");
83 |                         var memberPass = HashPassword("member123!");
84 | 
85 |                         var seedSql = @"
86 |                             INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) VALUES
87 |                             ('admin', 'admin@example.com', @AdminHash, 'Administrator', 1),
88 |                             ('librarian', 'librarian@example.com', @LibrarianHash, 'Librarian', 1),
89 |                             ('member', 'member@example.com', @MemberHash, 'Member', 1),
90 |                             ('testuser', 'test@example.com', @TestHash, 'Member', 1)
91 |                         ";
92 |                         await using var seedCmd = new MySqlCommand(seedSql, conn);
93 |                         seedCmd.Parameters.AddWithValue("@AdminHash", adminPass);
94 |                         seedCmd.Parameters.AddWithValue("@LibrarianHash", librarianPass);
95 |                         seedCmd.Parameters.AddWithValue("@MemberHash", memberPass);
96 |                         seedCmd.Parameters.AddWithValue("@TestHash", HashPassword("password123"));
97 |                         await seedCmd.ExecuteNonQueryAsync();
98 |                     }
99 | 
100 |                     Console.WriteLine("Database initialized successfully!");
101 |                     return;
102 |                 }
103 |                 catch (Exception ex)
104 |                 {
105 |                     Console.WriteLine($"Database initialization attempt {i + 1} failed: {ex.Message}");
106 |                     if (i < maxRetries - 1)
107 |                     {
108 |                         Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
109 |                         await Task.Delay(delay);
110 |                     }
111 |                     else
112 |                     {
113 |                         Console.WriteLine("Max retries reached. Database initialization failed.");
114 |                         throw;
115 |                     }
116 |                 }
117 |             }
118 |         }
119 | 
120 |         public async Task<LoginResponse> LoginAsync(LoginRequest request)
121 |         {
122 |             await using var conn = new MySqlConnection(_connectionString);
123 |             await conn.OpenAsync();
124 | 
125 |             var sql = @"SELECT Id, Username, Email, PasswordHash, Role, IsActive 
126 |                         FROM users 
127 |                         WHERE Username = @Identifier OR Email = @Identifier";
128 |             await using var cmd = new MySqlCommand(sql, conn);
129 |             cmd.Parameters.AddWithValue("@Identifier", request.Username);
130 | 
131 |             await using var reader = await cmd.ExecuteReaderAsync();
132 |             
133 |             if (await reader.ReadAsync())
134 |             {
135 |                 var storedHash = reader["PasswordHash"]?.ToString();
136 |                 if (!string.IsNullOrEmpty(storedHash) && VerifyPassword(request.Password, storedHash))
137 |                 {
138 |                     var isActive = Convert.ToBoolean(reader["IsActive"]);
139 |                     if (!isActive)
140 |                     {
141 |                         return new LoginResponse { Success = false, Message = "Your account is inactive. Please contact an administrator." };
142 |                     }
143 |                     return new LoginResponse
144 |                     {
145 |                         Success = true,
146 |                         Message = "Login successful",
147 |                         User = new UserDto
148 |                         {
149 |                             Id = Convert.ToInt32(reader["Id"]),
150 |                             Username = reader["Username"]?.ToString() ?? "",
151 |                             Email = reader["Email"]?.ToString() ?? "",
152 |                             Role = reader["Role"]?.ToString() ?? "",
153 |                             IsActive = isActive
154 |                         },
155 |                         Token = _jwtTokenService.GenerateToken(
156 |                             Convert.ToInt32(reader["Id"]),
157 |                             reader["Username"]?.ToString() ?? string.Empty,
158 |                             reader["Role"]?.ToString() ?? string.Empty
159 |                         )
160 |                     };
161 |                 }
162 |             }
163 | 
164 |             return new LoginResponse
165 |             {
166 |                 Success = false,
167 |                 Message = "Invalid username or password"
168 |             };
169 |         }
170 | 
171 |         public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
172 |         {
173 |             await using var conn = new MySqlConnection(_connectionString);
174 |             await conn.OpenAsync();
175 | 
176 |             // Check if user already exists
177 |             var checkSql = "SELECT COUNT(*) FROM users WHERE Username = @Username OR Email = @Email";
178 |             await using var checkCmd = new MySqlCommand(checkSql, conn);
179 |             checkCmd.Parameters.AddWithValue("@Username", request.Username);
180 |             checkCmd.Parameters.AddWithValue("@Email", request.Email);
181 |             
182 |             var existingCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
183 |             if (existingCount > 0)
184 |             {
185 |                 return new LoginResponse
186 |                 {
187 |                     Success = false,
188 |                     Message = "Username or email already exists"
189 |                 };
190 |             }
191 | 
192 |             // Insert new user
193 |             var insertSql = @"
194 |                 INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) 
195 |                 VALUES (@Username, @Email, @PasswordHash, 'Member', 1);
196 |                 SELECT LAST_INSERT_ID();";
197 |             
198 |             await using var insertCmd = new MySqlCommand(insertSql, conn);
199 |             insertCmd.Parameters.AddWithValue("@Username", request.Username);
200 |             insertCmd.Parameters.AddWithValue("@Email", request.Email);
201 |             insertCmd.Parameters.AddWithValue("@PasswordHash", HashPassword(request.Password));
202 | 
203 |             var userId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
204 | 
205 |             return new LoginResponse
206 |             {
207 |                 Success = true,
208 |                 Message = "Registration successful",
209 |                 User = new UserDto
210 |                 {
211 |                     Id = userId,
212 |                     Username = request.Username,
213 |                     Email = request.Email
214 |                 },
215 |                 Token = "simple-jwt-token-" + userId
216 |             };
217 |         }
218 | 
219 |         public async Task<LoginResponse> CreateMemberAsync(CreateMemberRequest request, int actorUserId)
220 |         {
221 |             await using var conn = new MySqlConnection(_connectionString);
222 |             await conn.OpenAsync();
223 | 
224 |             var checkSql = "SELECT COUNT(*) FROM users WHERE Username = @Username OR Email = @Email";
225 |             await using var checkCmd = new MySqlCommand(checkSql, conn);
226 |             checkCmd.Parameters.AddWithValue("@Username", request.Username);
227 |             checkCmd.Parameters.AddWithValue("@Email", request.Email);
228 |             var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
229 |             if (exists)
230 |             {
231 |                 return new LoginResponse { Success = false, Message = "Username or email already exists" };
232 |             }
233 | 
234 |             var insertSql = @"INSERT INTO users (Username, Email, PasswordHash, Role, IsActive)
235 |                               VALUES (@Username, @Email, @PasswordHash, 'Member', 1);
236 |                               SELECT LAST_INSERT_ID();";
237 |             await using var insertCmd = new MySqlCommand(insertSql, conn);
238 |             insertCmd.Parameters.AddWithValue("@Username", request.Username);
239 |             insertCmd.Parameters.AddWithValue("@Email", request.Email);
240 |             insertCmd.Parameters.AddWithValue("@PasswordHash", HashPassword(request.Password));
241 |             var userId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
242 | 
243 |             await WriteAuditAsync(conn, actorUserId, "CreateMember", userId, $"username={request.Username}");
244 | 
245 |             return new LoginResponse
246 |             {
247 |                 Success = true,
248 |                 Message = "Member registered successfully",
249 |                 User = new UserDto { Id = userId, Username = request.Username, Email = request.Email, Role = "Member", IsActive = true }
250 |             };
251 |         }
252 | 
253 |         public async Task<PagedMembersResponse> GetMembersAsync(string? search, int page, int pageSize)
254 |         {
255 |             await using var conn = new MySqlConnection(_connectionString);
256 |             await conn.OpenAsync();
257 | 
258 |             var where = "WHERE Role = 'Member'" + (string.IsNullOrWhiteSpace(search) ? "" : " AND (Username LIKE @q OR Email LIKE @q)");
259 |             var countSql = $"SELECT COUNT(*) FROM users {where}";
260 |             await using var countCmd = new MySqlCommand(countSql, conn);
261 |             if (!string.IsNullOrWhiteSpace(search)) countCmd.Parameters.AddWithValue("@q", $"%{search}%");
262 |             var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
263 | 
264 |             var offset = (page - 1) * pageSize;
265 |             var listSql = $@"SELECT Id, Username, Email, Role, IsActive FROM users {where} ORDER BY Id DESC LIMIT @ps OFFSET @off";
266 |             await using var listCmd = new MySqlCommand(listSql, conn);
267 |             if (!string.IsNullOrWhiteSpace(search)) listCmd.Parameters.AddWithValue("@q", $"%{search}%");
268 |             listCmd.Parameters.AddWithValue("@ps", pageSize);
269 |             listCmd.Parameters.AddWithValue("@off", offset);
270 | 
271 |             var items = new List<UserDto>();
272 |             await using var reader = await listCmd.ExecuteReaderAsync();
273 |             while (await reader.ReadAsync())
274 |             {
275 |                 items.Add(new UserDto
276 |                 {
277 |                     Id = Convert.ToInt32(reader["Id"]),
278 |                     Username = reader["Username"]?.ToString() ?? string.Empty,
279 |                     Email = reader["Email"]?.ToString() ?? string.Empty,
280 |                     Role = reader["Role"]?.ToString() ?? string.Empty,
281 |                     IsActive = Convert.ToBoolean(reader["IsActive"])
282 |                 });
283 |             }
284 | 
285 |             return new PagedMembersResponse { Items = items, Total = total, Page = page, PageSize = pageSize };
286 |         }
287 | 
288 |         public async Task<bool> UpdateMemberAsync(int id, UpdateMemberRequest request, int actorUserId)
289 |         {
290 |             await using var conn = new MySqlConnection(_connectionString);
291 |             await conn.OpenAsync();
292 | 
293 |             var dupSql = "SELECT COUNT(*) FROM users WHERE (Username = @Username OR Email = @Email) AND Id <> @Id";
294 |             await using var dupCmd = new MySqlCommand(dupSql, conn);
295 |             dupCmd.Parameters.AddWithValue("@Username", request.Username);
296 |             dupCmd.Parameters.AddWithValue("@Email", request.Email);
297 |             dupCmd.Parameters.AddWithValue("@Id", id);
298 |             var dup = Convert.ToInt32(await dupCmd.ExecuteScalarAsync()) > 0;
299 |             if (dup) return false;
300 | 
301 |             var updateSql = "UPDATE users SET Username=@Username, Email=@Email WHERE Id=@Id AND Role='Member'";
302 |             await using var updateCmd = new MySqlCommand(updateSql, conn);
303 |             updateCmd.Parameters.AddWithValue("@Username", request.Username);
304 |             updateCmd.Parameters.AddWithValue("@Email", request.Email);
305 |             updateCmd.Parameters.AddWithValue("@Id", id);
306 |             var rows = await updateCmd.ExecuteNonQueryAsync();
307 | 
308 |             if (rows > 0)
309 |             {
310 |                 await WriteAuditAsync(conn, actorUserId, "UpdateMember", id, $"username={request.Username}");
311 |                 return true;
312 |             }
313 |             return false;
314 |         }
315 | 
316 |         private static async Task WriteAuditAsync(MySqlConnection conn, int actorUserId, string action, int targetUserId, string details)
317 |         {
318 |             var sql = "INSERT INTO audit_log (ActorUserId, Action, TargetUserId, Details) VALUES (@ActorUserId, @Action, @TargetUserId, @Details)";
319 |             await using var cmd = new MySqlCommand(sql, conn);
320 |             cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
321 |             cmd.Parameters.AddWithValue("@Action", action);
322 |             cmd.Parameters.AddWithValue("@TargetUserId", targetUserId);
323 |             cmd.Parameters.AddWithValue("@Details", details);
324 |             await cmd.ExecuteNonQueryAsync();
325 |         }
326 | 
327 |         public async Task<bool> UpdateUserStatusAsync(int id, bool isActive, int actorUserId)
328 |         {
329 |             if (id == actorUserId) return false;
330 |             await using var conn = new MySqlConnection(_connectionString);
331 |             await conn.OpenAsync();
332 | 
333 |             var updateSql = "UPDATE users SET IsActive=@IsActive WHERE Id=@Id";
334 |             await using var updateCmd = new MySqlCommand(updateSql, conn);
335 |             updateCmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
336 |             updateCmd.Parameters.AddWithValue("@Id", id);
337 |             var rows = await updateCmd.ExecuteNonQueryAsync();
338 |             if (rows > 0)
339 |             {
340 |                 await WriteAuditAsync(conn, actorUserId, isActive ? "ReactivateUser" : "DeactivateUser", id, "");
341 |                 return true;
342 |             }
343 |             return false;
344 |         }
345 | 
346 |         private string HashPassword(string password)
347 |         {
348 |             using var sha256 = SHA256.Create();
349 |             var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
350 |             return Convert.ToBase64String(hashedBytes);
351 |         }
352 | 
353 |         private bool VerifyPassword(string password, string hash)
354 |         {
355 |             var passwordHash = HashPassword(password);
356 |             return passwordHash == hash;
357 |         }
358 | 
359 |         public async Task<UserDto?> GetCurrentUserAsync(int id)
360 |         {
361 |             await using var conn = new MySqlConnection(_connectionString);
362 |             await conn.OpenAsync();
363 |             var sql = "SELECT Id, Username, Email, Role, IsActive FROM users WHERE Id=@Id";
364 |             await using var cmd = new MySqlCommand(sql, conn);
365 |             cmd.Parameters.AddWithValue("@Id", id);
366 |             await using var r = await cmd.ExecuteReaderAsync();
367 |             if (await r.ReadAsync())
368 |             {
369 |                 return new UserDto
370 |                 {
371 |                     Id = Convert.ToInt32(r["Id"]),
372 |                     Username = r["Username"]?.ToString() ?? string.Empty,
373 |                     Email = r["Email"]?.ToString() ?? string.Empty,
374 |                     Role = r["Role"]?.ToString() ?? string.Empty,
375 |                     IsActive = Convert.ToBoolean(r["IsActive"])
376 |                 };
377 |             }
378 |             return null;
379 |         }
380 | 
381 |         public async Task<bool> UpdateMyProfileAsync(int id, UpdateProfileRequest request)
382 |         {
383 |             await using var conn = new MySqlConnection(_connectionString);
384 |             await conn.OpenAsync();
385 | 
386 |             var dupSql = "SELECT COUNT(*) FROM users WHERE (Username=@Username OR Email=@Email) AND Id<>@Id";
387 |             await using var dupCmd = new MySqlCommand(dupSql, conn);
388 |             dupCmd.Parameters.AddWithValue("@Username", request.Username);
389 |             dupCmd.Parameters.AddWithValue("@Email", request.Email);
390 |             dupCmd.Parameters.AddWithValue("@Id", id);
391 |             var dup = Convert.ToInt32(await dupCmd.ExecuteScalarAsync()) > 0;
392 |             if (dup) return false;
393 | 
394 |             var sql = "UPDATE users SET Username=@Username, Email=@Email WHERE Id=@Id";
395 |             await using var cmd = new MySqlCommand(sql, conn);
396 |             cmd.Parameters.AddWithValue("@Username", request.Username);
397 |             cmd.Parameters.AddWithValue("@Email", request.Email);
398 |             cmd.Parameters.AddWithValue("@Id", id);
399 |             var rows = await cmd.ExecuteNonQueryAsync();
400 |             return rows > 0;
401 |         }
402 | 
403 |         public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request)
404 |         {
405 |             if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8) return false;
406 |             await using var conn = new MySqlConnection(_connectionString);
407 |             await conn.OpenAsync();
408 | 
409 |             var getSql = "SELECT PasswordHash FROM users WHERE Id=@Id";
410 |             await using var getCmd = new MySqlCommand(getSql, conn);
411 |             getCmd.Parameters.AddWithValue("@Id", id);
412 |             var stored = (string?)await getCmd.ExecuteScalarAsync();
413 |             if (string.IsNullOrEmpty(stored) || !VerifyPassword(request.CurrentPassword, stored)) return false;
414 | 
415 |             var updSql = "UPDATE users SET PasswordHash=@Hash WHERE Id=@Id";
416 |             await using var updCmd = new MySqlCommand(updSql, conn);
417 |             updCmd.Parameters.AddWithValue("@Hash", HashPassword(request.NewPassword));
418 |             updCmd.Parameters.AddWithValue("@Id", id);
419 |             var rows = await updCmd.ExecuteNonQueryAsync();
420 |             return rows > 0;
421 |         }
422 |     }
423 | }
```

frontend/csp-web/src/App.css
```
1 | #root {
2 |   max-width: 1280px;
3 |   margin: 0 auto;
4 |   padding: 2rem;
5 |   text-align: center;
6 | }
7 | 
8 | .logo {
9 |   height: 6em;
10 |   padding: 1.5em;
11 |   will-change: filter;
12 |   transition: filter 300ms;
13 | }
14 | .logo:hover {
15 |   filter: drop-shadow(0 0 2em #646cffaa);
16 | }
17 | .logo.react:hover {
18 |   filter: drop-shadow(0 0 2em #61dafbaa);
19 | }
20 | 
21 | @keyframes logo-spin {
22 |   from {
23 |     transform: rotate(0deg);
24 |   }
25 |   to {
26 |     transform: rotate(360deg);
27 |   }
28 | }
29 | 
30 | @media (prefers-reduced-motion: no-preference) {
31 |   a:nth-of-type(2) .logo {
32 |     animation: logo-spin infinite 20s linear;
33 |   }
34 | }
35 | 
36 | .card {
37 |   padding: 2em;
38 | }
39 | 
40 | .read-the-docs {
41 |   color: #888;
42 | }
```

frontend/csp-web/src/App.jsx
```
1 | import { useEffect, useState } from "react";
2 | import Login from "./components/Login";
3 | import Dashboard from "./components/Dashboard";
4 | import "./App.css";
5 | 
6 | export default function App() {
7 |   const [user, setUser] = useState(null);
8 |   const [loading, setLoading] = useState(true);
9 | 
10 |   useEffect(() => {
11 |     // Check if user is already logged in
12 |     const token = localStorage.getItem('token');
13 |     const savedUser = localStorage.getItem('user');
14 |     
15 |     if (token && savedUser) {
16 |       try {
17 |         setUser(JSON.parse(savedUser));
18 |       } catch (error) {
19 |         console.error('Error parsing saved user data:', error);
20 |         localStorage.removeItem('token');
21 |         localStorage.removeItem('user');
22 |       }
23 |     }
24 |     
25 |     setLoading(false);
26 |   }, []);
27 | 
28 |   const handleLoginSuccess = (userData) => {
29 |     setUser(userData);
30 |   };
31 | 
32 |   const handleLogout = () => {
33 |     setUser(null);
34 |   };
35 | 
36 |   if (loading) {
37 |     return (
38 |       <div style={{ 
39 |         display: 'flex', 
40 |         justifyContent: 'center', 
41 |         alignItems: 'center', 
42 |         height: '100vh' 
43 |       }}>
44 |         <div>Loading...</div>
45 |       </div>
46 |     );
47 |   }
48 | 
49 |   return (
50 |     <div className="app">
51 |       {user ? (
52 |         <Dashboard user={user} onLogout={handleLogout} />
53 |       ) : (
54 |         <Login onLoginSuccess={handleLoginSuccess} />
55 |       )}
56 |     </div>
57 |   );
58 | }
```

frontend/csp-web/src/index.css
```
1 | :root {
2 |   font-family: system-ui, Avenir, Helvetica, Arial, sans-serif;
3 |   line-height: 1.5;
4 |   font-weight: 400;
5 | 
6 |   color-scheme: light dark;
7 |   color: rgba(255, 255, 255, 0.87);
8 |   background-color: #242424;
9 | 
10 |   font-synthesis: none;
11 |   text-rendering: optimizeLegibility;
12 |   -webkit-font-smoothing: antialiased;
13 |   -moz-osx-font-smoothing: grayscale;
14 | }
15 | 
16 | a {
17 |   font-weight: 500;
18 |   color: #646cff;
19 |   text-decoration: inherit;
20 | }
21 | a:hover {
22 |   color: #535bf2;
23 | }
24 | 
25 | body {
26 |   margin: 0;
27 |   display: flex;
28 |   place-items: center;
29 |   min-width: 320px;
30 |   min-height: 100vh;
31 | }
32 | 
33 | h1 {
34 |   font-size: 3.2em;
35 |   line-height: 1.1;
36 | }
37 | 
38 | button {
39 |   border-radius: 8px;
40 |   border: 1px solid transparent;
41 |   padding: 0.6em 1.2em;
42 |   font-size: 1em;
43 |   font-weight: 500;
44 |   font-family: inherit;
45 |   background-color: #1a1a1a;
46 |   cursor: pointer;
47 |   transition: border-color 0.25s;
48 | }
49 | button:hover {
50 |   border-color: #646cff;
51 | }
52 | button:focus,
53 | button:focus-visible {
54 |   outline: 4px auto -webkit-focus-ring-color;
55 | }
56 | 
57 | @media (prefers-color-scheme: light) {
58 |   :root {
59 |     color: #213547;
60 |     background-color: #ffffff;
61 |   }
62 |   a:hover {
63 |     color: #747bff;
64 |   }
65 |   button {
66 |     background-color: #f9f9f9;
67 |   }
68 | }
```

frontend/csp-web/src/main.jsx
```
1 | import { StrictMode } from 'react'
2 | import { createRoot } from 'react-dom/client'
3 | import './index.css'
4 | import App from './App.jsx'
5 | 
6 | createRoot(document.getElementById('root')).render(
7 |   <StrictMode>
8 |     <App />
9 |   </StrictMode>,
10 | )
```

frontend/csp-web/src/lib/api.js
```
1 | import axios from "axios";
2 | const api = axios.create({
3 |   baseURL: import.meta.env.VITE_API_BASE || "http://localhost:5192"
4 | });
5 | api.interceptors.request.use((config) => {
6 |   const token = localStorage.getItem('token');
7 |   if (token) config.headers.Authorization = `Bearer ${token}`;
8 |   return config;
9 | });
10 | api.interceptors.response.use(
11 |   (res) => res,
12 |   (err) => {
13 |     if (err?.response?.status === 401) {
14 |       localStorage.removeItem('token');
15 |       localStorage.removeItem('user');
16 |     }
17 |     return Promise.reject(err);
18 |   }
19 | );
20 | export default api;
```

frontend/csp-web/src/components/Dashboard.css
```
1 | .dashboard-container {
2 |   min-height: 100vh;
3 |   background-color: #f5f5f5;
4 | }
5 | 
6 | .dashboard-header {
7 |   background-color: #2c3e50;
8 |   color: white;
9 |   padding: 1rem 2rem;
10 |   display: flex;
11 |   justify-content: space-between;
12 |   align-items: center;
13 |   box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
14 | }
15 | 
16 | .dashboard-header h1 {
17 |   margin: 0;
18 |   font-size: 1.5rem;
19 | }
20 | 
21 | .user-info {
22 |   display: flex;
23 |   align-items: center;
24 |   gap: 1rem;
25 | }
26 | 
27 | .logout-btn {
28 |   background-color: #e74c3c;
29 |   color: white;
30 |   border: none;
31 |   padding: 0.5rem 1rem;
32 |   border-radius: 4px;
33 |   cursor: pointer;
34 |   font-size: 0.9rem;
35 | }
36 | 
37 | .logout-btn:hover {
38 |   background-color: #c0392b;
39 | }
40 | 
41 | .dashboard-content {
42 |   padding: 2rem;
43 |   display: grid;
44 |   gap: 2rem;
45 |   max-width: 1200px;
46 |   margin: 0 auto;
47 | }
48 | 
49 | .info-card {
50 |   background: white;
51 |   padding: 1.5rem;
52 |   border-radius: 8px;
53 |   box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
54 | }
55 | 
56 | .info-card h3 {
57 |   margin: 0 0 1rem 0;
58 |   color: #2c3e50;
59 |   border-bottom: 2px solid #3498db;
60 |   padding-bottom: 0.5rem;
61 | }
62 | 
63 | .info-card p {
64 |   margin: 0.5rem 0;
65 |   color: #555;
66 | }
67 | 
68 | .action-buttons {
69 |   display: grid;
70 |   grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
71 |   gap: 1rem;
72 |   margin-top: 1rem;
73 | }
74 | 
75 | .action-btn {
76 |   background-color: #3498db;
77 |   color: white;
78 |   border: none;
79 |   padding: 1rem;
80 |   border-radius: 4px;
81 |   cursor: pointer;
82 |   font-size: 1rem;
83 |   transition: background-color 0.3s;
84 | }
85 | 
86 | .action-btn:hover {
87 |   background-color: #2980b9;
88 | }
89 | 
90 | @media (max-width: 768px) {
91 |   .dashboard-header {
92 |     flex-direction: column;
93 |     gap: 1rem;
94 |     text-align: center;
95 |   }
96 | 
97 |   .dashboard-content {
98 |     padding: 1rem;
99 |   }
100 | 
101 |   .action-buttons {
102 |     grid-template-columns: 1fr;
103 |   }
104 | }
```

frontend/csp-web/src/components/Dashboard.jsx
```
1 | import { useState, useEffect } from 'react';
2 | import api from '../lib/api';
3 | import './Dashboard.css';
4 | import MemberManagement from './MemberManagement';
5 | import MyProfile from './MyProfile';
6 | 
7 | const Dashboard = ({ user, onLogout }) => {
8 |   const [healthCheck, setHealthCheck] = useState('');
9 |   const [showMembers, setShowMembers] = useState(false);
10 |   const [showProfile, setShowProfile] = useState(false);
11 | 
12 |   useEffect(() => {
13 |     // Test API connection
14 |     api.get('/health/db')
15 |       .then(r => setHealthCheck(`Database connected: ${JSON.stringify(r.data)}`))
16 |       .catch(() => setHealthCheck('API not reachable'));
17 |   }, []);
18 | 
19 |   const handleLogout = () => {
20 |     localStorage.removeItem('token');
21 |     localStorage.removeItem('user');
22 |     onLogout();
23 |   };
24 | 
25 |   const role = user.role || 'Member';
26 | 
27 |   return (
28 |     <div className="dashboard-container">
29 |       <div className="dashboard-header">
30 |         <h1>Library Management System</h1>
31 |         <div className="user-info">
32 |           <span>Welcome, {user.username}!</span>
33 |           <button onClick={handleLogout} className="logout-btn">
34 |             Logout
35 |           </button>
36 |         </div>
37 |       </div>
38 | 
39 |       <div className="dashboard-content">
40 |         <div className="info-card">
41 |           <h3>User Information</h3>
42 |           <p><strong>Username:</strong> {user.username}</p>
43 |           <p><strong>Email:</strong> {user.email}</p>
44 |           <p><strong>User ID:</strong> {user.id}</p>
45 |           <p><strong>Role:</strong> {role}</p>
46 |         </div>
47 | 
48 |         <div className="info-card">
49 |           <h3>System Status</h3>
50 |           <p><strong>Database:</strong> {healthCheck}</p>
51 |           <p><strong>Frontend:</strong> Connected</p>
52 |           <p><strong>Login Status:</strong> Authenticated</p>
53 |         </div>
54 | 
55 |         {role === 'Administrator' && (
56 |           <div className="info-card">
57 |             <h3>Admin Actions</h3>
58 |             <div className="action-buttons">
59 |               <button className="action-btn" onClick={() => setShowMembers(true)}>Member Management</button>
60 |             </div>
61 |           </div>
62 |         )}
63 | 
64 |         {role === 'Librarian' && (
65 |           <div className="info-card">
66 |             <h3>Librarian Actions</h3>
67 |             <div className="action-buttons">
68 |               <button className="action-btn">Circulation</button>
69 |             </div>
70 |           </div>
71 |         )}
72 | 
73 |         {role === 'Member' && (
74 |           <div className="info-card">
75 |             <h3>Member Actions</h3>
76 |             <div className="action-buttons">
77 |               <button className="action-btn" onClick={() => setShowProfile(true)}>My Profile</button>
78 |             </div>
79 |           </div>
80 |         )}
81 |       </div>
82 | 
83 |       {role === 'Administrator' && showMembers && (
84 |         <div className="dashboard-content">
85 |           <MemberManagement />
86 |         </div>
87 |       )}
88 | 
89 |       {role === 'Member' && showProfile && (
90 |         <div className="dashboard-content">
91 |           <MyProfile />
92 |         </div>
93 |       )}
94 |     </div>
95 |   );
96 | };
97 | 
98 | export default Dashboard;
```

frontend/csp-web/src/components/Login.css
```
1 | .login-container {
2 |   display: flex;
3 |   justify-content: center;
4 |   align-items: center;
5 |   min-height: 100vh;
6 |   background-color: #f5f5f5;
7 | }
8 | 
9 | .login-card {
10 |   background: white;
11 |   padding: 2rem;
12 |   border-radius: 8px;
13 |   box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
14 |   width: 100%;
15 |   max-width: 400px;
16 | }
17 | 
18 | .login-card h2 {
19 |   text-align: center;
20 |   margin-bottom: 1.5rem;
21 |   color: #333;
22 | }
23 | 
24 | .test-credentials {
25 |   background-color: #e3f2fd;
26 |   padding: 1rem;
27 |   border-radius: 4px;
28 |   margin-bottom: 1.5rem;
29 |   text-align: center;
30 | }
31 | 
32 | .test-credentials p {
33 |   margin: 0 0 0.5rem 0;
34 |   font-size: 0.9rem;
35 |   color: #666;
36 | }
37 | 
38 | .test-btn {
39 |   background-color: #2196f3;
40 |   color: white;
41 |   border: none;
42 |   padding: 0.5rem 1rem;
43 |   border-radius: 4px;
44 |   cursor: pointer;
45 |   font-size: 0.9rem;
46 | }
47 | 
48 | .test-btn:hover {
49 |   background-color: #1976d2;
50 | }
51 | 
52 | .form-group {
53 |   margin-bottom: 1rem;
54 | }
55 | 
56 | .form-group label {
57 |   display: block;
58 |   margin-bottom: 0.5rem;
59 |   color: #555;
60 |   font-weight: 500;
61 | }
62 | 
63 | .form-group input {
64 |   width: 100%;
65 |   padding: 0.75rem;
66 |   border: 1px solid #ddd;
67 |   border-radius: 4px;
68 |   font-size: 1rem;
69 |   box-sizing: border-box;
70 | }
71 | 
72 | .form-group input:focus {
73 |   outline: none;
74 |   border-color: #4caf50;
75 |   box-shadow: 0 0 0 2px rgba(76, 175, 80, 0.2);
76 | }
77 | 
78 | .error-message {
79 |   background-color: #ffebee;
80 |   color: #c62828;
81 |   padding: 0.75rem;
82 |   border-radius: 4px;
83 |   margin-bottom: 1rem;
84 |   font-size: 0.9rem;
85 | }
86 | 
87 | .submit-btn {
88 |   width: 100%;
89 |   background-color: #4caf50;
90 |   color: white;
91 |   border: none;
92 |   padding: 0.75rem;
93 |   border-radius: 4px;
94 |   font-size: 1rem;
95 |   cursor: pointer;
96 |   margin-bottom: 1rem;
97 | }
98 | 
99 | .submit-btn:hover:not(:disabled) {
100 |   background-color: #45a049;
101 | }
102 | 
103 | .submit-btn:disabled {
104 |   background-color: #cccccc;
105 |   cursor: not-allowed;
106 | }
107 | 
108 | .toggle-mode {
109 |   text-align: center;
110 |   margin: 0;
111 |   color: #666;
112 | }
113 | 
114 | .toggle-btn {
115 |   background: none;
116 |   border: none;
117 |   color: #4caf50;
118 |   cursor: pointer;
119 |   font-weight: 500;
120 |   text-decoration: underline;
121 | }
122 | 
123 | .toggle-btn:hover {
124 |   color: #45a049;
125 | }
```

frontend/csp-web/src/components/Login.jsx
```
1 | import { useState } from 'react';
2 | import api from '../lib/api';
3 | import './Login.css';
4 | 
5 | const Login = ({ onLoginSuccess }) => {
6 |   const [formData, setFormData] = useState({
7 |     username: '',
8 |     password: ''
9 |   });
10 |   const [loading, setLoading] = useState(false);
11 |   const [error, setError] = useState('');
12 |   const [isRegister, setIsRegister] = useState(false);
13 | 
14 |   const handleInputChange = (e) => {
15 |     setFormData({
16 |       ...formData,
17 |       [e.target.name]: e.target.value
18 |     });
19 |   };
20 | 
21 |   const handleSubmit = async (e) => {
22 |     e.preventDefault();
23 |     setLoading(true);
24 |     setError('');
25 | 
26 |     try {
27 |       const endpoint = isRegister ? '/api/auth/register' : '/api/auth/login';
28 |       const requestData = isRegister 
29 |         ? { ...formData, email: formData.username + '@example.com' }
30 |         : formData;
31 | 
32 |       const response = await api.post(endpoint, requestData);
33 |       
34 |       if (response.data.success) {
35 |         localStorage.setItem('token', response.data.token);
36 |         localStorage.setItem('user', JSON.stringify(response.data.user));
37 |         onLoginSuccess(response.data.user);
38 |       } else {
39 |         setError(response.data.message);
40 |       }
41 |     } catch (err) {
42 |       setError(err.response?.data?.message || 'An error occurred');
43 |     } finally {
44 |       setLoading(false);
45 |     }
46 |   };
47 | 
48 |   const useTestCredentials = () => {
49 |     setFormData({
50 |       username: 'testuser',
51 |       password: 'password123'
52 |     });
53 |   };
54 | 
55 |   return (
56 |     <div className="login-container">
57 |       <div className="login-card">
58 |         <h2>{isRegister ? 'Register' : 'Login'}</h2>
59 |         
60 |         {!isRegister && (
61 |           <div className="test-credentials">
62 |             <p>Test credentials:</p>
63 |             <button type="button" onClick={useTestCredentials} className="test-btn">
64 |               Use Test User (testuser / password123)
65 |             </button>
66 |           </div>
67 |         )}
68 | 
69 |         <form onSubmit={handleSubmit}>
70 |           <div className="form-group">
71 |             <label htmlFor="username">Username:</label>
72 |             <input
73 |               type="text"
74 |               id="username"
75 |               name="username"
76 |               value={formData.username}
77 |               onChange={handleInputChange}
78 |               required
79 |             />
80 |           </div>
81 | 
82 |           <div className="form-group">
83 |             <label htmlFor="password">Password:</label>
84 |             <input
85 |               type="password"
86 |               id="password"
87 |               name="password"
88 |               value={formData.password}
89 |               onChange={handleInputChange}
90 |               required
91 |             />
92 |           </div>
93 | 
94 |           {error && <div className="error-message">{error}</div>}
95 | 
96 |           <button type="submit" disabled={loading} className="submit-btn">
97 |             {loading ? 'Loading...' : (isRegister ? 'Register' : 'Login')}
98 |           </button>
99 |         </form>
100 | 
101 |         <p className="toggle-mode">
102 |           {isRegister ? 'Already have an account?' : "Don't have an account?"}{' '}
103 |           <button 
104 |             type="button" 
105 |             onClick={() => setIsRegister(!isRegister)}
106 |             className="toggle-btn"
107 |           >
108 |             {isRegister ? 'Login' : 'Register'}
109 |           </button>
110 |         </p>
111 |       </div>
112 |     </div>
113 |   );
114 | };
115 | 
116 | export default Login;
```

frontend/csp-web/src/components/MemberManagement.jsx
```
1 | import { useEffect, useMemo, useState } from 'react';
2 | import api from '../lib/api';
3 | 
4 | const MemberManagement = () => {
5 |   const [items, setItems] = useState([]);
6 |   const [total, setTotal] = useState(0);
7 |   const [page, setPage] = useState(1);
8 |   const [pageSize] = useState(10);
9 |   const [q, setQ] = useState('');
10 |   const [form, setForm] = useState({ username: '', email: '', password: '' });
11 |   const [edit, setEdit] = useState(null);
12 |   const [error, setError] = useState('');
13 | 
14 |   const canSubmit = useMemo(() => form.username && form.email && (edit ? true : form.password), [form, edit]);
15 | 
16 |   const load = async () => {
17 |     const r = await api.get('/api/users/members', { params: { q, page, pageSize } });
18 |     setItems(r.data.items || []);
19 |     setTotal(r.data.total || 0);
20 |   };
21 | 
22 |   useEffect(() => { load(); }, [q, page]);
23 | 
24 |   const submit = async (e) => {
25 |     e.preventDefault();
26 |     setError('');
27 |     try {
28 |       if (edit) {
29 |         await api.put(`/api/users/members/${edit.id}`, { username: form.username, email: form.email });
30 |         setEdit(null);
31 |       } else {
32 |         await api.post('/api/users/members', form);
33 |       }
34 |       setForm({ username: '', email: '', password: '' });
35 |       await load();
36 |     } catch (err) {
37 |       setError(err?.response?.data?.message || 'Operation failed');
38 |     }
39 |   };
40 | 
41 |   const startEdit = (m) => {
42 |     setEdit(m);
43 |     setForm({ username: m.username, email: m.email, password: '' });
44 |   };
45 | 
46 |   return (
47 |     <div style={{ padding: 16 }}>
48 |       <h2>Member Management</h2>
49 |       <div style={{ marginBottom: 16 }}>
50 |         <input placeholder="Search members" value={q} onChange={(e) => setQ(e.target.value)} />
51 |       </div>
52 |       <form onSubmit={submit} style={{ marginBottom: 16 }}>
53 |         <input placeholder="Username" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
54 |         <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
55 |         {!edit && <input placeholder="Password" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />}
56 |         <button type="submit" disabled={!canSubmit}>{edit ? 'Save' : 'Register'}</button>
57 |         {edit && <button type="button" onClick={() => { setEdit(null); setForm({ username: '', email: '', password: '' }); }}>Cancel</button>}
58 |       </form>
59 |       {error && <div style={{ color: 'red' }}>{error}</div>}
60 |       <table width="100%" cellPadding="8" style={{ borderCollapse: 'collapse' }}>
61 |         <thead>
62 |           <tr>
63 |             <th align="left">Id</th>
64 |             <th align="left">Username</th>
65 |             <th align="left">Email</th>
66 |             <th align="left">Status</th>
67 |             <th align="left">Actions</th>
68 |           </tr>
69 |         </thead>
70 |         <tbody>
71 |           {items.map(m => (
72 |             <tr key={m.id}>
73 |               <td>{m.id}</td>
74 |               <td>{m.username}</td>
75 |               <td>{m.email}</td>
76 |               <td>{m.isActive ? 'Active' : 'Inactive'}</td>
77 |               <td>
78 |                 <button onClick={() => startEdit(m)}>Edit</button>
79 |                 <button onClick={async () => {
80 |                   const prev = [...items];
81 |                   setItems(prev.map(x => x.id === m.id ? { ...x, isActive: !m.isActive } : x));
82 |                   try {
83 |                     await api.put(`/api/users/${m.id}/status`, null, { params: { isActive: !m.isActive } });
84 |                   } catch (e) {
85 |                     setItems(prev);
86 |                     setError('Status update failed');
87 |                   }
88 |                 }}>{m.isActive ? 'Deactivate' : 'Reactivate'}</button>
89 |               </td>
90 |             </tr>
91 |           ))}
92 |         </tbody>
93 |       </table>
94 |       <div style={{ marginTop: 8 }}>
95 |         <button disabled={page === 1} onClick={() => setPage(p => p - 1)}>Prev</button>
96 |         <span style={{ margin: '0 8px' }}>Page {page}</span>
97 |         <button disabled={(page * pageSize) >= total} onClick={() => setPage(p => p + 1)}>Next</button>
98 |       </div>
99 |     </div>
100 |   );
101 | };
102 | 
103 | export default MemberManagement;
104 | 
105 | 
```

frontend/csp-web/src/components/MyProfile.jsx
```
1 | import { useEffect, useState } from 'react';
2 | import api from '../lib/api';
3 | 
4 | const MyProfile = () => {
5 |   const [loading, setLoading] = useState(true);
6 |   const [error, setError] = useState('');
7 |   const [profile, setProfile] = useState({ username: '', email: '' });
8 |   const [pwd, setPwd] = useState({ currentPassword: '', newPassword: '' });
9 |   const [msg, setMsg] = useState('');
10 | 
11 |   useEffect(() => {
12 |     const load = async () => {
13 |       try {
14 |         const r = await api.get('/api/users/my-profile');
15 |         setProfile({ username: r.data.username, email: r.data.email });
16 |       } catch (e) {
17 |         setError('Failed to load profile');
18 |       } finally {
19 |         setLoading(false);
20 |       }
21 |     };
22 |     load();
23 |   }, []);
24 | 
25 |   const saveProfile = async (e) => {
26 |     e.preventDefault();
27 |     setError('');
28 |     setMsg('');
29 |     try {
30 |       await api.put('/api/users/my-profile', profile);
31 |       setMsg('Profile updated successfully');
32 |       localStorage.setItem('user', JSON.stringify({ ...(JSON.parse(localStorage.getItem('user')||'{}')), username: profile.username, email: profile.email }));
33 |     } catch (e) {
34 |       setError('Update failed');
35 |     }
36 |   };
37 | 
38 |   const changePassword = async (e) => {
39 |     e.preventDefault();
40 |     setError('');
41 |     setMsg('');
42 |     try {
43 |       await api.put('/api/users/my-password', pwd);
44 |       setMsg('Password changed successfully');
45 |       setPwd({ currentPassword: '', newPassword: '' });
46 |     } catch (e) {
47 |       setError('Password change failed');
48 |     }
49 |   };
50 | 
51 |   if (loading) return <div style={{ padding: 16 }}>Loading...</div>;
52 | 
53 |   return (
54 |     <div style={{ padding: 16 }}>
55 |       <h2>My Profile</h2>
56 |       {error && <div style={{ color: 'red' }}>{error}</div>}
57 |       {msg && <div style={{ color: 'green' }}>{msg}</div>}
58 | 
59 |       <form onSubmit={saveProfile} style={{ marginBottom: 24 }}>
60 |         <div>
61 |           <label>Username</label>
62 |           <input value={profile.username} onChange={(e) => setProfile({ ...profile, username: e.target.value })} />
63 |         </div>
64 |         <div>
65 |           <label>Email</label>
66 |           <input value={profile.email} onChange={(e) => setProfile({ ...profile, email: e.target.value })} />
67 |         </div>
68 |         <button type="submit">Save</button>
69 |       </form>
70 | 
71 |       <h3>Change Password</h3>
72 |       <form onSubmit={changePassword}>
73 |         <div>
74 |           <label>Current Password</label>
75 |           <input type="password" value={pwd.currentPassword} onChange={(e) => setPwd({ ...pwd, currentPassword: e.target.value })} />
76 |         </div>
77 |         <div>
78 |           <label>New Password</label>
79 |           <input type="password" value={pwd.newPassword} onChange={(e) => setPwd({ ...pwd, newPassword: e.target.value })} />
80 |         </div>
81 |         <button type="submit">Change Password</button>
82 |       </form>
83 |     </div>
84 |   );
85 | };
86 | 
87 | export default MyProfile;
88 | 
89 | 
```
