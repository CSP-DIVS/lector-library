using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Csp.E2E.Tests
{
    public class AdminUserManagementE2E : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private readonly string baseUrl = Environment.GetEnvironmentVariable("CSP_WEB_URL") ?? "http://localhost:5173";
        private readonly bool runHeaded;

        public AdminUserManagementE2E()
        {
            var options = new ChromeOptions();
            var headedEnv = Environment.GetEnvironmentVariable("CSP_E2E_HEADED");
            runHeaded = string.Equals(headedEnv, "true", StringComparison.OrdinalIgnoreCase);
            if (!runHeaded)
            {
                options.AddArgument("--headless=new");
            }
            options.AddArgument("--window-size=1280,900");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(250));
        }

        [Fact]
        public void Admin_Deactivate_Reactivate_User_And_Verify_Login_Behavior()
        {
            // Login as admin
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            Pause(1000);

            // Navigate to User Management (try sidebar first, then dashboard button)
            var navigated = false;
            try
            {
                var navItem = wait.Until(d =>
                {
                    var items = d.FindElements(By.CssSelector(".sidebar .nav-item"));
                    return items.FirstOrDefault(el => (el.Text ?? string.Empty).Contains("User Management", StringComparison.OrdinalIgnoreCase));
                });
                if (navItem != null)
                {
                    navItem.Click();
                    navigated = true;
                }
            }
            catch { /* fallback below */ }

            if (!navigated)
            {
                var memberMgmtBtn = wait.Until(d =>
                {
                    var buttons = d.FindElements(By.CssSelector(".action-btn"));
                    return buttons.FirstOrDefault(el => (el.Text ?? string.Empty).Contains("Member Management", StringComparison.OrdinalIgnoreCase));
                });
                Assert.NotNull(memberMgmtBtn);
                memberMgmtBtn!.Click();
            }

            // Wait for User Management view to render
            wait.Until(d => d.FindElement(By.CssSelector(".user-management")));
            wait.Until(d => d.FindElement(By.CssSelector(".users-table")));
            // Ensure loading state (if any) is gone
            wait.Until(d => d.FindElements(By.CssSelector(".loading-state")).Count == 0);
            Pause(1000);

            // Locate seeded 'member' row (by email first, then username as fallback)
            var memberRow = wait.Until(d =>
            {
                var byEmail = d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(., 'member@example.com')]]")).FirstOrDefault();
                if (byEmail != null) return byEmail;
                var byUsername = d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(@class,'username-cell') and contains(., 'member')]]")).FirstOrDefault();
                return byUsername;
            });
            Assert.NotNull(memberRow);

            // Capture current status text
            var statusCell = memberRow.FindElement(By.CssSelector("td:nth-child(5) .badge"));
            bool BadgeIsActive(IWebElement badge) => string.Equals((badge.Text ?? string.Empty).Trim(), "Active", StringComparison.OrdinalIgnoreCase);
            var wasActive = BadgeIsActive(statusCell);

            // Click the Deactivate/Activate button
            var actionBtn = memberRow.FindElements(By.CssSelector(".action-buttons button")).Last();
            var expectedAfterToggle = actionBtn.Text.Contains("Deactivate", StringComparison.OrdinalIgnoreCase)
                ? "Inactive"
                : "Active";
            var expectedIsActive = string.Equals(expectedAfterToggle, "Active", StringComparison.OrdinalIgnoreCase);
            Pause(800);
            actionBtn.Click();

            // Confirm in modal (role-based selector)
            var dialog = wait.Until(d => d.FindElement(By.CssSelector("[role='dialog']")));
            var confirmBtn = dialog.FindElements(By.CssSelector("button")).Last();
            Pause(500);
            confirmBtn.Click();

            // Wait until badge reflects expected state
            wait.Until(d =>
            {
                var row = d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(., 'member@example.com')]]")).FirstOrDefault()
                          ?? d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(@class,'username-cell') and contains(., 'member')]]")).FirstOrDefault();
                if (row == null) return false;
                var badge = row.FindElement(By.CssSelector("td:nth-child(5) .badge"));
                var isActive = BadgeIsActive(badge);
                return isActive == expectedIsActive;
            });
            Pause(1000);

            // Verify state
            memberRow = wait.Until(d =>
            {
                return d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(., 'member@example.com')]]")).FirstOrDefault()
                    ?? d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(@class,'username-cell') and contains(., 'member')]]")).First();
            });
            statusCell = memberRow.FindElement(By.CssSelector("td:nth-child(5) .badge"));
            var isActiveNow = BadgeIsActive(statusCell);
            Assert.Equal(expectedIsActive, isActiveNow);

            // If deactivated, verify member cannot log in; then reactivate to restore
            if (!isActiveNow)
            {
                // Log out by clearing localStorage (simple approach)
                ((IJavaScriptExecutor)driver).ExecuteScript("window.localStorage.clear();");
                driver.Navigate().GoToUrl(baseUrl);

                // Try member login
                wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("member");
                driver.FindElement(By.Id("password")).SendKeys("member123!");
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                // Expect error message on UI
                wait.Until(d => d.FindElement(By.CssSelector(".error-message")));

                // Reactivate: login as admin again
                driver.Navigate().GoToUrl(baseUrl);
                wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("admin");
                driver.FindElement(By.Id("password")).SendKeys("admin123!");
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();
                Pause(800);
                var reNavigated = false;
                try
                {
                    var navItem2 = wait.Until(d =>
                    {
                        var items = d.FindElements(By.CssSelector(".sidebar .nav-item"));
                        return items.FirstOrDefault(el => (el.Text ?? string.Empty).Contains("User Management", StringComparison.OrdinalIgnoreCase));
                    });
                    if (navItem2 != null)
                    {
                        navItem2.Click();
                        reNavigated = true;
                    }
                }
                catch { /* fallback below */ }

                if (!reNavigated)
                {
                    wait.Until(d =>
                    {
                        var buttons = d.FindElements(By.CssSelector(".action-btn"));
                        return buttons.FirstOrDefault(el => (el.Text ?? string.Empty).Contains("Member Management", StringComparison.OrdinalIgnoreCase));
                    })!.Click();
                }
                wait.Until(d => d.FindElement(By.CssSelector(".user-management")));
                wait.Until(d => d.FindElement(By.CssSelector(".users-table")));
                wait.Until(d => d.FindElements(By.CssSelector(".loading-state")).Count == 0);
                Pause(800);
                memberRow = wait.Until(d =>
                {
                    return d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(., 'member@example.com')]]")).FirstOrDefault()
                        ?? d.FindElements(By.XPath("//table[contains(@class,'users-table')]//tbody//tr[.//td[contains(@class,'username-cell') and contains(., 'member')]]")).First();
                });
                actionBtn = memberRow.FindElements(By.CssSelector(".action-buttons button")).Last();
                Pause(500);
                actionBtn.Click();
                dialog = wait.Until(d => d.FindElement(By.CssSelector("[role='dialog']")));
                confirmBtn = dialog.FindElements(By.CssSelector("button")).Last();
                Pause(500);
                confirmBtn.Click();
                wait.Until(d => d.FindElement(By.CssSelector(".users-table")));
                Pause(800);
            }
        }

        private void Pause(int milliseconds)
        {
            if (runHeaded && milliseconds > 0)
            {
                Thread.Sleep(milliseconds);
            }
        }

        public void Dispose()
        {
            try { driver.Quit(); } catch { }
            driver.Dispose();
        }
    }
}
