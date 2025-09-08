using System.Text.RegularExpressions;

namespace Csp.Api.Tests;

public class ValidationTests
{
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static bool IsStrongPassword(string pwd)
    {
        if (string.IsNullOrEmpty(pwd) || pwd.Length < 8) return false;
        return true;
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("user@sub.example.co", true)]
    [InlineData("invalid", false)]
    [InlineData("user@", false)]
    [InlineData("@example.com", false)]
    public void Email_Validation(string email, bool expected)
    {
        Assert.Equal(expected, IsValidEmail(email));
    }

    [Theory]
    [InlineData("1234567", false)]
    [InlineData("12345678", true)]
    [InlineData("P@ssw0rd", true)]
    public void Password_Strength(string pwd, bool expected)
    {
        Assert.Equal(expected, IsStrongPassword(pwd));
    }
}


