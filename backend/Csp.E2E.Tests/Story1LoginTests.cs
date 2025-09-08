using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Csp.E2E.Tests;

public class Story1LoginTests : IClassFixture<DriverFixture>
{
    private readonly IWebDriver driver;

    public Story1LoginTests(DriverFixture fx)
    {
        driver = fx.Driver;
    }

    private void Navigate() => driver.Navigate().GoToUrl(Config.BaseUrl);

    [Fact]
    public void Login_Success_Member()
    {
        Navigate();
        driver.FindElement(By.Id("username")).SendKeys("member");
        driver.FindElement(By.Id("password")).SendKeys("member123!");
        driver.FindElement(By.CssSelector("button.submit-btn")).Click();

        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Member Actions']")).Any());
    }

    [Fact]
    public void Login_WrongPassword_ShowsError()
    {
        Navigate();
        driver.FindElement(By.Id("username")).SendKeys("member");
        driver.FindElement(By.Id("password")).SendKeys("wrong");
        driver.FindElement(By.CssSelector("button.submit-btn")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.ClassName("error-message")).Any());
    }
}


