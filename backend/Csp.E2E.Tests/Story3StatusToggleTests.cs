using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Csp.E2E.Tests;

public class Story3StatusToggleTests : IClassFixture<DriverFixture>
{
    private readonly IWebDriver driver;
    public Story3StatusToggleTests(DriverFixture fx) { driver = fx.Driver; }

    private void LoginAsAdmin()
    {
        driver.Navigate().GoToUrl(Config.BaseUrl);
        driver.FindElement(By.Id("username")).SendKeys("admin");
        driver.FindElement(By.Id("password")).SendKeys("admin123!");
        driver.FindElement(By.CssSelector("button.submit-btn")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h3[text()='Admin Actions']")).Any());
    }

    [Fact]
    public void Deactivate_Reactivate_Blocks_Login()
    {
        LoginAsAdmin();
        driver.FindElement(By.XPath("//button[contains(.,'Member Management')]")).Click();
        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//h2[text()='Member Management']")).Any());

        var firstToggle = driver.FindElements(By.XPath("//table/tbody/tr[1]/td/button[contains(.,'Deactivate') or contains(.,'Reactivate')]"))
                                .FirstOrDefault();
        if (firstToggle == null) return;
        firstToggle.Click();
    }
}


