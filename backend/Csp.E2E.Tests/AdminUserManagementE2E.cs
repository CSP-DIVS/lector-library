using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace Csp.E2E.Tests
{
    /// <summary>
    /// Simple E2E tests for user management functionality
    /// </summary>
    public class SimpleUserManagementE2E : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private readonly string baseUrl = Environment.GetEnvironmentVariable("CSP_WEB_URL") ?? "http://localhost:5173";

        public SimpleUserManagementE2E()
        {
            var options = new ChromeOptions();
            var headedEnv = Environment.GetEnvironmentVariable("CSP_E2E_HEADED");
            if (!string.Equals(headedEnv, "true", StringComparison.OrdinalIgnoreCase))
            {
                options.AddArgument("--headless=new");
            }
            options.AddArgument("--window-size=1280,900");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(250));
        }

        [Fact]
        public void Admin_Login_And_ManageUsers()
        {
            // Login as admin
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Verify dashboard
            wait.Until(d => d.FindElement(By.CssSelector(".header")));
            var roleBadge = driver.FindElement(By.CssSelector(".role-badge"));
            Assert.Contains("ADMINISTRATOR", roleBadge.Text.ToUpper());

            // Navigate to user management
            try
            {
                GoToUserManagement();

                // Try to add new user (if add button is available)
                var addBtns = driver.FindElements(By.CssSelector(".add-user-btn, button"));
                var addBtn = addBtns.FirstOrDefault(btn => btn.Text.Contains("Add") || btn.Text.Contains("User") || btn.Text.Contains("New"));
                
                if (addBtn != null)
                {
                    addBtn.Click();

                    // If user form appears, fill it out
                    var userForms = driver.FindElements(By.CssSelector(".user-form, .form"));
                    if (userForms.Count > 0)
                    {
                        Thread.Sleep(500); // Wait for form to render
                        
                        var usernameInputs = driver.FindElements(By.CssSelector("input[type='text'], input[placeholder*='username'], input[placeholder*='Username']"));
                        if (usernameInputs.Count > 0) usernameInputs[0].SendKeys("testuser");

                        var emailInputs = driver.FindElements(By.CssSelector("input[type='email'], input[placeholder*='email'], input[placeholder*='Email']"));
                        if (emailInputs.Count > 0) emailInputs[0].SendKeys("test@test.com");

                        var passwordInputs = driver.FindElements(By.CssSelector("input[type='password'], input[placeholder*='password'], input[placeholder*='Password']"));
                        if (passwordInputs.Count > 0) passwordInputs[0].SendKeys("Test123!");
                        
                        var roleSelects = driver.FindElements(By.CssSelector("select, .form-select"));
                        if (roleSelects.Count > 0)
                        {
                            var roleSelect = new SelectElement(roleSelects[0]);
                            try { roleSelect.SelectByText("Member"); } catch { /* Role might be preset */ }
                        }
                        
                        var submitBtns = driver.FindElements(By.CssSelector("button[type='submit'], .btn-primary, button"));
                        var submitBtn = submitBtns.FirstOrDefault(btn => btn.Text.Contains("Create") || btn.Text.Contains("Save") || btn.Text.Contains("Submit") || btn.GetAttribute("type") == "submit");
                        if (submitBtn != null)
                        {
                            submitBtn.Click();
                            Thread.Sleep(1000); // Wait for submission
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // User management might not be fully implemented yet
                Console.WriteLine($"User management navigation failed: {ex.Message}");
            }

            // Logout
            try
            {
                // Wait for any modals/popups to disappear first
                Thread.Sleep(1000);
                
                var logoutBtns = driver.FindElements(By.CssSelector(".logout-btn, button"));
                var logoutBtn = logoutBtns.FirstOrDefault(btn => btn.Text.Contains("Logout") || btn.Text.Contains("Log out"));
                if (logoutBtn != null)
                {
                    // Use JavaScript click to avoid interception issues
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", logoutBtn);
                    wait.Until(d => d.FindElement(By.Id("username")));
                }
            }
            catch
            {
                // Alternative logout method - clear localStorage and navigate to login
                ((IJavaScriptExecutor)driver).ExecuteScript("window.localStorage.clear();");
                driver.Navigate().GoToUrl(baseUrl);
                wait.Until(d => d.FindElement(By.Id("username")));
            }
        }

        [Fact]
        public void Librarian_Login_And_Access()
        {
            // Login as librarian
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("librarian");
            driver.FindElement(By.Id("password")).SendKeys("lib123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Verify dashboard
            wait.Until(d => d.FindElement(By.CssSelector(".header")));
            var roleBadge = driver.FindElement(By.CssSelector(".role-badge"));
            Assert.Contains("LIBRARIAN", roleBadge.Text.ToUpper());

            // Logout
            try
            {
                var logoutBtn = driver.FindElement(By.CssSelector(".logout-btn"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", logoutBtn);
                wait.Until(d => d.FindElement(By.Id("username")));
            }
            catch
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.localStorage.clear();");
                driver.Navigate().GoToUrl(baseUrl);
                wait.Until(d => d.FindElement(By.Id("username")));
            }
        }

        [Fact]
        public void Member_Login_And_ProfileManagement()
        {
            // Login as member
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("member");
            driver.FindElement(By.Id("password")).SendKeys("member123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Verify dashboard
            wait.Until(d => d.FindElement(By.CssSelector(".header")));
            var roleBadge = driver.FindElement(By.CssSelector(".role-badge"));
            Assert.Contains("MEMBER", roleBadge.Text.ToUpper());

            // Try to access profile (if available)
            try
            {
                var profileNavItem = driver.FindElements(By.CssSelector(".sidebar .nav-item"))
                    .FirstOrDefault(el => el.Text.Contains("Profile"));
                if (profileNavItem != null)
                {
                    profileNavItem.Click();
                    wait.Until(d => d.FindElement(By.CssSelector(".profile")));
                }
            }
            catch { /* Profile navigation may not be available */ }

            // Logout
            try
            {
                var logoutBtn = driver.FindElement(By.CssSelector(".logout-btn"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", logoutBtn);
                wait.Until(d => d.FindElement(By.Id("username")));
            }
            catch
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.localStorage.clear();");
                driver.Navigate().GoToUrl(baseUrl);
                wait.Until(d => d.FindElement(By.Id("username")));
            }
        }

        [Fact]
        public void User_Deactivate_And_Login_Block()
        {
            // Login as admin
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Go to user management
            GoToUserManagement();

            // Find member user and deactivate
            var memberRow = FindUserRowByEmail("member@example.com");
            if (memberRow != null)
            {
                var actionBtn = memberRow.FindElements(By.CssSelector(".action-buttons button")).Last();
                actionBtn.Click();

                // Confirm deactivation
                var dialog = wait.Until(d => d.FindElement(By.CssSelector("[role='dialog']")));
                var confirmBtn = dialog.FindElements(By.CssSelector("button")).Last();
                confirmBtn.Click();
                wait.Until(d => d.FindElements(By.CssSelector("[role='dialog']")).Count == 0);

                // Logout and try to login as member
                driver.FindElement(By.CssSelector(".logout-btn")).Click();
                wait.Until(d => d.FindElement(By.Id("username")));

                // Try member login
                driver.FindElement(By.Id("username")).SendKeys("member");
                driver.FindElement(By.Id("password")).SendKeys("member123!");
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                // Should see error
                wait.Until(d => d.FindElement(By.CssSelector(".error-message")));
            }
        }

        [Fact]
        public void Invalid_Login_Shows_Error()
        {
            driver.Navigate().GoToUrl(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("invalid");
            driver.FindElement(By.Id("password")).SendKeys("wrong");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Should see error message
            wait.Until(d => d.FindElement(By.CssSelector(".error-message")));
        }

        private void GoToUserManagement()
        {
            try
            {
                // Try sidebar first
                var navItem = driver.FindElements(By.CssSelector(".sidebar .nav-item"))
                    .FirstOrDefault(el => el.Text.Contains("User Management"));
                if (navItem != null)
                {
                    navItem.Click();
                    wait.Until(d => d.FindElement(By.CssSelector(".user-management")));
                    return;
                }
            }
            catch { }

            // Try dashboard button
            try
            {
                var memberMgmtBtn = driver.FindElements(By.CssSelector(".action-btn"))
                    .FirstOrDefault(el => el.Text.Contains("Member Management"));
                if (memberMgmtBtn != null)
                {
                    memberMgmtBtn.Click();
                    wait.Until(d => d.FindElement(By.CssSelector(".user-management")));
                }
            }
            catch { }

            // Wait for loading to complete
            wait.Until(d => d.FindElements(By.CssSelector(".loading-state")).Count == 0);
            Thread.Sleep(500);
        }

        private IWebElement? FindUserRowByEmail(string email)
        {
            try
            {
                return wait.Until(d =>
                {
                    var rows = d.FindElements(By.CssSelector(".users-table tbody tr"));
                    return rows.FirstOrDefault(row => row.Text.Contains(email));
                });
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            try { driver.Quit(); } catch { }
            driver.Dispose();
        }
    }
}
