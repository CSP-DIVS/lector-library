namespace Csp.E2E.Tests;

public static class Config
{
    public static string BaseUrl => Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5173";
}


