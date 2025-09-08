using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Csp.E2E.Tests;

public class DriverFixture : IDisposable
{
    public IWebDriver Driver { get; }

    public DriverFixture()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");
        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    public void Dispose()
    {
        Driver.Quit();
    }
}


