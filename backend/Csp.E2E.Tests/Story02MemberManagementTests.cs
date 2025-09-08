using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Csp.E2E.Tests;

public class Story02MemberManagementTests : IClassFixture<DriverFixture>
{
    private readonly IWebDriver driver;
    public Story02MemberManagementTests(DriverFixture fx) { driver = fx.Driver; }

    private void LoginAsAdmin()
    {
        driver.Navigate().GoToUrl(Config.BaseUrl);
        driver.FindElement(By.Id("username")).SendKeys("admin");
        driver.FindElement(By.Id("password")).SendKeys("admin123!");
        driver.FindElement(By.CssSelector("button.submit-btn")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Admin Actions']")).Any());
    }

    [Fact]
    public void Register_List_Search_Edit_Member()
    {
        LoginAsAdmin();
        driver.FindElement(By.XPath("//button[contains(.,'Member Management')]")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h2[text()='Member Management']")).Any());

        var username = "user" + Guid.NewGuid().ToString("N").Substring(0, 6);
        driver.FindElement(By.CssSelector("input[placeholder='Username']")).SendKeys(username);
        driver.FindElement(By.CssSelector("input[placeholder='Email']")).SendKeys(username + "@e.com");
        driver.FindElement(By.CssSelector("input[placeholder='Password']")).SendKeys("Passw0rd!");
        driver.FindElement(By.CssSelector("form button[type='submit']")).Click();

        driver.FindElement(By.CssSelector("input[placeholder='Search members']")).SendKeys(username);
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath($"//td[text()='{username}']")).Any());

        driver.FindElement(By.XPath($"//td[text()='{username}']/../td/button[contains(.,'Edit')]")).Click();
        var email = driver.FindElement(By.CssSelector("input[placeholder='Email']"));
        email.Clear();
        email.SendKeys(username + "+edited@e.com");
        driver.FindElement(By.CssSelector("form button[type='submit']")).Click();
    }
}


