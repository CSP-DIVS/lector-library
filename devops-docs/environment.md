# Environment & App Settings

## `backend/Csp.Api/appsettings.Development.json`
Defines settings optimized for local development:
- Sets log level to `Information`, minimizing console noise but surfacing actionable info/errors.
- Connects to local MySQL instance with default credentials; adjust only for local testing, not production.

## `backend/Csp.Api/appsettings.Production.json`
Applies all production-only settings:
- Increases warning and error thresholds in logs for noise reduction (`Warning` level across .NET and EF Core).
- Fully managed cloud DB connection string (Azure MySQL), contains DB, host, user/id, password, SSL config.
- Strict CORS: allows only requests from the deployed production domain, guarding against XSS/CSRF.
- `JwtSettings` uses environment interpolated secret, with controlled issuer, audience, and token lifetime to secure user sessions.

## `backend/Csp.Api/appsettings.json`
Common/shared defaults for both environments:
- Sets up connection string and logging format, with settings overridden by environment-specific JSON.
- Wildcard `AllowedHosts` for .NET host resolving (can configure strict rules in prod as needed).
- Defines common `JwtSettings` template, to be securely injected at runtime.

## `backend/Csp.Api/Properties/launchSettings.json`
Defines debug launch profiles for local development:
- Presents `http` and `https` launch options for VS Code/Visual Studio and command line workflows.
- Sets `ASPNETCORE_ENVIRONMENT` to `Development`, ensuring that dev features are always enabled when running locally.
