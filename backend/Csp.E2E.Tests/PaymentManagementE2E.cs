using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Csp.E2E.Tests
{
    public class PaymentManagementE2E : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private readonly string baseUrl = Environment.GetEnvironmentVariable("CSP_WEB_URL") ?? "http://localhost:5173";

        public PaymentManagementE2E()
        {
            var options = new ChromeOptions();
            var headlessEnv = Environment.GetEnvironmentVariable("CSP_E2E_HEADLESS");
            if (string.Equals(headlessEnv, "true", StringComparison.OrdinalIgnoreCase))
            {
                options.AddArgument("--headless=new");
            }
            options.AddArgument("--window-size=1280,900");
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-web-security");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(15), TimeSpan.FromMilliseconds(250));
        }

        [Fact]
        public void Librarian_Can_Navigate_To_Fines_And_See_Tabs()
        {
            // Login as librarian
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("librarian");
            driver.FindElement(By.Id("password")).SendKeys("lib123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Navigate to Fines & Payment
            var navItems = wait.Until(d => d.FindElements(By.CssSelector(".sidebar .nav-item")));
            var finesNavItem = navItems.FirstOrDefault(el => el.Text.ToLower().Contains("fines"));
            Assert.NotNull(finesNavItem);
            finesNavItem!.Click();

            // Verify page loads
            wait.Until(d => d.FindElement(By.CssSelector(".fines-payment-page")));
            var header = driver.FindElement(By.CssSelector(".fines-payment-page .page-header h1"));
            Assert.Contains("Fines", header.Text, StringComparison.OrdinalIgnoreCase);

            // Verify tabs present and switch to Payment History
            var tabs = driver.FindElements(By.CssSelector(".tabs .tab"));
            Assert.True(tabs.Count >= 2);
            var historyTab = tabs.FirstOrDefault(t => t.Text.Contains("History", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(historyTab);
            historyTab!.Click();

            // Expect payment history section area present (data may be empty)
            Thread.Sleep(500);
            var sectionExists = driver.FindElements(By.CssSelector(".payment-history-section, .empty-state")).Count > 0;
            Assert.True(sectionExists);
        }

        [Fact]
        public void Member_Can_View_Fines_Page()
        {
            // Login as member
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("member");
            driver.FindElement(By.Id("password")).SendKeys("member123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Go to My Fines & Payments
            var navItems = wait.Until(d => d.FindElements(By.CssSelector(".sidebar .nav-item")));
            var finesNavItem = navItems.FirstOrDefault(el => el.Text.ToLower().Contains("fines") || el.Text.ToLower().Contains("payments"));
            Assert.NotNull(finesNavItem);
            finesNavItem!.Click();

            // Verify page loads
            wait.Until(d => d.FindElement(By.CssSelector(".fines-payment-page")));
            var header = driver.FindElement(By.CssSelector(".fines-payment-page .page-header h1"));
            Assert.True(header.Text.Contains("Payments", StringComparison.OrdinalIgnoreCase) || header.Text.Contains("Fines", StringComparison.OrdinalIgnoreCase));

            // Tabs visible
            var tabs = driver.FindElements(By.CssSelector(".tabs .tab"));
            Assert.True(tabs.Count >= 1);
        }

        public void Dispose()
        {
            try { driver.Quit(); } catch { }
            driver.Dispose();
        }
    }
}