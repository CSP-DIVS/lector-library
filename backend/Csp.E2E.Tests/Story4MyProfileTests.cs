using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Csp.E2E.Tests;

public class Story4MyProfileTests : IClassFixture<DriverFixture>
{
    private readonly IWebDriver driver;
    public Story4MyProfileTests(DriverFixture fx) { driver = fx.Driver; }

    private void LoginAsMember()
    {
        driver.Navigate().GoToUrl(Config.BaseUrl);
        driver.FindElement(By.Id("username")).SendKeys("member");
        driver.FindElement(By.Id("password")).SendKeys("member123!");
        driver.FindElement(By.CssSelector("button.submit-btn")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Member Actions']")).Any());
    }

    [Fact]
    public void Update_Profile_And_Change_Password()
    {
        LoginAsMember();
        driver.FindElement(By.XPath("//button[contains(.,'My Profile')]")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h2[text()='My Profile']")).Any());

        var email = driver.FindElement(By.XPath("//label[text()='Email']/following::input[1]"));
        var current = email.GetAttribute("value");
        email.Clear();
        email.SendKeys(current);
        driver.FindElement(By.XPath("//form[.//label[text()='Email']]//button[text()='Save']")).Click();

        driver.FindElement(By.XPath("//label[text()='Current Password']/following::input[1]")).SendKeys("member123!");
        driver.FindElement(By.XPath("//label[text()='New Password']/following::input[1]")).SendKeys("member123!x");
        driver.FindElement(By.XPath("//form[.//label[text()='New Password']]//button[text()='Change Password']")).Click();
    }
}


